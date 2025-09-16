using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

[PublicAPI]
public abstract record MessagePart;
    
[PublicAPI]
public record TimestampPart(string Timestamp) : MessagePart
{
    public DateTime ToDateTime() => DateTimeOffset.FromUnixTimeSeconds(long.Parse(Timestamp)).DateTime;
    public DateTimeOffset ToDateTimeOffset() => DateTimeOffset.FromUnixTimeSeconds(long.Parse(Timestamp));
}
    
[PublicAPI]
public record GameTypePart(string GameType) : MessagePart
{
    public GameType ToEnum() => Enum.Parse<GameType>(GameType, ignoreCase: true);
    
    public bool TryToEnum(out GameType gameType) => 
        Enum.TryParse(GameType, ignoreCase: true, out gameType);
}

[PublicAPI]
public record UnknownPart(string Content) : MessagePart;