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

        var pistol = (Pistol)target;
        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.Toggle("Is Bullet In Chamber", pistol.IsBulletInChamber);
            EditorGUILayout.ObjectField("Current Mag", pistol.CurrentMag, typeof(PistolMag), true);
            if (pistol.CurrentMag != null)
                EditorGUILayout.IntField("Bullet Amount", pistol.CurrentMag.AmmoAmount);
        }

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
        {
            if (GUILayout.Button("Shoot"))
                pistol.Shoot();
            if (GUILayout.Button("ReleaseMag"))
                pistol.ReleaseMag();
        }
    }
}
