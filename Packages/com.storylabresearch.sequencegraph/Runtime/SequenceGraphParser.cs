using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;

namespace StoryLabResearch.SequenceGraph
{
    public class SequenceGraphParser : MonoBehaviour
    {
        public SequenceGraph Sequence;

        private BaseControlNode _activeNode;
        private bool _parsing;

        public int recursionLimit = 1023;
        private int _recursions;

        public bool ParseOnEnable;
        public bool ResetOnDisable;

        public UnityEvent OnGraphCompleted;

#if UNITY_EDITOR
        [MenuItem("StoryLabResearch/Sequence Graph")]
        private static void AddParserToScene()
        {
            GameObject go = new();
            go.name = "=== SEQUENCE GRAPH PARSER ===";
            go.AddComponent<SequenceGraphParser>();
            Undo.RegisterCreatedObjectUndo(go, "Added sequence graph parser");
        }
#endif

        private void Awake()
        {
            if (Sequence == null)
            {
                // The graph is a ScriptableObject serialised into the scene. A prefab
                // asset cannot hold one, so a parser authored in Prefab Mode reaches
                // play mode with a null Sequence. Say why, rather than throwing a bare
                // NullReferenceException out of Stop().
                Debug.LogError(
                    "SequenceGraphParser on \"" + name + "\" has no Sequence Graph and will not run. " +
                    "A Sequence Graph is a ScriptableObject held in the scene, and Unity cannot store " +
                    "one inside a prefab - if this parser was authored in Prefab Mode, the graph was " +
                    "discarded when the prefab closed. Sequence Graph parsers must live directly in a " +
                    "scene; use Unity Timeline for sequencing that has to ship inside a prefab.", this);
                return;
            }

            Stop();
        }

        private void OnEnable()
        {
            if (ParseOnEnable) Play();
        }

        private void OnDisable()
        {
            if (ResetOnDisable) Stop();
        }

        private void Update()
        {
            if (_parsing)
            {
                _recursions = 0;
                EvaluateRecursively();
            }
        }

        private void EvaluateRecursively()
        {
            if (_recursions++ > recursionLimit)
            {
                Debug.LogWarning("we appear to be in an infinite loop, aborting after " + _recursions + " recursions");
                return;
            }

            if (_activeNode.EvaluateNode())
            {
                // do the actions
                foreach (ActionNode actionNode in _activeNode.GetActionNodes()) actionNode.OnTrigger();

                // move to the next control node
                _activeNode = _activeNode.GetNextControlNode();
                if (_activeNode != null)
                {
                    // continue evaluation until we reach something which makes us wait
                    EvaluateRecursively();
                }
                else
                {
                    // or if there isn't another control node, stop
                    Debug.Log("Reached the end of SequenceGraph on " + name);
                    OnGraphCompleted?.Invoke();
                    Stop();
                }
            }
        }

        public void Play()
        {
            // Awake has already reported the missing graph; stay quiet and inert here.
            if (Sequence == null) return;

            // Covers a Sequence assigned after Awake, which would otherwise leave
            // _activeNode null and fault on the first Update.
            if (_activeNode == null) Stop();

            _parsing = true;
        }

        public void Stop()
        {
            _parsing = false;
            if (Sequence != null) _activeNode = Sequence.GetEntryNode();
        }

        public void Pause()
        {
            _parsing = false;
        }
    }
}
