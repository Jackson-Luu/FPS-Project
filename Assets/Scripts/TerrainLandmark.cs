using UnityEngine;
using Mirror;

public class TerrainLandmark : NetworkBehaviour
{
    [SerializeField]
    Transform[] itemSpawnPoints;

    bool server = false;

    public override void OnStartServer()
    {
        server = true;
    }

    void OnEnable()
    {
        if (server)
        {
            Debug.Log("TESTSTST");
            Random.InitState(GameManager.instance.seed + (int)(transform.position.x + transform.position.y));
            int length = ObjectPooler.Instance.items.Count;
            float meshWorldSize = ServerTerrainGenerator.instance.meshSettings.meshWorldSize;
            Vector2 chunkCoord = new Vector2(Mathf.RoundToInt(transform.position.x / meshWorldSize), Mathf.RoundToInt(transform.position.y / meshWorldSize));

            foreach (Transform item in itemSpawnPoints)
            {
                GameObject newItem = ObjectPooler.Instance.SpawnFromPool(ObjectPooler.Instance.items[Random.Range(0, length)].prefab.name);
                newItem.transform.position = item.position;
                newItem.transform.rotation = Quaternion.identity;
                newItem.transform.SetParent(item);
                newItem.SetActive(true);
                ServerTerrainGenerator.instance.addChunkItem(chunkCoord, newItem.GetComponent<ItemPickup>());

                NetworkServer.Spawn(newItem);
            }
        }
    }
}
