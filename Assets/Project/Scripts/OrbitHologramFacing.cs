using UnityEngine;

namespace OrbitLanes
{
    [DisallowMultipleComponent]
    public sealed class OrbitHologramFacing : MonoBehaviour
    {
        OrbitView view;

        void Start() => view = FindAnyObjectByType<OrbitView>();
        void OnEnable() => Application.onBeforeRender += FacePlayer;
        void OnDisable() => Application.onBeforeRender -= FacePlayer;
        void LateUpdate() => FacePlayer();

        void FacePlayer()
        {
            // The tracked camera transform is shared by both eyes.
            Camera viewer = view ? view.headset : null;
            if (!viewer || !viewer.isActiveAndEnabled)
                viewer = view && view.spectator ? view.spectator : Camera.main;
            if (!viewer) return;

            // A Unity quad faces local -Z. Keep the projection upright when the player crouches.
            Vector3 away = Vector3.ProjectOnPlane(transform.position - viewer.transform.position, Vector3.up);
            if (away.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(away, Vector3.up);
        }
    }
}
