using JetBrains.Annotations;

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
    {
        return new GenElement(int.Parse(genNum));
    }
}

/// <summary>
///     The name of the format being played. (see also <see cref="Definitions.FormatID" />).
/// </summary>
/// <param name="FormatName"></param>
[PublicAPI]
public sealed record TierElement(string FormatName) : ProtocolElement;

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
public sealed record PokeElement(int Player, string Details, string Item) : ProtocolElement;

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
///         <see cref="Status" /> can be left <see langword="null" />, or it can be <c>slp</c>, <c>par</c>, etc.
///     </para>
/// </summary>
/// <remarks>
///     A <see cref="SwitchElement" /> indicates an intentional switch, in contrast with <see cref="DragElement" />,
///     which indicates it was unintentional (forced by Whirlwind, Roar, etc).
/// </remarks>
[PublicAPI]
public sealed record SwitchElement(string Pokemon, string Details, string HP, string? Status) : ProtocolElement
{
    public static (string hp, string? status) ParseHPStatus(string hpStatus)
    {
        var parts = hpStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return (parts[0], parts.Length > 1 ? parts[1] : null);
    }
}

/// <inheritdoc cref="SwitchElement" />
[PublicAPI]
public sealed record DragElement(string Pokemon, string Details, string HP, string? Status) : ProtocolElement;

/// <summary>
///     <para>
///         The specified <see cref="Pokemon" /> has used move <see cref="Move" /> at <see cref="Target" />. If a move has
///         multiple targets or no target,
///         <see cref="Target" /> should be ignored. If a move targets a side, <see cref="Target" /> will be a (possibly
///         fainted) Pokémon on that side.
///     </para>
///     <see cref="Details" /> contains secondary information.
/// </summary>
[PublicAPI]
public sealed record MoveElement(string Pokemon, string Move, string Target, MoveDetails Details) : ProtocolElement;

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
///     The Pokémon <see cref="Pokemon" /> has fainted.
/// </summary>
[PublicAPI]
public sealed record FaintElement(string Pokemon) : ProtocolElement;

/// <summary>
///     The specified <see cref="Action" /> has failed against the <see cref="Pokemon" /> targeted. The
///     <see cref="Action" /> in question should be a move that fails due to its own mechanics. Moves (or effect
///     activations) that fail because they're blocked by another effect should use <c>-block</c> instead.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record FailElement(string Pokemon, string Action) : ProtocolElement;

/// <summary>
///     An effect targeted at <see cref="Pokemon" /> was blocked by <see cref="Effect" />. This may optionally specify that
///     the effect was a <see cref="Move" /> from <see cref="Attacker" />. <c>[of]SOURCE</c> will note the owner of the
///     <see cref="Effect" />, in the case that it's not <see cref="Effect" /> (for instance, an ally with Aroma Veil.)
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record BlockElement(string Pokemon, string Effect, string? Move, string? Attacker) : ProtocolElement
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
///     The move used by the <see cref="Source" /> Pokémon missed (maybe absent) the <see cref="Target" /> Pokémon.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record MissElement(string Source, string? Target) : ProtocolElement
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
public sealed record DamageElement(string Pokemon, string HP, string Status) : ProtocolElement
{
    public static DamageElement Parse(string pokemon, string hpStatus)
    {
        var parts = hpStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new DamageElement(pokemon, parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
}

/// <summary>
///     Same as <see cref="DamageElement" />, but the Pokémon has healed damage instead.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record HealElement(string Pokemon, string HP, string Status) : ProtocolElement
{
    public static HealElement Parse(string pokemon, string hpStatus)
    {
        var parts = hpStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new HealElement(pokemon, parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
}

/// <summary>
///     A <see href="https://bulbapedia.bulbagarden.net/wiki/Status_condition#Volatile_status"><i>volatile</i> status</see>
///     has been inflicted on the <see cref="Pokemon" /> Pokémon by <see cref="Effect" />. (For example: confusion, Taunt,
///     Substitute).
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record StartVolatileElement(string Pokemon, string Effect) : ProtocolElement;

/// <summary>
///     The volatile status from <see cref="Effect" /> inflicted on the <see cref="Pokemon" /> Pokémon has ended.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record EndVolatileElement(string Pokemon, string Effect) : ProtocolElement;

/// <summary>
///     A move has dealt a critical hit against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record CritElement(string Pokemon) : ProtocolElement;

/// <summary>
///     A move was super effective against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record SuperEffectiveElement(string Pokemon) : ProtocolElement;

/// <summary>
///     A move was not very effective against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ResistedElement(string Pokemon) : ProtocolElement;

/// <summary>
///     The <see cref="Pokemon" /> was immune to a move.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record ImmuneElement(string Pokemon) : ProtocolElement;

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
public sealed record ActivateElement(string Pokemon, string Effect) : ProtocolElement;

/// <summary>
///     A multi-hit move hit the <see cref="Pokemon" /> <see cref="Num" /> times.
/// </summary>
[PublicAPI]
[MinorAction]
public sealed record HitCountElement(string Pokemon, int Num) : ProtocolElement
{
    public static HitCountElement Parse(string pokemon, string num)
    {
        return new HitCountElement(pokemon, int.Parse(num));
    }
}

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
public sealed record ErrorElement(ErrorType Type, string Message) : ProtocolElement;

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