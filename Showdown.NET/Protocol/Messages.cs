using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

[PublicAPI]
public abstract record ParsedMessage(List<MessagePart> Parts);

[PublicAPI]
public record UpdateMessage(List<MessagePart> Parts) : ParsedMessage(Parts);

[PublicAPI]
public record SideUpdateMessage(string Player, List<MessagePart> Parts) : ParsedMessage(Parts);