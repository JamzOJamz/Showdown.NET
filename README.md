> [!WARNING]
> **Experimental**
>
> Showdown.NET is currently **unstable** and APIs may change at any time. Use with caution!

# Showdown.NET

**Showdown.NET** is a **.NET library** that provides a C# interface for the [Pokémon Showdown](https://pokemonshowdown.com) battle simulator ([repo](https://github.com/smogon/pokemon-showdown)).

## Quick Start

```csharp
using Showdown.NET;
using Showdown.NET.Simulator;
using Showdown.NET.Exceptions;

try
{
    // Initialize the battle simulator
    ShowdownHost.Init();

    // Create a battle stream with automatic disposal
    using var stream = new BattleStream();

    // Start a random battle
    stream.Write(""">start {"formatid":"gen7randombattle"}""");
    stream.Write(""">player p1 {"name":"Alice"}""");
    stream.Write(""">player p2 {"name":"Bob"}""");

    // Read battle outputs until completion
    await foreach (var output in stream.ReadOutputsAsync())
    {
        Console.WriteLine(output);
    }
}
catch (ShowdownInitializationException ex)
{
    Console.WriteLine($"Failed to initialize: {ex.Message}");
}
```

### Using the Protocol Codec

For easier command encoding, use the `ProtocolCodec` helper:

```csharp
using Showdown.NET.Protocol;
using Showdown.NET.Definitions;

// Encode commands using the helper
stream.Write(ProtocolCodec.EncodeStartCommand(FormatID.Gen9CustomGame));
stream.Write(ProtocolCodec.EncodeSetPlayerCommand(1, "Alice"));
stream.Write(ProtocolCodec.EncodeSetPlayerCommand(2, "Bob"));

// Make player choices
stream.Write(ProtocolCodec.EncodePlayerChoiceCommand(1, "move", "1"));
```

## Purpose

The primary goal of Showdown.NET is to **expose Pokémon Showdown's battle simulator through a C# API, handling JavaScript interop behind the scenes.**

This library was originally created to add turn-based battling capabilities to [Terramon](https://github.com/JamzOJamz/Terramon), a Pokémon mod for Terraria, but it can be used in any .NET project that needs Pokémon battle mechanics.

## How It Works

Showdown.NET uses [ClearScript](https://github.com/microsoft/ClearScript) to run the JavaScript build of Pokémon Showdown inside a native V8 script engine. A [Pokémon Showdown fork](https://github.com/JamzOJamz/pokemon-showdown), only lightly modified to be compatible with Showdown.NET, is included as a submodule. This submodule is copied to the library's output directory during builds and loaded at runtime.

This approach allows C# code to interact with Showdown’s battle logic directly, without rewriting the simulator from scratch.

## Error Handling

Showdown.NET provides custom exception types for better error handling:

```csharp
using Showdown.NET.Exceptions;

try
{
    ShowdownHost.Init("path/to/pokemon-showdown/dist");
}
catch (ShowdownInitializationException ex)
{
    // Handle initialization errors
    Console.WriteLine($"Failed to initialize: {ex.Message}");
}
catch (ShowdownException ex)
{
    // Handle other Showdown.NET errors
    Console.WriteLine($"Showdown error: {ex.Message}");
}
```

### Common Errors

- **ShowdownInitializationException**: Thrown when the Pokémon Showdown distribution directory is not found or initialization fails. Ensure the `pokemon-showdown` submodule is initialized and built.
- **ShowdownException**: Base exception for Showdown.NET operations. Check that `ShowdownHost.Init()` was called before using any battle features.
- **ObjectDisposedException**: Thrown when attempting to use a disposed `BattleStream`. Always use the `using` statement or call `Dispose()` when finished.

## Why Showdown.NET Exists

Currently, there are very few Pokémon battle simulators that are accurate and fully featured. Most alternatives are incomplete, less popular, or untested for parity with the official game mechanics.

Instead of building a simulator from scratch—which would be a massive undertaking—Showdown.NET wraps the proven, accurate Pokémon Showdown engine. While this approach requires some setup and interop, it provides a **reliable, feature-complete battle engine accessible from C#**.

## API Documentation

All public APIs include comprehensive XML documentation with IntelliSense support. Key namespaces:

- **Showdown.NET**: Main initialization via `ShowdownHost`
- **Showdown.NET.Simulator**: Battle simulation with `BattleStream`, `PokemonSet`, etc.
- **Showdown.NET.Protocol**: Protocol encoding/decoding via `ProtocolCodec`
- **Showdown.NET.Definitions**: Constants and enums (`FormatID`, `StatID`, `StatusID`, etc.)
- **Showdown.NET.Exceptions**: Custom exception types for error handling

## License

Showdown.NET is released under the [same license as Pokémon Showdown](https://github.com/smogon/pokemon-showdown/blob/master/LICENSE) (MIT). See the [LICENSE](LICENSE) file for details.
