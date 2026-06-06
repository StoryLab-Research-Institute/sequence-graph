using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace StoryLabResearch.SequenceGraph
{
    [CustomEditor(typeof(SequenceGraphParser), true)]
    public class SequenceGraphParserEditor : Editor
    {
        private SequenceGraphParser sequenceParser;
        private bool removeSafely;

        public override void OnInspectorGUI()
        {
            if (sequenceParser.Sequence == null)
            {
                if (GUILayout.Button("New graph", GUILayout.Height(40)))
                {
                    Undo.RecordObject(sequenceParser, "Created Sequence Graph");
                    sequenceParser.Sequence = CreateInstance(typeof(SequenceGraph)) as SequenceGraph;
                    sequenceParser.Sequence.name = sequenceParser.name + " Graph";
                    sequenceParser.Sequence.AddNode<EntryNode>();
                }
            }
            else
            {
                if (GUILayout.Button("Open graph", GUILayout.Height(40)))
                {
                    NodeEditorWindow.Open(sequenceParser.Sequence);
                }
                if (removeSafely)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Really remove graph?");
                    GUI.color = new Color(1, 0.8f, 0.8f);
                    if (GUILayout.Button("Delete"))
                    {
                        removeSafely = false;
                        Undo.RecordObject(sequenceParser, "Removed Sequence Graph");
                        sequenceParser.Sequence = null;
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("Cancel"))
                    {
                        removeSafely = false;
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUI.color = new Color(1, 0.8f, 0.8f);
                    if (GUILayout.Button("Remove graph"))
                    {
                        removeSafely = true;
                    }
                    GUI.color = Color.white;
                }
            }
            DrawDefaultInspector();
        }

        private void OnEnable()
        {
            sequenceParser = target as SequenceGraphParser;
        }
    }
}
