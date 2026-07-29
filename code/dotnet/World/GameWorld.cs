using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Glue;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.World.Managers;
using Godot;

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
    public LevelManager Level { get; private set; } = null!;

    [Inject]
    protected CoreGame Core { get; } = null!;
    
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
    }

    public void DestroyObject(KableId kableId)
    {
        if (!CheckObjectExists(kableId))
            return;

        IGameObject? target = null;

        target = Actors.GetActor(kableId);

        if (target == null)
            target = Controllers.GetController(kableId);

        if (target == null)
        {
            GD.PushError($"Tried to destroy an unknown GameObject with KableId of '{kableId}'");
            return;
        }

        DestroyObject(target);
    }

    public void DestroyObject(IGameObject target)
    {
        target.Stop();
        target.Teardown();

        if (target is Node node)
            node.QueueFree();
    }

    public T? GetGameObject<T>(KableId kableId) where T : IGameObject
    {
        var found = default(T);

        found ??= (T?)Actors.GetActor(kableId);
        found ??= (T?)Controllers.GetController(kableId);
        
        return found;
    }

    public IGameObject? GetGameObject(KableId kableId)
    {
        return GetGameObject<IGameObject>(kableId);
    }

    public bool CheckObjectExists(KableId kableId)
    {
        return Actors.CheckActorExists(kableId) || Controllers.CheckControllerExists(kableId);
    }

    // public PlayerController? GetPlayerController()
    // {
    // 	return (PlayerController?)_spawnedControllers.FirstOrDefault(controller => controller.GetType() == typeof(PlayerController)).Value;
    // }




    private void OnNodeAdded(Node node)
    {
        if (node is not IGameObject gameObject)
            return;

        if (!gameObject.KableId.IsValid)
            gameObject.KableSetup(Core.GenerateKableId());

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
                Actors.HandleOutgoingActor(actor);
                break;
            }
            case IController controller:
            {
                Controllers.HandleOutgoingController(controller);
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


    internal void ProcessNetTick(uint tick)
    {
        foreach (var controller in Controllers.SpawnedControllers)
            controller.HandleNetTick(tick);

        foreach (var actor in Actors.SpawnedActors)
            actor.HandleNetTick(tick);
    }

    public void ChangeMapTo(PackedScene map)
    {
        Level.ChangeMapTo(map);
    }

    public void GotoMainMenu()
    {
        Core.World.ChangeMapTo(Core.Resources.ScreenPrefabs.MainMenuScreen);
    }

    public List<Node> GetAllDescendantsOf(Node rootNode)
    {
        List<Node> currentList = [rootNode];

        foreach (var child in rootNode.GetChildren())
            currentList.AddRange(GetAllDescendantsOf(child));

        return currentList;
    }

    public ReadOnlyCollection<IKableObject> GetAllOwnedBy(KableConnection owner)
    {
        List<IKableObject> currentList = new();
        currentList.AddRange(Actors.SpawnedActors);
        currentList.AddRange(Controllers.SpawnedControllers);

        return currentList.Where(x => x.AuthorityConnectionId == owner.ConnectionId).ToList().AsReadOnly();
    }
}