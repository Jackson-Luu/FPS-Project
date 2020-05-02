using UnityEngine;
using Mirror;

public class ServerTerrainGenerator : NetworkBehaviour
{
    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    [HideInInspector]
    public int chunkRadius;

    private void Start()
    {
        UpdateVisibleChunks();    
    }

    public void UpdateVisibleChunks()
    {
        //int chunkRadius = Mathf.CeilToInt(GameManager.MAP_SIZE / meshSettings.meshWorldSize / 2); 
        chunkRadius = 3;
        for (int yOffset = -chunkRadius; yOffset <= chunkRadius; yOffset++)
        {
            for (int xOffset = -chunkRadius; xOffset <= chunkRadius; xOffset++)
            {
                TerrainChunk newChunk = new TerrainChunk(new Vector2(xOffset, yOffset), heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, null, null, true);
                newChunk.Load();
            }
        }
    }
}