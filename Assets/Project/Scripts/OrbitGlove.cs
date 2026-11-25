using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitLanes
{
    public class OrbitGlove : MonoBehaviour
    {
        public InputActionReference grip, gripPress, trigger;
        public Transform[] knuckles, tips;
        Quaternion[] basePose, tipPose;
        float curl, point;
#if UNITY_EDITOR
        [System.NonSerialized] internal float[] simulatedFingerCurls;
#endif
        void Awake()
        {
            basePose = new Quaternion[knuckles.Length];
            tipPose = new Quaternion[tips.Length];
            for (int i = 0; i < knuckles.Length; i++) { basePose[i] = knuckles[i].localRotation; tipPose[i] = tips[i].localRotation; }
        }
        void LateUpdate()
        {
            float squeeze = grip ? grip.action.ReadValue<float>() : 0f;
            if (gripPress && gripPress.action.IsPressed()) squeeze = 1f;
            curl = Mathf.MoveTowards(curl, squeeze, Time.deltaTime * 12);
            point = Mathf.MoveTowards(point, trigger ? trigger.action.ReadValue<float>() : 0, Time.deltaTime * 12);
            for (int i = 0; i < knuckles.Length; i++)
            {
                float bend = i == 0 ? Mathf.Max(point, curl) : curl;
#if UNITY_EDITOR
                if (simulatedFingerCurls != null && i < simulatedFingerCurls.Length) bend = simulatedFingerCurls[i];
#endif
                if (i == 4)
                {
                    // The thumb crosses the folded fingers from opposite sides on the two hands.
                    float side = Mathf.Sign(knuckles[i].localPosition.x);
                    Quaternion foldedBase = Quaternion.Euler(50f, -23f * side, -38f * side);
                    Quaternion foldedTip = Quaternion.Inverse(foldedBase) * Quaternion.Euler(12f, -58f * side, -55f * side);
                    knuckles[i].localRotation = Quaternion.Slerp(basePose[i], foldedBase, bend);
                    tips[i].localRotation = Quaternion.Slerp(tipPose[i], foldedTip, bend);
                    continue;
                }
                knuckles[i].localRotation = basePose[i] * Quaternion.Euler(80f * bend, 0, 0);
                tips[i].localRotation = tipPose[i] * Quaternion.Euler(76f * bend, 0, 0);
            }
        }
    }
}
