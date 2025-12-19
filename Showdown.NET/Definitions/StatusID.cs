using JetBrains.Annotations;

namespace Showdown.NET.Definitions;

/// <summary>
/// Represents the major status conditions in Pokémon battles.
/// </summary>
[PublicAPI]
public enum StatusID
{
    /// <summary>No status condition.</summary>
    None,
    
    /// <summary>Fainted - the Pokémon has been knocked out.</summary>
    Fnt,
    
    /// <summary>Burned - takes damage each turn and reduces Attack.</summary>
    Brn,
    
    /// <summary>Frozen - cannot move (may thaw).</summary>
    Frz,
    
    /// <summary>Paralyzed - reduces Speed and may prevent movement.</summary>
    Par,
    
    /// <summary>Poisoned - takes damage each turn.</summary>
    Psn,
    
    /// <summary>Badly Poisoned - takes increasing damage each turn.</summary>
    Tox,
    
    /// <summary>Asleep - cannot move (for several turns).</summary>
    Slp,
}
