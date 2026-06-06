using UnityEngine;

namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Action Nodes/Fog")]
    public class FogNode : ActionNode
    {
        [SerializeField] private Color color;
        [SerializeField] private float density;
        [SerializeField] private FogMode fogMode;

        public override void OnTrigger()
        {
            RenderSettings.fogColor = color;
            RenderSettings.fogDensity = density;
            RenderSettings.fogMode = fogMode;
        }
    }
}
