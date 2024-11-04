using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public class EventManager : MonoBehaviour
    {
        private static readonly Dictionary<string, Action> EventDictionary = new Dictionary<string, Action>();

        public static void StartListening(string eventName, Action listener)
        {
            if (EventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent += listener;
                EventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                EventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action listener)
        {
            if (EventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent -= listener;
                EventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName)
        {
            if (EventDictionary.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent?.Invoke();
            }
        }
    }
}