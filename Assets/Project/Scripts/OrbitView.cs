using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
#endif

namespace OrbitLanes
{
    public class OrbitView : MonoBehaviour
    {
        public Camera spectator, headset;
        public AudioListener spectatorEars, headsetEars;
        readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
        float nextCheck;
#if UNITY_EDITOR
        // The sample remains an editor dependency even if its prefab is moved or renamed.
        const string SimulatorGuid = "58d0a4ac86f2348deb02f3880c71378e";
        XRInteractionSimulator simulator;
        bool ownsSimulator;
        bool reportedMissingPrefab;
        GameObject leftHand, rightHand;
        readonly List<(InputAction action, System.Guid id)> gripBindings = new();

        void AddGripKey(GameObject controller, string key)
        {
            var glove = controller ? controller.GetComponentInChildren<OrbitGlove>(true) : null;
            if (!glove) return;
            foreach (var reference in new[] { glove.gripPress, glove.grip })
            {
                if (!reference || reference.action == null) continue;
                var binding = reference.action.AddBinding(key);
                gripBindings.Add((reference.action, binding.binding.id));
            }
        }

        void RemoveGripKeys()
        {
            foreach (var (action, id) in gripBindings)
            {
                var binding = action.ChangeBindingWithId(id);
                if (binding.valid) binding.Erase();
            }
            gripBindings.Clear();
        }

        void ReleaseSimulationInputs()
        {
            RemoveGripKeys();
            if (leftHand) { leftHand.SetActive(false); Destroy(leftHand); }
            if (rightHand) { rightHand.SetActive(false); Destroy(rightHand); }
        }

        void OnDestroy() => ReleaseSimulationInputs();
#endif

        void Update()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + .25f;
            SubsystemManager.GetSubsystems(displays);
            bool vr = displays.Exists(d => d.running);
            foreach (var device in InputSystem.devices)
            {
                if (device is not XRHMD hmd || !hmd.isTracked.isPressed) continue;
#if UNITY_EDITOR
                if (hmd is XRSimulatedHMD) continue;
#endif
                vr = true;
                break;
            }
#if UNITY_EDITOR
            if (vr && ownsSimulator && simulator)
            {
                simulator.gameObject.SetActive(false);
                ReleaseSimulationInputs();
                Destroy(simulator.gameObject);
                simulator = null;
                ownsSimulator = false;
            }
            if (!vr && !simulator && !reportedMissingPrefab)
            {
                simulator = FindAnyObjectByType<XRInteractionSimulator>();
                if (!simulator)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(SimulatorGuid));
                    if (prefab)
                    {
                        simulator = Instantiate(prefab, transform).GetComponent<XRInteractionSimulator>();
                        simulator.cameraTransform = headset.transform;
                        ownsSimulator = true;
                        var rig = headset.GetComponentInParent<XRInputModalityManager>();
                        if (rig)
                        {
                            AddGripKey(rig.leftController, "<Keyboard>/f");
                            AddGripKey(rig.rightController, "<Keyboard>/b");
                            if (!rig.leftHand) rig.leftHand = leftHand = OrbitSimulatorHand.Create(rig.leftController, true);
                            if (!rig.rightHand) rig.rightHand = rightHand = OrbitSimulatorHand.Create(rig.rightController, false);
                            if (rig.leftHand) simulator.leftHandAimTransform = rig.leftHand.transform;
                            if (rig.rightHand) simulator.rightHandAimTransform = rig.rightHand.transform;
                        }
                    }
                    else
                    {
                        Debug.LogError("Отсутствует префаб XR Interaction Simulator из Samples XR Interaction Toolkit.", this);
                        reportedMissingPrefab = true;
                    }
                }
            }
            vr |= simulator && simulator.isActiveAndEnabled;
#endif
            spectator.enabled = spectatorEars.enabled = !vr;
            headset.enabled = headsetEars.enabled = vr;
        }
    }
}
