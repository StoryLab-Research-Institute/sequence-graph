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

        // Every field written here must be wrapped in Undo.RecordObject, and the write
        // must happen after the record. A graph node is an unowned ScriptableObject
        // serialised inline into the scene file: it is neither a persistent asset nor a
        // scene object, so EditorUtility.SetDirty does nothing for it and a plain field
        // write never marks the owning scene dirty. Unity then never offers to save the
        // scene and the edit is silently discarded on the next reload - which is exactly
        // how _behaviour used to unassign itself. Registering an undo is the only thing
        // that dirties the scene holding the graph.
        //
        // Note that when this node is the internal condition of a WaitForConditionNode,
        // "this" is that internal node, which is likewise unowned and likewise dirties
        // the scene when recorded.
        public override void OnBodyGUI()
        {
            EditorGUI.BeginChangeCheck();
            EventStartTime startTime = (EventStartTime)EditorGUILayout.EnumPopup("Start Time", _startTime);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Changed event start time");
                _startTime = startTime;
            }

            EditorGUI.BeginChangeCheck();
#pragma warning disable CS0618 // Type or member is obsolete
            Behaviour behaviour = (Behaviour)EditorGUILayout.ObjectField("Behaviour", _behaviour, typeof(Behaviour));
#pragma warning restore CS0618 // Type or member is obsolete
            if (EditorGUI.EndChangeCheck() && behaviour != _behaviour)
            {
                Undo.RecordObject(this, "Changed event behaviour");
                _behaviour = behaviour;
                // UpdateEvents rewrites _eventNameStrings, _eventIndex and
                // _eventFieldName; it runs inside the recorded block so those are
                // covered by the same undo entry.
                UpdateEvents(true);
            }

            EditorGUI.BeginChangeCheck();
            int eventIndex = EditorGUILayout.Popup("Event", _eventIndex, _eventNameStrings);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Changed event");
                _eventIndex = eventIndex;
                _eventFieldName = _eventNameStrings[_eventIndex];
            }
        }
#endif
    }
}
