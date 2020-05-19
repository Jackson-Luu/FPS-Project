using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class TerrainChunk
{

    const float colliderGenerationDistanceThreshold = 5;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    public GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;
    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDst;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    bool server;
    public bool highestLODset = false;

    // Mesh delegate for server
    public delegate void OnMeshBuiltCallback(Vector2 chunkCoord, Mesh mesh);
    public OnMeshBuiltCallback onMeshBuiltCallback;

    // Highest LOD selected delegate for client
    public delegate void OnHighestLODCallback(Vector2 chunkCoord);
    public OnHighestLODCallback onHighestLODCallback;

    Vector2 regionSize;

    /// <summary>
    /// Delegate fired to signal items to add or remove an observer
    /// </summary>
    /// <param name="observer"></param>
    /// <param name="addObserver"> true if adding observer </param>
    public delegate void OnObserverChangedCallback(NetworkConnection observer, bool addObserver);
    public OnObserverChangedCallback onObserverChangedCallback;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material, bool server)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        this.server = server;
        this.regionSize = new Vector2(meshSettings.meshWorldSize, meshSettings.meshWorldSize);

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);


        meshObject = new GameObject("Terrain Chunk");
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;

        if (!server)
        {
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            SetVisible(false);
            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        } else
        {
            lodMeshes = new LODMesh[1];
            lodMeshes[0] = new LODMesh(detailLevels[0].lod);
            lodMeshes[0].updateCallback += ServerUpdateCallback;
        }
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        if (!this.server)
        {
            UpdateTerrainChunk();
        } else
        {
            lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
        }
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }


    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;

                        // Generate trees when switching to highest LOD mesh
                        if (lodIndex == 0)
                        {
                            if (onHighestLODCallback != null)
                            {
                                onHighestLODCallback.Invoke(coord);
                            }
                            SpawnTerrain();
                            highestLODset = true;
                        }
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            if (wasVisible != visible)
            {

                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void ServerUpdateCallback()
    {
        if (meshCollider == null)
        {
            return;
        }
        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
        SpawnTerrain();
        if (onMeshBuiltCallback != null)
        {
            onMeshBuiltCallback.Invoke(coord, lodMeshes[colliderLODIndex].mesh);
        }
    }

    void SpawnTerrain()
    {
        List<Vector2> points;
        int seedOffset = (int)(sampleCentre.x + sampleCentre.y);

        // Generate tree spawn points within each chunk
        Random.InitState(GameManager.instance.seed + seedOffset);

        points = PoissonDiscSample.GeneratePoints(Random.Range(10, 15), regionSize, seedOffset, 30);

        foreach (Vector2 point in points)
        {
            // Get world position and height
            int meshPosition = (int)point.y * (int)meshSettings.meshWorldSize + (int)point.x;

            Vector3 spawnPoint = lodMeshes[colliderLODIndex].mesh.vertices[meshPosition];
            spawnPoint.x += (coord.x * meshSettings.meshWorldSize);
            spawnPoint.z += (coord.y * meshSettings.meshWorldSize);
            spawnPoint.y -= 0.1f;

            GameObject spawnObject = ObjectPooler.Instance.RandomlySpawnFromPool(ObjectPooler.Instance.terrain, Random.value);
            spawnObject.transform.position = spawnPoint;
            //spawnObject.transform.rotation = Quaternion.identity;
            Util.AlignTransform(spawnObject.transform, lodMeshes[colliderLODIndex].mesh.normals[meshPosition]);
            spawnObject.SetActive(true);
            spawnObject.transform.parent = meshObject.transform;
        }

        SpawnLandmarks();
    }

    
    void SpawnLandmarks()
    {
        List<Vector2> points;
        int seedOffset = (int)(sampleCentre.x + sampleCentre.y);

        // Generate tree spawn points within each chunk
        Random.InitState(GameManager.instance.seed + seedOffset);

        points = PoissonDiscSample.GeneratePoints(Random.Range(70, 80), regionSize, seedOffset, 30);

        foreach (Vector2 point in points)
        {
            // Get world position and height
            int meshPosition = (int)point.y * (int)meshSettings.meshWorldSize + (int)point.x;

            Vector3 spawnPoint = lodMeshes[colliderLODIndex].mesh.vertices[meshPosition];
            spawnPoint.x += (coord.x * meshSettings.meshWorldSize);
            spawnPoint.z += (coord.y * meshSettings.meshWorldSize);

            GameObject spawnObject = ObjectPooler.Instance.SpawnFromPool("Campfire");
            spawnObject.transform.position = spawnPoint;
            Util.AlignTransform(spawnObject.transform, lodMeshes[colliderLODIndex].mesh.normals[meshPosition]);
            spawnObject.SetActive(true);
            spawnObject.transform.parent = meshObject.transform;
            if (server)
            {
                spawnObject.GetComponent<TerrainLandmark>().Setup(coord);
            }
        }
    }
    

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
        if (!visible)
        {
            // Return trees to object pool
            foreach (Transform child in meshObject.transform)
            {
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
                Debug.Log("Returning Tree");
            }
        }
    } 

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

}

class LODMesh
{

    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }

}