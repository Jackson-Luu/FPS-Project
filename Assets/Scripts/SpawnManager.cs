using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] enemies;
    public GameObject[] powerups;

    //[SerializeField]
    //private GameManager gameManager;

    private float xSpawnRange;
    private float zSpawnRange;
    private float groundRadius;
    private float edgeMargin = 5;

    private float startDelay = 2.0f;
    private float spawnInterval = 4.0f;

    public override void OnStartServer()
    {
        xSpawnRange = zSpawnRange = (GameObject.Find("Ground").GetComponent<Renderer>().bounds.size.x / 2) - edgeMargin;
        InvokeRepeating("SpawnEnemy", startDelay, spawnInterval);
    }

    void SpawnEnemy()
    {
        Debug.Log(GameManager.GetPlayerCount() + "PLAYERS ONLINE");
        foreach (KeyValuePair<string, Player> entry in GameManager.instance.GetPlayers)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemy = Instantiate(enemies[randomIndex], RandomPosition(randomIndex), enemies[randomIndex].transform.rotation);
            enemy.GetComponent<EnemyMove>().SetPlayer = entry.Value.gameObject;
            NetworkServer.Spawn(enemy);
        }
    }

    private Vector3 RandomPosition(int enemy)
    {
        float randomX = Random.Range(-xSpawnRange, xSpawnRange);
        float randomZ = Random.Range(-zSpawnRange, zSpawnRange);
        return new Vector3(randomX, enemies[enemy].transform.position.y, randomZ);
    }
}
