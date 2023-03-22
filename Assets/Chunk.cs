using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    public MeshGenerator generator;
    public Biome biome;

    public NoiseSettings[] biomeSettings = new NoiseSettings[1];

    [HideInInspector] public Vector3 worldSpace = Vector3.zero;

    public BiomeData CurrentBiome
    {
        get
        {
            return generator.colorSettings.biome.biomes[generator.colorSettings.biome.currentBiome];
        }
    }

    public void Initialize()
    {
        generator = GetComponent<MeshGenerator>();
        generator.Initialize();
        biome = GetComponent<Biome>();
    }

    public void GenerateChunk()
    {
        DateTime before = DateTime.Now;
        generator.GeneratePlane(transform.position.x, transform.position.z);
        ApplyBiome();
        ApplyHeight();
        ApplyMoisture();
        ApplyFoliage();
        GenerateColours();
        generator.Refresh();
        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log($"Generated a {CurrentBiome.name} biome in {duration.Milliseconds}ms");
    }

    public void UpdateChunk()
    {
        ApplyBiome();
        ApplyHeight();
        ApplyMoisture();
        ApplyFoliage();
        GenerateColours();
        generator.Refresh();
    }

    public void Refresh()
    {
        GenerateColours();
        generator.Refresh();
    }

    public void ApplyBiome()
    {
        float average = 1000f;
        for (int z = 0; z < MeshGenerator.resolution.y; z++)
        {
            for (int x = 0; x < MeshGenerator.resolution.x; x++)
            {
                float noise = CalculateHeight(x, z, NoiseSettings.NoiseMap.TEMPERATURE, biomeSettings);
                float noise2 = CalculateHeight(x + 1, z, NoiseSettings.NoiseMap.TEMPERATURE, biomeSettings);
                float noise3 = CalculateHeight(x, z + 1, NoiseSettings.NoiseMap.TEMPERATURE, biomeSettings);
                float noise4 = CalculateHeight(x + 1, z + 1, NoiseSettings.NoiseMap.TEMPERATURE, biomeSettings);

                float avg = (noise + noise2 + noise3 + noise4) / 4;

                if (avg < average)
                {
                    average = avg;
                }
            }
        }

        Debug.Log("Chunk " + this.name + " : " + CurrentBiome.name + " avg: " + average);
        generator.colorSettings.biome.currentBiome = average < 10.1f ? 0 : 1;
    }

    public void ApplyHeight()
    {
        // Generate the heightmap
        NoiseSettings[] heightmapSettings = CurrentBiome.heightmapSettings;
        for (int z = 0; z < MeshGenerator.resolution.y; z++)
        {
            for (int x = 0; x < MeshGenerator.resolution.x; x++)
            {
                float noise = CalculateHeight(x, z, NoiseSettings.NoiseMap.HEIGHT, heightmapSettings);
                float noise2 = CalculateHeight(x + 1, z, NoiseSettings.NoiseMap.HEIGHT, heightmapSettings);
                float noise3 = CalculateHeight(x, z + 1, NoiseSettings.NoiseMap.HEIGHT, heightmapSettings);
                float noise4 = CalculateHeight(x + 1, z + 1, NoiseSettings.NoiseMap.HEIGHT, heightmapSettings);

                float avg = noise + noise2 + noise3 + noise4 / 4;

                int index = (int)z * MeshGenerator.resolution.x + (int)x;
                generator.UpdateHeights(index, new float[] { noise, noise2, noise3, noise4 });
            }
        }
    }

    public void ApplyMoisture()
    {
        // Generate water layer
        NoiseSettings[] moistureSettings = CurrentBiome.moistureSettings;
        for (int z = 0; z < MeshGenerator.resolution.y; z++)
        {
            for (int x = 0; x < MeshGenerator.resolution.x; x++)
            {
                float noise = CalculateHeight(x, z, NoiseSettings.NoiseMap.MOISTURE, moistureSettings);
                float noise2 = CalculateHeight(x + 1, z, NoiseSettings.NoiseMap.MOISTURE, moistureSettings);
                float noise3 = CalculateHeight(x, z + 1, NoiseSettings.NoiseMap.MOISTURE, moistureSettings);
                float noise4 = CalculateHeight(x + 1, z + 1, NoiseSettings.NoiseMap.MOISTURE, moistureSettings);

                float avg = noise + noise2 + noise3 + noise4 / 4;

                int index = (int)z * MeshGenerator.resolution.x + (int)x;
            }
        }
    }

    public void ApplyFoliage()
    {
        // Spawn trees, grass, rocks etc
    }

    float CalculateHeight(float x, float z, NoiseSettings.NoiseMap noiseType, NoiseSettings[] noiseSettings)
    {
        float sample = 0;
        float offset = 5000.0f;
        float amplitude = 1;

        float chunkX = (worldSpace.x * 2) + MeshGenerator.resolution.x;
        float chunkZ = (worldSpace.z * 2) + MeshGenerator.resolution.y;

        int seed = WorldGenerator.instance.seed;

        for (int i = 0; i < noiseSettings.Length; i++)
        {
            if (noiseSettings[i].enabled)
            {
                switch (noiseSettings[i].map)
                {
                    case NoiseSettings.NoiseMap.HEIGHT:
                        seed = WorldGenerator.instance.seed;
                        break;
                    case NoiseSettings.NoiseMap.TEMPERATURE:
                        seed = WorldGenerator.instance.temperatureSeed;
                        break;
                    case NoiseSettings.NoiseMap.MOISTURE:
                        seed = WorldGenerator.instance.moistureSeed;
                        break;
                    default:
                        seed = WorldGenerator.instance.seed;
                        break;
                }

                float frequancy = noiseSettings[i].scale;
                for (int j = 0; j < noiseSettings[i].interations; j++)
                {
                    float noiseX = seed + (offset + chunkX + x) * noiseSettings[i].roughness / MeshGenerator.resolution.x;
                    float noiseZ = seed + (offset + chunkZ + z) * noiseSettings[i].roughness / MeshGenerator.resolution.y;
                    switch (noiseSettings[i].noiseType)
                    {
                        case NoiseSettings.NoiseType.Perlin:
                            sample += Mathf.PerlinNoise(noiseX, noiseZ) * frequancy;
                            break;
                        case NoiseSettings.NoiseType.Perlin_Abs:
                            sample += 1f - Mathf.Abs(Mathf.Sin(Mathf.PerlinNoise(noiseX, noiseZ))) * frequancy * amplitude;
                            break;
                        case NoiseSettings.NoiseType.Simplex:
                            sample += SimplexNoise.SimplexNoise.Generate(noiseX, noiseZ) * frequancy * amplitude;
                            break;
                        case NoiseSettings.NoiseType.Fractal:
                            sample += ((SimplexNoise.SimplexNoise.Generate(noiseX, noiseZ) * frequancy) * 2 - 1) * amplitude;
                            break;
                        default:
                            // Default algorithm is perlin
                            sample += Mathf.PerlinNoise(noiseX, noiseZ) * frequancy * amplitude;
                            break;
                    }
                    amplitude *= noiseSettings[i].persistance;
                    frequancy *= noiseSettings[i].lacunarity;
                }
                sample = Mathf.Max(0, sample - noiseSettings[i].minValue);
                sample *= noiseSettings[i].strength;
            }
        }
        //WorldGenerator.instance.minMax.AddValue(sample);
        return sample;
    }

    public void GenerateColours()
    {
        UpdateMinMax();
        UpdateColors();
    }

    void UpdateMinMax()
    {
        if (generator == null)
            return;

        generator.colorSettings.material.SetVector("_minMax", new Vector4(WorldGenerator.instance.minMax.Min, WorldGenerator.instance.minMax.Max));
    }

    void UpdateColors()
    {
        if (generator == null)
            return;

        Color[] colors = new Color[generator.colorSettings.textureResolution];
        for (int i = 0; i < generator.colorSettings.textureResolution; i++)
        {
            colors[i] = CurrentBiome.gradient.Evaluate(i / (generator.colorSettings.textureResolution - 1f));
        }
        generator.colorSettings.texture.SetPixels(colors);
        generator.colorSettings.texture.Apply();
        generator.colorSettings.material.SetTexture("_texture", generator.colorSettings.texture);
    }
}
