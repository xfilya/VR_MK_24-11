using UnityEngine;

namespace OrbitLanes
{
    // TextMesh uses a dynamic font atlas. Keep it bound when Unity rebuilds the glyphs.
    [ExecuteAlways, RequireComponent(typeof(TextMesh))]
    public class OrbitWorldText : MonoBehaviour
    {
        public Material depthMaterial;
        MaterialPropertyBlock properties;
        void OnEnable() { Font.textureRebuilt += AtlasChanged; Refresh(); }
        void OnDisable() => Font.textureRebuilt -= AtlasChanged;
        void AtlasChanged(Font font) { if (GetComponent<TextMesh>().font == font) Refresh(); }
        public void Refresh()
        {
            var label = GetComponent<TextMesh>();
            if (!depthMaterial || !label.font) return;
            var renderer = GetComponent<MeshRenderer>();
            renderer.sharedMaterial = depthMaterial;
            properties ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            properties.SetTexture("_MainTex", label.font.material.mainTexture);
            renderer.SetPropertyBlock(properties);
        }
    }
}
