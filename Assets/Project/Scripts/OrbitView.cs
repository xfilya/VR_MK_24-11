using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using System.Collections.Generic;

namespace OrbitLanes
{
    // A fixed spectator view for the classroom computer until a headset starts tracking.
    public class OrbitView : MonoBehaviour
    {
        public Camera spectator, headset;
        public AudioListener spectatorEars, headsetEars;
        readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
        float nextCheck;
        void Update()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + .25f;
            SubsystemManager.GetSubsystems(displays);
            bool vr = displays.Exists(d => d.running);
            if (!vr)
                foreach (var device in InputSystem.devices)
                    if (device is XRHMD hmd && hmd.isTracked.isPressed) { vr = true; break; }
            spectator.enabled = spectatorEars.enabled = !vr;
            headset.enabled = headsetEars.enabled = vr;
        }
    }
}