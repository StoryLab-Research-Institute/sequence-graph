using UnityEngine;

namespace StoryLabResearch.SequenceGraph
{
    [CreateNodeMenu("Action Nodes/Debug Log")]
    public class DebugLogNode : ActionNode
    {
        public enum LogModeEnum
        {
            Log,
            Warning,
            Error
        }

        [NodeEnum] public LogModeEnum LogMode;
        public string text;
        public override void OnTrigger()
        {
            switch(LogMode)
            {
                case LogModeEnum.Warning:
                    Debug.LogWarning(text);
                    break;
                case LogModeEnum.Error:
                    Debug.LogError(text);
                    break;
                default:
                    Debug.Log(text);
                    break;
            }
        }
    }
}
