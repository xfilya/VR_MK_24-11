using UnityEngine;

namespace OrbitLanes
{
    public enum SkyStyle { [InspectorName("Мандариновая орбита")] Orbit, [InspectorName("Полярное сияние")] Aurora, [InspectorName("Лиловые сумерки")] Dusk }
    public class OrbitTheme : MonoBehaviour
    {
        public SkyStyle theme;
        public Renderer[] panels, accents, planets;
        public Material[] panelColors, accentColors, planetColors;
        public void Apply()
        {
            Paint(panels, panelColors);
            Paint(accents, accentColors);
            Paint(planets, planetColors);
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