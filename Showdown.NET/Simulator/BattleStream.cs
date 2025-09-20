using JetBrains.Annotations;
using Microsoft.ClearScript;

namespace Showdown.NET.Simulator;

[PublicAPI]
public class BattleStream : IDisposable
{
    private readonly dynamic _wrappedBattleStream;
    private bool _disposed;

    public BattleStream()
    {
        ShowdownHost.EnsureInitialized();
        _wrappedBattleStream = ShowdownHost.Engine!.EvaluateModule("""
            const BattleStream = require('./sim/battle-stream.js').BattleStream;
            return new BattleStream();
        """);
    }

    public void Write(string command)
    {
        ThrowIfDisposed();
        _wrappedBattleStream.write(command);
    }
    
    public async Task<string> ReadAsync()
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

            return (result as string)!;
        }
        finally
        {
            engine.Script.x = Undefined.Value;
        }
    }
    
    public async IAsyncEnumerable<string> ReadOutputsAsync()
    {
        ThrowIfDisposed();
        while (true)
        {
            var output = await ReadAsync();
            if (output == null) yield break;
            yield return output;
        }
    }
    
    private void ThrowIfDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(BattleStream));
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                if (_wrappedBattleStream is ScriptObject so) {
                    Console.WriteLine("Freeing script object");
                    so.Dispose();
                }
            }
            catch
            {
                // Ignore errors during disposal
            }
            _disposed = true;
        }
    }
}