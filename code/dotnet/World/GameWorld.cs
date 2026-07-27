using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Data;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Glue;
using AvoidClaws.code.dotnet.Glue.Managers;
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

    public Node? SpawnPrefab(PackedScene? prefab, KableId? presetKableId = null)
    {
        if (_currentMapNode is null)
            return null;


        if (!(prefab?.CanInstantiate()).GetValueOrDefault(false))
            return null;

        if (presetKableId?.Id < 1)
            presetKableId = null;

        presetKableId ??= GenerateKableId();

        if (GetKableObject<IKableObject>(presetKableId.Value) != null)
        {
            GD.PrintErr("Level: Tried to spawn multiple KableObject with the same KableId!");
            return null;
        }

        var spawned = prefab?.Instantiate();

        if (_currentMapNode is null)
            return null;

        if (spawned is not IKableObject kableObject)
        {
            _currentMapNode.AddChild(spawned);
            return spawned;
        }

        kableObject.KableSetup(presetKableId.Value);
        kableObject.SetKableAuthority(Core.Network.GetServerConnectionId());

        _currentMapNode.AddChild(spawned);

        switch (spawned)
        {
            case IActor actor:
            {
                if (_spawnedActors.TryAdd(presetKableId.Value, actor)) GD.Print($"Spawned actor: {presetKableId}");

                break;
            }
            case IController controller:
            {
                if (_spawnedControllers.TryAdd(presetKableId.Value, controller)) GD.Print($"Spawned controller: {presetKableId}");

                break;
            }
        }

        return spawned;
    }

    public T? SpawnPrefab<T>(PackedScene? prefab, KableId? presetKableId = null) where T : Node
    {
        return (T?)SpawnPrefab(prefab, presetKableId);
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


    public T? GetKableObject<T>(uint kableId) where T : IKableObject
    {
        var found = (T?)_spawnedActors.FirstOrDefault(x => x.Key.Id == kableId).Value;
        if (found is not null)
            return found;

        found = (T?)_spawnedControllers.FirstOrDefault(x => x.Key.Id == kableId).Value;

        return found;
    }

    public T? GetKableObject<T>(KableId kableId) where T : IKableObject
    {
        if (_spawnedActors.TryGetValue(kableId, out var actor))
            return (T)actor;

        if (_spawnedControllers.TryGetValue(kableId, out var controller))
            return (T)controller;

        return default;
    }

    public bool CheckObjectExists(KableId kableId)
    {
        return Actors.CheckActorExists(kableId) || Controllers.CheckControllerExists(kableId);
    }

    public IKableObject? GetKableObject(KableId kableId)
    {
        return GetKableObject<IKableObject>(kableId);
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
        for (var i = 0; i < DisplayedErrorMessages.Count; i++)
        {
            DisplayedErrorMessages[i].SecondsRemaining -= NetworkManager.TickDeltaTimeF;
            if (DisplayedErrorMessages[i].SecondsRemaining <= 0) DisplayedErrorMessages.RemoveAt(i);
        }

        foreach (var controller in SpawnedControllers)
            controller.HandleNetTick(tick);

        foreach (var actor in SpawnedActors)
            actor.HandleNetTick(tick);
    }

    public void ChangeMapTo(PackedScene? map)
    {
        if (_mapSocketNode == null)
        {
            GD.PrintErr("Tried to change map but 'MapRootNode' is null.");
            return;
        }

        ResetWorld();

        _currentMapNode = map?.Instantiate();
        _mapSocketNode.AddChild(_currentMapNode);
    }

    public void GotoMainMenu()
    {
        Core.World.ChangeMapTo(Core.Resources.ScreenPrefabs.MainMenuScreen);
    }

    public void SetDisplayedErrorMessage(string errorMessage, float showForSeconds = 10.0f)
    {
        ErrorMessage msg = new()
        {
            Message = errorMessage,
            SecondsRemaining = showForSeconds
        };
        DisplayedErrorMessages.Add(msg);
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
        currentList.AddRange(_spawnedActors.Values.ToList());
        currentList.AddRange(_spawnedControllers.Values.ToList());

        return currentList.Where(x => x.AuthorityConnectionId == owner.ConnectionId).ToList().AsReadOnly();
    }
}