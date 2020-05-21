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
                GameObject newItem = ObjectPooler.Instance.IndexSpawnFromPool(pool, prng.Next(0, pool.Count));
                newItem.transform.position = item.position;
                newItem.transform.Rotate(transform.rotation.eulerAngles);
                newItem.SetActive(true);

                NetworkServer.Spawn(newItem);
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
