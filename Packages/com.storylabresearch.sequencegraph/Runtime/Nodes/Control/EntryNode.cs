namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("")]
    public class EntryNode : BaseControlNode
    {
        protected override void Init() => name = "Entry";

        public override bool EvaluateNode() => true;
    }
}
