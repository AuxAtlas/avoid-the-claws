using AvoidClaws.code.dotnet.Networking.Data;

namespace AvoidClaws.code.dotnet.Events.Networking;

public class NetJoinedEvent : GameEvent
{
    public uint JoinedNetTick { get; set; }
    public KableConnection KableConnection { get; set; }
}