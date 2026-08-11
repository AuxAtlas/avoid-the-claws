#region

using AvoidClaws.code.dotnet.Networking.Data;

#endregion

namespace AvoidClaws.code.dotnet.Events.Networking;

public class NetPlayerJoinedEvent : GameEvent
{
    public required uint JoinedNetTick { get; init; }
    public required KableConnectionId KableConnectionId { get; init; }
}