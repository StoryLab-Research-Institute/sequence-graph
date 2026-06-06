using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace StoryLabResearch.SequenceGraph
{
    [Serializable] public class Control { };
    [Serializable] public class Trigger { };

    [NodeTint("#504B87")]
    public abstract class BaseControlNode : Node
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public Control Output;
        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)] public Trigger Trigger;

        // not actually going to use this value, there will only ever be one output on these nodes so we can simplify the whole dealio and write our own explicit EvaluateNode method
        public override object GetValue(NodePort port) => null;
        public abstract bool EvaluateNode();

        public BaseControlNode GetNextControlNode()
        {
            // The Output port carries at most one connection (ConnectionType.Override).
            NodePort port = GetOutputPort(nameof(Output));
            if (port == null || port.ConnectionCount == 0) return null;
            return port.GetConnection(0).node as BaseControlNode;
        }

        // The Trigger -> ActionNode list is fixed by the graph topology, which only
        // changes in the editor. GetActionNodes is called every frame a node is
        // evaluated (e.g. while a Wait node blocks), so we build the list once and
        // cache it rather than re-allocating a List + LINQ chain per frame.
        private List<ActionNode> _cachedActionNodes;

        public List<ActionNode> GetActionNodes()
        {
            if (_cachedActionNodes != null) return _cachedActionNodes;

            _cachedActionNodes = new List<ActionNode>();
            NodePort port = GetOutputPort(nameof(Trigger));
            if (port != null)
            {
                int count = port.ConnectionCount;
                for (int i = 0; i < count; i++)
                {
                    if (port.GetConnection(i).node is ActionNode actionNode) _cachedActionNodes.Add(actionNode);
                }
            }
            return _cachedActionNodes;
        }

        // Call if the graph topology changes at runtime (rare). The editor rebuilds
        // on domain reload anyway, so this is only needed for runtime graph edits.
        public void InvalidateActionNodeCache() => _cachedActionNodes = null;

        public virtual void EnterNode() { }
    }


    public abstract class ControlNode : BaseControlNode
    {
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)] public Control Input;
    }

    [NodeTint("#C15E9F")]
    public abstract class ActionNode : Node
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)] public Trigger Trigger;
        public abstract void OnTrigger();
    }

    public enum ConditionCombinationMode
    {
        All,
        Any,
        NotAll,
        None
    }

    [NodeTint("#30995F")]
    public abstract class ConditionNode : Node
    {
        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)] public bool Condition;
        public override object GetValue(NodePort port) => null;
        public abstract bool TestCondition();

        public virtual void OnBodyGUI() { }

        public static bool CombinePortConditions(NodePort port, ConditionCombinationMode mode)
        {
            // Called every frame while a Wait node blocks. Iterate the connections
            // directly and short-circuit where possible, rather than materialising a
            // List<bool> via LINQ each frame. Each connected condition is evaluated at
            // most once; the conditions themselves change frame-to-frame so they can't
            // be cached, but the surrounding machinery need not allocate.
            int count = port.ConnectionCount;

            if (count < 1)
            {
                Debug.LogWarning("No conditions connected to port " + port.fieldName + " on node " + port.node);
                return false;
            }

            switch (mode)
            {
                case ConditionCombinationMode.All:
                    for (int i = 0; i < count; i++)
                        if (!(port.GetConnection(i).node as ConditionNode).TestCondition()) return false;
                    return true;
                case ConditionCombinationMode.Any:
                    for (int i = 0; i < count; i++)
                        if ((port.GetConnection(i).node as ConditionNode).TestCondition()) return true;
                    return false;
                case ConditionCombinationMode.NotAll:
                    int trueCount = 0;
                    for (int i = 0; i < count; i++)
                        if ((port.GetConnection(i).node as ConditionNode).TestCondition()) trueCount++;
                    return trueCount < count - 1;
                case ConditionCombinationMode.None:
                    for (int i = 0; i < count; i++)
                        if ((port.GetConnection(i).node as ConditionNode).TestCondition()) return false;
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        #if UNITY_EDITOR
        private static string _namespaceName;
        public static string GetNamespaceName()
        {
            if (_namespaceName == null) _namespaceName = UnityEditor.ObjectNames.NicifyVariableName(typeof(ConditionNode).Namespace).Replace(".", "/") + "/";
            return _namespaceName;
        }

        public static void GetConditionNodeTypes(out Type[] types, out string[] typeNames)
        {
            var typesList = new List<Type>();
            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyTypes = domainAssembly.GetTypes()
                    .Where(type => type.IsSubclassOf(typeof(ConditionNode)) && !type.IsAbstract && !(type == typeof(CombineConditionsNode)));

                typesList.AddRange(assemblyTypes);
            }

            typesList = typesList.OrderBy(t => t.Name).ToList();
            typesList.Insert(0, null);
            types = typesList.ToArray();
            typeNames = types.Select(x => GetConditionNodeName(x)).ToArray();
        }

        public static string GetConditionNodeName(Type t)
        {
            if (t == null) return "None";
            return NodeDefaultPath(t).Replace(GetNamespaceName(), "").Replace(" Condition", "");
        }

        // Inlined from xNode's NodeEditorUtilities.NodeDefaultPath so this runtime
        // assembly need not reference the editor-only XNodeEditor assembly. Behaviour
        // is identical to the original call.
        private static string NodeDefaultPath(Type type)
        {
            string typePath = type.ToString().Replace('.', '/');
            if (typePath.EndsWith("Node")) typePath = typePath.Substring(0, typePath.LastIndexOf("Node"));
            typePath = UnityEditor.ObjectNames.NicifyVariableName(typePath);
            return typePath;
        }
        #endif
    }
}
