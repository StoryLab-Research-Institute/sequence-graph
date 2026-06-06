using XNode;

namespace StoryLabResearch.SequenceGraph
{
    [RequireNode(typeof(EntryNode))]
    public class SequenceGraph : NodeGraph
    {
        public BaseControlNode GetEntryNode()
        {
            foreach (Node node in nodes) if (node is EntryNode) return node as BaseControlNode;
            throw new System.Exception("No entry node in this graph - how on earth has that happened?");
        }
    }
}
