#region

using System;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Networking;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using AvoidClaws.code.dotnet.Networking.Packets.State;

#endregion

namespace AvoidClaws.code.dotnet.Glue.Core.NetworkWorld;

public partial class PlayerJoinedGlue : GameGlue
{
    public override void _Ready()
    {
        base._Ready();

        Core.EventBus.Subscribe<NetPlayerJoinedEvent>(this, HandleNetJoinEvent);
    }

    private void HandleNetJoinEvent(NetPlayerJoinedEvent e)
    {
        if (IsClient)
        {
            Core.World.ChangeMapTo(Core.Resources.MapPrefabs.DevEnvMap);
            return;
        }

        var spawnedActorNode = Core.World.Actors.SpawnActorPrefab(Core.Resources.ActorPrefabs.PlayerActorPrefab);
        if (spawnedActorNode is not IActor spawnedActor)
            return;
        spawnedActor.SetKableAuthority(e.KableConnectionId);

        var spawnedControllerNode = Core.World.Controllers.SpawnControllerPrefab(Core.Resources.ControllerPrefabs.DummyControllerPrefab);
        if (spawnedControllerNode is not IController spawnedController)
            return;

        spawnedController.SetKableAuthority(e.KableConnectionId);

        SpawnControllerPacket spawnControllerPacket = new()
        {
            SpawnedObjectId = spawnedController.KableId,
            AuthorityConnectionId = spawnedController.AuthorityConnectionId,
            Type = CoreGame.ControllerType.Dummy
        };
        // Tell all OTHER peers to spawn a 'remote player' controller representing this new player
        Core.Network.SendToAllReliableOrdered(spawnControllerPacket, [e.KableConnectionId]);

        var newPlayerKableConnection = Core.Network.GetKableConnectionFromId(e.KableConnectionId);
        if (newPlayerKableConnection == null) throw new InvalidOperationException("NetPlayerJoinedEvent fired with a KableConnectionId that is unknown to the network.");

        // Tell THIS new client to spawn a 'PlayerController'(LocalPlayer) for themselves
        Core.Network.SendToClientReliableOrdered
        (
            new SpawnControllerPacket
            {
                SpawnedObjectId = spawnedController.KableId,
                Type = CoreGame.ControllerType.LocalPlayer,
                AuthorityConnectionId = spawnedController.AuthorityConnectionId
            },
            newPlayerKableConnection
        );

        SpawnActorPacket spawnActorPacket = new()
        {
            ActorId = spawnedActor.KableId,
            AuthorityConnectionId = spawnedActor.AuthorityConnectionId,
            Type = Core.Resources.GetActorType(spawnedActor)
        };
        Core.Network.SendToAllReliableOrdered(spawnActorPacket);

        spawnedActor.Respawn();

        // Inform the new player of the current game state
        foreach (var actor in Core.World.Actors.SpawnedActors)
        {
            if (actor.KableId != spawnedActor.KableId)
                Core.Network.SendToClientReliableOrdered
                (
                    new SpawnActorPacket
                    {
                        ActorId = actor.KableId,
                        AuthorityConnectionId = actor.AuthorityConnectionId,
                        Type = Core.Resources.GetActorType(actor)
                    },
                    newPlayerKableConnection
                );

            foreach (var buff in actor.GetBuffs())
            {
                CoreGame.BuffType? buffType = Core.Resources.GetBuffType(buff);
                if (buffType == null)
                    throw new NullReferenceException("ResourceManager GetBuffType returned null.");

                SpawnBuffPacket spawnBuffPacket = new()
                {
                    SpawnedBuffId = buff.KableId,
                    OwnerActorId = buff.OwnerActor.KableId,
                    AuthorityConnectionId = buff.OwnerActor.AuthorityConnectionId,
                    BuffType = buffType.Value
                };
                Core.Network.SendToClientReliableOrdered(spawnBuffPacket, newPlayerKableConnection);
            }
        }

        foreach (var controller in Core.World.Controllers.SpawnedControllers)
        {
            if (controller.KableId == spawnedController.KableId)
                continue;

            var boundType = Core.Resources.GetControllerType(controller);
            if (boundType == CoreGame.ControllerType.LocalPlayer) boundType = CoreGame.ControllerType.Dummy;

            Core.Network.SendToClientReliableOrdered
            (
                new SpawnControllerPacket
                {
                    SpawnedObjectId = controller.KableId,
                    Type = boundType,
                    AuthorityConnectionId = controller.AuthorityConnectionId
                },
                newPlayerKableConnection
            );
        }

        spawnedController.Attach(spawnedActor);

        Core.Network.SendToClientReliableOrdered
        (
            new NetworkStatePacket
            {
                State = Core.Network.GetStateOrCached()
            },
            newPlayerKableConnection
        );
    }
}