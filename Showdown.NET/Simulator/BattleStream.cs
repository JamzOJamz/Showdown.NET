using JetBrains.Annotations;
using Microsoft.ClearScript;

namespace Showdown.NET.Simulator;

/// <summary>
/// Represents a bidirectional stream for communicating with the Pokémon Showdown battle simulator.
/// </summary>
/// <remarks>
/// <para>
/// BattleStream provides a high-level interface for sending commands to the simulator
/// and reading battle updates. Commands are sent using <see cref="Write"/> and responses
/// are read using <see cref="ReadAsync"/> or <see cref="ReadOutputsAsync"/>.
/// </para>
/// <example>
/// Basic usage:
/// <code>
/// ShowdownHost.Init();
/// using var stream = new BattleStream();
/// 
/// stream.Write("&gt;start {\"formatid\":\"gen7randombattle\"}");
/// stream.Write("&gt;player p1 {\"name\":\"Alice\"}");
/// stream.Write("&gt;player p2 {\"name\":\"Bob\"}");
/// 
/// await foreach (var output in stream.ReadOutputsAsync())
/// {
///     Console.WriteLine(output);
/// }
/// </code>
/// </example>
/// </remarks>
[PublicAPI]
public class BattleStream : IDisposable
{
    private readonly dynamic _wrappedBattleStream;
    
    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BattleStream"/> class.
    /// </summary>
    /// <exception cref="Exceptions.ShowdownException">
    /// Thrown when ShowdownHost has not been initialized.
    /// </exception>
    public BattleStream()
    {
        ShowdownHost.EnsureInitialized();
        _wrappedBattleStream = ShowdownHost.Engine!.EvaluateModule("""
            const BattleStream = require('./sim/battle-stream.js').BattleStream;
            return new BattleStream();
        """);
    }

    /// <summary>
    /// Writes a command to the battle stream.
    /// </summary>
    /// <param name="command">
    /// The command to send to the simulator. Must start with '&gt;' (e.g., "&gt;start").
    /// </param>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="command"/> is null.
    /// </exception>
    public void Write(string command)
    {
        ArgumentNullException.ThrowIfNull(command);
        ThrowIfDisposed();
        _wrappedBattleStream.write(command);
    }
    
    /// <summary>
    /// Asynchronously reads the next output from the battle stream.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains
    /// the next output string from the simulator, or null if the stream has ended.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    public async Task<string?> ReadAsync()
    {
        ThrowIfDisposed();
        var engine = ShowdownHost.Engine!;
        try
        {
            ShowdownHost.Engine!.Script.x = _wrappedBattleStream;
            var result = await (Task<object>)ShowdownHost.Engine.EvaluateScript("""
                (async () => {
                    var c = await x.next();
                    return c.value ?? null;
                })();
            """);

            return result as string;
        }
        finally
        {
            engine.Script.x = Undefined.Value;
        }
    }
    
    /// <summary>
    /// Returns an async enumerable that reads all outputs from the battle stream until completion.
    /// </summary>
    /// <returns>
    /// An async enumerable of output strings from the simulator.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance has been disposed.
    /// </exception>
    /// <remarks>
    /// This method is useful for processing all battle updates in a loop.
    /// The enumeration ends when the battle completes or the stream is disposed.
    /// </remarks>
    public async IAsyncEnumerable<string> ReadOutputsAsync()
    {
        ThrowIfDisposed();
        while (!IsDisposed)
        {
            var output = await ReadAsync();
            if (output == null) yield break;
            yield return output;
        }
    }
    
    private void ThrowIfDisposed()
    {
        if (!IsDisposed) return;
        throw new ObjectDisposedException(nameof(BattleStream));
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            try
            {
                if (_wrappedBattleStream is ScriptObject so)
                {
                    so.Dispose();
                }
            }
            catch
            {
                // Ignore errors during disposal to prevent exceptions in Dispose
            }
            IsDisposed = true;
        }
    }
}