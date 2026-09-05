#region

using System;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Networking;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using AvoidClaws.code.dotnet.Networking.Packets.State;

#endregion

namespace AvoidClaws.code.dotnet.Glue.Core.NetworkWorld;

public partial class PlayerJoinedGlue : GameGlue
{
    private readonly SpawnActorPacket _spawnActorPacketReusable = new();
    private readonly SpawnControllerPacket _spawnControllerPacketReusable = new();
    private readonly SpawnBuffPacket _spawnBuffPacketReusable = new();
    private readonly NetworkStatePacket _networkStatePacketReusable = new();
    
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

        _spawnControllerPacketReusable.SpawnedObjectId = spawnedController.KableId;
        _spawnControllerPacketReusable.AuthorityConnectionId = spawnedController.AuthorityConnectionId;
        _spawnControllerPacketReusable.Type = CoreGame.ControllerType.Dummy;
        // Tell all OTHER peers to spawn a 'remote player' controller representing this new player
        Core.Network.SendToAllReliableOrdered(_spawnControllerPacketReusable, [e.KableConnectionId]);

        var newPlayerKableConnection = Core.Network.GetKableConnectionFromId(e.KableConnectionId);
        if (newPlayerKableConnection == null) throw new InvalidOperationException("NetPlayerJoinedEvent fired with a KableConnectionId that is unknown to the network.");

        _spawnControllerPacketReusable.Type = CoreGame.ControllerType.LocalPlayer;
        // Tell THIS new client to spawn a 'PlayerController'(LocalPlayer) for themselves
        Core.Network.SendToClientReliableOrdered(_spawnControllerPacketReusable, newPlayerKableConnection);

        _spawnActorPacketReusable.ActorId = spawnedActor.KableId;
        _spawnActorPacketReusable.AuthorityConnectionId = spawnedActor.AuthorityConnectionId;
        _spawnActorPacketReusable.Type = Core.Resources.GetActorType(spawnedActor);
        Core.Network.SendToAllReliableOrdered(_spawnActorPacketReusable);

        spawnedActor.Respawn();

        // Inform the new player of the current game state
        foreach (IActor actor in Core.World.Actors.SpawnedActors)
        {
            if (actor.KableId != spawnedActor.KableId)
            {
                _spawnActorPacketReusable.ActorId = actor.KableId;
                _spawnActorPacketReusable.AuthorityConnectionId = actor.AuthorityConnectionId;
                _spawnActorPacketReusable.Type = Core.Resources.GetActorType(actor);
                Core.Network.SendToClientReliableOrdered(_spawnActorPacketReusable, newPlayerKableConnection); 
            }

            foreach (IBuff buff in actor.GetBuffs())
            {
                CoreGame.BuffType? buffType = Core.Resources.GetBuffType(buff);
                if (buffType == null)
                    throw new NullReferenceException("ResourceManager GetBuffType returned null.");

                _spawnBuffPacketReusable.SpawnedBuffId = buff.KableId;
                _spawnBuffPacketReusable.OwnerActorId = buff.OwnerActor.KableId;
                _spawnBuffPacketReusable.AuthorityConnectionId = buff.OwnerActor.AuthorityConnectionId;
                _spawnBuffPacketReusable.BuffType = buffType.Value;
                Core.Network.SendToClientReliableOrdered(_spawnBuffPacketReusable, newPlayerKableConnection);
            }
        }

        foreach (IController controller in Core.World.Controllers.SpawnedControllers)
        {
            if (controller.KableId == spawnedController.KableId)
                continue;

            var boundType = Core.Resources.GetControllerType(controller);
            if (boundType == CoreGame.ControllerType.LocalPlayer) boundType = CoreGame.ControllerType.Dummy;

            _spawnControllerPacketReusable.SpawnedObjectId = controller.KableId;
            _spawnControllerPacketReusable.AuthorityConnectionId = controller.AuthorityConnectionId;
            _spawnControllerPacketReusable.Type = boundType;
            Core.Network.SendToClientReliableOrdered(_spawnControllerPacketReusable, newPlayerKableConnection);
        }

        spawnedController.Attach(spawnedActor);

        _networkStatePacketReusable.State = Core.Network.GetStateOrCached();
        Core.Network.SendToClientReliableOrdered(_networkStatePacketReusable, newPlayerKableConnection);
    }
}