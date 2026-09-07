#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvoidClaws.code.dotnet.Actors;
using AvoidClaws.code.dotnet.Buffs;
using AvoidClaws.code.dotnet.Controllers;
using AvoidClaws.code.dotnet.Services;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Resources;

public partial class GameResources : Node, IService
{
    [Export]
    public Control LoadingScreenHandle { get; private set; } = null!;
    
    [Export]
    public ActorPrefabs ActorPrefabs { get; private set; } = null!;

    [Export]
    public BuffPrefabs BuffPrefabs { get; private set; } = null!;

    [Export]
    public ControllerPrefabs ControllerPrefabs { get; private set; } = null!;
    
    [Export]
    public LevelPrefabs LevelPrefabs { get; private set; } = null!;

    private readonly Dictionary<CoreGame.ActorTypesEnum, Tuple<Type, PackedScene>> _actorRegistry = new();
    private readonly Dictionary<CoreGame.ControllerTypesEnum, Tuple<Type, PackedScene>> _controllerRegistry = new();
    private readonly Dictionary<CoreGame.BuffTypesEnum, Tuple<Type, PackedScene>> _buffRegistry = new();
    
    
    private readonly List<PropertyInfo> _prefabClassPropertyInfosReusable = new();

    public override void _Ready()
    {
        ActorPrefabs ??= (ActorPrefabs)FindChild("ActorPrefabs");
        ControllerPrefabs ??= (ControllerPrefabs)FindChild("ControllerPrefabs");
        LevelPrefabs ??= (LevelPrefabs)FindChild("LevelPrefabs");
        BuffPrefabs ??= (BuffPrefabs)FindChild("BuffPrefabs");
        RecalculatePrefabRegistry();
    }

    private void RecalculatePrefabRegistry()
    {
        _prefabClassPropertyInfosReusable.Clear();
        
        // Register Actors
        _prefabClassPropertyInfosReusable.AddRange((ActorPrefabs.GetType().GetProperties()));
        foreach (PropertyInfo propertyInfo in _prefabClassPropertyInfosReusable)
        {
            if (propertyInfo.PropertyType == typeof(PackedScene))
            {
                PackedScene? tmpScene = (PackedScene?)propertyInfo.GetValue(ActorPrefabs);
                if (tmpScene == null)
                    continue;

                Type? rootNodeScriptType = GetRootScriptType(tmpScene, typeof(IActor));
                
                if (rootNodeScriptType?.IsSubclassOf(typeof(IActor)) == true)
                {
                    ActorMarker? marker = rootNodeScriptType.GetCustomAttribute<ActorMarker>();
                    if (marker == null)
                        continue;
                    
                    RegisterActor(rootNodeScriptType, marker.ActorTypesEnum, tmpScene);
                }
            }
        }
        _prefabClassPropertyInfosReusable.Clear();
        
        // Register Buffs
        _prefabClassPropertyInfosReusable.AddRange((BuffPrefabs.GetType().GetProperties()));
        foreach (PropertyInfo propertyInfo in _prefabClassPropertyInfosReusable)
        {
            if (propertyInfo.PropertyType == typeof(PackedScene))
            {
                PackedScene? tmpScene = (PackedScene?)propertyInfo.GetValue(BuffPrefabs);
                if (tmpScene == null)
                    continue;

                Type? rootNodeScriptType = GetRootScriptType(tmpScene, typeof(IBuff));
                
                if (rootNodeScriptType?.IsSubclassOf(typeof(IBuff)) == true)
                {
                    BuffMarker? marker = rootNodeScriptType.GetCustomAttribute<BuffMarker>();
                    if (marker == null)
                        continue;
                    
                    RegisterBuff(rootNodeScriptType, marker.BuffTypesEnum, tmpScene);
                }
            }
        }
        _prefabClassPropertyInfosReusable.Clear();
        
        // Register Controllers
        _prefabClassPropertyInfosReusable.AddRange((ControllerPrefabs.GetType().GetProperties()));
        foreach (PropertyInfo propertyInfo in _prefabClassPropertyInfosReusable)
        {
            if (propertyInfo.PropertyType == typeof(PackedScene))
            {
                PackedScene? tmpScene = (PackedScene?)propertyInfo.GetValue(ControllerPrefabs);
                if (tmpScene == null)
                    continue;

                Type? rootNodeScriptType = GetRootScriptType(tmpScene, typeof(IController));
                
                if (rootNodeScriptType?.IsSubclassOf(typeof(IController)) == true)
                {
                    ControllerMarker? marker = rootNodeScriptType.GetCustomAttribute<ControllerMarker>();
                    if (marker == null)
                        continue;
                    
                    RegisterController(rootNodeScriptType, marker.ControllerTypesEnum, tmpScene);
                }
            }
        }
        _prefabClassPropertyInfosReusable.Clear();;
    }

    private void RegisterActor(Type actorType, CoreGame.ActorTypesEnum typesEnum, PackedScene prefab)
    {
        if (_actorRegistry.ContainsKey(typesEnum))
            return;

        _actorRegistry.Add(typesEnum, Tuple.Create(actorType, prefab));
    }

    private void RegisterBuff(Type buffType, CoreGame.BuffTypesEnum typesEnum, PackedScene prefab)
    {
        if (_buffRegistry.ContainsKey(typesEnum))
            return;

        _buffRegistry.Add(typesEnum, Tuple.Create(buffType, prefab));
    }
    private void RegisterController(Type controllerType, CoreGame.ControllerTypesEnum typesEnum, PackedScene prefab)
    {
        if (_controllerRegistry.ContainsKey(typesEnum))
            return;

        _controllerRegistry.Add(typesEnum, Tuple.Create(controllerType, prefab));
    }

    public PackedScene? GetActorPrefab(CoreGame.ActorTypesEnum typesEnum)
    {
        var result = _actorRegistry.FirstOrDefault
        (x =>
            {
                if (Attribute.GetCustomAttribute(x.Value.Item1, typeof(ActorMarker)) is ActorMarker actorAttribute)
                    if (actorAttribute.ActorTypesEnum == typesEnum)
                        return true;

                return false;
            }
        );
        return result.Value.Item2;
    }

    public PackedScene? GetBuffPrefab(CoreGame.BuffTypesEnum typesEnum)
    {
        foreach (var pair in _buffRegistry)
            if (Attribute.GetCustomAttribute(pair.Value.Item1, typeof(BuffMarker)) is BuffMarker buffAttribute)
                if (buffAttribute.BuffTypesEnum == typesEnum)
                    return pair.Value.Item2;

        return null;
    }

    public CoreGame.ActorTypesEnum GetActorType(IActor input)
    {
        foreach (var pair in _actorRegistry)
            if (pair.Value.Item1 == input.GetType())
                return pair.Key;

        throw new InvalidOperationException("Tried to retrieve unregistered actor");
    }

    public CoreGame.ControllerTypesEnum GetControllerType(IController input)
    {
        foreach (var pair in _controllerRegistry)
            if (pair.Value.Item1 == input.GetType())
                return pair.Key;

        // There are some controllers that don't need to be networked almost at all because they
        // function server-side only. For example: DeathZoneController.
        // To accommodate these just assume any unregistered IController is server-side only and return "Dummy" type.
        return CoreGame.ControllerTypesEnum.Dummy;
    }

    public CoreGame.BuffTypesEnum GetBuffType(IBuff input)
    {
        foreach (var pair in _buffRegistry)
            if (pair.Value.Item1 == input.GetType())
                return pair.Key;

        throw new InvalidOperationException("Tried to retrieve unregistered buff");
    }

    public CoreGame.BuffTypesEnum GetBuffType(PackedScene input)
    {
        foreach (var pair in _buffRegistry)
            if (pair.Value.Item2 == input)
                return pair.Key;

        throw new InvalidOperationException("Tried to retrieve unregistered buff");
    }
    /// <summary>
    /// Retrieves the C# System.Type of the root node's script without instantiating the scene.
    /// </summary>
    public static Type? GetRootScriptType(PackedScene packedScene, Type targetTypeBase)
    {
        SceneState state = packedScene.GetState();
        
        const int rootNodeIndex = 0; 
        
        Script? rootScript = null;

        // 2. Look for the "script" property among the root node's properties
        int propCount = state.GetNodePropertyCount(rootNodeIndex);
        for (int i = 0; i < propCount; i++)
        {
            if (state.GetNodePropertyName(rootNodeIndex, i) == "script")
            {
                rootScript = state.GetNodePropertyValue(rootNodeIndex, i).As<Script>();
                break;
            }
        }
        
        if (rootScript == null)
            return null;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsSubclassOf(targetTypeBase))
                .FirstOrDefault(t => t.GetCustomAttribute<ScriptPathAttribute>()?.Path == rootScript.ResourcePath);

            if (type != null)
                return type;
        }

        return null;
    }
}