using JetBrains.Annotations;

namespace Showdown.NET.Definitions;

/// <summary>
///     Represents the six core stats in Pokémon battles.
/// </summary>
[PublicAPI]
public enum StatID
{
    /// <summary>Hit Points - determines how much damage a Pokémon can take.</summary>
    HP,

    /// <summary>Attack - affects the power of physical moves.</summary>
    Atk,

    /// <summary>Defense - reduces damage from physical moves.</summary>
    Def,

    /// <summary>Special Attack - affects the power of special moves.</summary>
    SpA,

    /// <summary>Special Defense - reduces damage from special moves.</summary>
    SpD,

    /// <summary>Speed - determines turn order in battle.</summary>
    Spe
}