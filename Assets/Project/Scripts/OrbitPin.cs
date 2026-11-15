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
        Vector3 home;
        Quaternion upright;
        Rigidbody body;
        OrbitClub club;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            home = body.position;
            upright = body.rotation;
            body.mass = mass;
            body.centerOfMass = new Vector3(0, .18f, 0);
        }
        void Start() => club = FindAnyObjectByType<OrbitClub>();
        void OnValidate() { if (TryGetComponent(out Rigidbody rb)) rb.mass = mass; }

        void FixedUpdate()
        {
            if (Down || !club || !club.Scoring) return;
            if (Vector3.Dot(transform.up, upright * Vector3.up) > .67f && body.position.y > home.y - .22f) return;
            Down = true;
            if (sparks && (reaction == HitReaction.Celebrate || reaction == HitReaction.Sparks)) sparks.Play();
            club.Record(this);
            onKnocked.Invoke();
        }
        public void Stand()
        {
            Down = false;
            body.linearVelocity = body.angularVelocity = Vector3.zero;
            body.position = home;
            body.rotation = upright;
            body.Sleep();
            if (sparks) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}