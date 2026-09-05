#region

using AvoidClaws.code.dotnet.Networking.Data;

#endregion

namespace AvoidClaws.code.dotnet.Events.Networking;

public record NetPlayerJoinedEvent : GameEvent
{
    public uint JoinedNetTick { get; set; } = 0;
    public KableConnectionId KableConnectionId { get; set; } = null!;
}