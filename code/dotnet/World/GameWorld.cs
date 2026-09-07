#region

using System.Collections.Generic;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Glue;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.World.Managers;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.World;

public partial class GameWorld : Node, IService
{
    [Export]
    public Camera3D? DefaultSpectatorCamera { get; private set; }

    [Export]
    public ActorManager Actors { get; private set; } = null!;

    [Export]
    public ControllerManager Controllers { get; private set; } = null!;

    [Export]
    public BuffManager Buffs { get; private set; } = null!;

    [Export]
    public Node3D LevelRoot { get; private set; } = null!;

    [Inject]
    protected CoreGame Core { get; } = null!;
    
    private uint _latestNetworkTick = 0;

    public IEnumerable<IGameObject> GameObjects => Actors.SpawnedActors.Concat<IGameObject>(Controllers.SpawnedControllers);

    public override void _EnterTree()
    {
        base._EnterTree();

        GetTree().NodeAdded += OnNodeAdded;
        GetTree().NodeRemoved += OnNodeRemoved;
    }

    public override void _ExitTree()
    {
        GetTree().NodeAdded -= OnNodeAdded;
        GetTree().NodeRemoved -= OnNodeRemoved;
    }

    public void ResetWorld()
    {
        List<IGameObject> objectsToDestroy = new();
        objectsToDestroy.AddRange(Actors.SpawnedActors);
        objectsToDestroy.AddRange(Controllers.SpawnedControllers);
        objectsToDestroy.ForEach(x => DestroyObject(x.KableId));
        
        foreach (var child in LevelRoot.GetChildren())
            child.QueueFree();
    }

    public void DestroyObject(KableId kableId)
    {
        if (!DoesKableIdExist(kableId))
            return;

        IGameObject? target = null;

        target = Actors.GetById(kableId);

        if (target == null)
            target = Controllers.GetById(kableId);

        if (target == null)
        {
            GD.PushError($"Tried to destroy an unknown GameObject with KableId of '{kableId}'");
            return;
        }

        DestroyObject(target);
    }

    public void DestroyObject(IGameObject target)
    {
        target.Stop(_latestNetworkTick);
        target.Teardown(_latestNetworkTick);

        if (target is Node node)
            node.QueueFree();
    }

    public T? GetGameObject<T>(KableId kableId) where T : IGameObject
    {
        var found = default(T);

        found ??= (T?)Actors.GetById(kableId);
        found ??= (T?)Controllers.GetById(kableId);

        return found;
    }

    public IGameObject? GetGameObject(KableId kableId)
    {
        return GetGameObject<IGameObject>(kableId);
    }

    public bool DoesKableIdExist(KableId kableId)
    {
        return DoesKableIdExist(kableId.Id);
    }
    public bool DoesKableIdExist(uint rawKableId)
    {
        return Actors.CheckExists(rawKableId) || Controllers.CheckExists(rawKableId) || Buffs.CheckExists(rawKableId);
    }




    private void OnNodeAdded(Node node)
    {
        if (node is not IGameObject gameObject)
            return;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (gameObject.KableId == null || !gameObject.KableId.IsValid)
            gameObject.KableSetup(Core.GenerateUniqueKableId());

        switch (node)
        {
            case IActor actor:
            {
                Actors.HandleIncomingActor(actor);
                break;
            }
            case IController controller:
            {
                Controllers.HandleIncomingController(controller);
                break;
            }
        }
    }

    private void OnNodeRemoved(Node node)
    {
        if (node is not IGameObject gameObject)
            return;

        if (!gameObject.KableId.IsValid)
            return;

        switch (node)
        {
            case IActor actor:
            {
                Actors.HandleOutgoingActor(actor, _latestNetworkTick);
                break;
            }
            case IController controller:
            {
                Controllers.HandleOutgoingController(controller, _latestNetworkTick);
                break;
            }
        }

        Core.EventBus.Publish
        (
            new GameObjectDespawnedEvent
            {
                GameObject = gameObject
            }
        );
    }


    internal void ProcessNetTick(uint tick, bool isReconciliation = false)
    {
        _latestNetworkTick = tick;
        
        if(!isReconciliation)
        {
            foreach (IController controller in Controllers.SpawnedControllers)
            {
                controller.HandleNetTick(tick);
            }
        }

        foreach (IActor actor in Actors.SpawnedActors)
        {
            actor.HandleNetTick(tick);
        }
        
        Core.StepPhysics3D(NetworkManager.TickDeltaTime);

        if (!isReconciliation)
        {
            Core.FlushPhysics3D();
            foreach (IActor actor in Actors.SpawnedActors)
            {
                if (actor is PhysicsBody3D physicsActor3D)
                {
                    physicsActor3D.ForceUpdateTransform();
                }
            }
        }
    }

    public void ChangeMapTo(PackedScene map)
    {
        ResetWorld();

        var mapNode = map.Instantiate();
        if (mapNode is null)
            return;

        LevelRoot.AddChild(mapNode);
    }

    public List<Node> GetAllDescendantsOf(Node rootNode)
    {
        List<Node> currentList = [rootNode];

        foreach (var child in rootNode.GetChildren())
            currentList.AddRange(GetAllDescendantsOf(child));

        return currentList;
    }

    public IEnumerable<IGameObject> GetAllOwnedBy(KableConnection owner)
    {
        return GameObjects.Where(x => x.AuthorityConnectionId == owner.ConnectionId).ToList().AsReadOnly();
    }
}