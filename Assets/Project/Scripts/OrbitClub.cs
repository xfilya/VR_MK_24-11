using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitLanes
{
    public class OrbitClub : MonoBehaviour
    {
        [Range(1, 5)] public int throwsPerRound = 2;
        [Range(.5f, 5)] public float settleSeconds = 2.3f;
        [Range(0, .65f)] public float musicLevel = .17f;
        public TextMesh scoreLabel, turnLabel, feedback;
        public Transform launch;
        public OrbitBall practiceBall;
        public Transform ballStand;
        public AudioSource music, chime;
        public AudioClip pinTone, strikeTone;
        public OrbitTheme decor;
        public int Score { get; private set; }
        public int Throws { get; private set; }
        public int Round { get; private set; } = 1;
        public bool Scoring { get; private set; }
        bool muted;
        float lastChange;
        OrbitPin[] pins;
        OrbitBall[] balls;
        float rackEnd;

        void Start()
        {
            pins = FindObjectsByType<OrbitPin>();
            balls = FindObjectsByType<OrbitBall>();
            if (launch && pins.Length > 0)
                rackEnd = pins.Max(p => Vector3.Dot(p.transform.position - launch.position, launch.forward)) + .65f;
            if (!decor) decor = FindAnyObjectByType<OrbitTheme>();
            if (music) { music.volume = musicLevel; music.Play(); }
            Say("Возьми шар со стойки");
            Display();
        }
        public bool PastRack(Vector3 position) => launch && rackEnd > 0f &&
            Vector3.Dot(position - launch.position, launch.forward) > rackEnd;

        public bool CanTake(OrbitBall ball) => !Scoring && balls != null &&
            !balls.Any(other => other && other != ball && (other.Held || other.ReleasePending)) && !pins.Any(p => p && p.Clearing);

        public bool TryStartRoll(OrbitBall ball)
        {
            if (!CanTake(ball)) return false;
            Throws++;
            Scoring = true;
            lastChange = Time.time;
            Say("Шар в игре");
            Display();
            return true;
        }
        public void Record(OrbitPin pin)
        {
            if (!Scoring) return;
            Score += pin.points;
            lastChange = Time.time;
            bool clear = pins.Length > 0 && pins.All(p => !p || p.Down);
            if (clear) Say(Throws == 1 ? "СТРАЙК!" : "Все кегли сбиты!");
            else if (pin.reaction == HitReaction.Celebrate || pin.reaction == HitReaction.Message) Say(pin.message);
            var tone = clear ? strikeTone : pinTone;
            if (chime && tone) chime.PlayOneShot(tone, clear ? .9f : .32f);
            Display();
        }
        void Update()
        {
            if (music) music.volume = muted ? 0 : musicLevel;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                bool legacyKeys = true;
#if UNITY_EDITOR
                // Space and R control devices while the simulator is active.
                legacyKeys = !UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator.instance;
#endif
                if (keyboard.f7Key.wasPressedThisFrame || (legacyKeys && keyboard.spaceKey.wasPressedThisFrame)) Practice();
                if (keyboard.f8Key.wasPressedThisFrame || (legacyKeys && keyboard.rKey.wasPressedThisFrame))
                {
                    if (keyboard.shiftKey.isPressed) Restart();
                    else ClearLane();
                }
            }
            if (!Scoring || Time.time - lastChange < settleSeconds) return;
            if (pins.Any(p => p && p.Clearing)) return;
            bool clear = pins.Length > 0 && pins.All(p => !p || p.Down);
            if (!clear && balls.Any(b => b && b.InFlight)) return;
            Scoring = false;
            if (clear || Throws >= throwsPerRound)
            {
                Round++;
                Rerack();
                Say("Новый раунд");
            }
            else Say("Ещё один бросок");
            Display();
        }
        public void ClearLane()
        {
            // Return the ball. Fallen pins finish their normal clearance before play resumes.
            foreach (var ball in balls) if (ball) ball.Dock();
            foreach (var pin in pins) if (pin) pin.ClearIfFallen();
            if (Scoring) Say("Убираем дорожку");
        }
        void Rerack()
        {
            Throws = 0;
            foreach (var b in balls) if (b) b.Dock();
            foreach (var p in pins) if (p) p.Stand();
        }
        public void Restart()
        {
            Scoring = false;
            Rerack();
            Score = 0;
            Round = 1;
            Say("Возьми шар со стойки");
            Display();
        }
        public void Practice()
        {
            if (Scoring || !launch || balls == null || balls.Any(b => b && (b.InFlight || b.Held || b.ReleasePending))) return;
            var ball = practiceBall ? practiceBall : balls.FirstOrDefault(b => b);
            if (ball) ball.Practice(launch.position, launch.forward);
        }
        public void ChangeTheme() { if (decor) decor.Next(); }
        public void ToggleMusic() { muted = !muted; Say(muted ? "Музыка выключена" : "Музыка включена"); }
        public void Say(string text)
        {
            if (!feedback) return;
            feedback.text = (text ?? "").Replace('\n', ' ').Replace('\r', ' ');
            var size = feedback.GetComponent<MeshRenderer>().localBounds.size.x;
            feedback.transform.localScale = Vector3.one * (size > 4.5f ? 4.5f / size : 1f);
        }
        void Display()
        {
            if (scoreLabel) scoreLabel.text = Score.ToString("00");
            if (turnLabel) turnLabel.text = $"РАУНД  {Round:00}     /     БРОСОК  {Mathf.Min(Throws + (Scoring ? 0 : 1), throwsPerRound)} ИЗ {throwsPerRound}";
        }
    }
}
