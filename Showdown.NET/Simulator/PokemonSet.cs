using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Showdown.NET.Simulator;

/// <summary>
/// Represents a Pokémon set including species, moves, stats, and other battle-relevant data.
/// </summary>
/// <remarks>
/// <para>
/// This class is used to define a Pokémon's complete battle configuration including
/// its species, moves, ability, item, EVs, IVs, nature, and other attributes.
/// </para>
/// <para>
/// All string properties that accept IDs or names (like species, moves, items, abilities)
/// should be converted to lowercase IDs before sending to the simulator.
/// </para>
/// </remarks>
[PublicAPI]
public class PokemonSet
{
    /// <summary>
    /// Nickname. Should be identical to its base species if not specified
    /// by the player, e.g. "Minior".
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Species name (including forme if applicable), e.g. "Minior-Red".
    /// This should always be converted to an id before use.
    /// </summary>
    [JsonPropertyName("species")]
    public required string Species { get; set; }

    /// <summary>
    /// This can be an id, e.g. "whiteherb" or a full name, e.g. "White Herb".
    /// This should always be converted to an id before use.
    /// </summary>
    [JsonPropertyName("item")]
    public string Item { get; set; } = "";

    /// <summary>
    /// This can be an id, e.g. "shieldsdown" or a full name,
    /// e.g. "Shields Down".
    /// This should always be converted to an id before use.
    /// </summary>
    [JsonPropertyName("ability")]
    public string Ability { get; set; } = "";

    /// <summary>
    /// Each move can be an id, e.g. "shellsmash" or a full name,
    /// e.g. "Shell Smash"
    /// These should always be converted to ids before use.
    /// </summary>
    [JsonPropertyName("moves")]
    public List<string> Moves { get; set; } = [];

    /// <summary>
    /// This can be an id, e.g. "adamant" or a full name, e.g. "Adamant".
    /// This should always be converted to an id before use.
    /// </summary>
    [JsonPropertyName("nature")]
    public string Nature { get; set; } = "";
    
    /// <summary>
    /// The Pokémon's gender (typically "M", "F", or empty for genderless).
    /// </summary>
    [JsonPropertyName("gender")]
    public string Gender { get; set; } = "";

    /// <summary>
    /// Effort Values, used in stat calculation.
    /// These must be between 0 and 255, inclusive.
    ///
    /// Also used to store AVs for Let's Go
    /// </summary>
    [JsonPropertyName("evs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatsTable EVs { get; set; }

    /// <summary>
    /// Individual Values, used in stat calculation.
    /// These must be between 0 and 31, inclusive.
    ///
    /// These are also used as DVs, or determinant values, in Gens
    /// 1 and 2, which are represented as even numbers from 0 to 30.
    ///
    /// In Gen 2-6, these must match the Hidden Power type.
    ///
    /// In Gen 7+, Bottle Caps means these can either match the
    /// Hidden Power type or 31.
    /// </summary>
    [JsonPropertyName("ivs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatsTable IVs { get; set; }

    /// <summary>
    /// This is usually between 1 and 100, inclusive,
    /// but the simulator supports levels up to 9999 for testing purposes.
    /// </summary>
    [JsonPropertyName("level")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Level { get; set; }

    /// <summary>
    /// While having no direct competitive effect, certain Pokemon cannot
    /// be legally obtained as shiny, either as a whole or with certain
    /// event-only abilities or moves.
    /// </summary>
    [JsonPropertyName("shiny")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Shiny { get; set; }

    /// <summary>
    /// This is technically "Friendship", but the community calls this
    /// "Happiness".
    ///
    /// It's used to calculate the power of the moves Return and Frustration.
    /// This value must be between 0 and 255, inclusive.
    /// </summary>
    [JsonPropertyName("happiness")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Happiness { get; set; }

    /// <summary>
    /// The pokeball this Pokemon is in. Like shininess, this property
    /// has no direct competitive effects, but has implications for
    /// event legality. For example, any Rayquaza that knows V-Create
    /// must be sent out from a Cherish Ball.
    ///
    /// TODO: actually support this in the validator, switching animations,
    /// and the teambuilder.
    /// </summary>
    [JsonPropertyName("pokeball")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Pokeball { get; set; }

    /// <summary>
    /// Hidden Power type. Optional in older gens, but used in Gen 7+
    /// because `ivs` contain post-Battle-Cap values.
    /// </summary>
    [JsonPropertyName("hpType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string HpType { get; set; }

    /// <summary>
    /// Dynamax Level. Affects the amount of HP gained when Dynamaxed.
    /// This value must be between 0 and 10, inclusive.
    /// </summary>
    [JsonPropertyName("dynamaxLevel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DynamaxLevel { get; set; }
    
    /// <summary>
    /// Whether this Pokémon has the Gigantamax factor (Gen 8+).
    /// </summary>
    [JsonPropertyName("gigantamax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Gigantamax { get; set; }

    /// <summary>
    /// Tera Type
    /// </summary>
    [JsonPropertyName("teraType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string TeraType { get; set; }
}