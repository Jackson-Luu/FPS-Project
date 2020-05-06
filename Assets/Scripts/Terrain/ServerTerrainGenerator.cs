using UnityEngine;
using UnityEngine.AI;
using System.Collections;
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

    [SerializeField]
    private ObjectPooler objectPooler;

    private void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        GameManager.instance.clientChangeTerrainCallback += UpdateVisibleChunks;
        GameManager.instance.terrainObserverCallback += EditChunkObserver;
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
            foreach (ItemPickup item in terrainItemsDictionary[chunkCoord])
            {
                objectPooler.ReturnToPool(item.gameObject);
            }
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
        spawnManager.SpawnObjects(chunkCoord, mesh);
    }

    public void EditChunkObserver(Vector2 chunkCoord, NetworkConnection observer, bool addObserver)
    {
        if (!terrainItemsDictionary.ContainsKey(chunkCoord))
        {
            // Items have not finished being generated, wait for a bit
            StartCoroutine(WaitForChunk(chunkCoord, observer, addObserver));
        } else
        {
            EditObserver(chunkCoord, observer, addObserver);
        }
    }

    // Wait until items have been generated
    private IEnumerator WaitForChunk(Vector2 chunkCoord, NetworkConnection observer, bool addObserver)
    {
        while (!terrainItemsDictionary.ContainsKey(chunkCoord))
        {
            yield return new WaitForSeconds(1.0f);
        }

        EditObserver(chunkCoord, observer, addObserver);
    }

    // Add or remove observer to item
    private void EditObserver(Vector2 chunkCoord, NetworkConnection observer, bool addObserver)
    {
        foreach (ItemPickup item in terrainItemsDictionary[chunkCoord])
        {
            item.EditObservers(observer, addObserver);
        }
    }
}