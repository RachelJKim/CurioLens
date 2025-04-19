using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnchorManager))]
public class AnchorManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnchorManager manager = (AnchorManager)target;

        if (GUILayout.Button("Save Anchors to Disk"))
        {
            manager.SaveToDisk();
        }

        if (GUILayout.Button("Load Anchors from Disk"))
        {
            manager.LoadFromDisk();
        }
    }
}
