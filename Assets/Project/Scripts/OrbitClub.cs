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
        bool nextRack, muted;
        float lastChange;
        OrbitPin[] pins;
        OrbitBall[] balls;

        void Start()
        {
            pins = FindObjectsByType<OrbitPin>();
            balls = FindObjectsByType<OrbitBall>();
            if (music) { music.volume = musicLevel; music.Play(); }
            Display();
        }
        public void RollStarted()
        {
            if (nextRack) Rerack(false);
            Throws++;
            Scoring = true;
            lastChange = Time.time;
            Say("Шар в игре");
            Display();
        }
        public void Record(OrbitPin pin)
        {
            Score += pin.points;
            lastChange = Time.time;
            bool clear = pins.Length > 0 && pins.All(p => !p || p.Down);
            if (clear) Say(Throws == 1 ? "СТРАЙК!" : "Все кегли сбиты!");
            else if (pin.reaction == HitReaction.Celebrate || pin.reaction == HitReaction.Message) Say(pin.message);
            if (chime) chime.PlayOneShot(clear ? strikeTone : pinTone, clear ? .9f : .32f);
            Display();
        }
        void Update()
        {
            if (music) music.volume = muted ? 0 : musicLevel;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) Practice();
                if (keyboard.rKey.wasPressedThisFrame) Restart();
            }
            if (!Scoring || Time.time - lastChange < settleSeconds) return;
            bool clear = pins.Length > 0 && pins.All(p => !p || p.Down);
            if (!clear && balls.Any(b => b && b.InFlight)) return;
            Scoring = false;
            nextRack = clear || Throws >= throwsPerRound;
            if (!clear) Say(nextRack ? "Новая расстановка" : "Ещё один бросок");
            if (nextRack) { Rerack(true); }
            Display();
        }
        void Rerack(bool returnBalls)
        {
            Round++;
            Throws = 0;
            nextRack = false;
            foreach (var p in pins) if (p) p.Stand();
            if (returnBalls) foreach (var b in balls) if (b) b.Dock();
        }
        public void Restart()
        {
            Scoring = false;
            Rerack(true);
            Score = 0;
            Round = 1;
            Say("Возьми шар боковой кнопкой");
            Display();
        }
        public void Practice()
        {
            if (!launch || balls == null || balls.Any(b => b && (b.InFlight || b.Held))) return;
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
