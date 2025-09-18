using JetBrains.Annotations;
using Microsoft.ClearScript;
using SharpCompress.Archives.Zip;
using Showdown.NET.Core;

namespace Showdown.NET;

[PublicAPI]
public static class ShowdownHost
{
    private static bool _initialized;
    private static readonly object InitLock = new();
    internal static ShowdownEngine? Engine;

    public static void Init(string showdownDistPath = @".\pokemon-showdown\dist", string? v8RuntimeSearchPath = null)
    {
        var absolutePath = Path.GetFullPath(showdownDistPath);
        if (!Directory.Exists(absolutePath))
            throw new DirectoryNotFoundException($"Path does not exist: '{absolutePath}'");

        lock (InitLock)
        {
            if (_initialized) return;
            SetV8Path(v8RuntimeSearchPath);
            Engine = new ShowdownEngine(absolutePath);
            _initialized = true;
        }
    }

    public static void InitFromArchive(Stream showdownArchiveStream, string? v8RuntimeSearchPath = null)
    {
        ArgumentNullException.ThrowIfNull(showdownArchiveStream);

        lock (InitLock)
        {
            if (_initialized) return;
            SetV8Path(v8RuntimeSearchPath);
            var archive = ZipArchive.Open(showdownArchiveStream);
            Engine = new ShowdownEngine(archive);
            _initialized = true;
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
                throw new InvalidOperationException(
                    "Showdown has not been initialized. Did you forget to call Showdown.Init(pathToDist) first?"
                );
        }
    }
}