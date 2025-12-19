using JetBrains.Annotations;

namespace Showdown.NET.Definitions;

/// <summary>
///     Represents a player side in a Pokémon battle.
/// </summary>
/// <param name="Player">The player number (1 or 2).</param>
/// <param name="Username">The username of the player.</param>
[PublicAPI]
public record struct Side(int Player, string Username)
{
    /// <summary>
    ///     Parses a side identifier string into a <see cref="Side" /> instance.
    /// </summary>
    /// <param name="input">
    ///     The side identifier string (e.g., "p1: Alice" for player 1 named Alice).
    /// </param>
    /// <returns>A <see cref="Side" /> instance representing the parsed data.</returns>
    public static Side Parse(string input)
    {
        var side = input[1] - '0';
        var username = input[4..];
        return new Side(side, username);
    }
}