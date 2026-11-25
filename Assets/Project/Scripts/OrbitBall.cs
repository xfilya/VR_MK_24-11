using UnityEngine;
using Unity.XR.CoreUtils;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
#endif
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace OrbitLanes
{
    [RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
    public class OrbitBall : MonoBehaviour, IXRSelectFilter
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
        public bool ReleasePending { get; private set; }
        Rigidbody body;
        XRGrabInteractable grab;
        OrbitClub club;
        Vector3 home;
        Quaternion rotation;
        float releaseTime, nextSound;
        bool dockAfterCancel;
        Vector3 forwardThrowVelocity;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            grab = GetComponent<XRGrabInteractable>();
            grab.selectMode = InteractableSelectMode.Single;
            grab.useDynamicAttach = true;
            grab.matchAttachPosition = grab.matchAttachRotation = true;
            grab.forceGravityOnDetach = true;
            home = body.position;
            rotation = body.rotation;
            ApplySettings();
            body.useGravity = false;
            grab.selectEntered.AddListener(PickUp);
            grab.selectExited.AddListener(Release);
            grab.activated.AddListener(ThrowFromController);
        }
        void Start() => club = FindAnyObjectByType<OrbitClub>();
        void OnEnable() => grab.selectFilters.Add(this);
        void OnDisable() => grab.selectFilters.Remove(this);
        public bool canProcess => isActiveAndEnabled;
        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) =>
            forwardThrowVelocity == Vector3.zero && (Held || (!InFlight && (!club || club.CanTake(this))));
        void OnDestroy()
        {
            if (!grab) return;
            grab.selectEntered.RemoveListener(PickUp);
            grab.selectExited.RemoveListener(Release);
            grab.activated.RemoveListener(ThrowFromController);
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
            forwardThrowVelocity = Vector3.zero;
            ReleasePending = false;
            InFlight = false;
            dockAfterCancel = false;
            if (ribbon) { ribbon.emitting = false; ribbon.Clear(); }
        }
        void Release(SelectExitEventArgs args)
        {
            if (args.isCanceled)
            {
                forwardThrowVelocity = Vector3.zero;
                dockAfterCancel = true;
            }
            else ReleasePending = true;
        }
        void ThrowFromController(ActivateEventArgs args)
        {
            if (args.interactorObject is IXRSelectInteractor hand && grab.IsSelected(hand))
                AimForwardThrow(hand);
        }
        void AimForwardThrow(IXRSelectInteractor hand)
        {
            var origin = hand.transform.GetComponentInParent<XROrigin>();
            var aim = origin && origin.Camera ? origin.Camera.transform : hand.transform;
            var forward = Vector3.ProjectOnPlane(aim.forward, Vector3.up);
            if (forward.sqrMagnitude < .001f) forward = Vector3.Cross(aim.right, Vector3.up);
            forwardThrowVelocity = forward.normalized * (5.6f * speed) + Vector3.up * 1.1f;
        }
        void Update()
        {
            if (!Held) return;
#if UNITY_EDITOR
            var keys = Keyboard.current;
            if (XRInteractionSimulator.instance && keys != null &&
                (keys.enterKey.wasPressedThisFrame || keys.numpadEnterKey.wasPressedThisFrame))
                AimForwardThrow(grab.firstInteractorSelecting);
#endif
            // Leave XRI's activation callback before asking the manager to end the selection.
            if (forwardThrowVelocity != Vector3.zero)
                grab.interactionManager.SelectExit(grab.firstInteractorSelecting, (IXRSelectInteractable)grab);
        }
        void LateUpdate()
        {
            // XRI exits the first hand before it enters the second during a transfer.
            if (!ReleasePending) return;
            ReleasePending = false;
            if (!Held && !BeginRoll()) dockAfterCancel = true;
        }
        bool BeginRoll()
        {
            if (club && !club.TryStartRoll(this)) return false;
            releaseTime = Time.time;
            InFlight = true;
            body.useGravity = true;
            if (ribbon) { ribbon.Clear(); ribbon.emitting = true; }
            return true;
        }
        public void Practice(Vector3 from, Vector3 forward)
        {
            if (Held || InFlight || ReleasePending || !BeginRoll()) return;
            body.position = from;
            body.angularVelocity = Vector3.zero;
            body.linearVelocity = forward * (5.6f * speed);
        }
        public void Dock()
        {
            forwardThrowVelocity = Vector3.zero;
            ReleasePending = false;
            if (Held)
            {
                grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
                dockAfterCancel = true;
                return;
            }
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
            // XRI writes its tracked-hand release velocity in LateUpdate after detaching.
            if (forwardThrowVelocity != Vector3.zero)
            {
                body.linearVelocity = forwardThrowVelocity;
                body.angularVelocity = Vector3.zero;
                forwardThrowVelocity = Vector3.zero;
            }
            float age = Time.time - releaseTime;
            if ((club && club.PastRack(body.position)) || body.position.y < home.y - 3 ||
                age > 11 || (age > 1.7f && body.linearVelocity.sqrMagnitude < .05f)) Dock();
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
