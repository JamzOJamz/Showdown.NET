using Microsoft.ClearScript;
using Showdown.NET.Core;

namespace Showdown.NET;

public static class ShowdownHost
{
    private static bool _initialized;
    private static readonly object InitLock = new();
    internal static ShowdownEngine? Engine;

    public static void Init(string showdownDistPath = @".\pokemon-showdown\dist", string? v8RuntimeSearchPath = null)
    {
        var absoluteShowdownDistPath = Path.GetFullPath(showdownDistPath);

        if (!Directory.Exists(absoluteShowdownDistPath))
            throw new DirectoryNotFoundException(
                $"The specified Showdown distribution path does not exist: '{absoluteShowdownDistPath}'"
            );

        lock (InitLock)
        {
            if (_initialized)
                return;

            if (v8RuntimeSearchPath != null)
                HostSettings.AuxiliarySearchPath = v8RuntimeSearchPath;

            Engine = new ShowdownEngine(absoluteShowdownDistPath);

            _initialized = true;
        }
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