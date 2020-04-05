using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] enemies;
    public GameObject[] powerups;

    private float xSpawnRange;
    private float zSpawnRange;
    private float groundRadius;
    private float edgeMargin = 5;

    private float startDelay = 2.0f;
    private float spawnInterval = 2.0f;

    // Start is called before the first frame update
    public override void OnStartServer()
    {
        xSpawnRange = zSpawnRange = (GameObject.Find("Ground").GetComponent<Renderer>().bounds.size.x / 2) - edgeMargin;
        InvokeRepeating("SpawnEnemy", startDelay, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, enemies.Length);
        float randomX = Random.Range(-xSpawnRange, xSpawnRange);
        float randomZ = Random.Range(-zSpawnRange, zSpawnRange);
        Vector3 spawnPos = new Vector3(randomX, enemies[randomIndex].transform.position.y, randomZ);

        GameObject enemy = Instantiate(enemies[randomIndex], spawnPos, enemies[randomIndex].transform.rotation);
        //NetworkServer.Spawn(enemy);
    }
}
