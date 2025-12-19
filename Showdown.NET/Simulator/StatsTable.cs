using JetBrains.Annotations;

namespace Showdown.NET.Simulator;

/// <summary>
/// Represents a table of Pokémon stats (HP, Attack, Defense, Special Attack, Special Defense, Speed).
/// </summary>
/// <remarks>
/// This class is used to store EVs (Effort Values), IVs (Individual Values),
/// or base stats for a Pokémon.
/// </remarks>
[PublicAPI]
public class StatsTable
{
    /// <summary>Gets or sets the Hit Points stat value.</summary>
    public int HP { get; set; }
    
    /// <summary>Gets or sets the Attack stat value.</summary>
    public int Atk { get; set; }
    
    /// <summary>Gets or sets the Defense stat value.</summary>
    public int Def { get; set; }
    
    /// <summary>Gets or sets the Special Attack stat value.</summary>
    public int SpA { get; set; }
    
    /// <summary>Gets or sets the Special Defense stat value.</summary>
    public int SpD { get; set; }
    
    /// <summary>Gets or sets the Speed stat value.</summary>
    public int Spe { get; set; }
}