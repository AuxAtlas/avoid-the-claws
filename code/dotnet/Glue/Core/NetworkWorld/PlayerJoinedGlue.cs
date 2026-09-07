#region

using System;
using System.IO;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Networking;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using AvoidClaws.code.dotnet.Networking.Packets.State;
using Godot;

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
            Core.World.ChangeMapTo(Core.Resources.LevelPrefabs.DevEnvMap);
            return;
        }

        // This will return null if the 'Joining Net Player' is actually the server itself (as in, when the server first starts up)
        KableConnection? newPlayerKableConnection = Core.Network.GetKableConnectionFromId(e.KableConnectionId);
        
        var spawnedActorNode = Core.World.Actors.SpawnPrefab(Core.Resources.ActorPrefabs.PlayerActorPrefab, e.JoinedNetTick);
        if (spawnedActorNode is not IActor spawnedActor)
            return;
        
        spawnedActor.SetKableAuthority(e.KableConnectionId);

        PackedScene? controllerPrefab = Core.Resources.ControllerPrefabs.DummyControllerPrefab;
        if (newPlayerKableConnection == null)
        {
            controllerPrefab = Core.Resources.ControllerPrefabs.PlayerControllerPrefab;
        }
        
        IController? spawnedController = Core.World.Controllers.SpawnPrefab(controllerPrefab, e.JoinedNetTick);
        if (spawnedController == null)
            throw new InvalidDataException($"PlayerJoinedGlue: Spawned a controller prefab that doesnt implement IController");

        spawnedController.SetKableAuthority(e.KableConnectionId);

        if (newPlayerKableConnection != null)
        {
            _spawnControllerPacketReusable.SpawnedObjectId = spawnedController.KableId;
            _spawnControllerPacketReusable.AuthorityConnectionId = spawnedController.AuthorityConnectionId;
            _spawnControllerPacketReusable.TypesEnum = CoreGame.ControllerTypesEnum.Dummy;
            // Tell all OTHER peers to spawn a 'remote player' controller representing this new player
            Core.Network.SendToAllReliableOrdered(_spawnControllerPacketReusable, [e.KableConnectionId]);
            
            _spawnControllerPacketReusable.TypesEnum = CoreGame.ControllerTypesEnum.LocalPlayer;
            // Tell THIS new client to spawn a 'PlayerController'(LocalPlayer) for themselves
            Core.Network.SendToClientReliableOrdered(_spawnControllerPacketReusable, newPlayerKableConnection);

            _spawnActorPacketReusable.ActorId = spawnedActor.KableId;
            _spawnActorPacketReusable.AuthorityConnectionId = spawnedActor.AuthorityConnectionId;
            _spawnActorPacketReusable.TypesEnum = Core.Resources.GetActorType(spawnedActor);
            Core.Network.SendToAllReliableOrdered(_spawnActorPacketReusable);
        }

        // Trigger respawn on the actor immediately; as a safetynet in case of future dev mistakes implementing spawn VS. respawn code.
        spawnedActor.Respawn();
        
        if (newPlayerKableConnection != null)
        {
            // Inform the new player of the current game state
            foreach (IActor actor in Core.World.Actors.SpawnedActors)
            {
                if (actor.KableId != spawnedActor.KableId)
                {
                    _spawnActorPacketReusable.ActorId = actor.KableId;
                    _spawnActorPacketReusable.AuthorityConnectionId = actor.AuthorityConnectionId;
                    _spawnActorPacketReusable.TypesEnum = Core.Resources.GetActorType(actor);
                    Core.Network.SendToClientReliableOrdered(_spawnActorPacketReusable, newPlayerKableConnection);
                }

                foreach (IBuff buff in actor.GetBuffs())
                {
                    CoreGame.BuffTypesEnum? buffType = Core.Resources.GetBuffType(buff);
                    if (buffType == null)
                        throw new NullReferenceException("ResourceManager GetBuffType returned null.");

                    _spawnBuffPacketReusable.SpawnedBuffId = buff.KableId;
                    _spawnBuffPacketReusable.OwnerActorId = buff.ParentActor?.KableId ?? KableId.Empty;
                    _spawnBuffPacketReusable.AuthorityConnectionId = buff.ParentActor?.AuthorityConnectionId ?? KableConnectionId.Empty;
                    _spawnBuffPacketReusable.BuffTypesEnum = buffType.Value;
                    Core.Network.SendToClientReliableOrdered(_spawnBuffPacketReusable, newPlayerKableConnection);
                }
            }

            foreach (IController controller in Core.World.Controllers.SpawnedControllers)
            {
                if (controller.KableId == spawnedController.KableId)
                    continue;

                var boundType = Core.Resources.GetControllerType(controller);
                if (boundType == CoreGame.ControllerTypesEnum.LocalPlayer) boundType = CoreGame.ControllerTypesEnum.Dummy;

                _spawnControllerPacketReusable.SpawnedObjectId = controller.KableId;
                _spawnControllerPacketReusable.AuthorityConnectionId = controller.AuthorityConnectionId;
                _spawnControllerPacketReusable.TypesEnum = boundType;
                Core.Network.SendToClientReliableOrdered(_spawnControllerPacketReusable, newPlayerKableConnection);
            }
        }

        spawnedController.Attach(spawnedActor);

        if (newPlayerKableConnection == null)
            return;

        _networkStatePacketReusable.State = Core.Network.GetStateOrCached();
        Core.Network.SendToClientReliableOrdered(_networkStatePacketReusable, newPlayerKableConnection);
    }
}