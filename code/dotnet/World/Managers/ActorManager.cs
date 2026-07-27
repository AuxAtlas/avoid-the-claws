using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

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

    internal void HandleOutgoingActor(IActor actor)
    {
        if (!_spawnedActors.ContainsKey(actor.KableId))
            return;

        actor.Teardown();
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
}