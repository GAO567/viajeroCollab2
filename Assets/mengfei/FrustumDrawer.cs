using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FrustumDrawer : MonoBehaviour
{
    public float fov = 60f;
    public float aspectRatio = 1.777f;
    public float near = 0.3f;
    public float far = 10f;

    private MeshFilter meshFilter;
    private Mesh mesh;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        UpdateFrustum();
    }

    void Update()
    {
        UpdateFrustum();
    }

    void UpdateFrustum()
    {
        // Generate frustum vertices
        float halfHeightNear = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * near;
        float halfHeightFar = Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * far;
        float halfWidthNear = halfHeightNear * aspectRatio;
        float halfWidthFar = halfHeightFar * aspectRatio;

        Vector3[] vertices = new Vector3[8];

        // Near plane
        vertices[0] = new Vector3(-halfWidthNear, -halfHeightNear, near);
        vertices[1] = new Vector3(halfWidthNear, -halfHeightNear, near);
        vertices[2] = new Vector3(halfWidthNear, halfHeightNear, near);
        vertices[3] = new Vector3(-halfWidthNear, halfHeightNear, near);

        // Far plane
        vertices[4] = new Vector3(-halfWidthFar, -halfHeightFar, far);
        vertices[5] = new Vector3(halfWidthFar, -halfHeightFar, far);
        vertices[6] = new Vector3(halfWidthFar, halfHeightFar, far);
        vertices[7] = new Vector3(-halfWidthFar, halfHeightFar, far);

        mesh.vertices = vertices;

        // Define the indices for the triangles
        int[] indices = {
            0, 1, 2, 2, 3, 0, // Near plane
            4, 5, 6, 6, 7, 4, // Far plane
            0, 1, 5, 5, 4, 0, // Bottom
            1, 2, 6, 6, 5, 1, // Right
            2, 3, 7, 7, 6, 2, // Top
            3, 0, 4, 4, 7, 3  // Left
        };

        mesh.triangles = indices;
        mesh.RecalculateNormals();
    }
}
