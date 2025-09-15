using System.Security.Cryptography;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace Showdown.NET;

internal class ShowdownEngine
{
    public ShowdownEngine(string showdownDistPath)
    {
        Engine = CreateV8Engine(showdownDistPath);
        ExposeHostObjects();
    }

    private V8ScriptEngine Engine { get; }

    public dynamic Script => Engine.Script;

    public void Execute(string code)
    {
        Engine.Execute(new DocumentInfo { Category = ModuleCategory.CommonJS }, code);
    }

    public object EvaluateModule(string code)
    {
        return Engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, code);
    }

    public object EvaluateScript(string code)
    {
        return Engine.Evaluate(code);
    }

    private static V8ScriptEngine CreateV8Engine(string showdownDistPath)
    {
        var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports |
                                        V8ScriptEngineFlags.EnableTaskPromiseConversion);

        engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
        engine.DocumentSettings.SearchPath = showdownDistPath;
        engine.DocumentSettings.Loader = new ShowdownDocumentLoader();

        return engine;
    }

    private void ExposeHostObjects()
    {
        Engine.AddHostType("Console", typeof(Console)); // Debugging

        Engine.AddHostObject("hostFs", new
        {
            readdirSync = new Func<string, object[]>(path =>
                Directory.GetFileSystemEntries(path).Select(Path.GetFileName)!.ToArray<object>())
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
}