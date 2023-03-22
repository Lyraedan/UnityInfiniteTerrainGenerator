using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Biome))]
[CanEditMultipleObjects]
public class BiomeEditor : Editor
{
    SerializedProperty chunk;

    private void OnEnable()
    {
        chunk = serializedObject.FindProperty("chunk");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Biome biome = (Biome) target;
        Chunk chunk = biome.chunk;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            if (chunk != null && chunk.generator != null)
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
