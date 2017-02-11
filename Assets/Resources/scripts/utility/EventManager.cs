using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using Tuples;

// From the Unity tutorial: https://unity3d.com/learn/tutorials/topics/scripting/events-creating-simple-messaging-system
public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> eventDictionary;
    private Queue<UnityEvent> eventQueue;
    private Dictionary<Type, Dictionary<string, Events.IOneArgEvent>> oneArgEventDictionary;
    private Queue<Tuple<Type, Events.IOneArgEvent, object>> oneArgEventQueue;
    private Dictionary<Tuple<Type, Type>, Dictionary<string, Events.ITwoArgEvent>> twoArgEventDictionary;
    private Queue<Tuple<Tuple<Type, Type>, Events.ITwoArgEvent, Tuple<object, object>>> twoArgEventQueue;

    private const int EventBatchSize = 500;

    private static EventManager eventManager;

    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }

            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
            eventDictionary = new Dictionary<string, UnityEvent>();
        if ( eventQueue == null)
            eventQueue = new Queue<UnityEvent>();
        if (oneArgEventDictionary == null)
            oneArgEventDictionary = new Dictionary<Type, Dictionary<string, Events.IOneArgEvent>>();
        if (oneArgEventQueue == null)
            oneArgEventQueue = new Queue<Tuple<Type, Events.IOneArgEvent, object>>();
        if (twoArgEventDictionary == null)
            twoArgEventDictionary = new Dictionary<Tuple<Type, Type>, Dictionary<string, Events.ITwoArgEvent>>();
        if (twoArgEventQueue == null)
            twoArgEventQueue = new Queue<Tuple<Tuple<Type, Type>, Events.ITwoArgEvent, Tuple<object, object>>>();
        
    }
    
    void Update()
    {
        int eventsProcessed = 0;
        while( eventQueue.Count > 0 && eventsProcessed < EventBatchSize )
        {
            UnityEvent thisEvent = eventQueue.Dequeue();
            thisEvent.Invoke();
            eventsProcessed += 1;
        }
        while(oneArgEventQueue.Count > 0 && eventsProcessed < EventBatchSize)
        {
            Tuple<Type, Events.IOneArgEvent, object> tuple = oneArgEventQueue.Dequeue();
            tuple.Item2.Invoke(tuple.Item3);
            eventsProcessed += 1;
        }
        while(twoArgEventQueue.Count > 0 && eventsProcessed < EventBatchSize)
        {
            Tuple<Tuple<Type, Type>, Events.ITwoArgEvent, Tuple<object, object>> tuple = twoArgEventQueue.Dequeue();
            tuple.Item2.Invoke(tuple.Item3.Item1, tuple.Item3.Item2);
            eventsProcessed += 1;
        }
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }
    
    public static void StartListening<T>(string eventName, UnityAction<T> listener)
    {
        Events.IOneArgEvent thisEvent = null;
        Type mytype = typeof(T);
        if (instance.oneArgEventDictionary.ContainsKey(mytype))
        {
            if (instance.oneArgEventDictionary[mytype].TryGetValue(eventName, out thisEvent))
            {
                Events.GenericEvent<T> myEvent = (Events.GenericEvent<T>)(thisEvent);
                myEvent.AddListener(listener);
            }
            else
            {
                Events.GenericEvent<T> myEvent = new Events.GenericEvent<T>();
                myEvent.AddListener(listener);
                instance.oneArgEventDictionary[mytype].Add(eventName, myEvent);
            }
        }
        else
        {
            instance.oneArgEventDictionary.Add(mytype, new Dictionary<string, Events.IOneArgEvent>());
            Events.GenericEvent<T> myEvent = new Events.GenericEvent<T>();
            myEvent.AddListener(listener);
            instance.oneArgEventDictionary[mytype].Add(eventName, myEvent);
        }
    }


    public static void StartListening<T1, T2>(string eventName, UnityAction<T1, T2> listener)
    {
        Events.ITwoArgEvent thisEvent = null;
        Type mytype1 = typeof(T1);
        Type mytype2 = typeof(T2);
        Tuple<Type, Type> key = new Tuple<Type, Type>(mytype1, mytype2);

        if (instance.twoArgEventDictionary.ContainsKey(key))
        {
            if (instance.twoArgEventDictionary[key].TryGetValue(eventName, out thisEvent))
            {
                Events.GenericEvent<T1, T2> myEvent = (Events.GenericEvent<T1, T2>)(thisEvent);
                myEvent.AddListener(listener);
            }
            else
            {
                Events.GenericEvent<T1, T2> myEvent = new Events.GenericEvent<T1, T2>();
                myEvent.AddListener(listener);
                instance.twoArgEventDictionary[key].Add(eventName, myEvent);
            }
        }
        else
        {
            instance.twoArgEventDictionary.Add(key, new Dictionary<string, Events.ITwoArgEvent>());
            Events.GenericEvent<T1, T2> myEvent = new Events.GenericEvent<T1, T2>();
            myEvent.AddListener(listener);
            instance.twoArgEventDictionary[key].Add(eventName, myEvent);
        }
    }

    public static void StopListening<T>(string eventName, UnityAction<T> listener)
    {
        if (eventManager == null) return;
        Events.IOneArgEvent thisEvent = null;
        Type mytype = typeof(T);
        if (instance.oneArgEventDictionary.ContainsKey(mytype))
        {
            if(instance.oneArgEventDictionary[mytype].TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }
    }

    public static void StopListening<T1, T2>(string eventName, UnityAction<T1, T2> listener)
    {
        if (eventManager == null) return;
        Events.ITwoArgEvent thisEvent = null;
        Tuple<Type, Type> mytype = new Tuple<Type, Type>(typeof(T1), typeof(T2));
        if (instance.twoArgEventDictionary.ContainsKey(mytype))
        {
            if (instance.twoArgEventDictionary[mytype].TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }
    }

    public static void TriggerEvent<T>(string eventName, T argument)
    {
        Events.IOneArgEvent thisEvent = null;
        Type mytype = typeof(T);
        if(instance.oneArgEventDictionary.ContainsKey(mytype))
        {
            if(instance.oneArgEventDictionary[mytype].TryGetValue(eventName, out thisEvent))
            {
                // thisEvent.Invoke(argument);
                instance.oneArgEventQueue.Enqueue(new Tuple<Type, Events.IOneArgEvent, object>(mytype, thisEvent, argument));
            }
        }
    }

    public static void TriggerEvent<T1, T2>(string eventName, T1 argument1, T2 argument2)
    {
        Events.ITwoArgEvent thisEvent = null;
        Tuple<Type, Type> mytype = new Tuple<Type, Type>(typeof(T1), typeof(T2));
        if (instance.twoArgEventDictionary.ContainsKey(mytype))
        {
            if (instance.twoArgEventDictionary[mytype].TryGetValue(eventName, out thisEvent))
            {
                // ahhhhh!
                instance.twoArgEventQueue.Enqueue(new Tuple<Tuple<Type, Type>, Events.ITwoArgEvent, Tuple<object, object>>(mytype, thisEvent, new Tuple<object, object>(argument1, argument2)));
            }
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        if (eventManager == null) return;
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }
    
    public static void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            instance.eventQueue.Enqueue(thisEvent);
        }
    }
}