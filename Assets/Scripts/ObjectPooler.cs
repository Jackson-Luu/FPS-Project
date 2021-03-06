﻿using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjectPooler : NetworkBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public float probability;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> items;
    public List<Pool> terrain;
    public List<Pool> enemies;
    public List<Pool> terrainLandmarks;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    private float[] probs;
    private float probTotal = 0;

    public static ObjectPooler Instance;

    private void Awake()
    {
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in items)
        {
            InitPool(pool);
        }

        foreach (Pool pool in enemies)
        {
            InitPool(pool);
        }
        AddTerrainPools();
    }

    private void AddTerrainPools()
    {
        probs = new float[terrain.Count];
        int index = 0;
        foreach (Pool pool in terrain)
        {
            InitPool(pool);
            probs[index] = pool.probability;
            index++;
        }

        foreach (Pool pool in terrainLandmarks)
        {
            InitPool(pool);
        }

        for (int i = 0; i < probs.Length; i++)
        {
            probTotal += probs[i];
        }
    }

    private void InitPool(Pool pool)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();

        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj = Instantiate(pool.prefab);
            obj.SetActive(false);
            obj.name = pool.prefab.name;
            objectPool.Enqueue(obj);
        }

        poolDictionary.Add(pool.prefab.name, objectPool);
    }

    public GameObject SpawnFromPool(string tag)
    {
        Queue<GameObject> q = poolDictionary[tag];
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        if (q.Count == 0)
        {
            //Debug.Log("Pool empty! Adding extra: " + tag);
            GameObject obj = Instantiate(objectToSpawn);
            obj.SetActive(false);
            obj.name = tag;
            q.Enqueue(obj);
        }

        return objectToSpawn;
    }

    public GameObject RandomlySpawnFromPool(List<Pool> pool, float random)
    {
        return SpawnFromPool(pool[ChooseProbability(random)].prefab.name);
    }

    public GameObject IndexSpawnFromPool(List<Pool> pool, int index)
    {
        return SpawnFromPool(pool[index].prefab.name);
    }

    public void ReturnToPool(GameObject objectToReturn)
    {
        poolDictionary[objectToReturn.name].Enqueue(objectToReturn);
        objectToReturn.SetActive(false);
        objectToReturn.transform.SetParent(null);
    }

    private int ChooseProbability(float random)
    {
        float randomPoint = random * probTotal;

        for (int i = 0; i < probs.Length; i++)
        {
            if (randomPoint < probs[i])
            {
                return i;
            }
            else
            {
                randomPoint -= probs[i];
            }
        }
        return probs.Length - 1;
    }
}
