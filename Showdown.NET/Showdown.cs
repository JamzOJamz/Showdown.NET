using Showdown.NET.Core;

namespace Showdown.NET;

public static class Showdown
{
    private static bool _initialized;
    private static readonly object InitLock = new();
    internal static ShowdownEngine? Engine;

    public static void Init(string showdownDistPath = @".\pokemon-showdown\dist")
    {
        var absolutePath = Path.GetFullPath(showdownDistPath);
        
        if (!Directory.Exists(absolutePath))
        {
            throw new DirectoryNotFoundException(
                $"The specified Showdown distribution path does not exist: '{absolutePath}'"
            );
        }
        
        lock (InitLock)
        {
            if (_initialized)
                return;

            Engine = new ShowdownEngine(absolutePath);
            _initialized = true;
        }
    }
    
    internal static void EnsureInitialized()
    {
        lock (InitLock)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(
                    "Showdown has not been initialized. Did you forget to call Showdown.Init(pathToDist) first?"
                );
            }
        }
    }
}