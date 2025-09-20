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
///     <para>
///         A miscellaneous effect has activated. This is triggered whenever an effect could not be better described by
///         one of the other minor messages: for example, healing abilities like Water Absorb simply use <c>-heal</c>.
///     </para>
///     <para>
///         Items usually activate with <c>-end</c>, although items with two messages, like Berries ("POKEMON ate the
///         Leppa Berry! POKEMON restored PP...!"), will send the "ate" message as <c>-eat</c>, and the "restored" message
///         as <c>-activate.</c>
///     </para>
/// </summary>
[PublicAPI]
[MinorAction]
public record ActivateElement(string Pokemon, string Effect) : ProtocolElement;

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
public record SplitElement<T>(string PlayerID, T Secret, T Public) : ProtocolElement where T : ProtocolElement;

[PublicAPI]
public record SpacerElement : ProtocolElement;

[PublicAPI]
public record UnknownElement(string Content) : ProtocolElement;