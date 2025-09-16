> [!WARNING]
> **Experimental**
>
> Showdown.NET is currently **unstable** and APIs may change at any time. Use with caution!

# Showdown.NET

**Showdown.NET** is a **.NET library** that provides a C# interface for the [Pokémon Showdown](https://pokemonshowdown.com) battle simulator ([repo](https://github.com/smogon/pokemon-showdown)).

## Quick Start

```csharp
using Showdown.NET.Simulator;

// Initialize the battle simulator
Showdown.Init();

// Create a battle stream
var stream = new BattleStream();

// Start a random battle
stream.Write(""">start {"formatid":"gen7randombattle"}""");
stream.Write(""">player p1 {"name":"Alice"}""");
stream.Write(""">player p2 {"name":"Bob"}""");

// Read battle outputs
await foreach (var output in stream.ReadOutputsAsync())
{
    Console.WriteLine(output);
}
```

## Purpose

The primary goal of Showdown.NET is to **expose Pokémon Showdown's battle simulator through a C# API, handling JavaScript interop behind the scenes.**

This library was originally created to add turn-based battling capabilities to [Terramon](https://github.com/JamzOJamz/Terramon), a Pokémon mod for Terraria, but it can be used in any .NET project that needs Pokémon battle mechanics.

## How It Works

Showdown.NET uses [ClearScript](https://github.com/microsoft/ClearScript) to run the JavaScript build of Pokémon Showdown inside a native V8 script engine. A [Pokémon Showdown fork](https://github.com/JamzOJamz/pokemon-showdown), only lightly modified to be compatible with Showdown.NET, is included as a submodule. This submodule is copied to the library's output directory during builds and loaded at runtime.

This approach allows C# code to interact with Showdown’s battle logic directly, without rewriting the simulator from scratch.

## Why Showdown.NET Exists

Currently, there are very few Pokémon battle simulators that are accurate and fully featured. Most alternatives are incomplete, less popular, or untested for parity with the official game mechanics.

Instead of building a simulator from scratch—which would be a massive undertaking—Showdown.NET wraps the proven, accurate Pokémon Showdown engine. While this approach requires some setup and interop, it provides a **reliable, feature-complete battle engine accessible from C#**.

## License

Showdown.NET is released under the [same license as Pokémon Showdown](https://github.com/smogon/pokemon-showdown/blob/master/LICENSE) (MIT). See the [LICENSE](LICENSE) file for details.
