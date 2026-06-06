namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Control Nodes/Wait For Conditions")]
    public class WaitForConditionsNode : ControlNode
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)] public bool Conditions;
        public ConditionCombinationMode Mode;

        public override bool EvaluateNode()
        {
            return ConditionNode.CombinePortConditions(GetInputPort(nameof(Conditions)), Mode);
        }
    }
}
