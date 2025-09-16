using JetBrains.Annotations;
using Microsoft.ClearScript;

namespace Showdown.NET.Simulator;

[PublicAPI]
public class BattleStream
{
    private readonly dynamic _wrappedBattleStream;

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
        _wrappedBattleStream.write(command);
    }
    
    public async Task<string> ReadAsync()
    {
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
        while (true)
        {
            var output = await ReadAsync();
            if (output == null) yield break;
            yield return output;
        }
    }
}