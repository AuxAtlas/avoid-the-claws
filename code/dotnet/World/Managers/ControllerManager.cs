#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class ControllerManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    private readonly Dictionary<KableId, IController> _spawnedControllers = new();
    public ReadOnlyCollection<IController> SpawnedControllers => _spawnedControllers.Values.ToList().AsReadOnly();

    internal void HandleIncomingController(IController controller)
    {
        _spawnedControllers.TryAdd(controller.KableId, controller);

        Core.EventBus.Publish
        (
            new ControllerSpawnedEvent
            {
                Controller = controller
            }
        );
    }

    internal void HandleOutgoingController(IController controller, uint tick)
    {
        if (!_spawnedControllers.ContainsKey(controller.KableId))
            return;

        controller.Teardown(tick);
        _spawnedControllers.Remove(controller.KableId);
    }

    public IController? GetController(KableId kableId)
    {
        return (IController?)_spawnedControllers.FirstOrDefault(x => x.Key == kableId).Value;
    }

    public bool CheckControllerExists(KableId kableId)
    {
        return _spawnedControllers.ContainsKey(kableId);
    }

    public Node? SpawnControllerPrefab(PackedScene? prefab, KableId? presetKableId = null)
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

        if (spawned is not IController controller)
        {
            spawned?.QueueFree();
            return spawned;
        }

        controller.KableSetup(presetKableId);
        controller.SetKableAuthority(Core.Network.GetServerConnectionId());

        AddChild(spawned);

        if (_spawnedControllers.TryAdd(presetKableId, controller))
            GD.Print($"Spawned controller: {presetKableId}");

        return spawned;
    }
}