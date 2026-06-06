using UnityEngine;

namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Action Nodes/Skybox Material")]
    public class SkyboxMaterialNode : ActionNode
    {
        [SerializeField] private Material material;

        public override void OnTrigger()
        {
            RenderSettings.skybox = material;
        }
    }
}
