using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatterMeshManager : MonoBehaviour
{
    public static SplatterMeshManager Instance { get; private set; }

    [Header("Splatter Materials")]
    [SerializeField] private Material[] splatterMaterials;

    [Header("Splatter Settings")]
    [SerializeField] private float minSize = 0.4f;
    [SerializeField] private float maxSize = 1.0f;
    [SerializeField] private float surfaceOffset = 0.01f;

    private SplatterBatch[] batches;

    private class SplatterBatch
    {
        public Transform transform;
        public Mesh mesh;

        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector3> normals = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<int> triangles = new List<int>();
    }

    private void Awake()
    {
        Instance = this;
        CreateBatches();
    }

    private void CreateBatches()
    {
        batches = new SplatterBatch[splatterMaterials.Length];

        for (int i = 0; i < splatterMaterials.Length; i++)
        {
            GameObject batchObject = new GameObject("SplatterBatch_" + i);
            batchObject.transform.SetParent(transform);
            batchObject.transform.localPosition = Vector3.zero;
            batchObject.transform.localRotation = Quaternion.identity;
            batchObject.transform.localScale = Vector3.one;

            MeshFilter meshFilter = batchObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = batchObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.name = "SplatterMesh_" + i;
            mesh.indexFormat = IndexFormat.UInt32;

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = splatterMaterials[i];

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            batches[i] = new SplatterBatch
            {
                transform = batchObject.transform,
                mesh = mesh
            };
        }
    }

    public void AddSplatter(int splatterType, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (splatterType < 0 || splatterType >= batches.Length)
        {
            Debug.LogWarning("Invalid splatter type: " + splatterType);
            return;
        }

        SplatterBatch batch = batches[splatterType];

        Vector3 normal = hitNormal.normalized;
        Vector3 center = hitPoint + normal * surfaceOffset;

        float size = Random.Range(minSize, maxSize);
        float halfSize = size * 0.5f;

        Vector3 reference = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f
            ? Vector3.right
            : Vector3.up;

        Vector3 tangent = Vector3.Cross(reference, normal).normalized;
        Vector3 bitangent = Vector3.Cross(normal, tangent).normalized;

        float randomRotation = Random.Range(0f, 360f);
        Quaternion rotation = Quaternion.AngleAxis(randomRotation, normal);

        tangent = rotation * tangent;
        bitangent = rotation * bitangent;

        Vector3 worldV0 = center - tangent * halfSize - bitangent * halfSize;
        Vector3 worldV1 = center - tangent * halfSize + bitangent * halfSize;
        Vector3 worldV2 = center + tangent * halfSize + bitangent * halfSize;
        Vector3 worldV3 = center + tangent * halfSize - bitangent * halfSize;

        int startIndex = batch.vertices.Count;

        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV0));
        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV1));
        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV2));
        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV3));

        batch.normals.Add(batch.transform.InverseTransformDirection(normal));
        batch.normals.Add(batch.transform.InverseTransformDirection(normal));
        batch.normals.Add(batch.transform.InverseTransformDirection(normal));
        batch.normals.Add(batch.transform.InverseTransformDirection(normal));

        batch.uvs.Add(new Vector2(0f, 0f));
        batch.uvs.Add(new Vector2(0f, 1f));
        batch.uvs.Add(new Vector2(1f, 1f));
        batch.uvs.Add(new Vector2(1f, 0f));

        batch.triangles.Add(startIndex + 0);
        batch.triangles.Add(startIndex + 2);
        batch.triangles.Add(startIndex + 1);

        batch.triangles.Add(startIndex + 0);
        batch.triangles.Add(startIndex + 3);
        batch.triangles.Add(startIndex + 2);

        ApplyMesh(batch);
    }

    private void ApplyMesh(SplatterBatch batch)
    {
        batch.mesh.Clear();

        batch.mesh.SetVertices(batch.vertices);
        batch.mesh.SetNormals(batch.normals);
        batch.mesh.SetUVs(0, batch.uvs);
        batch.mesh.SetTriangles(batch.triangles, 0);

        batch.mesh.RecalculateBounds();
    }

    [ContextMenu("Clear All Splatters")]
    public void ClearAllSplatters()
    {
        if (batches == null)
            return;

        foreach (SplatterBatch batch in batches)
        {
            batch.vertices.Clear();
            batch.normals.Clear();
            batch.uvs.Clear();
            batch.triangles.Clear();
            batch.mesh.Clear();
        }
    }
}