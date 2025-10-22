namespace Showdown.NET.Definitions;

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
    public static PokemonID Parse(string fullIdentifier)
    {
        int player = fullIdentifier[1] - '0';
        char position = fullIdentifier[2];
        char? actualPosition = position == ':' ? null : position;

        string[] parts = fullIdentifier.Split(' ', 2);

        return new PokemonID(player, actualPosition, parts[1]);
    }
}

/// <summary>
///     A comma-separated list of all information about a Pokémon visible on the battle screen: species, shininess, gender, and level.
/// </summary>
public readonly record struct Details(string Species, int Level, char? Gender, bool Shiny, string? Terastallized)
{
    public static Details Parse(string details)
    {
        int level = 100;
        char? gender = null;
        bool shiny = false;
        string? terastallized = null;

        var deets = details.Split(',', StringSplitOptions.TrimEntries);
        string species = deets[0];

        foreach (var deet in deets.AsSpan(1))
        {
            char c = deet[0];
            switch (c)
            {
                case 'L':
                    level = int.Parse(deet.AsSpan(1));
                    break;
                case 'M':
                case 'F':
                    gender = c;
                    break;
                case 's':
                    shiny = true;
                    break;
                case 't':
                    terastallized = deet[5..];
                    break;
            }
        }

        return new Details(species, level, gender, shiny, terastallized);
    }
}
