using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TrapezoidalPrismPlatform : MonoBehaviour
{
    // Define the base vertices of the mesh (for a unit cube)
    private Vector3[] baseVertices = new Vector3[]
    {
        // Front face
        new Vector3(-0.5f, -0.5f, -0.5f), // 0: Front bottom left
        new Vector3(0.5f, -0.5f, -0.5f),  // 1: Front bottom right
        new Vector3(0.5f, 0.5f, -0.5f),   // 2: Front top right
        new Vector3(-0.5f, 0.5f, -0.5f), // 3: Front top left

        // Back face
        new Vector3(-0.5f, -0.5f, 0.5f), // 4: Back bottom left
        new Vector3(0.5f, -0.5f, 0.5f),   // 5: Back bottom right
        new Vector3(0.5f, 0.5f, 0.5f),    // 6: Back top right
        new Vector3(-0.5f, 0.5f, 0.5f)    // 7: Back top left
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

    // Public fields to control the trapezoid shape
    [Header("Trapezoid Settings")]
    [Range(0.1f, 20f)] public float frontWidth = 1f;  // Width of the front face
    [Range(0.1f, 20f)] public float backWidth = 0.5f; // Width of the back face
    [Range(0.1f, 100f)] public float size = 1f;      // Overall size of the platform
    [Range(0.1f, 100f)] public float depth = 1f;     // Depth of the platform

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

        // Add and update the MeshCollider
        UpdateMeshCollider();
    }

    void UpdateMesh()
    {
        // Clear the mesh
        mesh.Clear();

        // Create a copy of the base vertices
        Vector3[] scaledVertices = new Vector3[baseVertices.Length];
        System.Array.Copy(baseVertices, scaledVertices, baseVertices.Length);

        // Adjust the front and back faces to create a trapezoidal prism
        for (int i = 0; i < 4; i++)
        {
            // Scale the front face
            scaledVertices[i].x *= frontWidth;

            // Scale the back face
            scaledVertices[i + 4].x *= backWidth;

            // Adjust the depth of the back face
            scaledVertices[i + 4].z *= depth;
        }

        // Scale the entire mesh by the size parameter
        for (int i = 0; i < scaledVertices.Length; i++)
        {
            scaledVertices[i] *= size;
        }

        // Assign scaled vertices, triangles, and UVs
        mesh.vertices = scaledVertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Recalculate normals for lighting
        mesh.RecalculateNormals();
    }

    void UpdateMeshCollider()
    {
        // Get or add a MeshCollider component
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        // Assign the mesh to the collider
        meshCollider.sharedMesh = mesh;

        // Enable convex for the MeshCollider (optional, depending on your use case)
        meshCollider.convex = true;
    }

    void OnValidate()
    {
        // Update the mesh in the editor when trapezoid settings are modified
        if (mesh != null)
        {
            UpdateMesh();
            UpdateMeshCollider();
        }
    }
}