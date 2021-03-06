﻿using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager instance = null;

    public GameObject[] enemies;
    public GameObject[] items;

    [HideInInspector]
    public GameObject[] players;

    private System.Guid[] enemyIds;
    public System.Guid[] itemIds;
    public Dictionary<System.Guid, GameObject> prefabs;

    public delegate GameObject SpawnDelegate(Vector3 position, System.Guid assetId);
    public delegate void UnSpawnDelegate(GameObject spawned);

    // Enemy spawning variables
    private float spawnRadius = 50;
    private float spawnInterval = 10.0f;

    void Awake()
    {
        if (instance != null) { Destroy(instance); }
        instance = this;

        enemyIds = new System.Guid[enemies.Length];
        prefabs = new Dictionary<System.Guid, GameObject>();
        for (int i = 0; i < enemies.Length; i++)
        {
            System.Guid assetId = enemies[i].GetComponent<NetworkIdentity>().assetId;
            enemyIds[i] = assetId;
            prefabs.Add(assetId, enemies[i]);
            ClientScene.RegisterSpawnHandler(assetId, SpawnObject, UnSpawnObject);
        }

        itemIds = new System.Guid[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            System.Guid assetId = items[i].GetComponent<NetworkIdentity>().assetId;
            itemIds[i] = assetId;
            prefabs.Add(assetId, items[i]);
            ClientScene.RegisterSpawnHandler(assetId, SpawnObject, UnSpawnObject);
        }
    }

    public override void OnStartServer()
    {
        InvokeRepeating("PlayersToSpawnEnemy", 20.0f, spawnInterval);
        //Invoke("PlayersToSpawnEnemy", 20.0f);
    }

    void PlayersToSpawnEnemy()
    {
        players = GameManager.GetAllPlayers();

        System.Guid randomId = enemyIds[Random.Range(0, enemies.Length)];
        if (GameManager.instance.isRoyale)
        {
            foreach (GameObject player in players) {
                if (RoyaleManager.GetStatus(player.name) == Player.PlayerStatus.Alive) { SpawnEnemy(player, randomId); }
            }
        } else
        {
            foreach (GameObject player in players) { SpawnEnemy(player, randomId); }
        }
    }

    void SpawnEnemy(GameObject player, System.Guid assetId)
    {
        Vector3 randomPosition = RandomPosition(player.transform.position);
        if (randomPosition != Vector3.down)
        {
            GameObject enemy = ObjectPooler.Instance.SpawnFromPool(prefabs[assetId].name);
            if (enemy != null)
            {
                enemy.GetComponent<NavMeshAgent>().Warp(randomPosition);
                enemy.transform.rotation = prefabs[assetId].transform.rotation;
                enemy.GetComponent<EnemyMove>().SetPlayer = player.gameObject;
                enemy.SetActive(true);
                NetworkServer.Spawn(enemy, assetId);
            }
        }
    }

    public GameObject SpawnObject(Vector3 position, System.Guid assetId)
    {
        return ObjectPooler.Instance.SpawnFromPool(prefabs[assetId].name);
    }

    public void UnSpawnObject(GameObject spawned)
    {
        ObjectPooler.Instance.ReturnToPool(spawned);
    }

    // Calculate random position
    private Vector3 RandomPosition(Vector3 position)
    {
        float randomX = Random.Range(position.x  - spawnRadius, position.x + spawnRadius);
        float randomZ = Random.Range(position.z - spawnRadius, position.z + spawnRadius);

        // Shoot raycast to find terrain height
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(randomX, 500, randomZ), Vector3.down, out hit, 600))
        {
            return new Vector3(randomX, hit.point.y + 0.01f, randomZ);
        }

        // if raycast did not hit return -1 vector
        return Vector3.down;
    }
}
