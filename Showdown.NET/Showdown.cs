using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace Showdown.NET;

public static class Showdown
{
    private static bool _initialized;
    private static readonly object InitLock = new();
    internal static ShowdownEngine? Engine;

    public static void Init(string showdownDistPath)
    {
        if (!Directory.Exists(showdownDistPath))
        {
            throw new DirectoryNotFoundException(
                $"The specified Showdown distribution path does not exist: '{showdownDistPath}'"
            );
        }
        
        lock (InitLock)
        {
            if (_initialized)
                return;

            Engine = new ShowdownEngine(showdownDistPath);
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
    
    private static V8ScriptEngine CreateV8Engine()
    {
        var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports);

        engine.AllowReflection = true;
        engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
        engine.DocumentSettings.Loader = new ShowdownDocumentLoader();
        
        return engine;
    }
}