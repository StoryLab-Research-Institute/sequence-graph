using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StoryLabResearch.SequenceGraph
{
    public enum TimeUnit
    {
        Seconds,
        Minutes,
        Hours
    }

    public enum StartTime
    {
        NodeActive,
        SceneLoaded,
        GameLoaded
    }

    [CreateNodeMenu("Condition Nodes/Time")]
    public class TimeConditionNode : ConditionNode
    {
        // we are hiding these so that we can draw it in a custom inspector which can be accessed from a Wait For node
        // this is so we can ensure the inspectors match, and we don't need duplicate fields on the Wait For node
        [HideInInspector] public StartTime StartTime;
        [HideInInspector] public float Duration;
        private float _durationSeconds;
        [HideInInspector] public TimeUnit TimeUnit;

        private bool _initialised;
        private float _startTime;

        public override bool TestCondition()
        {
            if (!_initialised) Initialise();

            return Time.time - _startTime >= _durationSeconds;
        }

        private void Initialise()
        {
            _durationSeconds = Duration * Mathf.Pow(60f, (int)TimeUnit);

            switch (StartTime)
            {
                case StartTime.NodeActive:
                    _startTime = Time.time;
                    break;
                case StartTime.SceneLoaded:
                    _startTime = Time.time - Time.timeSinceLevelLoad;
                    break;
            }

            _initialised = true;
        }

        #if UNITY_EDITOR
        public override void OnBodyGUI()
        {
            StartTime = (StartTime)EditorGUILayout.EnumPopup("Start Time", StartTime);
            Duration = EditorGUILayout.FloatField("Duration", Duration);
            TimeUnit = (TimeUnit)EditorGUILayout.EnumPopup("Time Unit", TimeUnit);
        }
        #endif
    }
}
