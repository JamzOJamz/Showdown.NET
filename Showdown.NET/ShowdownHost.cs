using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using SharpCompress.Archives.Zip;
using Showdown.NET.Core;
using Showdown.NET.Exceptions;

namespace Showdown.NET;

/// <summary>
///     Main entry point for initializing and managing the Pokémon Showdown battle simulator.
/// </summary>
/// <remarks>
///     <para>
///         ShowdownHost must be initialized before any battle operations can be performed.
///         Call <see cref="Init" /> or <see cref="InitFromArchive" /> once at application startup.
///     </para>
///     <example>
///         Basic initialization:
///         <code>
/// ShowdownHost.Init();
/// var stream = new BattleStream();
/// stream.Write("&gt;start {\"formatid\":\"gen7randombattle\"}");
/// </code>
///     </example>
/// </remarks>
[PublicAPI]
public static class ShowdownHost
{
    private static bool _initialized;
    private static readonly object InitLock = new();
    internal static ShowdownEngine? Engine;

    static ShowdownHost()
    {
        NativeLibrary.SetDllImportResolver(
            typeof(V8Runtime).Assembly, (name, _, _) =>
            {
                if (NativeLibrary.TryLoad(name, out var existingHandle)) return existingHandle;

                var searchDirs = HostSettings.AuxiliarySearchPath.Split(';');
                foreach (var searchDir in searchDirs)
                {
                    var libraryPath = Path.Combine(searchDir, name);
                    if (!File.Exists(libraryPath)) continue;
                    try
                    {
                        var libraryHandle = NativeLibrary.Load(libraryPath);
                        return libraryHandle;
                    }
                    catch
                    {
                        // Ignore load failures and continue searching other directories
                    }
                }

                return IntPtr.Zero;
            });
    }

    /// <summary>
    ///     Initializes the Showdown.NET library from a file system directory.
    /// </summary>
    /// <param name="showdownDistPath">
    ///     Path to the Pokémon Showdown distribution directory.
    ///     Defaults to ".\pokemon-showdown\dist".
    /// </param>
    /// <param name="v8RuntimeSearchPath">
    ///     Optional custom search path for V8 runtime binaries.
    /// </param>
    /// <exception cref="ShowdownInitializationException">
    ///     Thrown when the specified path does not exist or initialization fails.
    /// </exception>
    /// <remarks>
    ///     This method is thread-safe and idempotent. Calling it multiple times has no effect
    ///     after the first successful initialization.
    /// </remarks>
    public static void Init(string showdownDistPath = @".\pokemon-showdown\dist", string? v8RuntimeSearchPath = null)
    {
        var absolutePath = Path.GetFullPath(showdownDistPath);
        if (!Directory.Exists(absolutePath))
            throw new ShowdownInitializationException(
                $"Pokémon Showdown distribution directory not found at '{absolutePath}'. " +
                $"Ensure the pokemon-showdown submodule is initialized and the dist folder exists.");

        lock (InitLock)
        {
            if (_initialized) return;

            try
            {
                SetV8Path(v8RuntimeSearchPath);
                Engine = new ShowdownEngine(absolutePath);
                _initialized = true;
            }
            catch (Exception ex) when (ex is not ShowdownInitializationException)
            {
                throw new ShowdownInitializationException(
                    "Failed to initialize Showdown.NET. See inner exception for details.", ex);
            }
        }
    }

    /// <summary>
    ///     Initializes the Showdown.NET library from a ZIP archive stream.
    /// </summary>
    /// <param name="showdownArchiveStream">
    ///     Stream containing the Pokémon Showdown distribution as a ZIP archive.
    /// </param>
    /// <param name="v8RuntimeSearchPath">
    ///     Optional custom search path for V8 runtime binaries.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="showdownArchiveStream" /> is null.
    /// </exception>
    /// <exception cref="ShowdownInitializationException">
    ///     Thrown when initialization fails.
    /// </exception>
    /// <remarks>
    ///     This method is thread-safe and idempotent. Calling it multiple times has no effect
    ///     after the first successful initialization. This overload is useful for embedded
    ///     scenarios where the distribution is bundled as a resource.
    /// </remarks>
    public static void InitFromArchive(Stream showdownArchiveStream, string? v8RuntimeSearchPath = null)
    {
        ArgumentNullException.ThrowIfNull(showdownArchiveStream);

        lock (InitLock)
        {
            if (_initialized) return;

            try
            {
                SetV8Path(v8RuntimeSearchPath);
                var archive = ZipArchive.Open(showdownArchiveStream);
                Engine = new ShowdownEngine(archive);
                _initialized = true;
            }
            catch (Exception ex) when (ex is not ShowdownInitializationException)
            {
                throw new ShowdownInitializationException(
                    "Failed to initialize Showdown.NET from archive. See inner exception for details.", ex);
            }
        }
    }

    /// <summary>
    ///     Unloads the Showdown.NET library and releases all resources.
    /// </summary>
    /// <remarks>
    ///     After calling this method, <see cref="Init" /> or <see cref="InitFromArchive" />
    ///     must be called again before using the library. This method is thread-safe.
    /// </remarks>
    public static void Unload()
    {
        lock (InitLock)
        {
            if (!_initialized) return;

            Engine?.Dispose();
            Engine = null;
            _initialized = false;
        }
    }

    private static void SetV8Path(string? v8RuntimeSearchPath)
    {
        if (v8RuntimeSearchPath != null)
            HostSettings.AuxiliarySearchPath = v8RuntimeSearchPath;
    }

    internal static void EnsureInitialized()
    {
        lock (InitLock)
        {
            if (!_initialized)
                throw new ShowdownException(
                    "Showdown.NET has not been initialized. Call ShowdownHost.Init() or ShowdownHost.InitFromArchive() before using the library.");
        }
    }
}