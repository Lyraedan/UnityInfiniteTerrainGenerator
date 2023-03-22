using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(Chunk))]
[CanEditMultipleObjects]
public class ChunkEditor : Editor
{
    SerializedProperty biomeSettings;

    private void OnEnable()
    {
        biomeSettings = serializedObject.FindProperty("biomeSettings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Chunk chunk = (Chunk)target;

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(biomeSettings);
        bool update = GUILayout.Button("Update");
        if(update)
        {
            if (chunk.generator != null)
            {
                Debug.Log("Update!");
                chunk.UpdateChunk();
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (chunk.generator != null)
            {
                Debug.Log("Update!");
                chunk.UpdateChunk();
            }
        }
        EditorGUILayout.Vector3Field("Worldspace", chunk.worldSpace);

        //DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }
}
