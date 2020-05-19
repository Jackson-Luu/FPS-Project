using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class ServerTerrainGenerator : NetworkBehaviour
{
    public static ServerTerrainGenerator instance = null;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    public TerrainChunk GetChunk(Vector2 chunkCoord)
    {
        return instance.terrainChunkDictionary[chunkCoord];
    }

    private NavMeshSurface navMeshSurface;
    bool navMeshBuilt = false;
    
    [SerializeField]
    private SpawnManager spawnManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Debug.Log("Duplicate server terrain generator.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        GameManager.instance.clientChangeTerrainCallback += UpdateVisibleChunks;
    }

    private void OnDestroy()
    {
        // Clear delegates
        if (GameManager.instance != null)
        {
            GameManager.instance.clientChangeTerrainCallback -= UpdateVisibleChunks;
        }
    }
    

    public void UpdateVisibleChunks(Vector2 chunkCoord, bool addChunk)
    {
        if (addChunk)
        {
            TerrainChunk newChunk = new TerrainChunk(chunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, null, null, true);
            newChunk.onMeshBuiltCallback += BuildMesh;
            terrainChunkDictionary.Add(chunkCoord, newChunk);
            newChunk.Load();
        } else
        {
            GameObject mesh = terrainChunkDictionary[chunkCoord].meshObject;
            // Return trees to object pool
            foreach (Transform child in mesh.transform)
            {
                ObjectPooler.Instance.ReturnToPool(child.gameObject);
            }

            Destroy(mesh);
            terrainChunkDictionary.Remove(chunkCoord);
        }
    }

    public void BuildMesh(Vector2 chunkCoord, Mesh mesh)
    {
        if (navMeshBuilt)
        {
            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        } else
        {
            navMeshSurface.BuildNavMesh();
            navMeshBuilt = true;
        }
    }
}