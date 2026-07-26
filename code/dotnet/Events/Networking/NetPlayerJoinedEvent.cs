using AvoidClaws.code.dotnet.Networking.Data;

namespace AvoidClaws.code.dotnet.Events.Networking;

public class NetPlayerJoinedEvent : GameEvent
{
    public uint JoinedNetTick { get; set; }
    public KableConnectionId KableConnectionId { get; set; }
}