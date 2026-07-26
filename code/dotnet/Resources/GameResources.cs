using System;
using System.Collections.Generic;
using System.Linq;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Services;
using Godot;

namespace AvoidClaws.code.dotnet.Resources;

public partial class GameResources : Node, IService
{
    [Export]
    public Control LoadingScreenHandle { get; private set; } = null!;

    public ActorPrefabs ActorPrefabs { get; private set; } = null!;
    public ControllerPrefabs ControllerPrefabs { get; private set; } = null!;
    public PickupPrefabs PickupPrefabs { get; private set; } = null!;
    public LevelPrefabs MapPrefabs { get; private set; } = null!;
    public ScreenPrefabs ScreenPrefabs { get; private set; } = null!;

    private readonly Dictionary<CoreGame.ActorType, Tuple<Type, PackedScene>> _actorRegistry = new();
    private readonly Dictionary<CoreGame.ControllerType, Tuple<Type, PackedScene>> _controllerRegistry = new();
    private readonly Dictionary<CoreGame.BuffType, Tuple<Type, PackedScene>> _buffRegistry = new();

    public override void _Ready()
    {
        ActorPrefabs = (ActorPrefabs)FindChild("ActorPrefabs");
        ControllerPrefabs = (ControllerPrefabs)FindChild("ControllerPrefabs");
        PickupPrefabs = (PickupPrefabs)FindChild("PickupPrefabs");
        MapPrefabs = (LevelPrefabs)FindChild("LevelPrefabs");
        ScreenPrefabs = (ScreenPrefabs)FindChild("ScreenPrefabs");
    }

    private void Register<T>(CoreGame.ActorType type, PackedScene prefab) where T : IActor
    {
        if (_actorRegistry.ContainsKey(type))
            return;

        _actorRegistry.Add(type, Tuple.Create(typeof(T), prefab));
    }

    private void Register<T>(CoreGame.ControllerType type, PackedScene prefab) where T : IController
    {
        if (_controllerRegistry.ContainsKey(type))
            return;

        _controllerRegistry.Add(type, Tuple.Create(typeof(T), prefab));
    }

    private void Register<T>(CoreGame.BuffType type, PackedScene prefab) where T : IBuff
    {
        if (_buffRegistry.ContainsKey(type))
            return;

        _buffRegistry.Add(type, Tuple.Create(typeof(T), prefab));
    }

    public PackedScene? GetActorPrefab(CoreGame.ActorType type)
    {
        var result = _actorRegistry.FirstOrDefault(x =>
        {
            if (Attribute.GetCustomAttribute(x.Value.Item1, typeof(ActorAttribute)) is ActorAttribute actorAttribute)
                if (actorAttribute.ActorType == type)
                    return true;

            return false;
        });
        return result.Value.Item2;
    }

    public PackedScene? GetBuffPrefab(CoreGame.BuffType type)
    {
        foreach (var pair in _buffRegistry)
            if (Attribute.GetCustomAttribute(pair.Value.Item1, typeof(BuffAttribute)) is BuffAttribute buffAttribute)
                if (buffAttribute.BuffType == type)
                    return pair.Value.Item2;

        return null;
    }

    public CoreGame.ActorType GetActorType(IActor input)
    {
        foreach (var pair in _actorRegistry)
            if (pair.Value.Item1 == input.GetType())
                return pair.Key;

        throw new InvalidOperationException("Tried to retrieve unregistered actor");
    }

    public CoreGame.ControllerType GetControllerType(IController input)
    {
        foreach (var pair in _controllerRegistry)
            if (pair.Value.Item1 == input.GetType())
                return pair.Key;

        // There are some controllers that don't need to be networked almost at all because they
        // function server-side only. For example: DeathZoneController.
        // To accommodate these just assume any unregistered IController is server-side only and return "Dummy" type.
        return CoreGame.ControllerType.Dummy;
    }

    public CoreGame.BuffType GetBuffType(IBuff input)
    {
        foreach (var pair in _buffRegistry)
            if (pair.Value.Item1 == input.GetType())
                return pair.Key;

        throw new InvalidOperationException("Tried to retrieve unregistered buff");
    }

    public CoreGame.BuffType GetBuffType(PackedScene input)
    {
        foreach (var pair in _buffRegistry)
            if (pair.Value.Item2 == input)
                return pair.Key;

        throw new InvalidOperationException("Tried to retrieve unregistered buff");
    }
}