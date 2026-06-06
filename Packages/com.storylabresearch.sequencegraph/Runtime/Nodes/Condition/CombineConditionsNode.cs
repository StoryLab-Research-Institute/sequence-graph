namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Condition Nodes/Combine Conditions")]
    public class CombineConditionsNode : ConditionNode
    {
        [Input(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict)] public bool Conditions;
        public ConditionCombinationMode Mode;

        public override bool TestCondition()
        {
            return CombinePortConditions(GetPort(nameof(Conditions)), Mode);
        }
    }
}
