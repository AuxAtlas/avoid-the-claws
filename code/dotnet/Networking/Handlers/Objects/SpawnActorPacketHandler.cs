#region

using System.Linq;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Handlers.Objects;

public class SpawnActorPacketHandler : PacketHandler<SpawnActorPacket>
{
    protected override void Handle(SpawnActorPacket packet, KableConnection source)
    {
        if (IsServer)
            return;

        if (packet.ActorId.Id == 0)
            return;

        if (Core.World.Actors.SpawnedActors.Any(x => x.KableId == packet.ActorId))
            return;

        PackedScene? targetPrefab = null;
        switch (packet.Type)
        {
            case CoreGame.ActorType.Player:
                targetPrefab = Core.Resources.ActorPrefabs.PlayerActorPrefab;
                break;
        }

        Core.World.Actors.SpawnActorPrefab(targetPrefab, packet.ActorId);
    }
}