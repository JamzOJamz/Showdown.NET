using Showdown.NET.Sim;

namespace Showdown.NET.Demo.Basic;

/// <summary>
///     Basic demo of sending commands to and reading outputs from Pokémon Showdown using Showdown.NET.
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        // Initialize Showdown.NET (run once).
        // Looks for Pokémon Showdown in ".\pokemon-showdown\dist" by default, or provide your own path.
        Showdown.Init();

        var stream = new BattleStream();

        var readerTask = Task.Run(async () =>
        {
            await foreach (var output in stream.ReadOutputsAsync()) Console.WriteLine(output);
        });

        stream.Write("""
                     >start {"formatid":"gen7randombattle"}
                     """);

        stream.Write("""
                     >player p1 {"name":"Alice"}
                     """);

        stream.Write("""
                     >player p2 {"name":"Bob"}
                     """);

        await readerTask;
    }
}