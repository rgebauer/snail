// Messenger.cs v1.0 by Magnus Wolffelt, magnus.wolffelt@gmail.com
//
// Inspired by and based on Rod Hyde's Messenger:
// http://www.unifycommunity.com/wiki/index.php?title=CSharpMessenger
//
// This is a C# messenger (notification center). It uses delegates
// and generics to provide type-checked messaging between event producers and
// event consumers, without the need for producers or consumers to be aware of
// each other. The major improvement from Hyde's implementation is that
// there is more extensive error detection, preventing silent bugs.
//
// Usage example:
// Messenger<float>.AddListener("myEvent", MyEventHandler);
// ...
// Messenger<float>.Broadcast("myEvent", 1.0f);

using System;
using System.Collections.Generic;

public enum MessengerMode
{
    DONT_REQUIRE_LISTENER,
    REQUIRE_LISTENER,
}

internal class MessengerInternal
{
    public Dictionary<string, Delegate> EventTable = new Dictionary<string, Delegate>();
    static public readonly MessengerMode DEFAULT_MODE = MessengerMode.DONT_REQUIRE_LISTENER;

    public void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
    {
        if (!EventTable.ContainsKey(eventType))
        {
            EventTable.Add(eventType, null);
        }

        Delegate d = EventTable[eventType];
        if (d != null && d.GetType() != listenerBeingAdded.GetType())
        {
            throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}",
                eventType,
                d.GetType().Name,
                listenerBeingAdded.GetType().Name));
        }
    }

    public void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
    {
        if (EventTable.ContainsKey(eventType))
        {
            Delegate d = EventTable[eventType];

            if (d == null)
            {
                throw new ListenerException(string.Format("Attempting to remove listener with for event type {0} but current listener is null.",
                    eventType));
            }
            else if (d.GetType() != listenerBeingRemoved.GetType())
            {
                throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}",
                    eventType,
                    d.GetType().Name,
                    listenerBeingRemoved.GetType().Name));
            }
        }
        else
        {
            throw new ListenerException(string.Format("Attempting to remove listener for type {0} but Messenger doesn't know about this event type.",
                eventType));
        }
    }

    public void OnListenerRemoved(string eventType)
    {
        if (EventTable[eventType] == null)
        {
            EventTable.Remove(eventType);
        }
    }

    public void OnBroadcasting(string eventType, MessengerMode mode)
    {
        if (mode == MessengerMode.REQUIRE_LISTENER && !EventTable.ContainsKey(eventType))
        {
            throw new MessengerInternal.BroadcastException(string.Format("Broadcasting message {0} but no listener found.",
                eventType));
        }
    }

    public BroadcastException CreateBroadcastSignatureException(string eventType)
    {
        return new BroadcastException(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.",
            eventType));
    }

    public class BroadcastException : Exception
    {
        public BroadcastException(string msg)
			: base(msg)
        {
        }
    }

    public class ListenerException : Exception
    {
        public ListenerException(string msg)
			: base(msg)
        {
        }
    }
}
// No parameters
public class Messenger
{
    private Dictionary<string, Delegate> _eventTable;
    private MessengerInternal _messengerInternal;

    public Messenger()
    {
        _messengerInternal = new MessengerInternal();
        _eventTable = _messengerInternal.EventTable;
    }

    public void AddListener(string eventType, Callback handler)
    {
        _messengerInternal.OnListenerAdding(eventType, handler);
        _eventTable[eventType] = (Callback)_eventTable[eventType] + handler;
    }

    public void RemoveListener(string eventType, Callback handler)
    {
        _messengerInternal.OnListenerRemoving(eventType, handler);	
        _eventTable[eventType] = (Callback)_eventTable[eventType] - handler;
        _messengerInternal.OnListenerRemoved(eventType);
    }

    public void Broadcast(string eventType)
    {
        Broadcast(eventType, MessengerInternal.DEFAULT_MODE);
    }

    public void Broadcast(string eventType, MessengerMode mode)
    {
        _messengerInternal.OnBroadcasting(eventType, mode);
        Delegate d;
        if (_eventTable.TryGetValue(eventType, out d))
        {
            Callback callback = d as Callback;
            if (callback != null)
            {
                callback();
            }
            else
            {
                throw _messengerInternal.CreateBroadcastSignatureException(eventType);
            }
        }
    }
}
// One parameter
public class Messenger<T>
{
    private Dictionary<string, Delegate> _eventTable;
    private MessengerInternal _messengerInternal;

    public Messenger()
    {
        _messengerInternal = new MessengerInternal();
        _eventTable = _messengerInternal.EventTable;
    }

    public void AddListener(string eventType, Callback<T> handler)
    {
        _messengerInternal.OnListenerAdding(eventType, handler);
        _eventTable[eventType] = (Callback<T>)_eventTable[eventType] + handler;
    }

    public void RemoveListener(string eventType, Callback<T> handler)
    {
        _messengerInternal.OnListenerRemoving(eventType, handler);
        _eventTable[eventType] = (Callback<T>)_eventTable[eventType] - handler;
        _messengerInternal.OnListenerRemoved(eventType);
    }

    public void Broadcast(string eventType, T arg1)
    {
        Broadcast(eventType, arg1, MessengerInternal.DEFAULT_MODE);
    }

    public void Broadcast(string eventType, T arg1, MessengerMode mode)
    {
        _messengerInternal.OnBroadcasting(eventType, mode);
        Delegate d;
        if (_eventTable.TryGetValue(eventType, out d))
        {
            Callback<T> callback = d as Callback<T>;
            if (callback != null)
            {
                callback(arg1);
            }
            else
            {
                throw _messengerInternal.CreateBroadcastSignatureException(eventType);
            }
        }
    }
}
// Two parameters
public class Messenger<T, U>
{
    private Dictionary<string, Delegate> _eventTable;
    private MessengerInternal _messengerInternal;

    public Messenger()
    {
        _messengerInternal = new MessengerInternal();
        _eventTable = _messengerInternal.EventTable;
    }

    public void AddListener(string eventType, Callback<T, U> handler)
    {
        _messengerInternal.OnListenerAdding(eventType, handler);
        _eventTable[eventType] = (Callback<T, U>)_eventTable[eventType] + handler;
    }

    public void RemoveListener(string eventType, Callback<T, U> handler)
    {
        _messengerInternal.OnListenerRemoving(eventType, handler);
        _eventTable[eventType] = (Callback<T, U>)_eventTable[eventType] - handler;
        _messengerInternal.OnListenerRemoved(eventType);
    }

    public void Broadcast(string eventType, T arg1, U arg2)
    {
        Broadcast(eventType, arg1, arg2, MessengerInternal.DEFAULT_MODE);
    }

    public void Broadcast(string eventType, T arg1, U arg2, MessengerMode mode)
    {
        _messengerInternal.OnBroadcasting(eventType, mode);
        Delegate d;
        if (_eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U> callback = d as Callback<T, U>;
            if (callback != null)
            {
                callback(arg1, arg2);
            }
            else
            {
                throw _messengerInternal.CreateBroadcastSignatureException(eventType);
            }
        }
    }
}
// Three parameters
public class Messenger<T, U, V>
{
    private Dictionary<string, Delegate> _eventTable;
    private MessengerInternal _messengerInternal;

    public Messenger()
    {
        _messengerInternal = new MessengerInternal();
        _eventTable = _messengerInternal.EventTable;
    }

    public void AddListener(string eventType, Callback<T, U, V> handler)
    {
        _messengerInternal.OnListenerAdding(eventType, handler);
        _eventTable[eventType] = (Callback<T, U, V>)_eventTable[eventType] + handler;
    }

    public void RemoveListener(string eventType, Callback<T, U, V> handler)
    {
        _messengerInternal.OnListenerRemoving(eventType, handler);
        _eventTable[eventType] = (Callback<T, U, V>)_eventTable[eventType] - handler;
        _messengerInternal.OnListenerRemoved(eventType);
    }

    public void Broadcast(string eventType, T arg1, U arg2, V arg3)
    {
        Broadcast(eventType, arg1, arg2, arg3, MessengerInternal.DEFAULT_MODE);
    }

    public void Broadcast(string eventType, T arg1, U arg2, V arg3, MessengerMode mode)
    {
        _messengerInternal.OnBroadcasting(eventType, mode);
        Delegate d;
        if (_eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U, V> callback = d as Callback<T, U, V>;
            if (callback != null)
            {
                callback(arg1, arg2, arg3);
            }
            else
            {
                throw _messengerInternal.CreateBroadcastSignatureException(eventType);
            }
        }
    }
}

static public class MessengerGlobal<T>
{
    static private readonly Messenger<T> _messenger = new Messenger<T>();

    static public void AddListener(string eventType, Callback<T> handler)
    {
        _messenger.AddListener(eventType, handler);
    }

    static public void RemoveListener(string eventType, Callback<T> handler)
    {
        _messenger.RemoveListener(eventType, handler);
    }

    static public void Broadcast(string eventType, T arg1)
    {
        _messenger.Broadcast(eventType, arg1);
    }

    static public void Broadcast(string eventType, T arg1, MessengerMode mode)
    {
        _messenger.Broadcast(eventType, arg1, mode);
    }
}

static public class MessengerGlobal<T, U>
{
    static private readonly Messenger<T, U> _messenger = new Messenger<T, U>();

    static public void AddListener(string eventType, Callback<T, U> handler)
    {
        _messenger.AddListener(eventType, handler);
    }

    static public void RemoveListener(string eventType, Callback<T, U> handler)
    {
        _messenger.RemoveListener(eventType, handler);
    }

    static public void Broadcast(string eventType, T arg1, U arg2)
    {
        _messenger.Broadcast(eventType, arg1, arg2);
    }

    static public void Broadcast(string eventType, T arg1, U arg2, MessengerMode mode)
    {
        _messenger.Broadcast(eventType, arg1, arg2, mode);
    }
}

static public class MessengerGlobal<T, U, V>
{
    static private readonly Messenger<T, U, V> _messenger = new Messenger<T, U, V>();

    static public void AddListener(string eventType, Callback<T, U, V> handler)
    {
        _messenger.AddListener(eventType, handler);
    }

    static public void RemoveListener(string eventType, Callback<T, U, V> handler)
    {
        _messenger.RemoveListener(eventType, handler);
    }

    static public void Broadcast(string eventType, T arg1, U arg2, V arg3)
    {
        _messenger.Broadcast(eventType, arg1, arg2, arg3);
    }

    static public void Broadcast(string eventType, T arg1, U arg2, V arg3, MessengerMode mode)
    {
        _messenger.Broadcast(eventType, arg1, arg2, arg3, mode);
    }
}

