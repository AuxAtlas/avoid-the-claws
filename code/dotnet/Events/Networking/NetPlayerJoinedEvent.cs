#region

using AvoidClaws.code.dotnet.Networking.Data;

#endregion

namespace AvoidClaws.code.dotnet.Events.Networking;

public class NetPlayerJoinedEvent : GameEvent
{
    public uint JoinedNetTick { get; set; }
    public KableConnectionId KableConnectionId { get; set; }
}