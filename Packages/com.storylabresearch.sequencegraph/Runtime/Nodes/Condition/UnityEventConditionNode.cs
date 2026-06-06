using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StoryLabResearch.SequenceGraph
{
    public enum EventStartTime
    {
        NodeActive,
        SceneLoaded
    }

    [CreateNodeMenu("Condition Nodes/Unity Event")]
    public class UnityEventConditionNode : ConditionNode
    {
        private const string NO_EVENT_STRING = "None";
        private readonly bool DEBUG_LOG_EVENT_CACHE = false;

        // we are hiding these so that we can draw it in a custom inspector which can be accessed from a Wait For node
        // this is so we can ensure the inspectors match, and we don't need duplicate fields on the Wait For node
        [HideInInspector] public EventStartTime _startTime;
        [HideInInspector] public Behaviour _behaviour;

        private bool _listenerInitialised;

        private UnityEvent _event;
        private bool _eventFired;
        [HideInInspector] public string _eventFieldName;

#if UNITY_EDITOR
        [HideInInspector] public string[] _eventNameStrings;
        [HideInInspector] public int _eventIndex;

        // cache type string names so we don't have to look them up every time we open a dropdown
        private static Dictionary<Type, string[]> _cachedTypeEvents;
        private static bool _cacheInitialised;
#endif

        protected override void Init()
        {
#if UNITY_EDITOR
            // init runs whenever all scripts are recompiled, which means we can use it to check for events existing

            // initialise the cache on the first UnityEventConditionNode to init
            // we have to reinitialise it after compilation because the cached values may not match the current event names
            if (!_cacheInitialised)
            {
                _cachedTypeEvents = new Dictionary<Type, string[]>();
                _cacheInitialised = true;
                if (DEBUG_LOG_EVENT_CACHE) Debug.Log("Condition node type cache reinitialised");
            }

            UpdateEvents(false);
#endif

            if (_startTime == EventStartTime.SceneLoaded) InitialiseEventListener();
        }

        public override bool TestCondition()
        {
            if (!_listenerInitialised) InitialiseEventListener();
            return _eventFired;
        }

        private void ListenerMethod()
        {
            Debug.Log("Node " + name + " received event " + _eventFieldName + " from behaviour " + _behaviour.GetType() + " on GameObject " + _behaviour.gameObject.name);
            _eventFired = true;
            _event.RemoveListener(ListenerMethod);
        }

        private void InitialiseEventListener()
        {
            if (_behaviour == null)
            {
                Debug.LogWarning("No behaviour assigned on " + name + ", condition will never be true");
            }
            else if (_eventFieldName == NO_EVENT_STRING)
            {
                Debug.LogWarning("No event assigned on " + name + ", condition will never be true");
            }
            else
            {
                FieldInfo info = _behaviour.GetType().GetField(_eventFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (info != null) _event = info.GetValue(_behaviour) as UnityEvent;
                else _event = null;

                if (_event == null) Debug.LogWarning("Event " + _eventFieldName + " not found on " + name + ", condition will never be true");
                else _event.AddListener(ListenerMethod);
            }

            _listenerInitialised = true;
        }

#if UNITY_EDITOR
        public void UpdateEvents(bool manuallyReassigned = false)
        {
            if (_behaviour != null)
            {
                var type = _behaviour.GetType();
                if (_cachedTypeEvents.ContainsKey(type))
                {
                    _eventNameStrings = _cachedTypeEvents[type];
                    if(DEBUG_LOG_EVENT_CACHE) Debug.Log(type.Name + " loaded from type event list cache");
                }
                else
                {
                    var TypeFields = type.GetTypeInfo().GetFields();
                    var eventNameStringsList = new List<string>();
                    TypeFields.Where(f => f.FieldType.IsSubclassOf(typeof(UnityEventBase))).ToList().ForEach(x => eventNameStringsList.Add(x.Name));
                    eventNameStringsList.Insert(0, NO_EVENT_STRING);
                    _eventNameStrings = eventNameStringsList.ToArray();

                    _cachedTypeEvents.Add(type, _eventNameStrings);

                    if (DEBUG_LOG_EVENT_CACHE) Debug.Log(type.Name + " added to type event list cache");
                }
            }
            else
            {
                _eventNameStrings = new string[] { NO_EVENT_STRING };
            }

            // get index from current event name
            if (_behaviour == null)
            {
                _eventIndex = 0;
            }
            else
            {
                bool found = false;
                for (_eventIndex = 0; _eventIndex < _eventNameStrings.Length; _eventIndex++)
                {
                    if (_eventNameStrings[_eventIndex] == _eventFieldName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (!manuallyReassigned) Debug.LogWarning("Events in behaviour " + _behaviour.GetType() + " on GameObject " + _behaviour.name + " have changed, and no longer contain an event named " + _eventFieldName + " required by node " + name);
                    _eventIndex = 0;
                }
            }
            _eventFieldName = _eventNameStrings[_eventIndex];
        }

        public override void OnBodyGUI()
        {
            _startTime = (EventStartTime)EditorGUILayout.EnumPopup("Start Time", _startTime);

            Behaviour lastBehaviour = _behaviour;
#pragma warning disable CS0618 // Type or member is obsolete
            _behaviour = (Behaviour)EditorGUILayout.ObjectField("Behaviour", _behaviour, typeof(Behaviour));
#pragma warning restore CS0618 // Type or member is obsolete
            if (_behaviour != lastBehaviour) UpdateEvents(true);

            _eventIndex = EditorGUILayout.Popup("Event", _eventIndex, _eventNameStrings);
            _eventFieldName = _eventNameStrings[_eventIndex];
        }
#endif
    }
}
