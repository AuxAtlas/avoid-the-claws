using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using Godot;

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

    internal void HandleOutgoingController(IController controller)
    {
        if (!_spawnedControllers.ContainsKey(controller.KableId))
            return;

        controller.Teardown();
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
}