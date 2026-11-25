#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace OrbitLanes
{
    // Hand playback and physical controllers share the lane's grab interactions and glove art.
    internal static class OrbitSimulatorHand
    {
        static readonly XRHandFingerID[] Fingers = {
            XRHandFingerID.Index, XRHandFingerID.Middle, XRHandFingerID.Ring,
            XRHandFingerID.Little, XRHandFingerID.Thumb
        };

        internal static GameObject Create(GameObject controller, bool left)
        {
            if (!controller) return null;
            var source = controller.GetComponentInChildren<OrbitGlove>(true);
            var interactor = controller.GetComponentInChildren<NearFarInteractor>(true);
            if (!source || !interactor || !source.grip) return null;

            string side = left ? "Left" : "Right";
            string device = "<XRHandDevice>{" + side + "Hand}/";
            InputActionProperty Pose(string control) => new InputActionProperty(
                new InputAction(type: InputActionType.Value, binding: device + control));

            var root = new GameObject(side + " Simulated Hand");
            root.SetActive(false);
            root.transform.SetParent(controller.transform.parent, false);
            var driver = root.AddComponent<TrackedPoseDriver>();
            driver.positionInput = Pose("aimPosition");
            driver.rotationInput = Pose("aimRotation");
            driver.trackingStateInput = Pose("aimTrackingState");

            var handInteractor = Object.Instantiate(interactor, root.transform, false);
            var select = new XRInputButtonReader("Select with hand", inputSourceMode: XRInputButtonReader.InputSourceMode.ManualValue);
            // The recorded poses provide analog strengths even when the binary touch flags are absent.
            handInteractor.selectInput = select;
            handInteractor.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.State;
            var glove = Object.Instantiate(source, root.transform, false);
            var curls = new float[Fingers.Length];
            glove.simulatedFingerCurls = curls;
            var tracking = root.AddComponent<XRHandTrackingEvents>();
            tracking.handedness = left ? Handedness.Left : Handedness.Right;
            tracking.updateType = XRHandTrackingEvents.UpdateTypes.Dynamic;
            foreach (var mesh in glove.GetComponentsInChildren<Renderer>(true))
            {
                if (!mesh.enabled) continue;
                var visibility = mesh.gameObject.AddComponent<XRHandMeshController>();
                visibility.handTrackingEvents = tracking;
                visibility.handMeshRenderer = mesh;
            }
            tracking.jointsUpdated.AddListener(update =>
            {
                var tracked = left ? XRHandDevice.leftHand : XRHandDevice.rightHand;
                float strength = tracked == null ? 0f : Mathf.Max(tracked.pinchValue.ReadValue(), tracked.graspValue.ReadValue());
                bool holding = strength >= (select.manualPerformed ? .65f : .85f);
                if (holding && !select.manualPerformed) select.manualFramePerformed = Time.frameCount;
                if (!holding && select.manualPerformed) select.manualFrameCompleted = Time.frameCount;
                select.manualPerformed = holding;
                select.manualValue = strength;
                for (int i = 0; i < Fingers.Length; i++)
                {
                    var shape = update.hand.CalculateFingerShape(Fingers[i], XRFingerShapeTypes.FullCurl);
                    if (shape.TryGetFullCurl(out float curl)) curls[i] = curl;
                }
            });
            return root;
        }
    }
}
#endif
