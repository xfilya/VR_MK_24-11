using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace OrbitLanes
{
    [RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
    public class OrbitBall : MonoBehaviour
    {
        [Range(.5f, 3f)] public float speed = 1.45f;
        [Range(0, 7)] public float punch = 1.1f;
        [Range(.5f, 8)] public float mass = 2.8f;
        [Range(.05f, 1.2f)] public float trailTime = .32f;
        public TrailRenderer ribbon;
        public AudioSource voice;
        public AudioClip impact;
        public bool InFlight { get; private set; }
        public bool Held => grab.isSelected;
        Rigidbody body;
        XRGrabInteractable grab;
        OrbitClub club;
        Vector3 home;
        Quaternion rotation;
        float releaseTime, nextSound;
        bool dockAfterCancel;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            grab = GetComponent<XRGrabInteractable>();
            home = body.position;
            rotation = body.rotation;
            ApplySettings();
            body.useGravity = false;
            grab.selectEntered.AddListener(PickUp);
            grab.selectExited.AddListener(Release);
        }
        void Start() => club = FindAnyObjectByType<OrbitClub>();
        void OnDestroy()
        {
            if (!grab) return;
            grab.selectEntered.RemoveListener(PickUp);
            grab.selectExited.RemoveListener(Release);
        }
        void OnValidate() => ApplySettings();
        void ApplySettings()
        {
            if (TryGetComponent(out Rigidbody rb)) rb.mass = mass;
            if (TryGetComponent(out XRGrabInteractable xr)) xr.throwVelocityScale = speed;
            if (ribbon) ribbon.time = trailTime;
        }
        void PickUp(SelectEnterEventArgs args)
        {
            InFlight = false;
            dockAfterCancel = false;
            if (ribbon) { ribbon.emitting = false; ribbon.Clear(); }
        }
        void Release(SelectExitEventArgs args)
        {
            if (args.isCanceled) dockAfterCancel = true;
            else BeginRoll();
        }
        void BeginRoll()
        {
            releaseTime = Time.time;
            InFlight = true;
            body.useGravity = true;
            if (ribbon) { ribbon.Clear(); ribbon.emitting = true; }
            if (club) club.RollStarted();
        }
        public void Practice(Vector3 from, Vector3 forward)
        {
            if (Held || InFlight) return;
            body.position = from;
            body.angularVelocity = Vector3.zero;
            BeginRoll();
            body.linearVelocity = forward * (5.6f * speed);
        }
        public void Dock()
        {
            if (Held) return;
            dockAfterCancel = false;
            InFlight = false;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.position = home;
            body.rotation = rotation;
            body.Sleep();
            if (ribbon) { ribbon.emitting = false; ribbon.Clear(); }
        }
        void FixedUpdate()
        {
            if (dockAfterCancel && !Held) Dock();
            if (!InFlight || Held) return;
            float age = Time.time - releaseTime;
            if (body.position.y < home.y - 3 || age > 11 || (age > 1.7f && body.linearVelocity.sqrMagnitude < .05f)) Dock();
        }
        void OnCollisionEnter(Collision collision)
        {
            if (!InFlight || collision.relativeVelocity.sqrMagnitude < .15f) return;
            var target = collision.rigidbody;
            if (target && target.TryGetComponent(out OrbitPin pin))
            {
                Vector3 direction = Vector3.ProjectOnPlane(target.worldCenterOfMass - body.worldCenterOfMass, Vector3.up).normalized;
                target.AddForceAtPosition((direction + Vector3.up * .36f) * punch, collision.GetContact(0).point, ForceMode.Impulse);
            }
            if (voice && impact && Time.time > nextSound)
            {
                voice.pitch = target ? Random.Range(.90f, 1.18f) : .65f;
                voice.PlayOneShot(impact, target ? .8f : .25f);
                nextSound = Time.time + .1f;
            }
        }
    }
}
