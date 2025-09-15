using JetBrains.Annotations;
using Microsoft.ClearScript;

namespace Showdown.NET.Sim;

[PublicAPI]
public class BattleStream
{
    private readonly dynamic _wrappedBattleStream;

    public BattleStream()
    {
        Showdown.EnsureInitialized();
        _wrappedBattleStream = Showdown.Engine!.EvaluateModule("""
            const BattleStream = require('./sim/battle-stream.js').BattleStream;
            return new BattleStream();
        """);
    }

    public void Write(string chunk)
    {
        _wrappedBattleStream.write(chunk);
    }
    
    public async Task<string> ReadAsync()
    {
        var engine = Showdown.Engine!;
        try
        {
            Showdown.Engine!.Script.x = _wrappedBattleStream;
            var result = await (Task<object>)Showdown.Engine.EvaluateScript("""
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
        while (true)
        {
            var output = await ReadAsync();
            if (output == null) yield break;
            yield return output;
        }
    }
}