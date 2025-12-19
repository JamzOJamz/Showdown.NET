using JetBrains.Annotations;

namespace Showdown.NET.Definitions;

/// <summary>
/// Contains a list of Pokémon Showdown battle format IDs supported by the simulator.
/// </summary>
/// <remarks>
/// These format IDs are used with the <c>&gt;start</c> command to initialize a battle.
/// Custom game formats allow you to specify custom teams and rules.
/// </remarks>
[PublicAPI]
public static class FormatID
{
    /// <summary>Generation 9 custom game format (singles).</summary>
    public const string Gen9CustomGame = "gen9customgame";
    
    /// <summary>Generation 8 custom game format (singles).</summary>
    public const string Gen8CustomGame = "gen8customgame";
    
    /// <summary>Generation 7 custom game format (singles).</summary>
    public const string Gen7CustomGame = "gen7customgame";
    
    /// <summary>Generation 6 custom game format (singles).</summary>
    public const string Gen6CustomGame = "gen6customgame";
    
    /// <summary>Generation 5 custom game format (singles).</summary>
    public const string Gen5CustomGame = "gen5customgame";
    
    /// <summary>Generation 4 custom game format (singles).</summary>
    public const string Gen4CustomGame = "gen4customgame";
    
    /// <summary>Generation 3 custom game format (singles).</summary>
    public const string Gen3CustomGame = "gen3customgame";
    
    /// <summary>Generation 2 custom game format (singles).</summary>
    public const string Gen2CustomGame = "gen2customgame";
    
    /// <summary>Generation 1 custom game format (singles).</summary>
    public const string Gen1CustomGame = "gen1customgame";

    /// <summary>Generation 9 custom game format (doubles).</summary>
    public const string Gen9DoublesCustomGame = "gen9doublescustomgame";
    
    /// <summary>Generation 8 custom game format (doubles).</summary>
    public const string Gen8DoublesCustomGame = "gen8doublescustomgame";
    
    /// <summary>Generation 7 custom game format (doubles).</summary>
    public const string Gen7DoublesCustomGame = "gen7doublescustomgame";
    
    /// <summary>Generation 6 custom game format (doubles).</summary>
    public const string Gen6DoublesCustomGame = "gen6doublescustomgame";
    
    /// <summary>Generation 5 custom game format (doubles).</summary>
    public const string Gen5DoublesCustomGame = "gen5doublescustomgame";
    
    /// <summary>Generation 4 custom game format (doubles).</summary>
    public const string Gen4DoublesCustomGame = "gen4doublescustomgame";
    
    /// <summary>Generation 3 custom game format (doubles).</summary>
    public const string Gen3DoublesCustomGame = "gen3doublescustomgame";
}
