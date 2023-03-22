using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator instance;

    public float viewSize = 1f;
    public GameObject chunkPrefab;
    public Camera cam;
    public bool isInfinite = false;

    public int seed = 0; // Base seed - height seed
    public int temperatureSeed = 0;
    public int moistureSeed = 0;

    public Dictionary<string, GameObject> chunks = new Dictionary<string, GameObject>();

    [Header("Editor")]
    public bool displayChunkDetails = false;

    public MinMax minMax;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(seed == 0)
        {
            seed = Random.Range(0, 10000);
        }

        if(temperatureSeed == 0)
        {
            temperatureSeed = seed + 1;
        }

        if(moistureSeed == 0)
        {
            moistureSeed = temperatureSeed + 1;
        }

        minMax = new MinMax();
        minMax.AddValue(0);
        minMax.AddValue(15);
        if (!isInfinite)
            SpawnChunk(0, 0);
        else
            GenerateChunkIfWeNeedTo();
    }

    private void Update()
    {
        // Unrender chunks that are far away from the camera
        /*
        float camX = cam.transform.position.x / (MeshGenerator.tileSize * MeshGenerator.resolution);
        float camY = cam.transform.position.y / (MeshGenerator.tileSize * MeshGenerator.resolution);
        float camZ = cam.transform.position.z / (MeshGenerator.tileSize * MeshGenerator.resolution);

        for (int i = 0; i < chunks.Count; i++)
        {
            bool doEnable = MathUtils.DistanceFrom(chunks.Values.ElementAt(i).transform.position, new Vector3(camX, camY, camZ)) < (3 * MeshGenerator.resolution);
            chunks.Values.ElementAt(i).SetActive(doEnable);
        }
        */
        if(isInfinite)
            GenerateChunkIfWeNeedTo();
    }

    void SpawnChunk(float x, float z)
    {
        if (ChunkExistsAt(x, z))
            return;

        Vector3 worldspace = new Vector3(x, 0, z);
        GameObject spawned = Instantiate(chunkPrefab, worldspace, Quaternion.identity);
        spawned.transform.SetParent(transform);
        spawned.name = $"{x}_{z}";
        Chunk chunk = spawned.GetComponent<Chunk>();
        chunk.worldSpace = worldspace;
        chunk.Initialize();
        chunk.GenerateChunk();
        chunks.Add($"{x}_{z}", spawned);
    }

    void GenerateChunkIfWeNeedTo()
    {
        float camX = ChunkX();
        float camZ = ChunkZ();

        for(float x = camX - viewSize; x <= camX + viewSize; x++)
        {
            for(float z = camZ - viewSize; z <= camZ + viewSize; z++)
            {
                if(!ChunkExistsAt(x, z))
                {
                    SpawnChunk(x * MeshGenerator.resolution.x / 2, z * MeshGenerator.resolution.y / 2);
                }
            }
        }
    }

    bool ChunkExistsAt(float x, float z)
    {
        return chunks.ContainsKey($"{x}_{z}");
    }

    float ChunkX()
    {
        return Mathf.Round(cam.transform.position.x / MeshGenerator.resolution.x);
    }

    float ChunkZ()
    {
        return Mathf.Round(cam.transform.position.z / MeshGenerator.resolution.y);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                if (displayChunkDetails)
                    UnityEditor.Handles.Label(chunks.ElementAt(i).Value.transform.position, "(" + chunks.ElementAt(i).Value.transform.position.x + ", " + chunks.ElementAt(i).Value.transform.position.z + " : " + i + ")");
            }
        }
#endif
    }
}
