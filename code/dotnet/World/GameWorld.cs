using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Data;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Glue.Managers;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Networking.Packets.Objects;
using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.World;

public partial class GameWorld : Node, IService
{
    private readonly Dictionary<KableId, IActor> _spawnedActors = new();
    private readonly Dictionary<KableId, IController> _spawnedControllers = new();

    private Node? _currentMapNode;

    [Export]
    private Node? _mapSocketNode;

    [Export]
    public Camera3D? DefaultSpectatorCamera;

    [Inject]
    protected CoreGame Core { get; } = null!;

    public readonly List<ErrorMessage> DisplayedErrorMessages = new();

    public bool IsClient => !IsServer;
    public bool IsServer => Core.Network.IsServer;

    public ReadOnlyCollection<IActor> SpawnedActors => _spawnedActors.Values.ToList().AsReadOnly();
    public ReadOnlyCollection<IController> SpawnedControllers => _spawnedControllers.Values.ToList().AsReadOnly();

    public Random Random { get; private set; }

    public override void _Ready()
    {
        ChangeMapTo(Core.Resources.ScreenPrefabs.MainMenuScreen);
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        if (_mapSocketNode is null)
            GD.PrintErr("Level Error: 'MapSocketNode' is null");

        Random = new Random(Guid.NewGuid().GetHashCode());

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
        if (_currentMapNode is null)
            return;

        _currentMapNode.QueueFree();
        _spawnedActors.Clear();
        _spawnedControllers.Clear();
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
        Node? targetNode = null;
        if (_spawnedActors.Remove(kableId, out var targetActor))
            targetNode = (Node)targetActor;

        if (_spawnedControllers.Remove(kableId, out var targetController))
            targetNode = (Node)targetController;

        if (targetNode is null)
            return;

        if (IsServer)
            Core.Network.SendToAllReliableUnordered
            (
                new DestroyObjectPacket
                {
                    TargetObjectId = kableId
                }
            );

        targetNode.Free();
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

    public IKableObject? GetKableObject(KableId kableId)
    {
        return GetKableObject<IKableObject>(kableId);
    }

    // public PlayerController? GetPlayerController()
    // {
    // 	return (PlayerController?)_spawnedControllers.FirstOrDefault(controller => controller.GetType() == typeof(PlayerController)).Value;
    // }


    public KableId GenerateKableId()
    {
        return new KableId(GenerateRawKableId());
    }

    public uint GenerateRawKableId()
    {
        uint result = 0;
        while (result == 0)
            result = (uint)Random.Next() + (uint)Random.Next();
        return result;
    }


    private void OnNodeAdded(Node node)
    {
        if (node is not IKableObject kableObject)
            return;

        if (!kableObject.KableId.IsValid)
            kableObject.KableSetup(GenerateKableId());

        switch (node)
        {
            case IActor actor:
            {
                _spawnedActors.TryAdd(actor.KableId!, actor);
                Core.EventBus.Publish
                (
                    new ActorSpawnedEvent
                    {
                        Actor = actor,
                        RootNode = node,
                        Level = this
                    }
                );
                break;
            }
            case IController controller:
            {
                _spawnedControllers.TryAdd(controller.KableId!, controller);

                Core.EventBus.Publish
                (
                    new ControllerSpawnedEvent
                    {
                        Controller = controller,
                        RootNode = node,
                        Level = this
                    }
                );
                break;
            }
        }
    }

    private void OnNodeRemoved(Node node)
    {
        if (node is not IKableObject kableObject)
            return;

        if (!kableObject.KableId.IsValid)
            return;

        switch (node)
        {
            case IActor actor:
            {
                _spawnedActors.Remove(actor.KableId);
                Core.EventBus.Publish
                (
                    new ObjectDespawnedEvent
                    {
                        KableObject = actor,
                        RootNode = node,
                        GameWorld = this
                    }
                );
                break;
            }
            case IController controller:
            {
                _spawnedControllers.Remove(controller.KableId);
                Core.EventBus.Publish
                (
                    new ObjectDespawnedEvent
                    {
                        KableObject = controller,
                        RootNode = node,
                        GameWorld = this
                    }
                );
                break;
            }
        }
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