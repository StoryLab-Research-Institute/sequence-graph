using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using XNodeEditor;

namespace StoryLabResearch.SequenceGraph
{
    [CustomEditor(typeof(SequenceGraphParser), true)]
    public class SequenceGraphParserEditor : Editor
    {
        // A SequenceGraph is a ScriptableObject held by reference from the parser.
        // Unity serialises a referenced ScriptableObject instance inline into a scene
        // file, but a prefab asset cannot hold one. Authoring a graph in Prefab Mode
        // therefore appears to work and is then discarded without warning when the
        // prefab is closed. Rather than let that happen, block the graph controls
        // entirely in a prefab context and explain why. See the package README.
        private const string PREFAB_ASSET_ERROR =
            "<b>A Sequence Graph cannot live inside a prefab.</b>\n\n" +
            "The graph is a ScriptableObject held in the scene. Unity can serialise one into a " +
            "scene file but not into a prefab asset, so a graph authored here is discarded " +
            "without warning as soon as the prefab is closed.\n\n" +
            "Put this parser directly into a scene instead. If the sequencing has to ship inside " +
            "the prefab, use Unity Timeline - it keeps its scene references in a binding table on " +
            "the PlayableDirector rather than in the asset, which is why it can do what this " +
            "cannot.";

        // A prefab instance in a scene is a softer case: the graph reference is stored
        // as a scene override, so it works, but it can never be applied back to the
        // prefab asset. Warn instead of blocking.
        private const string PREFAB_INSTANCE_WARNING =
            "This parser is on a prefab instance. The graph is held as a scene override, so it " +
            "works in this scene, but it cannot be applied back to the prefab asset - \"Apply All\" " +
            "will discard it. Prefer a plain GameObject in the scene.";

        private const string REMOVE_CONFIRMATION =
            "Delete this graph? It is held in the scene, not in an asset file, so it cannot be " +
            "recovered afterwards.";

        private static readonly Color ErrorTint = new(1f, 0.45f, 0.45f);
        private static GUIStyle _errorBoxStyle;

        private SequenceGraphParser sequenceParser;
        private bool removeSafely;

        public override void OnInspectorGUI()
        {
            if (IsPartOfPrefabAsset(sequenceParser.gameObject))
            {
                DrawErrorBox(PREFAB_ASSET_ERROR);
                DrawDefaultInspector();
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(sequenceParser.gameObject))
            {
                EditorGUILayout.HelpBox(PREFAB_INSTANCE_WARNING, MessageType.Warning);
            }

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
                    EditorGUILayout.HelpBox(REMOVE_CONFIRMATION, MessageType.Warning);
                    GUILayout.BeginHorizontal();
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

        /// <summary>
        /// True when this GameObject belongs to a prefab asset, whether selected in the
        /// Project window or opened in Prefab Mode. Prefab Mode contents live in a
        /// preview scene, where IsPartOfPrefabAsset reports false, so both checks are
        /// needed to cover the case that actually loses people's work.
        /// </summary>
        private static bool IsPartOfPrefabAsset(GameObject go)
        {
            if (go == null) return false;
            return PrefabUtility.IsPartOfPrefabAsset(go)
                || PrefabStageUtility.GetPrefabStage(go) != null;
        }

        private static void DrawErrorBox(string message)
        {
            _errorBoxStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                wordWrap = true,
                richText = true,
                padding = new RectOffset(10, 10, 10, 10)
            };

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = ErrorTint;
            GUILayout.Label(message, _errorBoxStyle);
            GUI.backgroundColor = previousBackground;
            EditorGUILayout.Space();
        }

        private void OnEnable()
        {
            sequenceParser = target as SequenceGraphParser;
            removeSafely = false;
        }
    }
}
