using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

[PublicAPI]
public abstract record ProtocolElement
{
    public string[]? Tags { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class MinorActionAttribute : Attribute;

[PublicAPI]
public record TimestampElement(DateTimeOffset Value) : ProtocolElement
{
    public static TimestampElement Parse(string value)
    {
        return new TimestampElement(DateTimeOffset.FromUnixTimeSeconds(long.Parse(value)));
    }
}

[PublicAPI]
public record GameTypeElement(GameType GameType) : ProtocolElement
{
    public static GameTypeElement Parse(string gameType)
    {
        return new GameTypeElement(Enum.Parse<GameType>(gameType, true));
    }
}

[PublicAPI]
public record PlayerDetailsElement(string Player, string Username, string Avatar, string Rating) : ProtocolElement;

[PublicAPI]
public record GenElement(int GenNum) : ProtocolElement
{
    public static GenElement Parse(string genNum)
    {
        return new GenElement(int.Parse(genNum));
    }
}

[PublicAPI]
public record TierElement(string FormatName) : ProtocolElement;

[PublicAPI]
public record ClearPokeElement : ProtocolElement;

[PublicAPI]
public record PokeElement(string Player, string Details, string Item) : ProtocolElement;

[PublicAPI]
public record TeamPreviewElement : ProtocolElement;

[PublicAPI]
public record TeamSizeElement(string Player, int Size) : ProtocolElement
{
    public static TeamSizeElement Parse(string player, string size)
    {
        return new TeamSizeElement(player, int.Parse(size));
    }
}

[PublicAPI]
public record StartElement : ProtocolElement;

[PublicAPI]
public record WinElement(string Username) : ProtocolElement;

[PublicAPI]
public record SwitchElement(string Pokemon, string Details, string HP, string Status) : ProtocolElement
{
    public static SwitchElement Parse(string pokemon, string details, string hpStatus)
    {
        var parts = hpStatus.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new SwitchElement(pokemon, details, parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
}

[PublicAPI]
public record MoveElement(string Pokemon, string Move, string Target) : ProtocolElement;

[PublicAPI]
public record FaintElement(string Pokemon) : ProtocolElement;

/// <summary>
///     The specified <see cref="Action" /> has failed against the <see cref="Pokemon" /> targeted. The
///     <see cref="Action" /> in question should be a move that fails due to its own mechanics. Moves (or effect
///     activations) that fail because they're blocked by another effect should use <c>-block</c> instead.
/// </summary>
[PublicAPI]
[MinorAction]
public record FailElement(string Pokemon, string Action) : ProtocolElement;

/// <summary>
///     An effect targeted at <see cref="Pokemon" /> was blocked by <see cref="Effect" />. This may optionally specify that
///     the effect was a <see cref="Move" /> from <see cref="Attacker" />. <c>[of]SOURCE</c> will note the owner of the
///     <see cref="Effect" />, in the case that it's not <see cref="Effect" /> (for instance, an ally with Aroma Veil.)
/// </summary>
[PublicAPI]
[MinorAction]
public record BlockElement(string Pokemon, string Effect, string? Move, string? Attacker) : ProtocolElement
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
public record MissElement(string Source, string? Target) : ProtocolElement
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
public record DamageElement(string Pokemon, string HP, string Status) : ProtocolElement
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
public record HealElement(string Pokemon, string HP, string Status) : ProtocolElement
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
public record StartVolatileElement(string Pokemon, string Effect) : ProtocolElement;

/// <summary>
///     The volatile status from <see cref="Effect" /> inflicted on the <see cref="Pokemon" /> Pokémon has ended.
/// </summary>
[PublicAPI]
[MinorAction]
public record EndVolatileElement(string Pokemon, string Effect) : ProtocolElement;

/// <summary>
///     A move has dealt a critical hit against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public record CritElement(string Pokemon) : ProtocolElement;

/// <summary>
///     A move was super effective against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public record SuperEffectiveElement(string Pokemon) : ProtocolElement;

/// <summary>
///     A move was not very effective against the <see cref="Pokemon" />.
/// </summary>
[PublicAPI]
[MinorAction]
public record ResistedElement(string Pokemon) : ProtocolElement;

/// <summary>
///     The <see cref="Pokemon" /> was immune to a move.
/// </summary>
[PublicAPI]
[MinorAction]
public record ImmuneElement(string Pokemon) : ProtocolElement;

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
public record ActivateElement(string Pokemon, string Effect) : ProtocolElement;

/// <summary>
///     A multi-hit move hit the <see cref="Pokemon" /> <see cref="Num" /> times.
/// </summary>
[PublicAPI]
[MinorAction]
public record HitCountElement(string Pokemon, int Num) : ProtocolElement
{
    public static HitCountElement Parse(string pokemon, string num)
    {
        return new HitCountElement(pokemon, int.Parse(num));
    }
}

[PublicAPI]
public record UpkeepElement : ProtocolElement;

[PublicAPI]
public record TurnElement(int Number) : ProtocolElement
{
    public static TurnElement Parse(string number)
    {
        return new TurnElement(int.Parse(number));
    }
}

[PublicAPI]
public record RequestElement(string Request) : ProtocolElement;

[PublicAPI]
public interface ISplitElement
{
    ProtocolElement GetSecret();
    ProtocolElement GetPublic();
}

[PublicAPI]
public record SplitElement<T>(string PlayerID, T Secret, T Public)
    : ProtocolElement, ISplitElement
    where T : ProtocolElement
{
    public ProtocolElement GetSecret() => Secret;
    public ProtocolElement GetPublic() => Public;
}

[PublicAPI]
public record SpacerElement : ProtocolElement;

[PublicAPI]
public record DebugElement(string Message) : ProtocolElement;

[PublicAPI]
public record ErrorElement(string Message) : ProtocolElement;

[PublicAPI]
public record UnknownElement(string Content) : ProtocolElement;