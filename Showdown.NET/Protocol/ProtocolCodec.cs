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
                var sideUpdateParts = new List<MessagePart>();

                for (var i = 2; i < lines.Length; i++)
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
                            sideUpdateParts.Add(new TimestampPart(segments[1]));
                            break;
                        case "gametype":
                            sideUpdateParts.Add(new GameTypePart(segments[1]));
                            break;
                        default:
                            sideUpdateParts.Add(new UnknownPart(line));
                            break;
                    }
                }

                return new SideUpdateMessage(player, sideUpdateParts);
            default:
                Console.WriteLine($"Unknown message type: {messageType}");
                break;
        }

        return null;
    }
}