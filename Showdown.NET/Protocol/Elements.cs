using JetBrains.Annotations;
using Showdown.NET.Definitions;

namespace Showdown.NET.Protocol;

/// <summary>
///     Provides abstraction over the messages which are output by the Pokémon Showdown simulator protocol.
/// </summary>
[PublicAPI]
public abstract record ProtocolElement
{
    /// <summary>
    ///     Battle actions (especially minor actions) often come with tags such as <c>[from] EFFECT</c>, <c>[of] SOURCE</c>.
    ///     <c>EFFECT</c> will be an effect (move, ability, item, status, etc), and <c>SOURCE</c> will be a Pokémon.
    ///     These can affect the message or animation displayed, but do not affect anything else.
    ///     Other tags include <c>[still]</c> (suppress animation) and <c>[silent]</c> (suppress message).
    /// </summary>
    public string[]? Tags { get; set; }
}

/// <summary>
///     Decorates any <see cref="ProtocolElement" /> type which is considered a minor action.
///     Minor actions are less important than major actions. Pretty much anything that happens in a
///     battle other than a switch or the fact that a move was used is a minor action. So yes, the
///     effects of a move such as damage or stat boosts are minor actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MinorActionAttribute : Attribute;

/// <summary>
///     The current <see cref="DateTimeOffset" /> - useful for determining when events occurred in real time.
/// </summary>
[PublicAPI]
public sealed record TimestampElement(DateTimeOffset Value) : ProtocolElement
{
    /// <summary>
    ///     Creates a new <see cref="TimestampElement" /> given a string <paramref name="value" />
    ///     representing a UNIX timestamp (the number of seconds since 1970).
    /// </summary>
    public static TimestampElement Parse(string value)
    {
        return new TimestampElement(DateTimeOffset.FromUnixTimeSeconds(long.Parse(value)));
    }
}

/// <summary>
///     <see cref="GameType" /> is <see cref="GameType.Singles" />, <see cref="GameType.Doubles" />,
///     <see cref="GameType.Triples" />, <see cref="GameType.Multi" />, or <see cref="GameType.FreeForAll" />.
/// </summary>
[PublicAPI]
public sealed record GameTypeElement(GameType GameType) : ProtocolElement
{
    public static GameTypeElement Parse(string gameType)
    {
        return new GameTypeElement(Enum.Parse<GameType>(gameType, true));
    }
}

/// <summary>
///     <para>
///         Player details.
///     </para>
///     <list type="bullet">
///         <item>
///             <see cref="Player" /> is <c>1</c> or <c>2</c>.
///         </item>
///         <item>
///             <see cref="Player" /> may also be <c>3</c> or <c>4</c> in 4-player battles.
///         </item>
///         <item>
///             <see cref="Username" /> is the username.
///         </item>
///         <item>
///             <see cref="Avatar" /> is the player's avatar identifier (usually a number, but other values
///             can be used for custom avatars.)
///         </item>
///         <item>
///             <see cref="Rating" /> is the player's Elo rating in the format they're playing.
///             This will only be displayed in rated battles and when the player is first introduced otherwise it's blank
///         </item>
///     </list>
/// </summary>
[PublicAPI]
public sealed record PlayerDetailsElement(int Player, string Username, string Avatar, string Rating) : ProtocolElement;

/// <summary>
///     Generation number, from 1 to 9. Stadium counts as its respective gens; Let's Go counts as 7,
///     and modded formats count as whatever gen they were based on.
/// </summary>
[PublicAPI]
public sealed record GenElement(int GenNum) : ProtocolElement
{
    public static GenElement Parse(string genNum)
        => new(int.Parse(genNum));
}

/// <summary>
///     Will be sent if the game is official in some way.
///     <list type="bullet">
///         <item>
///             If <see cref="Message" /> is <see langword="null" />, the game will affect the player's ladder rating (Elo score).
///         </item>
///         <item>
///             Otherwise, indicates an official game that is not actually rated, such as being a tournament game.
///         </item>
///     </list>
/// </summary>
[PublicAPI]
public sealed record RatedElement(string? Message) : ProtocolElement
{
    public static RatedElement Parse(string[] segments, out int usedCount)
    {
        string? message = null;
        usedCount = 0;

        if (segments.Length > 1)
        {
            message = segments[1];
            usedCount = 1;
        }

        return new RatedElement(message);
    }
}

/// <summary>
///     The name of the format being played. (see also <see cref="FormatID" />).
/// </summary>
/// <param name="FormatName"></param>
[PublicAPI]
public sealed record TierElement(string FormatName) : ProtocolElement;

/// <summary>
///     Will appear multiple times, one for each rule.
/// </summary>
[PublicAPI]
public sealed record RuleElement(string Rule, string Description) : ProtocolElement
{
    public static RuleElement Parse(string[] segments)
    {
        var parts = segments[1].Split(':', 2, StringSplitOptions.TrimEntries);
        return new RuleElement(parts[0], parts[1]);
    }
}

/// <summary>
///     Marks the start of Team Preview.
/// </summary>
[PublicAPI]
public sealed record ClearPokeElement : ProtocolElement;

/// <summary>
///     Declares a Pokémon for Team Preview.
///     <list type="bullet">
///         <item>
///             <see cref="Player" /> is the player ID (see <see cref="PlayerDetailsElement" />).
///         </item>
///         <item>
///             <see cref="Details" /> describes the Pokémon (see <see cref="Definitions.Details" />).
///         </item>
///         <item>
///             <see cref="Item" /> will be <c>item</c> if the Pokémon is holding an item, or blank if it isn't.
///         </item>
///     </list>
///     Note that forme and shininess are hidden on this, unlike on the <see cref="SwitchElement" /> details message.
/// </summary>
[PublicAPI]
public sealed record PokeElement(int Player, string Details, string Item) : ProtocolElement, IDetailsArg;

/// <summary>
///     These messages appear if you're playing a format that uses team previews.
/// </summary>
[PublicAPI]
public sealed record TeamPreviewElement : ProtocolElement;

/// <summary>
///     <list type="bullet">
///         <item>
///             <see cref="Player" /> is <c>1</c>, <c>2</c>, <c>3</c>, or <c>4</c>
///         </item>
///         <item>
///             <see cref="Size" /> is the number of Pokémon your opponent starts with. In games without Team Preview,
///             you don't know which Pokémon your opponent has, but you at least know how many there are.
///         </item>
///     </list>
/// </summary>
[PublicAPI]
public sealed record TeamSizeElement(int Player, int Size) : ProtocolElement
{
    public static TeamSizeElement Parse(string player, string size)
    {
        return new TeamSizeElement(player[1] - '0', int.Parse(size));
    }
}

/// <summary>
///     Indicates that the game has started.
/// </summary>
[PublicAPI]
public sealed record StartElement : ProtocolElement;

/// <summary>
///     <see cref="Username" /> has won the battle.
/// </summary>
[PublicAPI]
public sealed record WinElement(string Username) : ProtocolElement;

/// <summary>
///     The battle has ended in a tie.
/// </summary>
[PublicAPI]
public sealed record TieElement : ProtocolElement;

/// <summary>
///     A message related to the battle timer has been sent. The official client displays these messages in red.
///     <see cref="InactiveElement" /> means that the timer is on at the time the message was sent,
///     while <see cref="InactiveOffElement" /> means that the timer is off.
/// </summary>
[PublicAPI]
public sealed record InactiveElement(string Message) : ProtocolElement;

/// <inheritdoc cref="InactiveElement" />
[PublicAPI]
public sealed record InactiveOffElement(string Message) : ProtocolElement;

/// <summary>
///     <para>
///         The specified <see cref="Pokemon" /> has used move <see cref="Move" /> at <see cref="Target" />. If a move has multiple targets or no target,
///         <see cref="Target" /> should be ignored. If a move targets a side, <see cref="Target" /> will be a (possibly fainted) Pokémon on that side.
///     </para>
///     <see cref="Details" /> contains secondary information.
/// </summary>
[PublicAPI]
public sealed record MoveElement(string Pokemon, string Move, string Target, MoveDetails Details) : ProtocolElement, IPokemonArgs
{
    public static MoveElement Parse(string[] segments)
    {
        MoveDetails details = new();
        if (segments.Length > 4)
        {
            foreach (var tag in segments.AsSpan(4))
            {
                switch (tag)
                {
                    case "[miss]":
                        details.Miss = true;
                        break;
                    case "[still]":
                        details.Still = true;
                        break;
                    default:
                    {
                        if (tag.StartsWith("[anim]"))
                            details.Animation = tag[7..];
                        break;
                    }
                }
            }
        }
        return new MoveElement(segments[1], segments[2], segments[3], details);
    }
}

/// <summary>
///     Contains secondary information about a move that was used in a battle.
/// </summary>
[PublicAPI]
public struct MoveDetails(bool miss, bool still, string? anim)
{
    /// <summary>
    ///     If <see langword="true" />, the move missed.
    /// </summary>
    public bool Miss = miss;
    /// <summary>
    ///     If <see langword="true" />, no animation should play.
    /// </summary>
    public bool Still = still;
    /// <summary>
    ///     If not <see langword="null" />, contains the name of the move whose
    ///     animation should be used instead of the move that was actually used.
    /// </summary>
    public string? Animation = anim;
}

/// <summary>
///     <para>
///         A Pokémon identified by <see cref="Pokemon" /> has switched in (if there was an old Pokémon in that
///         position, it is switched out).
///     </para>
///     <para>
///         For the <see cref="Details" /> format, see <see cref="Definitions.Details" />.
///     </para>
///     <para>
///         <see cref="Pokemon" /> and <see cref="Details" /> represent all the information that can be used to tell
///         Pokémon apart.
///         If two Pokémon have the same <see cref="Pokemon" /> and <see cref="Details" /> (which will never happen in any
///         format with Species Clause),
///         you usually won't be able to tell if the same Pokémon switched in or a different Pokémon switched in.
///     </para>
///     <para>
///         The switched Pokémon has HP <see cref="HP" />, and status <see cref="Status" />. <see cref="HP" /> is specified
///         as a fraction; if it is your own Pokémon
///         then it will be <c>CURRENT/MAX</c>, if not, it will be <c>/100</c> if HP Percentage Mod is in effect and
///         <c>/48</c> otherwise.
///         <see cref="Status" /> can be left <see cref="StatusID.None" />, or it can be <see cref="StatusID.Slp" />, <see cref="StatusID.Par" />, etc.
///     </para>
/// </summary>
/// <remarks>
///     A <see cref="SwitchElement" /> indicates an intentional switch, in contrast with <see cref="DragElement" />,
///     which indicates it was unintentional (forced by Whirlwind, Roar, etc).
/// </remarks>
[PublicAPI]
public sealed record SwitchElement(string Pokemon, string Details, string HP, StatusID Status) : ProtocolElement, IPokemonArgs, IDetailsArg
{
    public static (string hp, StatusID status) ParseHPStatus(string hpStatus)
    {
        var parts = hpStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hp = parts[0];
        var status = parts.Length > 1 ? Enum.Parse<StatusID>(parts[1], true) : StatusID.None;
        return (hp, status);
    }
}

/// <inheritdoc cref="SwitchElement" />
[PublicAPI]
public sealed record DragElement(string Pokemon, string Details, string HP, StatusID Status) : ProtocolElement, IPokemonArgs, IDetailsArg;

/// <summary>
///     <para>
///         The specified Pokémon has changed formes (via Mega Evolution, ability, etc.) to <c>SPECIES</c>
///         (provided in <see cref="Details" /> for <see cref="DetailsChangeElement" />, provided directly for <see cref="FormeChangeElement" />.)
///         If the forme change is permanent (Mega Evolution or a Shaymin-Sky that is frozen), then <see cref="DetailsChangeElement" /> will appear;
///         otherwise, the client will send <see cref="FormeChangeElement" />.
///     </para>
///     Syntax is the same as <see cref="SwitchElement" />.
/// </summary>
[PublicAPI]
public sealed record DetailsChangeElement(string Pokemon, string Details, string HP, StatusID Status) : ProtocolElement, IPokemonArgs, IDetailsArg;

/// <inheritdoc cref="DetailsChangeElement" />
[PublicAPI]
[MinorAction]
public sealed record FormeChangeElement(string Pokemon, string Species, string HP, StatusID Status) : ProtocolElement, IPokemonArgs;

/// <summary>
///     <para>
///         Illusion has ended for the specified Pokémon.
///         Syntax is the same as <see cref="SwitchElement" />, but remember that everything you thought you knew about the previous Pokémon is now wrong.
///     </para>
///     <see cref="Pokemon" /> will be the NEW Pokémon ID - i.e. it will have the nickname of the Zoroark (or other Illusion user).
/// </summary>
[PublicAPI]
public sealed record ReplaceElement(string Pokemon, string Details, string HP, StatusID Status) : ProtocolElement, IPokemonArgs, IDetailsArg;

/// <summary>
///     Moves already active <see cref="Pokemon" /> to active field <see cref="Position" /> where the leftmost position is 0 and each position to the right counts up by 1.
/// </summary>
[PublicAPI]
public sealed record SwapElement(string Pokemon, int Position) : ProtocolElement, IPokemonArgs
{
    public static SwapElement Parse(string[] segments)
        => new(segments[1], int.Parse(segments[2]));

}

/// <summary>
///     The Pokémon <see cref="Pokemon" /> could not perform a move because of the indicated <see cref="Reason" />
///     (such as paralysis, Disable, etc). Sometimes, the move it was trying to use is given.
/// </summary>
[PublicAPI]
public sealed record CantElement(string Pokemon, string Reason, string? Move) : ProtocolElement, IPokemonArgs
{
    public static CantElement Parse(string[] segments, out int usedCount)
    {
        string? move = null;
        usedCount = 2;
        if (segments.Length > 3)
        {
            move = segments[3];
            usedCount = 3;
        }
        return new CantElement(segments[1], segments[2], move);
    }
}
/// <summary>
///     The Pokémon <see cref="Pokemon" /> has fainted.
/// </summary>
[PublicAPI]
public sealed record FaintElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The specified <see cref="Action" /> has failed against the <see cref="Pokemon" /> targeted. The
///     <see cref="Action" /> in question should be a move that fails due to its own mechanics. Moves (or effect
///     activations) that fail because they're blocked by another effect should use <c>-block</c> instead.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record FailElement(string Pokemon, string Action) : ProtocolElement, IPokemonArgs;

/// <summary>
///     An effect targeted at <see cref="Pokemon" /> was blocked by <see cref="Effect" />. This may optionally specify that
///     the effect was a <see cref="Move" /> from <see cref="Attacker" />. <c>[of]SOURCE</c> will note the owner of the
///     <see cref="Effect" />, in the case that it's not <see cref="Effect" /> (for instance, an ally with Aroma Veil.)
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record BlockElement(string Pokemon, string Effect, string? Move, string? Attacker) : ProtocolElement, IPokemonArgs
{
    public static BlockElement Parse(string[] segments, out int usedCount)
    {
        if (segments.Length < 5)
        {
            usedCount = 2;
            return new BlockElement(segments[1], segments[2], null, null);
        }

        usedCount = 4;
        return new BlockElement(segments[1], segments[2], segments[3], segments[4]);
    }
}

/// <summary>
///     A move has failed due to there being no target Pokémon <see cref="Pokemon" />. <see cref="Pokemon" /> is <see langword="null" /> in Generation 1.
///     This action is specific to Generations 1-4 as in later Generations a failed move will display using <see cref="FailElement" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record NoTargetElement(string? Pokemon) : ProtocolElement, IPokemonArgs
{
    public static NoTargetElement Parse(string[] segments, out int usedCount)
    {
        string? pokemon = null;
        usedCount = 0;

        if (segments.Length > 1)
        {
            pokemon = segments[1];
            usedCount = 1;
        }

        return new NoTargetElement(pokemon);
    }
}

/// <summary>
///     The move used by the <see cref="Source" /> Pokémon missed (maybe absent) the <see cref="Target" /> Pokémon.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record MissElement(string Source, string? Target) : ProtocolElement, IPokemonArgs
{
    public static MissElement Parse(string[] segments, out int usedCount)
    {
        if (segments.Length < 3)
        {
            usedCount = 1;
            return new MissElement(segments[1], null);
        }

        usedCount = 2;
        return new MissElement(segments[1], segments[2]);
    }
}

/// <summary>
///     <para>
///         The specified Pokémon <see cref="Pokemon" /> has taken damage, and is now at <see cref="HP" />
///         <see cref="Status" /> (see <see cref="SwitchElement" /> for details).
///     </para>
///     <para>
///         If <see cref="HP" /> is 0, <see cref="Status" /> should be ignored. The current behavior is for
///         <see cref="Status" /> to be <c>fnt</c>, but this may change and
///         should not be relied upon.
///     </para>
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record DamageElement(string Pokemon, string HP, StatusID Status) : ProtocolElement, IPokemonArgs
{
    public static ProtocolElement Parse(string pokemon, string hpStatus, bool heal)
    {
        var parts = hpStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hp = parts[0];
        var status = parts.Length > 1 ? Enum.Parse<StatusID>(parts[1], true) : StatusID.None;
        if (heal)
            return new HealElement(pokemon, hp, status);
        return new DamageElement(pokemon, hp, status);
    }
}

/// <summary>
///     Same as <see cref="DamageElement" />, but the Pokémon has healed damage instead.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record HealElement(string Pokemon, string HP, StatusID Status) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The specified Pokémon <see cref="Pokemon" /> now has <see cref="HP" /> hit points.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SetHPElement(string Pokemon, string HP) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has been inflicted with <see cref="Status" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record StatusElement(string Pokemon, StatusID Status) : ProtocolElement, IPokemonArgs
{
    public static ProtocolElement Parse(string[] segments, bool cure)
    {
        var status = Enum.Parse<StatusID>(segments[2], true);
        if (cure)
            return new CureStatusElement(segments[1], status);
        return new StatusElement(segments[1], status);
    }
}

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has recovered from <see cref="Status" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CureStatusElement(string Pokemon, StatusID Status) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has used a move that cures its team of status effects, like Heal Bell.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CureTeamElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The specified Pokémon <see cref="Pokemon" /> has gained <see cref="Amount" /> in <see cref="Stat" />, using the standard rules for Pokémon stat changes in-battle.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record BoostElement(string Pokemon, StatID Stat, int Amount) : ProtocolElement, IPokemonArgs
{
    public static ProtocolElement Parse(string[] segments, bool? boost)
    {
        var pokemon = segments[1];
        var stat = Enum.Parse<StatID>(segments[2], true);
        var amount = int.Parse(segments[3]);

        if (boost.HasValue)
        {
            if (boost.Value)
                return new BoostElement(pokemon, stat, amount);
            return new UnboostElement(pokemon, stat, amount);
        }
        return new SetBoostElement(pokemon, stat, amount);
    }
}

/// <summary>
///     Same as <see cref="UnboostElement" />, but for negative stat changes instead.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record UnboostElement(string Pokemon, StatID Stat, int Amount) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Same as <see cref="BoostElement" /> and <see cref="UnboostElement" />, but <see cref="Stat" /> is set to <see cref="Amount" /> instead of boosted by <see cref="Amount" />.
///     (For example: Anger Point, Belly Drum)
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SetBoostElement(string Pokemon, StatID Stat, int Amount) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Swaps the boosts from <see cref="Stats" /> between the <see cref="Source" /> Pokémon and <see cref="Target" /> Pokémon.
///     (For example: Guard Swap, Heart Swap).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SwapBoostElement(string Source, string Target, StatID[] Stats) : ProtocolElement, IPokemonArgs
{
    public static SwapBoostElement Parse(string[] segments)
    {
        string[] statsStr = segments[3].Split(',');
        StatID[] stats = new StatID[statsStr.Length];

        for (int i = 0; i < statsStr.Length; i++)
            stats[i] = Enum.Parse<StatID>(statsStr[i], true);

        return new SwapBoostElement(segments[1], segments[2], stats);
    }
}

/// <summary>
///     Invert the boosts of the target Pokémon <see cref="Pokemon" />.
///     (For example: Topsy-Turvy).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record InvertBoostElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Clears all of the boosts of the target <see cref="Pokemon" />.
///     (For example: Clear Smog).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ClearBoostElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Clears all boosts from all Pokémon on both sides.
///     (For example: Haze).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ClearAllBoostElement : ProtocolElement;

/// <summary>
///     Clear the positive boosts from the <see cref="Target" /> Pokémon due to an EFFECT of the <see cref="Pokemon" /> Pokémon.
///     (For example: 'move: Spectral Thief').
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ClearPositiveBoostElement(string Target, string Pokemon, string Effect) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Clear the negative boosts from the target Pokémon <see cref="Pokemon" />.
///     (For example: usually as the result of a [zeffect]).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ClearNegativeBoostElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Copy the boosts from <see cref="Source" /> Pokémon to <see cref="Target" /> Pokémon
///     (For example: Psych Up).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CopyBoostElement(string Source, string Target) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Indicates the weather that is currently in effect.
///     If <see cref="Upkeep" /> is <see langword="true" />, it means that <see cref="Weather" /> was active previously and is still in effect that turn.
///     Otherwise, it means that the weather has changed due to a move or ability, or has expired, in which case <see cref="Weather" /> will be <see langword="null" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record WeatherElement(string? Weather, bool Upkeep) : ProtocolElement
{
    public static WeatherElement Parse(string[] segments)
    {
        bool upkeep = false;
        foreach (var segment in segments.AsSpan(2))
        {
            if (segment == "[upkeep]")
            { 
                upkeep = true;
                break;
            }
        }
        return new WeatherElement(segments[1], upkeep);
    }
}

/// <summary>
///     The field condition <see cref="Condition" /> has started.
///     Field conditions are all effects that affect the entire field and aren't a weather.
///     (For example: Trick Room, Grassy Terrain)
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record FieldStartElement(string Condition) : ProtocolElement;

/// <summary>
///     Indicates that the field condition <see cref="Condition" /> has ended.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record FieldEndElement(string Condition) : ProtocolElement;

/// <summary>
///     A side condition <see cref="Condition" /> has started on <see cref="Side" />.
///     Side conditions are all effects that affect one side of the field.
///     (For example: Tailwind, Stealth Rock, Reflect)
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SideStartElement(Side Side, string Condition) : ProtocolElement
{
    public static SideStartElement Parse(string[] segments)
        => new(Side.Parse(segments[1]), segments[2]);
}

/// <summary>
///     Indicates that the side condition <see cref="Condition" /> ended for the given <see cref="Side" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SideEndElement(Side Side, string Condition) : ProtocolElement
{
    public static SideEndElement Parse(string[] segments)
        => new(Side.Parse(segments[1]), segments[2]);
}

/// <summary>
///     Swaps side conditions between sides. Used for Court Change.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SwapSideConditionsElement : ProtocolElement;

/// <summary>
///     A <see href="https://bulbapedia.bulbagarden.net/wiki/Status_condition#Volatile_status"><i>volatile</i> status</see>
///     has been inflicted on the <see cref="Pokemon" /> Pokémon by <see cref="Effect" />. (For example: confusion, Taunt,
///     Substitute).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record StartVolatileElement(string Pokemon, string Effect) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The volatile status from <see cref="Effect" /> inflicted on the <see cref="Pokemon" /> Pokémon has ended.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record EndVolatileElement(string Pokemon, string Effect) : ProtocolElement, IPokemonArgs;

/// <summary>
///     A move has dealt a critical hit against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CritElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     A move was super effective against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SuperEffectiveElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     A move was not very effective against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ResistedElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The <see cref="Pokemon" /> was immune to a move.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ImmuneElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The <see cref="Item" /> held by the <see cref="Pokemon" /> has been changed or revealed.
///     If this includes a <c>[from]EFFECT</c> tag, it is due to a move or ability <c>EFFECT</c>.
///     Otherwise, it's because the Pokémon has just switched in, and its item is being announced to have a long-term effect.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ItemElement(string Pokemon, string Item) : ProtocolElement, IPokemonArgs;

/// <summary>
///     <para>
///         The <see cref="Item" /> held by <see cref="Pokemon" /> has been destroyed, and it now holds no item.
///         If this includes a <c>[from]EFFECT</c> tag, it is due to a move or ability (like Knock Off).
///         Otherwise, it is because the item has destroyed itself (consumed Berries, Air Balloon).
///         If a berry is consumed, the <c>[eat]</c> tag will be included.
///     </para>
///     This will be silent <c>[silent]</c> if the item's ownership was changed (with a move or ability like Thief or Trick),
///     even if the move or ability would result in a Pokémon without an item.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record EndItemElement(string Pokemon, string Item) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The <see cref="Ability" /> of the <see cref="Pokemon" /> has been changed or revealed.
///     If this includes a <c>[from]EFFECT</c> tag, it is due to a move or ability <c>EFFECT</c>.
///     Otherwise, it's because the Pokémon has just switched in, and its ability is being announced to have a long-term effect.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record AbilityElement(string Pokemon, string Ability) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The <see cref="Pokemon" /> has had its ability suppressed by Gastro Acid.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record EndAbilityElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has transformed into SPECIES by the move Transform or the ability Imposter.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record TransformElement(string Pokemon, string Species) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> used MEGASTONE to Mega Evolve.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record MegaElement(string Pokemon, string Megastone) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has reverted to its primal forme.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record PrimalElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has used ITEM to Ultra Burst into SPECIES.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record BurstElement(string Pokemon, string Species, string Item) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> has used the z-move version of its move.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ZPowerElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     A z-move has broken through protect and hit the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ZBrokenElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     <para>
///         A miscellaneous effect has activated. This is triggered whenever an effect could not be better described by
///         one of the other minor messages: for example, healing abilities like Water Absorb simply use <c>-heal</c>.
///     </para>
///     <para>
///         Items usually activate with <c>-end</c>, although items with two messages, like Berries ("POKÉMON ate the
///         Leppa Berry! POKÉMON restored PP...!"), will send the "ate" message as <c>-eat</c>, and the "restored" message
///         as <c>-activate.</c>
///     </para>
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ActivateElement(string Pokemon, string Effect) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Displays a message in parentheses to the client. Hint messages appear to explain
///     and clarify why certain actions, such as Fake Out and Mat Block failing, have occurred,
///     when there would normally be no in-game messages.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record HintElement(string Message) : ProtocolElement;

/// <summary>
///     Appears in Triple Battles when only one Pokémon remains on each side,
///     to indicate that the Pokémon have been automatically centered.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CenterElement : ProtocolElement;

/// <summary>
///     Displays a miscellaneous message to the client.
///     These messages are primarily used for messages from game mods that aren't supported by the client,
///     like rule clauses such as Sleep Clause, or other metagames with custom messages for specific scenarios.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record MessageElement(string Message) : ProtocolElement;

/// <summary>
///     A move has been combined with another.
///     (For example: Fire Pledge)
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CombineElement : ProtocolElement;

/// <summary>
///     The <see cref="Source" /> Pokémon has used a move and is waiting for the <see cref="Target" /> Pokémon (For example: Fire Pledge).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record WaitingElement(string Source, string Target) : ProtocolElement, IPokemonArgs;

/// <summary>
///     <para>
///         The <see cref="Attacker" /> Pokémon is preparing to use a charge <see cref="Move" /> on the <see cref="Defender" />. (For example: Sky Drop).
///     </para>
///     If <see cref="Defender" /> is <see langword="null" />, the target is unknown. (For example: Dig, Fly).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record PrepareElement(string Attacker, string Move, string? Defender) : ProtocolElement, IPokemonArgs
{
    public static PrepareElement Parse(string[] segments, out int usedCount)
    {
        string? defender = null;
        usedCount = 2;
        if (segments.Length > 3)
        {
            defender = segments[3];
            usedCount = 3;
        }
        return new PrepareElement(segments[1], segments[2], defender);
    }
}

/// <summary>
///     The Pokémon <paramref name="Pokemon" /> must spend the turn recharging from a previous move.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record MustRechargeElement(string Pokemon) : ProtocolElement, IPokemonArgs;

/// <summary>
///     <b>DEPRECATED:</b> A move did absolutely nothing. (For example: Splash).
///     In the future this will be of the form |-activate|POKEMON|move: Splash.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record NothingElement : ProtocolElement;

/// <summary>
///     A multi-hit move hit the <see cref="Pokemon" /> <see cref="Num" /> times.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record HitCountElement(string Pokemon, int Num) : ProtocolElement, IPokemonArgs
{
    public static HitCountElement Parse(string pokemon, string num)
    {
        return new HitCountElement(pokemon, int.Parse(num));
    }
}

/// <summary>
///     The Pokémon ¿<see cref="Pokemon" /> used move <see cref="Move" /> which causes a temporary effect lasting the duration of the move.
///     (For example: Grudge, Destiny Bond).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SingleMoveElement(string Pokemon, string Move) : ProtocolElement, IPokemonArgs;

/// <summary>
///     The Pokémon <see cref="Pokemon" /> used move <see cref="Move" /> which causes a temporary effect lasting the duration of the turn.
///     (For example: Protect, Focus Punch, Roost).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SingleTurnElement(string Pokemon, string Move) : ProtocolElement, IPokemonArgs;

/// <summary>
///     Signals the upkeep phase of the turn where the number of turns left for field conditions are updated.
/// </summary>
[PublicAPI]
public sealed record UpkeepElement : ProtocolElement;

/// <summary>
///     It is now turn <see cref="Number" />.
/// </summary>
[PublicAPI]
public sealed record TurnElement(int Number) : ProtocolElement
{
    public static TurnElement Parse(string number)
    {
        return new TurnElement(int.Parse(number));
    }
}

/// <summary>
///     Gives a JSON object containing a request for a choice (to move or switch).
///     See
///     <a
///         href="https://github.com/smogon/pokemon-showdown/blob/9562cec3897758c54b907cb29baf7573f3db4aec/sim/SIM-PROTOCOL.md#choice-requests">
///         this
///     </a>
///     for a more in-depth explanation.
/// </summary>
[PublicAPI]
public sealed record RequestElement(string Request) : ProtocolElement;

/// <summary>
///     <para>
///         Exposes the members of <see cref="SplitElement{T}" /> non-generically. Handy for easier mass parsing of unknown
///         elements.
///     </para>
///     <see cref="Secret" /> should always be used for internal parsing.
/// </summary>
[PublicAPI]
public interface ISplitElement
{
    ProtocolElement Secret { get; }
    ProtocolElement Public { get; }
    int PlayerID { get; }
}

/// <summary>
///     Contains two <see cref="ProtocolElement" />s of the same type:
///     <list type="bullet">
///         <item>
///             <see cref="Secret" /> is a message for the specific player or an omniscient observer (details which may
///             contain information about exact details of the player's set, like exact HP).
///         </item>
///         <item>
///             <see cref="Public" /> is a message with public details suitable for display to opponents / teammates /
///             spectators.
///             Note that this may be empty.
///         </item>
///     </list>
///     These can be exposed non-generically by casting the <see cref="ProtocolElement" /> to <see cref="ISplitElement" />.
/// </summary>
[PublicAPI]
public sealed record SplitElement<T>(int PlayerID, T Secret, T Public)
    : ProtocolElement, ISplitElement
    where T : ProtocolElement
{
    ProtocolElement ISplitElement.Secret => Secret;
    ProtocolElement ISplitElement.Public => Public;
}

/// <summary>
///     Clears the message-bar, and add a spacer to the battle history. This is usually done automatically
///     by detecting the message-type, but can also be forced to happen with this.
/// </summary>
[PublicAPI]
public sealed record SpacerElement : ProtocolElement;

/// <summary>
///     A debug message.
/// </summary>
[PublicAPI]
public sealed record DebugElement(string Message) : ProtocolElement;

/// <summary>
///     Signals that a decision was sent which is somehow invalid:
///     <list type="bullet">
///         <item>
///             If <see cref="Type" /> is <see cref="ErrorType.InvalidChoice" />, an invalid decision was sent
///             (trying to switch when you're trapped by Mean Look or something).
///         </item>
///         <item>
///             If <see cref="Type" /> is <see cref="ErrorType.UnavailableChoice" />, your previous choice revealed
///             additional information
///             (For example: a move disabled by Imprison or a trapping effect), and a <see cref="RequestElement" /> will
///             be sent to follow up on.
///         </item>
///     </list>
/// </summary>
[PublicAPI]
public sealed record ErrorElement(ErrorType Type, string Message) : ProtocolElement
{
    public static ErrorElement Parse(string message)
    {
        var errorMsg = message.TrimStart('[').Split(' ', 3);

        ErrorType error = ErrorType.Other;
        if (Enum.TryParse(errorMsg[0] + "Choice", out ErrorType specificError))
            error = specificError;

        return new ErrorElement(error, error == ErrorType.Other ? message : errorMsg[2]);
    }
}

/// <summary>
///     The type of error output by the Pokémon Showdown simulator, obtained in <see cref="ErrorElement" />.
/// </summary>
[PublicAPI]
public enum ErrorType
{
    Other,
    InvalidChoice,
    UnavailableChoice
}

/// <summary>
///     <see cref="ProtocolCodec" /> does not support this message type yet, so it is received as an unknown element.
/// </summary>
[PublicAPI]
public sealed record UnknownElement(string Content) : ProtocolElement;

public interface IPokemonArgs
{
    string? Pokemon => null;
    string? Source => null;
    string? Target => null;
    string? Attacker => null;
    string? Defender => null;
}

public interface IDetailsArg
{
    string Details { get; }
}
