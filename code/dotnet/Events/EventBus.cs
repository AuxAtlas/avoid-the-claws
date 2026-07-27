using System;
using System.Collections.Generic;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.Util;
using Godot;

namespace AvoidClaws.code.dotnet.Events;

public partial class EventBus : Node, IService
{
    private readonly Dictionary<ulong, List<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        if (!_handlers.ContainsKey(HashHelper.FastHash<T>()))
            _handlers.Add(HashHelper.FastHash<T>(), new List<Delegate>());

        _handlers[HashHelper.FastHash<T>()].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
    {
        if (!_handlers.ContainsKey(HashHelper.FastHash<T>()))
            return;

        _handlers[HashHelper.FastHash<T>()].Remove(handler);
    }

    public void Publish<T>(T @event) where T : GameEvent
    {
        if (_handlers.ContainsKey(HashHelper.FastHash<T>()))
            _handlers[HashHelper.FastHash<T>()].ForEach(x => ((Action<T>)x)(@event));
    }
}