#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.Util.Exceptions;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class ActorManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    [Export]
    private Node _actorsRoot = null!;

    private readonly Dictionary<KableId, IActor> _spawnedActors = new();
    public ReadOnlyCollection<IActor> SpawnedActors => _spawnedActors.Values.ToList().AsReadOnly();

    private readonly ActorSpawnedEvent _actorSpawnedEventReusable = new();

    internal void HandleIncomingActor(IActor actor)
    {
        _spawnedActors.TryAdd(actor.KableId, actor);
        _actorSpawnedEventReusable.Actor = actor;
        Core.EventBus.Publish(_actorSpawnedEventReusable);
    }

    internal void HandleOutgoingActor(IActor actor, uint tick)
    {
        if (!_spawnedActors.ContainsKey(actor.KableId))
            return;

        actor.Teardown(tick);
        _spawnedActors.Remove(actor.KableId);
    }

    public IActor? GetById(KableId kableId)
    {
        return (IActor?)_spawnedActors.FirstOrDefault(x => x.Key == kableId).Value;
    }
    public IActor? GetById(uint rawKableId)
    {
        return (IActor?)_spawnedActors.FirstOrDefault(x => x.Key.Id == rawKableId).Value;
    }

    public bool CheckExists(KableId kableId)
    {
        return CheckExists(kableId.Id);
    }

    public bool CheckExists(uint rawKableId)
    {
        return _spawnedActors.Any(x => x.Key.Id == rawKableId);
    }

    public IActor SpawnPrefab(PackedScene? prefab, uint tick, KableId? presetKableId = null)
    {
        if (!(prefab?.CanInstantiate()).GetValueOrDefault(false))
            return null;

        if (presetKableId?.Id < 1)
            presetKableId = null;

        presetKableId ??= Core.GenerateUniqueKableId();

        Node? spawned = prefab?.Instantiate();

        if (spawned is not IActor actor)
        {
            spawned?.QueueFree();
            throw new InvalidPrefabException("Tried to instantiate a non-actor prefab as an IActor");
        }

        actor.KableSetup(presetKableId);
        actor.SetKableAuthority(Core.Network.GetServerConnectionId());
        actor.Spawned(tick);

        _actorsRoot.AddChild(spawned);

        if (_spawnedActors.TryAdd(presetKableId, actor))
            GD.Print($"Spawned actor: {presetKableId}");

        actor.Start(tick);
        
        return actor;
    }
}