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

    [Header("Layering Settings")]
    [SerializeField] private float surfaceOffset = 0.01f;
    [SerializeField] private float layerOffset = 0.001f;
    [SerializeField] private float normalMatchThreshold = 0.95f;
    [SerializeField] private float surfaceDistanceTolerance = 0.05f;

    private readonly Dictionary<int, Dictionary<int, SplatterBatch>> batchesByGun = new Dictionary<int, Dictionary<int, SplatterBatch>>();
    private readonly List<SplatterData> allSplatters = new List<SplatterData>();

    private class SplatterBatch
    {
        public int gunIndex;
        public int layerIndex;

        public Transform transform;
        public Mesh mesh;
        public MeshRenderer meshRenderer;

        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector3> normals = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<int> triangles = new List<int>();
    }

    private class SplatterData
    {
        public Vector3 surfaceCenter;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 bitangent;

        public float halfSize;

        public int gunIndex;
        public int layerIndex;

        public SplatterBatch batch;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < splatterMaterials.Length; i++)
        {
            batchesByGun.Add(i, new Dictionary<int, SplatterBatch>());
        }
    }

    public void AddSplatter(int gunIndex, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (gunIndex < 0 || gunIndex >= splatterMaterials.Length)
        {
            Debug.LogWarning("Invalid gun/splatter index: " + gunIndex);
            return;
        }

        Vector3 normal = hitNormal.normalized;

        int targetLayer = GetTargetLayer(gunIndex, hitPoint, normal);

        SplatterBatch batch = GetOrCreateBatch(gunIndex, targetLayer);

        float size = Random.Range(minSize, maxSize);
        float halfSize = size * 0.5f;

        Vector3 tangent;
        Vector3 bitangent;
        CreateSurfaceDirections(normal, out tangent, out bitangent);

        float randomRotation = Random.Range(0f, 360f);
        Quaternion rotation = Quaternion.AngleAxis(randomRotation, normal);

        tangent = rotation * tangent;
        bitangent = rotation * bitangent;

        float finalSurfaceOffset = surfaceOffset + targetLayer * layerOffset;
        Vector3 renderCenter = hitPoint + normal * finalSurfaceOffset;

        Vector3 worldV0 = renderCenter - tangent * halfSize - bitangent * halfSize;
        Vector3 worldV1 = renderCenter - tangent * halfSize + bitangent * halfSize;
        Vector3 worldV2 = renderCenter + tangent * halfSize + bitangent * halfSize;
        Vector3 worldV3 = renderCenter + tangent * halfSize - bitangent * halfSize;

        int startIndex = batch.vertices.Count;

        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV0));
        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV1));
        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV2));
        batch.vertices.Add(batch.transform.InverseTransformPoint(worldV3));

        Vector3 localNormal = batch.transform.InverseTransformDirection(normal);

        batch.normals.Add(localNormal);
        batch.normals.Add(localNormal);
        batch.normals.Add(localNormal);
        batch.normals.Add(localNormal);

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

        allSplatters.Add(new SplatterData
        {
            surfaceCenter = hitPoint,
            normal = normal,
            tangent = tangent,
            bitangent = bitangent,
            halfSize = halfSize,
            gunIndex = gunIndex,
            layerIndex = targetLayer,
            batch = batch
        });

        ApplyMesh(batch);
    }

    private int GetTargetLayer(int currentGunIndex, Vector3 hitPoint, Vector3 hitNormal)
    {
        SplatterData topSplatter = GetTopSplatterAtPoint(hitPoint, hitNormal);

        if (topSplatter == null)
        {
            return 0;
        }

        if (topSplatter.gunIndex == currentGunIndex)
        {
            return topSplatter.layerIndex;
        }

        return topSplatter.layerIndex + 1;
    }

    private SplatterData GetTopSplatterAtPoint(Vector3 hitPoint, Vector3 hitNormal)
    {
        SplatterData topSplatter = null;
        int highestLayer = int.MinValue;

        for (int i = 0; i < allSplatters.Count; i++)
        {
            SplatterData splatter = allSplatters[i];

            float normalSimilarity = Vector3.Dot(hitNormal.normalized, splatter.normal);

            if (normalSimilarity < normalMatchThreshold)
            {
                continue;
            }

            if (!IsPointInsideSplatter(hitPoint, splatter))
            {
                continue;
            }

            if (splatter.layerIndex > highestLayer)
            {
                highestLayer = splatter.layerIndex;
                topSplatter = splatter;
            }
        }

        return topSplatter;
    }

    private bool IsPointInsideSplatter(Vector3 point, SplatterData splatter)
    {
        Vector3 toPoint = point - splatter.surfaceCenter;

        float distanceFromSurface = Mathf.Abs(Vector3.Dot(toPoint, splatter.normal));

        if (distanceFromSurface > surfaceDistanceTolerance)
        {
            return false;
        }

        float x = Vector3.Dot(toPoint, splatter.tangent);
        float y = Vector3.Dot(toPoint, splatter.bitangent);

        return Mathf.Abs(x) <= splatter.halfSize &&
               Mathf.Abs(y) <= splatter.halfSize;
    }

    private SplatterBatch GetOrCreateBatch(int gunIndex, int layerIndex)
    {
        Dictionary<int, SplatterBatch> gunBatches = batchesByGun[gunIndex];

        if (gunBatches.TryGetValue(layerIndex, out SplatterBatch existingBatch))
        {
            return existingBatch;
        }

        SplatterBatch newBatch = CreateBatch(gunIndex, layerIndex);
        gunBatches.Add(layerIndex, newBatch);

        return newBatch;
    }

    private SplatterBatch CreateBatch(int gunIndex, int layerIndex)
    {
        GameObject batchObject = new GameObject("Gun_" + gunIndex + "_Splatter_Layer_" + layerIndex);
        batchObject.transform.SetParent(transform);
        batchObject.transform.localPosition = Vector3.zero;
        batchObject.transform.localRotation = Quaternion.identity;
        batchObject.transform.localScale = Vector3.one;

        MeshFilter meshFilter = batchObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = batchObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = "Gun_" + gunIndex + "_SplatterMesh_Layer_" + layerIndex;
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = splatterMaterials[gunIndex];

        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        meshRenderer.sortingOrder = layerIndex;

        return new SplatterBatch
        {
            gunIndex = gunIndex,
            layerIndex = layerIndex,
            transform = batchObject.transform,
            mesh = mesh,
            meshRenderer = meshRenderer
        };
    }

    private void CreateSurfaceDirections(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
    {
        Vector3 reference = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f
            ? Vector3.right
            : Vector3.up;

        tangent = Vector3.Cross(reference, normal).normalized;
        bitangent = Vector3.Cross(normal, tangent).normalized;
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
        foreach (KeyValuePair<int, Dictionary<int, SplatterBatch>> gunEntry in batchesByGun)
        {
            foreach (KeyValuePair<int, SplatterBatch> layerEntry in gunEntry.Value)
            {
                SplatterBatch batch = layerEntry.Value;

                batch.vertices.Clear();
                batch.normals.Clear();
                batch.uvs.Clear();
                batch.triangles.Clear();

                batch.mesh.Clear();
            }
        }

        allSplatters.Clear();
    }
}
