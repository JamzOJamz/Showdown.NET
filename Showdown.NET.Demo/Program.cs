﻿using Showdown.NET.Protocol;
using Showdown.NET.Simulator;

namespace Showdown.NET.Demo;

/// <summary>
///     Simple interactive demo program for Showdown.NET.
///     Supports:
///     1) A basic demo with hardcoded commands
///     2) A scripted battle demo
///     3) A REPL mode for typing commands manually
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        // Initialize the Showdown.NET runtime
        // By default, it looks for Pokémon Showdown in ".\pokemon-showdown\dist"
        // but you can also provide a custom path if the default location doesn't match your setup
        ShowdownHost.Init();

        Console.WriteLine("Select demo mode:");
        Console.WriteLine("1) Basic demo (hardcoded commands)");
        Console.WriteLine("2) Scripted battle demo");
        Console.WriteLine("3) REPL demo (type commands manually)");
        Console.Write("Choice (1, 2, or 3): ");
        var choice = Console.ReadLine()?.Trim();

        switch (choice)
        {
            case "1":
                await RunBasicDemo();
                break;
            case "2":
                await RunScriptedBattleDemo();
                break;
            case "3":
                await RunReplDemo();
                break;
            default:
                Console.WriteLine("Invalid choice, exiting...");
                break;
        }
    }

    private static Task RunBasicDemo()
    {
        Console.Clear();

        var stream = new BattleStream();
        var outputTask = PrintOutputsToConsole(stream);

        WriteAndLog(stream, ProtocolCodec.EncodeStartCommand("gen7randombattle")); // >start {"formatid":"gen7randombattle"}
        WriteAndLog(stream, ProtocolCodec.EncodeSetPlayerCommand(1, "Alice")); // >player p1 {"name":"Alice"}
        WriteAndLog(stream, ProtocolCodec.EncodeSetPlayerCommand(2, "Bob")); // >player p2 {"name":"Bob"}

        return outputTask;
    }

    private static Task RunScriptedBattleDemo()
    {
        Console.Clear();

        var p1Team = new PokemonSet[]
        {
            new()
            {
                Species = "Bulbasaur",
                Level = 14,
                Moves = ["Tackle"]
            }
        };

        var p2Team = new PokemonSet[]
        {
            new()
            {
                Species = "Rattata",
                Level = 3,
                Moves = ["Scratch"]
            }
        };

        var stream = new BattleStream();
        var outputTask = PrintOutputsToConsole(stream);

        WriteAndLog(stream, ProtocolCodec.EncodeStartCommand("gen9customgame"));
        WriteAndLog(stream, ProtocolCodec.EncodeSetPlayerCommand(1, "Red", p1Team));
        WriteAndLog(stream, ProtocolCodec.EncodeSetPlayerCommand(2, "Green", p2Team));
        WriteAndLog(stream, ProtocolCodec.EncodePlayerChoiceCommand(1, "team", "123456"));
        WriteAndLog(stream, ProtocolCodec.EncodePlayerChoiceCommand(2, "team", "123456"));
        WriteAndLog(stream, ProtocolCodec.EncodePlayerChoiceCommand(1, "move", "1"));
        WriteAndLog(stream, ProtocolCodec.EncodePlayerChoiceCommand(2, "move", "1"));

        return outputTask;
    }

    private static Task RunReplDemo()
    {
        var stream = new BattleStream();
        Task.Run(async () => await PrintOutputsToConsole(stream));

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

    private static void WriteAndLog(BattleStream stream, string command)
    {
        Console.WriteLine($"[>>>] {command}");
        stream.Write(command);
    }

    private static async Task PrintOutputsToConsole(BattleStream stream)
    {
        await foreach (var output in stream.ReadOutputsAsync())
        {
            Console.WriteLine($"[<<<] {output}");
            
            // var parsed = ProtocolCodec.Parse(output);
            // Work with parsed message here
        }
    }
}