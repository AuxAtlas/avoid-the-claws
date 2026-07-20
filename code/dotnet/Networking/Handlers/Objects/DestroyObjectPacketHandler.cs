using AvoidClaws.code.dotnet.Glue.Misc;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;

namespace AvoidClaws.code.dotnet.Networking.Handlers.Objects;

public class DestroyObjectPacketHandler : PacketHandler<DestroyObjectPacket>
{
    protected override void Handle(DestroyObjectPacket packet, KableConnection source)
    {
        if (Core.Network.IsServer)
            return;

        Core.World.DestroyObject(packet.TargetObjectId);
    }
}