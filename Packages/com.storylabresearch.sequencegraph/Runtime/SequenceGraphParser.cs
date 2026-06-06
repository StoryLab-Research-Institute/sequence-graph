using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                    Stop();
                }
            }
        }

        public void Play()
        {
            _parsing = true;
        }

        public void Stop()
        {
            _parsing = false;
            _activeNode = Sequence.GetEntryNode();
        }

        public void Pause()
        {
            _parsing = false;
        }
    }
}
