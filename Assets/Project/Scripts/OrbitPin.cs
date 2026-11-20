using UnityEngine;
using UnityEngine.Events;

namespace OrbitLanes
{
    public enum HitReaction { [InspectorName("Текст и искры")] Celebrate, [InspectorName("Только текст")] Message, [InspectorName("Только искры")] Sparks, [InspectorName("Только очки")] Score }
    [RequireComponent(typeof(Rigidbody))]
    public class OrbitPin : MonoBehaviour
    {
        [Range(.12f, 4f)] public float mass = .58f;
        [Range(1, 10)] public int points = 1;
        public HitReaction reaction;
        public string message = "Есть попадание!";
        public UnityEvent onKnocked = new UnityEvent();
        public ParticleSystem sparks;
        public bool Down { get; private set; }
        public bool Clearing => Down && gameObject.activeSelf;
        [Min(0)] public float clearDelay = .65f;
        [Min(.05f)] public float clearDuration = .35f;
        float knockedAt;
        Vector3 homeScale;
        bool clearingStarted;
        Vector3 home;
        Quaternion upright;
        Rigidbody body;
        OrbitClub club;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            home = body.position;
            upright = body.rotation;
            homeScale = transform.localScale;
            body.mass = mass;
            body.centerOfMass = new Vector3(0, .18f, 0);
        }
        void Start() => club = FindAnyObjectByType<OrbitClub>();
        void OnValidate() { if (TryGetComponent(out Rigidbody rb)) rb.mass = mass; }

        void FixedUpdate() => ClearIfFallen();
        public void ClearIfFallen()
        {
            if (Down || (Vector3.Dot(transform.up, upright * Vector3.up) > .67f && body.position.y > home.y - .22f)) return;
            Down = true;
            knockedAt = Time.time;
            if (!club || !club.Scoring) return;
            if (sparks && (reaction == HitReaction.Celebrate || reaction == HitReaction.Sparks)) sparks.Play();
            club.Record(this);
            onKnocked.Invoke();
        }
        void Update()
        {
            if (!Down || Time.time < knockedAt + clearDelay) return;
            if (!clearingStarted)
            {
                clearingStarted = true;
                body.detectCollisions = false;
                body.isKinematic = true;
            }
            float progress = (Time.time - knockedAt - clearDelay) / Mathf.Max(.05f, clearDuration);
            if (progress >= 1f) gameObject.SetActive(false);
            else transform.localScale = homeScale * Mathf.Max(.01f, 1f - Mathf.SmoothStep(0f, 1f, progress));
        }
        public void Stand()
        {
            Down = false;
            clearingStarted = false;
            transform.localScale = homeScale;
            gameObject.SetActive(true);
            body.isKinematic = false;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            body.position = home;
            body.rotation = upright;
            transform.SetPositionAndRotation(home, upright);
            body.detectCollisions = true;
            body.Sleep();
            if (sparks) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
