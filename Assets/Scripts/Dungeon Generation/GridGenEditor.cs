using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(DungeonRenderer))]
public class GridGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonRenderer script = (DungeonRenderer)target;

        if (DrawDefaultInspector())
        {
            if (script.autoUpdate)
            {
                script.GenerateDungeon();
            }
        }

        if (GUILayout.Button("Update"))
        {
            script.GenerateDungeon();
        }
    }
}

#endif
