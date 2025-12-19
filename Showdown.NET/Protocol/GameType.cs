using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

/// <summary>
///     Represents the type of battle format (singles, doubles, etc.).
/// </summary>
[PublicAPI]
public enum GameType
{
    /// <summary>Singles battle - 1v1 with one active Pokémon per side.</summary>
    Singles,

    /// <summary>Doubles battle - 2v2 with two active Pokémon per side.</summary>
    Doubles,

    /// <summary>Triples battle - 3v3 with three active Pokémon per side.</summary>
    Triples,

    /// <summary>Multi battle - team battle with multiple trainers.</summary>
    Multi,

    /// <summary>Free-for-all battle - every player battles every other player.</summary>
    FreeForAll,
}