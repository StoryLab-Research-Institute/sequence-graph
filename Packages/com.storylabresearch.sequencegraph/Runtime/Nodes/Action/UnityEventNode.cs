using UnityEngine.Events;

namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Action Nodes/Unity Event")]
    public class UnityEventNode : ActionNode
    {
        public UnityEvent OnTriggered;
        public override void OnTrigger() => OnTriggered?.Invoke();
    }
}
