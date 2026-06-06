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
            BaseControlNode result = null;
            try
            {
                result = GetOutputPort(nameof(Output)).GetConnections()[0].node as BaseControlNode;
            }
            catch
            {
                Debug.Log("No output node for " + name);
            }
            return result;
        }

        public List<ActionNode> GetActionNodes()
        {
            var port = GetOutputPort(nameof(Trigger));
            var connections = port.GetConnections();
            var nodes = connections.Select(x => x.node as ActionNode).ToList();
            return nodes;
        }

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
            var conditions = port.GetConnections().Select(x => (x.node as ConditionNode).TestCondition()).ToList();

            if (conditions == null || conditions.Count < 1)
            {
                Debug.LogWarning("No conditions connected to port " + port.fieldName + " on node " + port.node);
                return false;
            }

            switch (mode)
            {
                case ConditionCombinationMode.All:
                    bool acc = true;
                    foreach (bool condition in conditions) acc &= condition;
                    return acc;
                case ConditionCombinationMode.Any:
                    foreach (bool condition in conditions) if (condition) return true;
                    return false;
                case ConditionCombinationMode.NotAll:
                    int trueCount = 0;
                    foreach (bool condition in conditions) if (condition) trueCount++;
                    return trueCount < conditions.Count - 1;
                case ConditionCombinationMode.None:
                    foreach (bool condition in conditions) if (condition) return false;
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
