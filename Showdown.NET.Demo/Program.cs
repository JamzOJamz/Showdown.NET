using Showdown.NET.Simulator;

namespace Showdown.NET.Demo;

/// <summary>
///     Simple interactive demo program for Showdown.NET.
///     Supports:
///     1) A basic demo with hardcoded commands
///     2) A REPL mode for typing commands manually
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        // Initialize the Showdown.NET runtime
        // By default, it looks for Pokémon Showdown in ".\pokemon-showdown\dist"
        // but you can also provide a custom path if the default location doesn't match your setup
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

        // BattleStream handles communication with the simulator
        var stream = new BattleStream();

        stream.Write(""">start {"formatid":"gen7randombattle"}""");
        stream.Write(""">player p1 {"name":"Alice"}""");
        stream.Write(""">player p2 {"name":"Bob"}""");

        // Print all simulator outputs (battle updates, logs, etc.)
        await foreach (var output in stream.ReadOutputsAsync()) Console.WriteLine(output);
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

            // Accept commands typed with or without a leading '>'
            if (input.StartsWith('>'))
                input = input[1..];

            stream.Write('>' + input);
        }

        Console.WriteLine("Exiting REPL...");
        return Task.CompletedTask;
    }
}