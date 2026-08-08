#region

using System.Linq;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Networking.Handlers.Objects;

public class SpawnControllerPacketHandler : PacketHandler<SpawnControllerPacket>
{
    protected override void Handle(SpawnControllerPacket packet, KableConnection source)
    {
        if (IsServer)
            return;

        if (packet.SpawnedObjectId.Id == 0)
            return;

        if (Core.World.Controllers.SpawnedControllers.Any(x => x.KableId == packet.SpawnedObjectId))
            return;

        PackedScene? targetPrefab;
        switch (packet.Type)
        {
            case CoreGame.ControllerType.LocalPlayer:
                targetPrefab = Core.Resources.ControllerPrefabs.PlayerControllerPrefab;
                break;
            default:
                targetPrefab = Core.Resources.ControllerPrefabs.DummyControllerPrefab;
                break;
        }

        Core.World.Controllers.SpawnControllerPrefab(targetPrefab, packet.SpawnedObjectId);
    }
}