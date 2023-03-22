using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    /// <summary>
    /// Ranges for best "fast" results (32 -> 128)
    /// </summary>
    public static Vector2Int resolution = new Vector2Int(128, 128); // if mesh index format is 16 bit there is a maximum of 65535 verts (255 x 255) - 256 x 256 is 1 over
    public static int vertexCount
    {
        get
        {
            return Mathf.RoundToInt(resolution.x * resolution.y) * 4;
        }
    }
    public static float tileSize = 1.0f;

    public ColorSettings colorSettings;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> tris = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();

    private Mesh mesh;
    [HideInInspector] public MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private bool generated = false;

    [Header("Editor")]
    [SerializeField] private bool showGrid = false;
    [SerializeField] private bool showVerticeValues = false;

    public void Initialize()
    {
        colorSettings = gameObject.GetComponent<ColorSettings>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        mesh = new Mesh();
        //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // enable for more vertices

        if(colorSettings.texture == null)
        {
            colorSettings.texture = new Texture2D(colorSettings.textureResolution, 1);
            colorSettings.material = new Material(colorSettings.baseMaterial);
        }
        meshRenderer.sharedMaterial = colorSettings.material;
    }

    private void OnValidate()
    {
        if(generated)
            Refresh();
    }

    public void GeneratePlane(float xOff, float zOff)
    {
        int index = 0;
        for (int z = 0; z < resolution.y; z++)
        {
            for (int x = 0; x < resolution.x; x++)
            {
                // Generate vertices
                Vector3 topLeft = new Vector3(xOff + x, 0, zOff + z);
                Vector3 topRight = new Vector3((xOff + x) + tileSize, 0, zOff + z);
                Vector3 bottomLeft = new Vector3(xOff + x, 0, (zOff + z) + tileSize);
                Vector3 bottomRight = new Vector3((xOff + x) + tileSize, 0, (zOff + z) + tileSize);

                vertices.Add(topLeft);
                vertices.Add(topRight);
                vertices.Add(bottomLeft);
                vertices.Add(bottomRight);

                // Generate triangles
                tris.Add(index);
                tris.Add(index + 2);
                tris.Add(index + 1);

                tris.Add(index + 1);
                tris.Add(index + 2);
                tris.Add(index + 3);

                // Generate normals
                Vector3 normalA = CalculateNormal(topLeft, bottomLeft, bottomRight);
                Vector3 normalB = CalculateNormal(topLeft, bottomRight, topRight);

                normals.Add(normalA);
                normals.Add(normalA);
                normals.Add(normalA);
                normals.Add(normalB);

                // Generate UVs
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                
                index += 4;
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        normals = mesh.normals.ToList();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        generated = true;
    }

    public void Refresh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        normals = mesh.normals.ToList();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

    }

    public Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = new Vector3(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
        Vector3 b = new Vector3(p3.x - p1.x, p3.y - p1.y, p3.z - p1.z);

        Vector3 normal = new Vector3(a.y * b.z - a.z * b.y,
                                     a.z * b.x - a.x * b.z,
                                     a.x * b.y - a.y * b.x);
        return normal.normalized;
    }

    /// <summary>
    /// Recalculate the normal at tile x
    /// </summary>
    /// <param name="tile">The tile index (Is multiplied by 4)</param>
    public void RecalculateNormalAt(int tile)
    {
        int index = tile * 4;

        Vector3 topLeft = vertices[index];
        Vector3 topRight = vertices[index + 1];
        Vector3 bottomLeft = vertices[index + 2];
        Vector3 bottomRight = vertices[index + 3];

        Vector3 normalA = CalculateNormal(topLeft, bottomLeft, bottomRight);
        Vector3 normalB = CalculateNormal(topLeft, bottomRight, topRight);

        normals[index] = normalA;
        normals[index + 1] = normalA;
        normals[index + 2] = normalA;
        normals[index + 3] = normalB;
    }

    public int GetIndex(float x, float z)
    {
        return (int) ((z * resolution.x) + x) * 4;
    }

    public Vector3 GetNormalAt(int index)
    {
        return normals[index];
    }

    public void UpdateHeightAt(int index, float value)
    {
        Vector3 vertex = vertices[index];
        vertex.y = value;
        vertices[index] = vertex;
    }

    public Vector3 GetNormalAt(float x, float z)
    {
        int index = GetIndex(x, z);
        return normals[index];
    }

    public void UpdateHeightAt(float x, float z, float value)
    {
        int index = GetIndex(x, z);
        Vector3 vertex = vertices[index];
        vertex.y = value;
        vertices[index] = vertex;
    }

    public void UpdateHeights(int tile, float[] heights)
    {
        int index = tile * 4;
        Vector3 topLeft = vertices[index];
        Vector3 topRight = vertices[index + 1];
        Vector3 bottomLeft = vertices[index + 2];
        Vector3 bottomRight = vertices[index + 3];

        topLeft.y = heights[0];
        topRight.y = heights[1];
        bottomLeft.y = heights[2];
        bottomRight.y = heights[3];

        vertices[index] = topLeft;
        vertices[index + 1] = topRight;
        vertices[index + 2] = bottomLeft;
        vertices[index + 3] = bottomRight;
    }

    public float GetAverage(int tile)
    {
        var index = tile * 4;
        float p1 = vertices[index].y;
        float p2 = vertices[index + 1].y;
        float p3 = vertices[index + 2].y;
        float p4 = vertices[index + 3].y;
        return p1 + p2 + p3 + p4 / 4;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            if (generated)
            {
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    if (showVerticeValues)
                    {
                        if (Vector3.Distance(UnityEditor.SceneView.currentDrawingSceneView.camera.transform.position, transform.position + vertices[i]) < 5f)
                        {
                            UnityEditor.Handles.Label(transform.position + vertices[i], "(" + vertices[i].x + ", " + vertices[i].z + " : " + i + ")\nHeight: " + (GetAverage(i / 4)));
                        }
                    }
                }
                if (showGrid)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireMesh(mesh, transform.position, Quaternion.identity, Vector3.one);
                }
            }
        }
#endif
    }
}