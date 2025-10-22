using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

[PublicAPI]
public abstract record ProtocolFrame(List<ProtocolElement>? Elements);

[PublicAPI]
public record UpdateFrame(List<ProtocolElement> Elements) : ProtocolFrame(Elements);

[PublicAPI]
public record SideUpdateFrame(string Player, List<ProtocolElement> Elements) : ProtocolFrame(Elements);

[PublicAPI]
public record EndFrame(string LogData) : ProtocolFrame((List<ProtocolElement>?)null);
