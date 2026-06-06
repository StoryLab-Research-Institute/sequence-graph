using UnityEngine;

namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Action Nodes/Destroy Objects")]
    public class DestroyObjectsNode : ActionNode
    {
        public GameObject[] objects;
        public override void OnTrigger()
        {
            foreach(GameObject go in objects)
            {
                Destroy(go);
            }
        }
    }
}
