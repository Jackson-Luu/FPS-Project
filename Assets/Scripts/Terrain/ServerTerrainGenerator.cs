using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Mirror;

public class ServerTerrainGenerator : NetworkBehaviour
{
    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    [HideInInspector]
    public int chunkRadius;

    int chunksBuilt = 0;

    // Finish building terrain delegate
    public delegate void TerrainBuiltCallback();
    public TerrainBuiltCallback terrainBuiltCallback;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    Dictionary<Vector2, List<ItemPickup>> terrainItemsDictionary = new Dictionary<Vector2, List<ItemPickup>>();

    public void addChunkItems(Vector2 chunkCoord, List<ItemPickup> itemsList)
    {
        terrainItemsDictionary[chunkCoord] = itemsList;
    }

    private NavMeshSurface navMeshSurface;
    bool navMeshBuilt = false;
    
    [SerializeField]
    private SpawnManager spawnManager;

    private void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
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
            Destroy(terrainChunkDictionary[chunkCoord].meshObject);
            terrainChunkDictionary.Remove(chunkCoord);
            terrainItemsDictionary.Remove(chunkCoord);
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
        List<ItemPickup> itemsList = new List<ItemPickup>();
        spawnManager.SpawnObjects(chunkCoord, mesh, itemsList);
    }
}