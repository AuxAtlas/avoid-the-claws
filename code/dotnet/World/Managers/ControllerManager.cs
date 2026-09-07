#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.Util.Exceptions;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class ControllerManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    [Export]
    private Node _controllersRoot = null!;

    private readonly Dictionary<KableId, IController> _spawnedControllers = new();
    public ReadOnlyCollection<IController> SpawnedControllers => _spawnedControllers.Values.ToList().AsReadOnly();

    private readonly ControllerSpawnedEvent _controllerSpawnedEventReusable = new();

    internal void HandleIncomingController(IController controller)
    {
        _spawnedControllers.TryAdd(controller.KableId, controller);

        _controllerSpawnedEventReusable.Controller = controller;
        Core.EventBus.Publish (_controllerSpawnedEventReusable);
    }

    internal void HandleOutgoingController(IController controller, uint tick)
    {
        if (!_spawnedControllers.ContainsKey(controller.KableId))
            return;

        controller.Teardown(tick);
        _spawnedControllers.Remove(controller.KableId);
    }

    public IController? GetById(KableId kableId)
    {
        return (IController?)_spawnedControllers.FirstOrDefault(x => x.Key == kableId).Value;
    }

    public bool CheckExists(KableId kableId)
    {
        return CheckExists(kableId.Id);
    }

    public bool CheckExists(uint rawKableId)
    {
        return _spawnedControllers.Any(x => x.Key.Id == rawKableId);
    }

    public IController SpawnPrefab(PackedScene? prefab, uint tick, KableId? presetKableId = null)
    {
        if (!(prefab?.CanInstantiate()).GetValueOrDefault(false))
            throw new InvalidPrefabException();

        if (presetKableId?.Id < 1)
            presetKableId = null;

        presetKableId ??= Core.GenerateUniqueKableId();

        var spawned = prefab?.Instantiate();

        if (spawned is not IController controller)
        {
            spawned?.QueueFree();
            throw new InvalidPrefabException("Tried to instantiate a non-controller prefab as an IController");
        }

        controller.KableSetup(presetKableId);
        controller.SetKableAuthority(Core.Network.GetServerConnectionId());
        controller.Spawned(tick);

        _controllersRoot.AddChild(spawned);

        if (_spawnedControllers.TryAdd(presetKableId, controller))
            GD.Print($"Spawned controller: {presetKableId}");

        controller.Start(tick);
        
        return controller;
    }
}