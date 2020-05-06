using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjectPooler : NetworkBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    public override void OnStartServer()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.name = pool.tag;
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag)
    {
        Queue<GameObject> q = poolDictionary[tag];
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        if (q.Count == 0)
        {
            GameObject obj = Instantiate(objectToSpawn);
            obj.SetActive(false);
            obj.name = tag;
            q.Enqueue(obj);
        }

        objectToSpawn.SetActive(true);
        return objectToSpawn;
    }

    public void ReturnToPool(GameObject objectToReturn)
    {
        poolDictionary[objectToReturn.name].Enqueue(objectToReturn);
        objectToReturn.SetActive(false);
    }
}
