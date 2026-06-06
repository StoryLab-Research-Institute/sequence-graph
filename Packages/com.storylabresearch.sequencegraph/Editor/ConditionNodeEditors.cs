using XNodeEditor;

namespace StoryLabResearch.SequenceGraph
{
    // These thin NodeEditor wrappers used to live alongside their nodes in the
    // runtime files. They derive from XNodeEditor.NodeEditor (editor-only), so with
    // an asmdef boundary in place they must live in the editor assembly. Each simply
    // forwards to the runtime node's own OnBodyGUI().

    [CustomNodeEditor(typeof(TimeConditionNode))]
    public class TimeConditionNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            base.OnBodyGUI();
            (target as TimeConditionNode).OnBodyGUI();
        }
    }

    [CustomNodeEditor(typeof(UnityEventConditionNode))]
    public class UnityEventConditionNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            base.OnBodyGUI();
            (target as UnityEventConditionNode).OnBodyGUI();
        }
    }

    [CustomNodeEditor(typeof(WaitForConditionNode))]
    public class WaitForConditionNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            base.OnBodyGUI();
            (target as WaitForConditionNode).OnBodyGUI();
        }
    }
}
