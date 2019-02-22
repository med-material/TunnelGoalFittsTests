using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
 
[CustomEditor(typeof(ReadDemo))]
public class ReadDemoEditor : Editor {
 
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ReadDemo myScript = (ReadDemo)target;
        if(GUILayout.Button("Read Configuration File"))
        {
            myScript.Read(); 
        }
    }

}