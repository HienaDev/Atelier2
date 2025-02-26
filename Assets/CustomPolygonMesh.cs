using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CustomPolygonMesh : MonoBehaviour
{
    // Define the vertices of the mesh
    public Vector3[] vertices = new Vector3[]
    {
        new Vector3(-0.5f, -0.5f, -0.5f), // 0
        new Vector3(0.5f, -0.5f, -0.5f),  // 1
        new Vector3(0.5f, 0.5f, -0.5f),   // 2
        new Vector3(-0.5f, 0.5f, -0.5f), // 3
        new Vector3(-0.5f, -0.5f, 0.5f), // 4
        new Vector3(0.5f, -0.5f, 0.5f),   // 5
        new Vector3(0.5f, 0.5f, 0.5f),    // 6
        new Vector3(-0.5f, 0.5f, 0.5f)    // 7
    };

    // Define the triangles (indices of vertices)
    private int[] triangles = new int[]
    {
        // Front face
        0, 2, 1,
        0, 3, 2,
        // Back face
        4, 5, 6,
        4, 6, 7,
        // Left face
        0, 7, 3,
        0, 4, 7,
        // Right face
        1, 2, 6,
        1, 6, 5,
        // Top face
        2, 3, 6,
        3, 7, 6,
        // Bottom face
        0, 1, 5,
        0, 5, 4
    };

    // Define UVs for texture mapping
    private Vector2[] uvs = new Vector2[]
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(0, 1),
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(0, 1)
    };

    // Public field to assign a material
    public Material meshMaterial;

    // Public field to control the size of each face
    [Range(0.1f, 10f)]
    public float faceSize = 1f;

    private Mesh mesh;

    void Start()
    {
        // Create a new mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Assign the material to the MeshRenderer
        if (meshMaterial != null)
        {
            GetComponent<MeshRenderer>().material = meshMaterial;
        }

        // Update the mesh with the defined vertices, triangles, and UVs
        UpdateMesh();

        // Add a convex MeshCollider
        AddMeshCollider();
    }

    void UpdateMesh()
    {
        // Clear the mesh
        mesh.Clear();

        // Scale vertices based on faceSize
        Vector3[] scaledVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            scaledVertices[i] = vertices[i] * faceSize;
        }

        // Assign scaled vertices, triangles, and UVs
        mesh.vertices = scaledVertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Recalculate normals for lighting
        mesh.RecalculateNormals();
    }

    void AddMeshCollider()
    {
        // Get or add a MeshCollider component
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        // Assign the mesh to the collider
        meshCollider.sharedMesh = mesh;

        // Enable convex for the MeshCollider
        meshCollider.convex = true;
    }

    void OnValidate()
    {
        // Update the mesh in the editor when vertices or faceSize are modified
        if (mesh != null)
        {
            UpdateMesh();
            AddMeshCollider();
        }
    }
}