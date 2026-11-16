using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PistolMag))]
public class PistolMagEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.IntField("Ammo Amount", ((PistolMag)target).AmmoAmount);

        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            var mag = (PistolMag)target;
            if (GUILayout.Button("EjectBullet"))
                mag.EjectBullet();
            if (GUILayout.Button("AddBullet"))
                mag.AddBullet();
        }
    }
}

[CustomEditor(typeof(Pistol))]
public class PistolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("ReleaseMag"))
                ((Pistol)target).ReleaseMag();
        }
    }
}
