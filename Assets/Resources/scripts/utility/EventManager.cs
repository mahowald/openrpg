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
    private Dictionary<string, Events.Vector3Event> v3EventDictionary;
    private static Queue<UnityEvent> eventQueue;
    private static Queue<Tuple<Events.Vector3Event, Vector3>> v3EventQueue;
    

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
        {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }
        if ( v3EventDictionary == null)
        {
            v3EventDictionary = new Dictionary<string, Events.Vector3Event>();
        }
        if ( eventQueue == null)
        {
            eventQueue = new Queue<UnityEvent>();
        }
        if ( v3EventQueue == null)
        {
            v3EventQueue = new Queue<Tuple<Events.Vector3Event, Vector3>>();
        }
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
        while ( v3EventQueue.Count > 0 && eventsProcessed < EventBatchSize )
        {
            Tuple<Events.Vector3Event, Vector3> tuple = v3EventQueue.Dequeue();
            tuple.Item1.Invoke(tuple.Item2);
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


    public static void StartListening(string eventName, UnityAction<Vector3> listener)
    {
        Events.Vector3Event thisEvent = null;
        if (instance.v3EventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new Events.Vector3Event();
            thisEvent.AddListener(listener);
            instance.v3EventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<Vector3> listener)
    {
        if (eventManager == null) return;
        Events.Vector3Event thisEvent = null;
        if (instance.v3EventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
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

    // Events that don't take any variables and are triggered by name
    public static void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            eventQueue.Enqueue(thisEvent);
        }
    }

    // Events that take a Vector3 argument and are triggered by name
    public static void TriggerEvent(string eventName, Vector3 argument)
    {
        Events.Vector3Event thisEvent = null;
        if (instance.v3EventDictionary.TryGetValue(eventName, out thisEvent))
        {
            v3EventQueue.Enqueue(new Tuple<Events.Vector3Event, Vector3>(thisEvent, argument));
        }
    }
}