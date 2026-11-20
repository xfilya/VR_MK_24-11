using UnityEngine;

namespace OrbitLanes
{
    public enum SkyStyle { [InspectorName("Мандариновая орбита")] Orbit, [InspectorName("Полярное сияние")] Aurora, [InspectorName("Лиловые сумерки")] Dusk }
    public class OrbitTheme : MonoBehaviour
    {
        public SkyStyle theme;
        public Renderer[] panels, accents, planets;
        public Material[] panelColors, accentColors, planetColors;
        public Material[] skyboxes;
        public void Apply()
        {
            Paint(panels, panelColors);
            Paint(accents, accentColors);
            Paint(planets, planetColors);
            int index = (int)theme;
            if (gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene() &&
                skyboxes != null && index >= 0 && index < skyboxes.Length && skyboxes[index])
                RenderSettings.skybox = skyboxes[index];
        }
        void Paint(Renderer[] objects, Material[] colors)
        {
            int i = (int)theme;
            if (objects == null || colors == null || colors.Length <= i) return;
            foreach (var r in objects) if (r) r.sharedMaterial = colors[i];
        }
        void Start() => Apply();
        public void Next() { theme = (SkyStyle)(((int)theme + 1) % 3); Apply(); }
    }
}
