using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Events.Lifecycle;
using AvoidClaws.code.dotnet.Networking.Data;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.Util.Exceptions;
using Godot;

namespace AvoidClaws.code.dotnet.World.Managers;

public partial class BuffManager : Node, IService
{
    [Inject]
    protected CoreGame Core { get; } = null!;

    private readonly Dictionary<KableId, IBuff> _spawnedBuffs = new();
    public IReadOnlyCollection<IBuff> SpawnedBuffs => _spawnedBuffs.AsReadOnly().Values;
    
    private readonly BuffSpawnedEvent _buffSpawnedEventReusable = new();

    internal void HandleIncomingBuff(IBuff buff)
    {
        _spawnedBuffs.TryAdd(buff.KableId, buff);

        _buffSpawnedEventReusable.Buff = buff;
        Core.EventBus.Publish(_buffSpawnedEventReusable);
    }

    internal void HandleOutgoingBuff(IBuff buff, uint tick)
    {
        if (!_spawnedBuffs.ContainsKey(buff.KableId))
            return;

        buff.Teardown(tick);
        _spawnedBuffs.Remove(buff.KableId);
    }

    public IBuff? GetById(KableId kableId)
    {
        return (IBuff?)_spawnedBuffs.FirstOrDefault(x => x.Key == kableId).Value;
    }

    public bool CheckExists(KableId kableId)
    {
        return CheckExists(kableId.Id);
    }

    public bool CheckExists(uint rawKableId)
    {
        return _spawnedBuffs.Any(x => x.Key.Id == rawKableId);
    }

    public IBuff SpawnBuffOntoActor(PackedScene? prefab, uint tick, IActor actor, KableId? presetKableId = null)
    {
        if (!(prefab?.CanInstantiate()).GetValueOrDefault(false))
            throw new InvalidPrefabException("");

        if (presetKableId?.Id < 1)
            presetKableId = null;

        presetKableId ??= Core.GenerateUniqueKableId();

        Node? spawned = prefab?.Instantiate();

        if (spawned is not IBuff buff)
        {
            spawned?.QueueFree();
            throw new InvalidPrefabException("Tried to instantiate a non-buff prefab as an IBuff");
        }

        buff.KableSetup(presetKableId);
        buff.SetKableAuthority(Core.Network.GetServerConnectionId());
        buff.Spawned(tick);

        if (_spawnedBuffs.TryAdd(presetKableId, buff))
            GD.Print($"Spawned buff: {presetKableId}");

        actor.ApplyBuff(buff, tick);
        
        return buff;
    }
}