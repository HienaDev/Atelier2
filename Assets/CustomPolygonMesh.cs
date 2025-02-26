using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CustomPolygonMesh : MonoBehaviour
{
    // Define the base vertices of the mesh (for a unit cube)
    private Vector3[] baseVertices = new Vector3[]
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

    // Public fields to control the size of each edge
    [Header("Edge Sizes")]
    [Range(1f, 50f)] public float frontBottomEdge = 1f;  // Edge between vertex 0 and 1
    [Range(1f, 50f)] public float frontRightEdge = 1f;   // Edge between vertex 1 and 2
    [Range(1f, 50f)] public float frontTopEdge = 1f;     // Edge between vertex 2 and 3
    [Range(1f, 50f)] public float frontLeftEdge = 1f;    // Edge between vertex 3 and 0
    [Range(1f, 50f)] public float backBottomEdge = 1f;   // Edge between vertex 4 and 5
    [Range(1f, 50f)] public float backRightEdge = 1f;    // Edge between vertex 5 and 6
    [Range(1f, 50f)] public float backTopEdge = 1f;      // Edge between vertex 6 and 7
    [Range(1f, 50f)] public float backLeftEdge = 1f;     // Edge between vertex 7 and 4
    [Range(1f, 50f)] public float bottomLeftEdge = 1f;   // Edge between vertex 0 and 4
    [Range(1f, 50f)] public float bottomRightEdge = 1f;  // Edge between vertex 1 and 5
    [Range(1f, 50f)] public float topRightEdge = 1f;     // Edge between vertex 2 and 6
    [Range(1f, 50f)] public float topLeftEdge = 1f;      // Edge between vertex 3 and 7

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

        // Create a copy of the base vertices
        Vector3[] scaledVertices = new Vector3[baseVertices.Length];
        System.Array.Copy(baseVertices, scaledVertices, baseVertices.Length);

        // Scale vertices for each edge
        ScaleEdge(scaledVertices, 0, 1, frontBottomEdge);  // Front bottom edge
        ScaleEdge(scaledVertices, 1, 2, frontRightEdge);   // Front right edge
        ScaleEdge(scaledVertices, 2, 3, frontTopEdge);     // Front top edge
        ScaleEdge(scaledVertices, 3, 0, frontLeftEdge);    // Front left edge
        ScaleEdge(scaledVertices, 4, 5, backBottomEdge);   // Back bottom edge
        ScaleEdge(scaledVertices, 5, 6, backRightEdge);   // Back right edge
        ScaleEdge(scaledVertices, 6, 7, backTopEdge);     // Back top edge
        ScaleEdge(scaledVertices, 7, 4, backLeftEdge);    // Back left edge
        ScaleEdge(scaledVertices, 0, 4, bottomLeftEdge);  // Bottom left edge
        ScaleEdge(scaledVertices, 1, 5, bottomRightEdge); // Bottom right edge
        ScaleEdge(scaledVertices, 2, 6, topRightEdge);    // Top right edge
        ScaleEdge(scaledVertices, 3, 7, topLeftEdge);     // Top left edge

        // Assign scaled vertices, triangles, and UVs
        mesh.vertices = scaledVertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Recalculate normals for lighting
        mesh.RecalculateNormals();
    }

    void ScaleEdge(Vector3[] vertices, int indexA, int indexB, float scale)
    {
        // Calculate the midpoint of the edge
        Vector3 midpoint = (vertices[indexA] + vertices[indexB]) * 0.5f;

        // Scale each vertex relative to the midpoint
        vertices[indexA] = midpoint + (vertices[indexA] - midpoint) * scale;
        vertices[indexB] = midpoint + (vertices[indexB] - midpoint) * scale;
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
        // Update the mesh in the editor when edge sizes are modified
        if (mesh != null)
        {
            UpdateMesh();
            AddMeshCollider();
        }
    }
}