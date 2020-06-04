using UnityEngine;
using Mirror;

public class TerrainLandmark : NetworkBehaviour
{
    [SerializeField]
    Transform[] itemSpawnPoint;

    LayerMask layerMask;

    private void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Terrain");
    }

    public void Setup(Vector2 chunkCoord)
    {
        if (GameManager.instance.server)
        {
            System.Random prng = new System.Random(GameManager.instance.seed + (int)(transform.position.x + transform.position.y));
            var pool = ObjectPooler.Instance.items;
            foreach (Transform item in itemSpawnPoint)
            {
                int random = prng.Next(0, pool.Count);
                GameObject newItem = ObjectPooler.Instance.IndexSpawnFromPool(pool, random);
                newItem.transform.rotation = item.rotation;
                if (newItem.CompareTag("Weapon"))
                {
                    newItem.transform.Rotate(new Vector3(0, 0, 90));
                    newItem.transform.position = item.position + new Vector3(0, 0.05f, 0);
                } else
                {
                    newItem.transform.position = item.position;
                }
                newItem.SetActive(true);

                NetworkServer.Spawn(newItem, SpawnManager.instance.itemIds[random]);
                ItemPickup itemPickup = newItem.GetComponent<ItemPickup>();
                ServerTerrainGenerator.instance.GetChunk(chunkCoord).onObserverChangedCallback += itemPickup.EditObservers;
                foreach (NetworkConnection observer in GameManager.GetObservers(chunkCoord))
                {
                    itemPickup.EditObservers(observer, true);
                }
                newItem.transform.SetParent(item);
            }
        }
    }

    private void OnEnable()
    {
        ClearSpace();
    }

    void ClearSpace()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 5, 1 << LayerMask.NameToLayer("Terrain"));
        foreach (Collider collider in hitColliders)
        {
            ObjectPooler.Instance.ReturnToPool(collider.gameObject);
        }
    }

    private void OnDisable()
    {
        foreach (Transform item in itemSpawnPoint)
        {
            if (item.childCount > 0)
            {
                ObjectPooler.Instance.ReturnToPool(item.GetChild(0).gameObject);
            }
        }
    }
}
