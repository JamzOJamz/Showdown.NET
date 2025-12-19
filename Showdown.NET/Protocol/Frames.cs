using JetBrains.Annotations;

namespace Showdown.NET.Protocol;

/// <summary>
///     Base class for all protocol frames received from the battle simulator.
/// </summary>
/// <param name="Elements">List of protocol elements contained in this frame.</param>
[PublicAPI]
public abstract record ProtocolFrame(List<ProtocolElement>? Elements);

/// <summary>
///     Represents a general battle update frame containing protocol elements.
/// </summary>
/// <param name="Elements">List of protocol elements representing battle events.</param>
[PublicAPI]
public record UpdateFrame(List<ProtocolElement> Elements) : ProtocolFrame(Elements);

/// <summary>
///     Represents a side-specific update frame with information visible only to one player.
/// </summary>
/// <param name="Player">The player identifier (e.g., "p1" or "p2").</param>
/// <param name="Elements">List of protocol elements for this player's side.</param>
[PublicAPI]
public record SideUpdateFrame(string Player, List<ProtocolElement> Elements) : ProtocolFrame(Elements);

/// <summary>
///     Represents the final frame of a battle containing log data.
/// </summary>
/// <param name="LogData">The battle log data as a string.</param>
[PublicAPI]
public record EndFrame(string LogData) : ProtocolFrame((List<ProtocolElement>?)null);