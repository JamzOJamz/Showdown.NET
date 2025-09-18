using System.Security.Cryptography;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using SharpCompress.Archives.Zip;

namespace Showdown.NET.Core;

internal sealed class ShowdownEngine : IDisposable
{
    private bool _disposed;

    public ShowdownEngine(string showdownDistPath)
    {
        Initialize(CreateV8Engine(new ShowdownDiskDocumentLoader(), showdownDistPath));
    }

    public ShowdownEngine(ZipArchive showdownArchive)
    {
        Initialize(CreateV8Engine(new ShowdownMemoryDocumentLoader(showdownArchive)));
    }

    private V8ScriptEngine Engine { get; set; } = null!;

    public dynamic Script => Engine.Script;

    private void Initialize(V8ScriptEngine engine)
    {
        Engine = engine;
        ExposeHostObjects();
    }

    public void Execute(string code)
    {
        ThrowIfDisposed();
        Engine.Execute(new DocumentInfo { Category = ModuleCategory.CommonJS }, code);
    }

    public object EvaluateModule(string code)
    {
        ThrowIfDisposed();
        return Engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, code);
    }

    public object EvaluateScript(string code)
    {
        ThrowIfDisposed();
        return Engine.Evaluate(code);
    }

    private static V8ScriptEngine CreateV8Engine(DocumentLoader loader, string? searchPath = null)
    {
        var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports |
                                        V8ScriptEngineFlags.EnableTaskPromiseConversion);

        engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
        engine.DocumentSettings.Loader = loader;

        if (!string.IsNullOrEmpty(searchPath)) engine.DocumentSettings.SearchPath = searchPath;

        return engine;
    }

    private static V8ScriptEngine CreateV8Engine(ZipArchive showdownArchive)
    {
        var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports |
                                        V8ScriptEngineFlags.EnableTaskPromiseConversion);

        engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
        engine.DocumentSettings.Loader = new ShowdownMemoryDocumentLoader(showdownArchive);

        return engine;
    }

    private void ExposeHostObjects()
    {
        // Engine.AddHostType("Console", typeof(Console)); // For debugging

        Engine.AddHostObject("hostFs", new
        {
            readdirSync = new Func<string, object[]>(path =>
            {
                if (!path.StartsWith("vfs://"))
                    return Directory.GetFileSystemEntries(path).Select(Path.GetFileName)!.ToArray<object>();

                // Special handling when using virtual file system
                var documentLoader = (ShowdownMemoryDocumentLoader)Engine.DocumentSettings.Loader;
                return documentLoader.GetVirtualFileSystemEntries(path).ToArray<object>();
            })
        });

        Engine.AddHostObject("hostThrow", new Action<string>(code => throw new Exception(code)));

        Engine.AddHostObject("hostCrypto", new
        {
            getRandomValues = new Func<object, object>(array =>
            {
                ArgumentNullException.ThrowIfNull(array);

                dynamic dynArray = array;
                var length = (int)dynArray.length;
                if (length == 0) return array;

                using var rng = RandomNumberGenerator.Create();
                var firstElem = dynArray[0];

                switch (firstElem)
                {
                    case byte:
                        FillByteArray(dynArray, length, rng);
                        break;
                    case sbyte:
                        FillSByteArray(dynArray, length, rng);
                        break;
                    case ushort:
                        FillUInt16Array(dynArray, length, rng);
                        break;
                    case short:
                        FillInt16Array(dynArray, length, rng);
                        break;
                    case uint:
                        FillUInt32Array(dynArray, length, rng);
                        break;
                    case int:
                        FillInt32Array(dynArray, length, rng);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported typed array element type: {firstElem.GetType()}");
                }

                return array;
            })
        });
    }

    private static void FillByteArray(dynamic array, int length, RandomNumberGenerator rng)
    {
        var buffer = new byte[length];
        rng.GetBytes(buffer);
        for (var i = 0; i < length; i++) array[i] = buffer[i];
    }

    private static void FillSByteArray(dynamic array, int length, RandomNumberGenerator rng)
    {
        var buffer = new byte[length];
        rng.GetBytes(buffer);
        for (var i = 0; i < length; i++) array[i] = (sbyte)buffer[i];
    }

    private static void FillUInt16Array(dynamic array, int length, RandomNumberGenerator rng)
    {
        var buffer = new byte[length * 2];
        rng.GetBytes(buffer);
        for (var i = 0; i < length; i++) array[i] = BitConverter.ToUInt16(buffer, i * 2);
    }

    private static void FillInt16Array(dynamic array, int length, RandomNumberGenerator rng)
    {
        var buffer = new byte[length * 2];
        rng.GetBytes(buffer);
        for (var i = 0; i < length; i++) array[i] = BitConverter.ToInt16(buffer, i * 2);
    }

    private static void FillUInt32Array(dynamic array, int length, RandomNumberGenerator rng)
    {
        var buffer = new byte[length * 4];
        rng.GetBytes(buffer);
        for (var i = 0; i < length; i++) array[i] = BitConverter.ToUInt32(buffer, i * 4);
    }

    private static void FillInt32Array(dynamic array, int length, RandomNumberGenerator rng)
    {
        var buffer = new byte[length * 4];
        rng.GetBytes(buffer);
        for (var i = 0; i < length; i++) array[i] = BitConverter.ToInt32(buffer, i * 4);
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed) return;
        throw new ObjectDisposedException(nameof(ShowdownEngine));
    }

    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing) return;
        Engine?.Dispose();
        _disposed = true;
    }
}