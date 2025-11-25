using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventManager
{
    private static readonly Dictionary<Type, List<IEventListener>> _eventListeners = new();

    private static readonly Dictionary<Type, Action<IGameEvent>> s_Events = new Dictionary<Type, Action<IGameEvent>>();

    private static readonly Dictionary<Delegate, Action<IGameEvent>> s_EventLookups =
        new Dictionary<Delegate, Action<IGameEvent>>();

    public static void AddListener<T>(Action<T> evt) where T : IGameEvent
    {
        if (!s_EventLookups.ContainsKey(evt))
        {
            Action<IGameEvent> newAction = (e) => evt((T)e);
            s_EventLookups[evt] = newAction;

            if (s_Events.TryGetValue(typeof(T), out Action<IGameEvent> internalAction))
                s_Events[typeof(T)] = internalAction += newAction;
            else
                s_Events[typeof(T)] = newAction;
        }
    }

    public static void RemoveListener<T>(Action<T> evt) where T : IGameEvent
    {
        if (s_EventLookups.TryGetValue(evt, out var action))
        {
            if (s_Events.TryGetValue(typeof(T), out var tempAction))
            {
                tempAction -= action;
                if (tempAction == null)
                    s_Events.Remove(typeof(T));
                else
                    s_Events[typeof(T)] = tempAction;
            }

            s_EventLookups.Remove(evt);
        }
    }

    public static void Trigger(IGameEvent evt)
    {
        if (s_Events.TryGetValue(evt.GetType(), out var action))
            action.Invoke(evt);
    }
}

public interface IEventListener
{
    void OnEventTriggeredRaw(object gameEvent);
}

public interface IEventListener<T> : IEventListener where T : IGameEvent
{
    void OnEventTriggered(T gameEvent);
}

public interface IGameEvent
{
}

public class UnLockStash : IGameEvent
{
    public KeyLockType KeyLockType;
}