using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace OrbitLanes
{
    public enum SwitchAction { [InspectorName("Пробный бросок")] Bowl, [InspectorName("Начать заново")] Restart, [InspectorName("Сменить оформление")] Theme, [InspectorName("Вкл. / выкл. музыку")] Music, [InspectorName("Вывести надпись")] Message }
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class OrbitSwitch : MonoBehaviour
    {
        public SwitchAction action;
        public string message = "Привет, орбита!";
        public UnityEvent onPress = new UnityEvent();
        public Transform cap;
        public TextMesh legend;
        public AudioSource click;
        OrbitClub club;
        float last = -1;
        Vector3 rest;
        void Awake()
        {
            club = FindAnyObjectByType<OrbitClub>();
            UpdateLegend();
            if (cap) rest = cap.localPosition;
            GetComponent<XRSimpleInteractable>().selectEntered.AddListener(Select);
        }
        void Select(SelectEnterEventArgs args) => Press();
        public void UpdateLegend()
        {
            if (!legend) return;
            legend.text = action switch
            {
                SwitchAction.Bowl => "БРОСОК",
                SwitchAction.Restart => "СБРОС",
                SwitchAction.Theme => "ЦВЕТ",
                SwitchAction.Music => "ЗВУК",
                _ => "ТЕКСТ"
            };
        }
        public void Press()
        {
            if (Time.time - last < .35f) return;
            last = Time.time;
            if (click) click.Play();
            if (club)
                switch (action)
                {
                    case SwitchAction.Bowl: club.Practice(); break;
                    case SwitchAction.Restart: club.Restart(); break;
                    case SwitchAction.Theme: club.ChangeTheme(); break;
                    case SwitchAction.Music: club.ToggleMusic(); break;
                    case SwitchAction.Message: club.Say(message); break;
                }
            onPress.Invoke();
        }
        void Update()
        {
            if (cap) cap.localPosition = rest - Vector3.up * (.022f * Mathf.Clamp01(1 - (Time.time - last) * 5));
        }
    }
}
