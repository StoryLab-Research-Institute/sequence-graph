using UnityEngine;

namespace StoryLabResearch.SequenceGraph.Revive
{
    // Enables Objects when control reaches this node, holds them for Duration, then
    // disables them and passes control on - replacing the Unity Event / Wait For Time /
    // Unity Event trio it would otherwise take.
    //
    // Note that action nodes hung off the Trigger port fire when the hold *ends*, not
    // when it starts; that is how the parser drives every control node.
    [CreateNodeMenu("Control Nodes/Enable Objects For Duration")]
    public class EnableObjectsForDurationNode : ControlNode
    {
        public GameObject[] Objects;
        public float Duration;
        public TimeUnit TimeUnit;

        private bool _holding;
        private float _startTime;
        private float _durationSeconds;

        public override bool EvaluateNode()
        {
            // the parser recurses into the next node as soon as the previous one passes,
            // so beginning the hold lazily here still enables on the frame control arrives
            if (!_holding) BeginHold();

            if (Time.time - _startTime < _durationSeconds) return false;

            SetObjectsActive(false);
            _holding = false;
            return true;
        }

        private void BeginHold()
        {
            _durationSeconds = Duration * Mathf.Pow(60f, (int)TimeUnit);
            _startTime = Time.time;
            SetObjectsActive(true);
            _holding = true;
        }

        private void SetObjectsActive(bool active)
        {
            foreach (GameObject go in Objects) go.SetActive(active);
        }
    }
}
