using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{
    public Chunk chunk;
    public int currentBiome = 0; // Current biome
    public List<BiomeData> biomes = new List<BiomeData>();

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            if (UnityEditor.Selection.activeTransform == null)
                return;

            GameObject go = UnityEditor.Selection.activeTransform.gameObject;
            if (go.GetComponent<Chunk>())
            {
                Chunk chunk = go.GetComponent<Chunk>();
                chunk.GenerateColours();
            }
        }
#endif
    }
}

[System.Serializable]
public class BiomeData
{
    public string name = "Unknown Biome";
    public Gradient gradient;
    public NoiseSettings[] heightmapSettings = new NoiseSettings[1];
    public NoiseSettings[] moistureSettings = new NoiseSettings[1];
}