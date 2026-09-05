#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class ActorManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    private readonly Dictionary<KableId, IActor> _spawnedActors = new();
    public ReadOnlyCollection<IActor> SpawnedActors => _spawnedActors.Values.ToList().AsReadOnly();

    internal void HandleIncomingActor(IActor actor)
    {
        _spawnedActors.TryAdd(actor.KableId, actor);
        Core.EventBus.Publish
        (
            new ActorSpawnedEvent
            {
                Actor = actor
            }
        );
    }

    internal void HandleOutgoingActor(IActor actor, uint tick)
    {
        if (!_spawnedActors.ContainsKey(actor.KableId))
            return;

        actor.Teardown(tick);
        _spawnedActors.Remove(actor.KableId);
    }

    public IActor? GetActor(KableId kableId)
    {
        return (IActor?)_spawnedActors.FirstOrDefault(x => x.Key == kableId).Value;
    }

    public bool CheckActorExists(KableId kableId)
    {
        return _spawnedActors.ContainsKey(kableId);
    }

    public Node? SpawnActorPrefab(PackedScene? prefab, KableId? presetKableId = null)
    {
        if (!(prefab?.CanInstantiate()).GetValueOrDefault(false))
            return null;

        if (presetKableId?.Id < 1)
            presetKableId = null;

        presetKableId ??= Core.GenerateKableId();

        if (Core.World.GetGameObject(presetKableId) != null)
        {
            GD.PrintErr("Level: Tried to spawn multiple KableObject with the same KableId!");
            return null;
        }

        var spawned = prefab?.Instantiate();

        if (spawned is not IActor actor)
        {
            spawned?.QueueFree();
            return spawned;
        }

        actor.KableSetup(presetKableId);
        actor.SetKableAuthority(Core.Network.GetServerConnectionId());

        AddChild(spawned);

        if (_spawnedActors.TryAdd(presetKableId, actor))
            GD.Print($"Spawned actor: {presetKableId}");

        return spawned;
    }
}