using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Control Nodes/Wait For Condition")]
    public class WaitForConditionNode : ControlNode
    {
        [HideInInspector] public ConditionNode _internalConditionNode;
        [HideInInspector] public int _conditionIndex;

        public override bool EvaluateNode()
        {
            if (_internalConditionNode != null) return _internalConditionNode.TestCondition();
            Debug.LogWarning("No condition assigned on " + name + ", node will never exit");
            return false;

        }

        #if UNITY_EDITOR
        private static Type[] _conditionNodeTypes;
        private static string[] _conditionNodeTypeNames;

        protected override void Init()
        {
            if (_conditionNodeTypes == null || _conditionNodeTypeNames == null) ConditionNode.GetConditionNodeTypes(out _conditionNodeTypes, out _conditionNodeTypeNames);

            // get index from current condition type
            Type internalConditionNodeType = _internalConditionNode == null ? null : _internalConditionNode.GetType();
            bool found = false;
            for (_conditionIndex = 0; _conditionIndex < _conditionNodeTypes.Length; _conditionIndex++)
            {
                if (_conditionNodeTypes[_conditionIndex] == internalConditionNodeType)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                throw new Exception("Internal condition node type " + internalConditionNodeType + " requested by node " + name + " exists but is not available - has it been moved outside the namespace, or had its base class changed?");
            }
        }

        public void OnBodyGUI()
        {
            int index = EditorGUILayout.Popup("Condition", _conditionIndex, _conditionNodeTypeNames);
            if(_conditionIndex != index)
            {
                UpdateCondition(index);
            }
            if (_internalConditionNode != null)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                _internalConditionNode.OnBodyGUI();
            }
        }

        private void UpdateCondition(int index = 0)
        {
            if(_internalConditionNode != null) DestroyImmediate(_internalConditionNode);

            index = Mathf.Clamp(index, 0, _conditionNodeTypes.Length - 1);

            name = "Wait for " + _conditionNodeTypeNames[index];
            if (_conditionNodeTypes[index] != null)
            {
                _internalConditionNode = (ConditionNode)CreateInstance(_conditionNodeTypes[index]);
                _internalConditionNode.name = name + " internal " + _conditionNodeTypeNames[index] + " node";
            }
            _conditionIndex = index;
        }

        // Fixes bug caused through xNode duplication where duplicated node shares condition with original one
        // Editing one causes both to change otherwise
        public void CloneInternalConditionNode()
        {
            if (_internalConditionNode == null) return;

            string conditionName = _internalConditionNode.name;
            _internalConditionNode = Instantiate(_internalConditionNode);
            _internalConditionNode.name = conditionName;
        }
        #endif
    }
}
