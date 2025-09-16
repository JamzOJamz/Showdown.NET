using Showdown.NET.Simulator;

namespace Showdown.NET.Demo;

/// <summary>
///     Basic demo Showdown.NET.
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        // Initialize Showdown.NET (run once).
        // Looks for Pokémon Showdown in ".\pokemon-showdown\dist" by default, or provide your own path.
        Showdown.Init();

        Console.WriteLine("Select demo mode:");
        Console.WriteLine("1) Basic demo (hardcoded commands)");
        Console.WriteLine("2) REPL demo (type commands manually)");
        Console.Write("Choice (1 or 2): ");
        var choice = Console.ReadLine()?.Trim();

        switch (choice)
        {
            case "1":
                await RunBasicDemo();
                break;
            case "2":
                await RunReplDemo();
                break;
            default:
                Console.WriteLine("Invalid choice, exiting...");
                break;
        }
    }

    private static async Task RunBasicDemo()
    {
        Console.Clear();

        var stream = new BattleStream();

        var readerTask = Task.Run(async () =>
        {
            await foreach (var output in stream.ReadOutputsAsync()) Console.WriteLine(output);
        });

        stream.Write(""">start {"formatid":"gen7randombattle"}""");
        stream.Write(""">player p1 {"name":"Alice"}""");
        stream.Write(""">player p2 {"name":"Bob"}""");

        await readerTask;
    }

    private static Task RunReplDemo()
    {
        var stream = new BattleStream();

        // Task to print outputs asynchronously
        Task.Run(async () =>
        {
            await foreach (var output in stream.ReadOutputsAsync()) Console.WriteLine(output);
        });

        Console.Clear();
        Console.WriteLine("Pokémon Showdown Battle Simulator REPL started. Type commands and press Enter.");
        Console.WriteLine("Type 'exit' to quit.");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (input == null || input.Trim().Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                break;

            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.StartsWith('>'))
                input = input[1..];
            stream.Write('>' + input);
        }

        Console.WriteLine("Exiting REPL...");
        return Task.CompletedTask;
    }
}