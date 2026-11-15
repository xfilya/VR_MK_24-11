using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitLanes
{
    public class OrbitGlove : MonoBehaviour
    {
        public InputActionReference grip, trigger;
        public Transform[] knuckles, tips;
        Quaternion[] basePose, tipPose;
        float curl, point;
        void Awake()
        {
            basePose = new Quaternion[knuckles.Length];
            tipPose = new Quaternion[tips.Length];
            for (int i = 0; i < knuckles.Length; i++) { basePose[i] = knuckles[i].localRotation; tipPose[i] = tips[i].localRotation; }
        }
        void LateUpdate()
        {
            curl = Mathf.MoveTowards(curl, grip ? grip.action.ReadValue<float>() : 0, Time.deltaTime * 12);
            point = Mathf.MoveTowards(point, trigger ? trigger.action.ReadValue<float>() : 0, Time.deltaTime * 12);
            for (int i = 0; i < knuckles.Length; i++)
            {
                float bend = i == 0 ? Mathf.Max(point, curl * .6f) : curl;
                knuckles[i].localRotation = basePose[i] * Quaternion.Euler(66 * bend, 0, 0);
                tips[i].localRotation = tipPose[i] * Quaternion.Euler(72 * bend, 0, 0);
            }
        }
    }
}