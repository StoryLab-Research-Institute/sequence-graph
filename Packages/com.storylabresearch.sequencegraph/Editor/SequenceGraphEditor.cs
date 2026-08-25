using System;
using XNodeEditor;

namespace StoryLabResearch.SequenceGraph
{
    [CustomNodeGraphEditor(typeof(SequenceGraph))]
    public class SequenceGraphEditor : NodeGraphEditor
    {
        public override string GetNodeMenuName(Type type)
        {
            string menuName = "";
            string namespaceName = UnityEditor.ObjectNames.NicifyVariableName(GetType().Namespace).Replace(".", "/") + "/";

            if(!string.IsNullOrEmpty(type.Namespace) && type.Namespace.Contains(GetType().Namespace))
            {
                menuName = base.GetNodeMenuName(type);
                menuName = menuName.Replace(namespaceName, "");
            }
            else if (type == typeof(XNode.NodeGroups.NodeGroup))
            {
                menuName = base.GetNodeMenuName(type);
            }

            return menuName;
        }

        public override void OnDropObjects(UnityEngine.Object[] objects)
        {
            // do nothing
        }

        public override XNode.Node CopyNode(XNode.Node original)
        {
            XNode.Node node = base.CopyNode(original);
            if (node is WaitForConditionNode waitForConditionNode) waitForConditionNode.CloneInternalConditionNode();
            return node;
        }
    }
}
