namespace Showdown.NET.Definitions;

/// <summary>
///     Provides a simple way to parse Showdown's Pokémon ID format <c>POKEMON</c> as well as <c>DETAILS</c>.
/// </summary>
public readonly record struct PokemonIdentifier(PokemonID ID, string? Details)
{
    public static PokemonIdentifier Parse(string fullIdentifier)
    {
        PokemonID id = PokemonID.Parse(fullIdentifier, out var details);
        return new PokemonIdentifier(id, details);
    }
}

/// <summary>
///     <para>
///         A Pokémon ID is in the form <c>POSITION: NAME</c>.
///     </para>
///     This struct separates <c>POSITION</c> into <see cref="Player" /> and <see cref="Position" />:
///     <list type="bullet">
///         <item>
///             <see cref="Player" /> contains the player number.
///         </item>
///         <item>
///             <para>
///                 <see cref="Position" /> is <see langword="null" /> if this Pokémon is in the team, but not in battle.
///                 Otherwise, refers to the physical location of the Pokémon in <see cref="Protocol.GameType.Doubles" />
///                 and <see cref="Protocol.GameType.Triples" /> battles.
///             </para>
///             In singles, this will always be <c>'a'</c>.
///         </item>
///     </list>
/// </summary>
public readonly record struct PokemonID(int Player, char? Position, string Name)
{
    public static PokemonID Parse(string fullIdentifier, out string? details)
    {
        details = null;

        int player = fullIdentifier[1] - '0';
        char position = fullIdentifier[2];
        char? actualPosition = position == ':' ? null : position;

        string[] parts = fullIdentifier.Split(' ', 3);
        if (parts.Length == 3)
            details = parts[2];

        return new PokemonID(player, actualPosition, parts[1]);
    }
}
