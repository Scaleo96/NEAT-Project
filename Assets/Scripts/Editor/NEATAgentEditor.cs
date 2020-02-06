using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(NEATAgent))]
public class NEATAgentEditor : Editor {

    Vector3 newPos;
    bool newNet;
    Brain brain;

    public override void OnInspectorGUI() {
        var componet = (NEATAgent)target;

        if(componet.newNetowrk) {
            base.OnInspectorGUI();
        } else {

            EditorGUI.BeginChangeCheck();
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Load settings");
            EditorStyles.label.fontStyle = FontStyle.Normal;
            newNet = EditorGUILayout.Toggle("Dont load brain", componet.newNetowrk);
            brain = EditorGUILayout.ObjectField(componet.brain, typeof(Brain), true) as Brain;

            if(EditorGUI.EndChangeCheck()) {
                componet.newNetowrk = newNet;
                componet.brain = brain;
                EditorUtility.SetDirty(componet);
            }

        }
    }

    private void OnSceneGUI() {
        var comp = (NEATAgent)target;

        EditorGUI.BeginChangeCheck();

        newPos = Handles.PositionHandle((Vector3)comp.offset + comp.transform.position, Quaternion.identity);

        if(EditorGUI.EndChangeCheck()) {
            comp.offset = newPos - comp.transform.position;
            EditorUtility.SetDirty(comp);
        }
    }
}
