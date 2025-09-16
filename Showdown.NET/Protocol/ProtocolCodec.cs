using System.Text.Json;
using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

[PublicAPI]
public static class ProtocolCodec
{
    public static string EncodeStartCommand(string formatId)
    {
        var payload = new { formatid = formatId };
        var json = JsonSerializer.Serialize(payload);
        return $">start {json}";
    }

    public static string EncodePlayerCommand(string player, string name)
    {
        var payload = new { name };
        var json = JsonSerializer.Serialize(payload);
        return $">player {player} {json}";
    }

    public static ParsedMessage? Parse(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var lines = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var messageType = lines[0].Trim();

        switch (messageType)
        {
            case "update":
                Console.WriteLine("Parsing update!");

                var parts = new List<MessagePart>();

                for (var i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();

                    if (!line.StartsWith('|'))
                        continue;

                    var segments = line.Split('|', StringSplitOptions.RemoveEmptyEntries);

                    if (segments.Length < 2)
                        continue;

                    var command = segments[0];

                    switch (command)
                    {
                        case "t:":
                            parts.Add(new TimestampPart(segments[1]));
                            break;
                        case "gametype":
                            parts.Add(new GameTypePart(segments[1]));
                            break;
                        default:
                            parts.Add(new UnknownPart(line));
                            break;
                    }
                }

                return new UpdateMessage(parts);
            case "sideupdate":
                if (lines.Length <= 1) break;

                var player = lines[1].Trim();
                Console.WriteLine($"Parsing sideupdate for player {player}");

                return new SideUpdateMessage(player, []);

            default:
                Console.WriteLine($"Unknown message type: {messageType}");
                break;
        }

        return null;
    }

    public abstract record MessagePart;

    public record TimestampPart(string Timestamp) : MessagePart;

    public record GameTypePart(string GameType) : MessagePart;

    public record UnknownPart(string Content) : MessagePart;


    public abstract record ParsedMessage(List<MessagePart> Parts);

    public record UpdateMessage(List<MessagePart> Parts) : ParsedMessage(Parts);

    public record SideUpdateMessage(string Player, List<MessagePart> Parts) : ParsedMessage(Parts);
}