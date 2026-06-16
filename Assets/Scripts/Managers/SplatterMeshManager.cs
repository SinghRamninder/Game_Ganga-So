using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplatterMeshManager : MonoBehaviour
{
    public static SplatterMeshManager Instance { get; private set; }

    [System.Serializable]
    public class SplatterSetup
    {
        public string name;
        public GameObject temporaryPrefab;
        public Material material;
        public float minSize = 0.4f;
        public float maxSize = 1.0f;
        public bool isInkSplatter;
    }

    [Header("Splatter Setups")]
    [SerializeField] private List<SplatterSetup> splatterSetups = new List<SplatterSetup>();

    [Header("Area Settings")]
    [SerializeField] private string defaultAreaId = "Default";

    [Header("Chunk Settings")]
    [SerializeField] private int maxSplattersPerChunk = 1000;

    [Header("Layering Settings")]
    [SerializeField] private float surfaceOffset = 0.01f;
    [SerializeField] private float layerOffset = 0.001f;
    [SerializeField] private float normalMatchThreshold = 0.95f;
    [SerializeField] private float surfaceDistanceTolerance = 0.05f;

    [Header("Temporary Pool Settings")]
    [SerializeField] private int poolSizePerSplatter = 50;
    [SerializeField] private int poolCreatePerFrame = 10;
    [SerializeField] private bool expandPoolIfEmpty = true;

    [Header("Delayed Mesh Build Settings")]
    [SerializeField] private float meshBuildDelay = 0.12f;
    [SerializeField] private int maxPendingSplatterCommitsPerFrame = 10;
    [SerializeField] private int maxDirtyChunksToApplyPerFrame = 2;

    [Header("Temporary Visual Mesh Fix")]
    [SerializeField] private bool reverseTemporaryMeshFacing = true;

    private Transform poolRoot;
    private Transform chunksRoot;

    private readonly Dictionary<string, AreaData> areas = new Dictionary<string, AreaData>();
    private readonly Queue<PendingSplatter> pendingSplatters = new Queue<PendingSplatter>();
    private readonly Queue<SplatterChunk> dirtyChunkQueue = new Queue<SplatterChunk>();
    private readonly HashSet<SplatterChunk> dirtyChunkSet = new HashSet<SplatterChunk>();

    private List<PooledSplatter>[] pools;

    private class AreaData
    {
        public string areaId;
        public Transform root;
        public readonly Dictionary<int, GunData> guns = new Dictionary<int, GunData>();
        public readonly List<SplatterData> splatters = new List<SplatterData>();
    }

    private class GunData
    {
        public int gunIndex;
        public Transform root;
        public readonly Dictionary<int, LayerData> layers = new Dictionary<int, LayerData>();
    }

    private class LayerData
    {
        public int layerIndex;
        public Transform root;
        public readonly List<SplatterChunk> chunks = new List<SplatterChunk>();
    }

    private class SplatterChunk
    {
        public string areaId;
        public int gunIndex;
        public int layerIndex;
        public int chunkIndex;

        public Transform transform;
        public Mesh mesh;
        public MeshRenderer meshRenderer;

        public int reservedSplatterCount;
        public int builtSplatterCount;

        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector3> normals = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<int> triangles = new List<int>();

        public readonly List<PooledSplatter> temporaryVisualsWaitingForMesh = new List<PooledSplatter>();
    }

    private class SplatterData
    {
        public string areaId;

        public Vector3 surfaceCenter;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 bitangent;

        public float halfSize;

        public int gunIndex;
        public int layerIndex;

        public SplatterChunk chunk;
    }

    private class PendingSplatter
    {
        public float buildTime;

        public SplatterData data;
        public SplatterChunk chunk;
        public PooledSplatter temporaryVisual;

        public Vector3 worldV0;
        public Vector3 worldV1;
        public Vector3 worldV2;
        public Vector3 worldV3;
        public Vector3 normal;
    }

    private class PooledSplatter
    {
        public int gunIndex;
        public GameObject gameObject;
        public Transform transform;
        public MeshRenderer[] meshRenderers;
        public bool inUse;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        poolRoot = new GameObject("Temporary Splatter Pool").transform;
        poolRoot.SetParent(transform);
        poolRoot.localPosition = Vector3.zero;
        poolRoot.localRotation = Quaternion.identity;
        poolRoot.localScale = Vector3.one;

        chunksRoot = new GameObject("Area Splatter Chunks").transform;
        chunksRoot.SetParent(transform);
        chunksRoot.localPosition = Vector3.zero;
        chunksRoot.localRotation = Quaternion.identity;
        chunksRoot.localScale = Vector3.one;

        pools = new List<PooledSplatter>[splatterSetups.Count];

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<PooledSplatter>();
        }
    }

    private void Start()
    {
        StartCoroutine(CreatePoolsGradually());
        StartCoroutine(ProcessPendingSplatters());
    }

    public float GetMaxSplatterSize(int gunIndex)
    {
        if (!IsValidGunIndex(gunIndex))
            return 1.0f;

        return splatterSetups[gunIndex].maxSize;
    }

    public void AddSplatter(int gunIndex, Vector3 hitPoint, Vector3 hitNormal)
    {
        AddSplatter(gunIndex, defaultAreaId, hitPoint, hitNormal);
    }

    public void AddSplatter(int gunIndex, string areaId, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!IsValidGunIndex(gunIndex))
            return;

        if (string.IsNullOrWhiteSpace(areaId))
            areaId = defaultAreaId;

        AreaData area = GetOrCreateArea(areaId);

        Vector3 normal = hitNormal.normalized;

        int targetLayer = GetTargetLayer(area, gunIndex, hitPoint, normal);

        float size = Random.Range(splatterSetups[gunIndex].minSize, splatterSetups[gunIndex].maxSize);
        float halfSize = size * 0.5f;

        Vector3 tangent;
        Vector3 bitangent;
        CreateSurfaceDirections(normal, out tangent, out bitangent);

        if (!splatterSetups[gunIndex].isInkSplatter)
        {
            float randomRotation = Random.Range(0f, 360f);
            Quaternion randomSpin = Quaternion.AngleAxis(randomRotation, normal);

            tangent = randomSpin * tangent;
            bitangent = randomSpin * bitangent;
        }

        float finalSurfaceOffset = surfaceOffset + targetLayer * layerOffset;
        Vector3 renderCenter = hitPoint + normal * finalSurfaceOffset;

        Vector3 worldV0 = renderCenter - tangent * halfSize - bitangent * halfSize;
        Vector3 worldV1 = renderCenter - tangent * halfSize + bitangent * halfSize;
        Vector3 worldV2 = renderCenter + tangent * halfSize + bitangent * halfSize;
        Vector3 worldV3 = renderCenter + tangent * halfSize - bitangent * halfSize;

        SplatterChunk chunk = GetWritableChunk(area, gunIndex, targetLayer);
        chunk.reservedSplatterCount++;

        SplatterData data = new SplatterData
        {
            areaId = areaId,
            surfaceCenter = hitPoint,
            normal = normal,
            tangent = tangent,
            bitangent = bitangent,
            halfSize = halfSize,
            gunIndex = gunIndex,
            layerIndex = targetLayer,
            chunk = chunk
        };

        area.splatters.Add(data);

        Quaternion visualRotation = Quaternion.LookRotation(normal, bitangent);
        PooledSplatter temporaryVisual = GetTemporaryVisual(gunIndex);

        if (temporaryVisual != null)
        {
            SetupTemporaryVisual(
                temporaryVisual,
                gunIndex,
                renderCenter,
                visualRotation,
                size,
                targetLayer
            );
        }

        PendingSplatter pending = new PendingSplatter
        {
            buildTime = Time.time + meshBuildDelay,
            data = data,
            chunk = chunk,
            temporaryVisual = temporaryVisual,
            worldV0 = worldV0,
            worldV1 = worldV1,
            worldV2 = worldV2,
            worldV3 = worldV3,
            normal = normal
        };

        if (splatterSetups[gunIndex].isInkSplatter && temporaryVisual != null)
        {
            Animator animator = temporaryVisual.gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Default", 0, 0f);
                float clipLength = meshBuildDelay;

                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    clipLength = clipInfo[0].clip.length;
                }

                StartCoroutine(EnqueueSplatterDelayed(clipLength, pending));
            }
            else
            {
                pendingSplatters.Enqueue(pending);
            }
        }
        else
        {
            pendingSplatters.Enqueue(pending);
        }
    }

    private IEnumerator EnqueueSplatterDelayed(float delay, PendingSplatter pending)
    {
        yield return new WaitForSeconds(delay);
        pending.buildTime = Time.time;
        pendingSplatters.Enqueue(pending);
    }

    private IEnumerator CreatePoolsGradually()
    {
        for (int gunIndex = 0; gunIndex < splatterSetups.Count; gunIndex++)
        {
            if (splatterSetups[gunIndex].temporaryPrefab == null)
            {
                Debug.LogWarning("Temporary prefab missing for splatter index: " + gunIndex);
                continue;
            }

            int createdThisFrame = 0;

            for (int i = 0; i < poolSizePerSplatter; i++)
            {
                CreatePooledSplatter(gunIndex);

                createdThisFrame++;

                if (createdThisFrame >= poolCreatePerFrame)
                {
                    createdThisFrame = 0;
                    yield return null;
                }
            }
        }
    }

    private IEnumerator ProcessPendingSplatters()
    {
        while (true)
        {
            int committedThisFrame = 0;

            while (pendingSplatters.Count > 0 &&
                   pendingSplatters.Peek().buildTime <= Time.time &&
                   committedThisFrame < maxPendingSplatterCommitsPerFrame)
            {
                PendingSplatter pending = pendingSplatters.Dequeue();

                CommitPendingSplatterToChunk(pending);
                MarkChunkDirty(pending.chunk);

                committedThisFrame++;
            }

            ApplyDirtyChunks();

            yield return null;
        }
    }

    private void CommitPendingSplatterToChunk(PendingSplatter pending)
    {
        SplatterChunk chunk = pending.chunk;

        int startIndex = chunk.vertices.Count;

        chunk.vertices.Add(chunk.transform.InverseTransformPoint(pending.worldV0));
        chunk.vertices.Add(chunk.transform.InverseTransformPoint(pending.worldV1));
        chunk.vertices.Add(chunk.transform.InverseTransformPoint(pending.worldV2));
        chunk.vertices.Add(chunk.transform.InverseTransformPoint(pending.worldV3));

        Vector3 localNormal = chunk.transform.InverseTransformDirection(pending.normal);

        chunk.normals.Add(localNormal);
        chunk.normals.Add(localNormal);
        chunk.normals.Add(localNormal);
        chunk.normals.Add(localNormal);

        chunk.uvs.Add(new Vector2(0f, 0f));
        chunk.uvs.Add(new Vector2(0f, 1f));
        chunk.uvs.Add(new Vector2(1f, 1f));
        chunk.uvs.Add(new Vector2(1f, 0f));

        chunk.triangles.Add(startIndex + 0);
        chunk.triangles.Add(startIndex + 2);
        chunk.triangles.Add(startIndex + 1);

        chunk.triangles.Add(startIndex + 0);
        chunk.triangles.Add(startIndex + 3);
        chunk.triangles.Add(startIndex + 2);

        chunk.builtSplatterCount++;

        if (pending.temporaryVisual != null)
        {
            chunk.temporaryVisualsWaitingForMesh.Add(pending.temporaryVisual);
        }
    }

    private void ApplyDirtyChunks()
    {
        int appliedThisFrame = 0;

        while (dirtyChunkQueue.Count > 0 && appliedThisFrame < maxDirtyChunksToApplyPerFrame)
        {
            SplatterChunk chunk = dirtyChunkQueue.Dequeue();
            dirtyChunkSet.Remove(chunk);

            ApplyMesh(chunk);
            DisableTemporaryVisualsWaitingForChunk(chunk);

            appliedThisFrame++;
        }
    }

    private void ApplyMesh(SplatterChunk chunk)
    {
        chunk.mesh.Clear();

        chunk.mesh.SetVertices(chunk.vertices);
        chunk.mesh.SetNormals(chunk.normals);
        chunk.mesh.SetUVs(0, chunk.uvs);
        chunk.mesh.SetTriangles(chunk.triangles, 0);

        chunk.mesh.RecalculateBounds();
    }

    private void DisableTemporaryVisualsWaitingForChunk(SplatterChunk chunk)
    {
        for (int i = 0; i < chunk.temporaryVisualsWaitingForMesh.Count; i++)
        {
            ReleaseTemporaryVisual(chunk.temporaryVisualsWaitingForMesh[i]);
        }

        chunk.temporaryVisualsWaitingForMesh.Clear();
    }

    private void MarkChunkDirty(SplatterChunk chunk)
    {
        if (dirtyChunkSet.Contains(chunk))
            return;

        dirtyChunkSet.Add(chunk);
        dirtyChunkQueue.Enqueue(chunk);
    }

    private int GetTargetLayer(AreaData area, int currentGunIndex, Vector3 hitPoint, Vector3 hitNormal)
    {
        SplatterData topSplatter = GetTopSplatterAtPoint(area, hitPoint, hitNormal);

        if (topSplatter == null)
            return 0;

        if (topSplatter.gunIndex == currentGunIndex)
            return topSplatter.layerIndex;

        return topSplatter.layerIndex + 1;
    }

    private SplatterData GetTopSplatterAtPoint(AreaData area, Vector3 hitPoint, Vector3 hitNormal)
    {
        SplatterData topSplatter = null;
        int highestLayer = int.MinValue;

        for (int i = 0; i < area.splatters.Count; i++)
        {
            SplatterData splatter = area.splatters[i];

            float normalSimilarity = Vector3.Dot(hitNormal.normalized, splatter.normal);

            if (normalSimilarity < normalMatchThreshold)
                continue;

            if (!IsPointInsideSplatter(hitPoint, splatter))
                continue;

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
            return false;

        float x = Vector3.Dot(toPoint, splatter.tangent);
        float y = Vector3.Dot(toPoint, splatter.bitangent);

        return Mathf.Abs(x) <= splatter.halfSize &&
               Mathf.Abs(y) <= splatter.halfSize;
    }

    private AreaData GetOrCreateArea(string areaId)
    {
        if (areas.TryGetValue(areaId, out AreaData existingArea))
            return existingArea;

        GameObject areaObject = new GameObject("Area_" + areaId);
        areaObject.transform.SetParent(chunksRoot);
        areaObject.transform.localPosition = Vector3.zero;
        areaObject.transform.localRotation = Quaternion.identity;
        areaObject.transform.localScale = Vector3.one;

        AreaData area = new AreaData
        {
            areaId = areaId,
            root = areaObject.transform
        };

        areas.Add(areaId, area);

        return area;
    }

    private GunData GetOrCreateGunData(AreaData area, int gunIndex)
    {
        if (area.guns.TryGetValue(gunIndex, out GunData existingGun))
            return existingGun;

        GameObject gunObject = new GameObject("Gun_" + gunIndex);
        gunObject.transform.SetParent(area.root);
        gunObject.transform.localPosition = Vector3.zero;
        gunObject.transform.localRotation = Quaternion.identity;
        gunObject.transform.localScale = Vector3.one;

        GunData gunData = new GunData
        {
            gunIndex = gunIndex,
            root = gunObject.transform
        };

        area.guns.Add(gunIndex, gunData);

        return gunData;
    }

    private LayerData GetOrCreateLayerData(AreaData area, int gunIndex, int layerIndex)
    {
        GunData gunData = GetOrCreateGunData(area, gunIndex);

        if (gunData.layers.TryGetValue(layerIndex, out LayerData existingLayer))
            return existingLayer;

        GameObject layerObject = new GameObject("Layer_" + layerIndex);
        layerObject.transform.SetParent(gunData.root);
        layerObject.transform.localPosition = Vector3.zero;
        layerObject.transform.localRotation = Quaternion.identity;
        layerObject.transform.localScale = Vector3.one;

        LayerData layerData = new LayerData
        {
            layerIndex = layerIndex,
            root = layerObject.transform
        };

        gunData.layers.Add(layerIndex, layerData);

        return layerData;
    }

    private SplatterChunk GetWritableChunk(AreaData area, int gunIndex, int layerIndex)
    {
        LayerData layerData = GetOrCreateLayerData(area, gunIndex, layerIndex);

        if (layerData.chunks.Count > 0)
        {
            SplatterChunk lastChunk = layerData.chunks[layerData.chunks.Count - 1];

            if (lastChunk.reservedSplatterCount < maxSplattersPerChunk)
                return lastChunk;
        }

        return CreateChunk(area, gunIndex, layerIndex, layerData);
    }

    private SplatterChunk CreateChunk(AreaData area, int gunIndex, int layerIndex, LayerData layerData)
    {
        int chunkIndex = layerData.chunks.Count;

        GameObject chunkObject = new GameObject(
            "Area_" + area.areaId +
            "_Gun_" + gunIndex +
            "_Layer_" + layerIndex +
            "_Chunk_" + chunkIndex
        );

        chunkObject.transform.SetParent(layerData.root);
        chunkObject.transform.localPosition = Vector3.zero;
        chunkObject.transform.localRotation = Quaternion.identity;
        chunkObject.transform.localScale = Vector3.one;

        MeshFilter meshFilter = chunkObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = "SplatterMesh_" + area.areaId + "_Gun_" + gunIndex + "_Layer_" + layerIndex + "_Chunk_" + chunkIndex;
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.MarkDynamic();

        meshFilter.sharedMesh = mesh;

        meshRenderer.sharedMaterial = splatterSetups[gunIndex].material;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.sortingOrder = layerIndex;

        SplatterChunk chunk = new SplatterChunk
        {
            areaId = area.areaId,
            gunIndex = gunIndex,
            layerIndex = layerIndex,
            chunkIndex = chunkIndex,
            transform = chunkObject.transform,
            mesh = mesh,
            meshRenderer = meshRenderer
        };

        layerData.chunks.Add(chunk);

        return chunk;
    }

    private PooledSplatter GetTemporaryVisual(int gunIndex)
    {
        if (!IsValidGunIndex(gunIndex))
            return null;

        List<PooledSplatter> pool = pools[gunIndex];

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].inUse)
                return pool[i];
        }

        if (!expandPoolIfEmpty)
            return null;

        return CreatePooledSplatter(gunIndex);
    }

    private PooledSplatter CreatePooledSplatter(int gunIndex)
    {
        GameObject prefab = splatterSetups[gunIndex].temporaryPrefab;

        if (prefab == null)
            return null;

        GameObject obj = Instantiate(prefab, poolRoot);
        obj.name = "PooledTempSplatter_Gun_" + gunIndex;

        if (reverseTemporaryMeshFacing)
        {
            ReverseTemporaryMeshFacing(obj);
        }

        obj.SetActive(false);

        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = splatterSetups[gunIndex].material;
            renderers[i].shadowCastingMode = ShadowCastingMode.Off;
            renderers[i].receiveShadows = false;
        }

        PooledSplatter pooled = new PooledSplatter
        {
            gunIndex = gunIndex,
            gameObject = obj,
            transform = obj.transform,
            meshRenderers = renderers,
            inUse = false
        };

        pools[gunIndex].Add(pooled);

        return pooled;
    }

    private void SetupTemporaryVisual(
        PooledSplatter pooled,
        int gunIndex,
        Vector3 position,
        Quaternion rotation,
        float size,
        int sortingOrder
    )
    {
        pooled.inUse = true;

        pooled.transform.position = position;
        pooled.transform.rotation = rotation;
        pooled.transform.localScale = Vector3.one * size;

        for (int i = 0; i < pooled.meshRenderers.Length; i++)
        {
            pooled.meshRenderers[i].sharedMaterial = splatterSetups[gunIndex].material;
            pooled.meshRenderers[i].sortingOrder = sortingOrder;
        }

        pooled.gameObject.SetActive(true);
    }

    private void ReleaseTemporaryVisual(PooledSplatter pooled)
    {
        if (pooled == null)
            return;

        pooled.gameObject.SetActive(false);
        pooled.transform.SetParent(poolRoot);
        pooled.inUse = false;
    }

    private void CreateSurfaceDirections(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
    {
        Vector3 reference = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.95f
            ? Vector3.right
            : Vector3.up;

        tangent = Vector3.Cross(reference, normal).normalized;
        bitangent = Vector3.Cross(normal, tangent).normalized;
    }

    private bool IsValidGunIndex(int gunIndex)
    {
        if (gunIndex < 0 || gunIndex >= splatterSetups.Count)
        {
            Debug.LogWarning("Invalid splatter/gun index: " + gunIndex);
            return false;
        }

        if (splatterSetups[gunIndex].material == null)
        {
            Debug.LogWarning("Missing splatter material for index: " + gunIndex);
            return false;
        }

        return true;
    }

    private void ReverseTemporaryMeshFacing(GameObject obj)
    {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);

        for (int i = 0; i < meshFilters.Length; i++)
        {
            Mesh originalMesh = meshFilters[i].sharedMesh;

            if (originalMesh == null)
                continue;

            Mesh clonedMesh = Instantiate(originalMesh);
            clonedMesh.name = originalMesh.name + "_ReversedFacing";

            int[] triangles = clonedMesh.triangles;

            for (int t = 0; t < triangles.Length; t += 3)
            {
                int temp = triangles[t + 1];
                triangles[t + 1] = triangles[t + 2];
                triangles[t + 2] = temp;
            }

            clonedMesh.triangles = triangles;
            clonedMesh.RecalculateNormals();

            meshFilters[i].sharedMesh = clonedMesh;
        }
    }


    [ContextMenu("Clear All Built Splatters")]
    public void ClearAllBuiltSplatters()
    {
        pendingSplatters.Clear();
        dirtyChunkQueue.Clear();
        dirtyChunkSet.Clear();
        areas.Clear();

        if (chunksRoot != null)
        {
            if (Application.isPlaying)
                Destroy(chunksRoot.gameObject);
            else
                DestroyImmediate(chunksRoot.gameObject);
        }

        chunksRoot = new GameObject("Area Splatter Chunks").transform;
        chunksRoot.SetParent(transform);
        chunksRoot.localPosition = Vector3.zero;
        chunksRoot.localRotation = Quaternion.identity;
        chunksRoot.localScale = Vector3.one;

        if (pools != null)
        {
            for (int i = 0; i < pools.Length; i++)
            {
                for (int j = 0; j < pools[i].Count; j++)
                {
                    ReleaseTemporaryVisual(pools[i][j]);
                }
            }
        }
    }
}