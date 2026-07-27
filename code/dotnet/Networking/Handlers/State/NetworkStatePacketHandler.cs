using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.State;

namespace AvoidClaws.code.dotnet.Networking.Handlers.State;

public class NetworkStatePacketHandler : PacketHandler<NetworkStatePacket>
{
    protected override void Handle(NetworkStatePacket packet, KableConnection source)
    {
        if (IsServer)
            return;

        Core.Network.IngestNetworkState(packet.State);
    }
}