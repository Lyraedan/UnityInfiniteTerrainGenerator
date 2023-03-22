using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSettings : MonoBehaviour
{
    public int textureResolution = 50;
    public Texture2D texture;
    public Material baseMaterial;
    public Material material;
    public Biome biome;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if(Application.isEditor)
        {
            if (UnityEditor.Selection.activeTransform == null)
                return;

            GameObject go = UnityEditor.Selection.activeTransform.gameObject;
            if(go.GetComponent<Chunk>())
            {
                Chunk chunk = go.GetComponent<Chunk>();
                chunk.GenerateColours();
            }
        }
#endif
    }
}
