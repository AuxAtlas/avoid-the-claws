#region

using System;
using System.Collections.Generic;
using System.Linq;
using AvoidClaws.code.dotnet.Services;
using AvoidClaws.code.dotnet.Util;
using Godot;

#endregion

namespace AvoidClaws.code.dotnet.Events;

public partial class EventBus : Node, IService
{
    private readonly Dictionary<ulong, List<WeakAction>> _handlers = new();

    public void Subscribe<T>(object subscriber, Action<T> handler) where T : GameEvent
    {
        ulong hash = HashHelper.FastHash<T>();
        if (!_handlers.ContainsKey(hash))
            _handlers[hash] = new List<WeakAction>();

        _handlers[hash].Add(new WeakAction(subscriber, handler));
    }

    public void Unsubscribe<T>(object subscriber, Action<T> handler) where T : GameEvent
    {
        ulong hash = HashHelper.FastHash<T>();
        if (!_handlers.ContainsKey(hash))
            return;
        
        _handlers[hash].RemoveAll(x => x.IsFrom(subscriber));
    }

    public void Publish<T>(T @event) where T : GameEvent
    {
        ulong hash = HashHelper.FastHash<T>();
        if (!_handlers.TryGetValue(hash, out var actionsList))
            return;

        actionsList.RemoveAll(x => !x.IsAlive);

        actionsList.ForEach(x => x.Invoke(@event));
    }
}