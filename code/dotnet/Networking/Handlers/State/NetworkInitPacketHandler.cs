using AvoidClaws.code.dotnet.Glue.Misc;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.State;

namespace AvoidClaws.code.dotnet.Networking.Handlers.State;

public class NetworkInitPacketHandler : PacketHandler<NetworkInitPacket>
{
    protected override void Handle(NetworkInitPacket packet, KableConnection source)
    {
        if (IsServer)
            return;

        Core.Network.MyConnectionId = packet.AssignedConnectionId;
        Core.Network.RegisterKableConnection(source);
    }
}