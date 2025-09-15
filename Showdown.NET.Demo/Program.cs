using Showdown.NET.Sim;

namespace Showdown.NET.Demo;

/// <summary>
/// Demonstrates basic usage of the Showdown.NET library for simulating Pokémon battles.
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        // Initialize Showdown.NET with the path to your local pokemon-showdown\dist folder
        Showdown.Init("<PATH_TO_POKEMON_SHOWDOWN_DIST>");
        
        var stream = new BattleStream();
        
        var readerTask = Task.Run(async () =>
        {
            await foreach (var output in stream.ReadOutputsAsync())
            {
                Console.WriteLine(output);
            }
        });
        
        stream.Write(">start {\"formatid\":\"gen7randombattle\"}");
        stream.Write(">player p1 {\"name\":\"Alice\"}");
        stream.Write(">player p2 {\"name\":\"Bob\"}");
        
        await readerTask;
    }
}