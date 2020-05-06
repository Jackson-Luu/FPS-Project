using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] enemies;
    public GameObject[] players;
    public GameObject[] objects;

    // Enemy spawning variables
    private float spawnRadius = 50;
    private float spawnInterval = 15.0f;

    // Object spawning variables (poisson)
    private float radius = 20;
    private Vector2 regionSize = new Vector2(120, 120);
    private int rejectionSamples = 30;
    private int offset;
    private int chunkSize;

    private ObjectPooler objectPooler;

    [SerializeField]
    private ServerTerrainGenerator serverTerrainGenerator;

    public override void OnStartServer()
    {
        objectPooler = ObjectPooler.Instance;
        chunkSize = (int)serverTerrainGenerator.meshSettings.meshWorldSize;
        offset = chunkSize / 2;
        InvokeRepeating("SpawnEnemy", 15.0f, spawnInterval);
    }

    void SpawnEnemy()
    {
        players = GameManager.GetAllPlayers();        
        foreach (GameObject player in players)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            Vector3 randomPosition = RandomPosition(player.transform);
            if (randomPosition != Vector3.down)
            {
                GameObject enemy = objectPooler.SpawnFromPool(enemies[randomIndex].name);
                if (enemy != null)
                {
                    enemy.GetComponent<NavMeshAgent>().Warp(randomPosition);
                    enemy.transform.rotation = enemies[randomIndex].transform.rotation;
                    enemy.GetComponent<EnemyMove>().SetPlayer = player.gameObject;
                    enemy.GetComponent<EnemyMove>().enabled = true;
                    NetworkServer.Spawn(enemy);
                }
            }
        }
    }

    public void SpawnObjects(Vector2 chunkCoord, Mesh mesh)
    {
        List<Vector2> points;
        List<ItemPickup> terrainItemsList = new List<ItemPickup>();

        // Generate item spawn points within each chunk
        points = PoissonDiscSample.GeneratePoints(radius, regionSize, rejectionSamples);

        foreach (Vector2 point in points)
        {
            // Get world position and height
            int meshPosition = (int)point.y * chunkSize + (int)point.x;
            float xPos = mesh.vertices[meshPosition].x + (chunkCoord.x * chunkSize);
            float zPos = mesh.vertices[meshPosition].z + (chunkCoord.y * chunkSize);

            Vector3 spawnPoint = new Vector3(xPos, mesh.vertices[meshPosition].y, zPos);

            // Pick random item to spawn
            int randomIndex = Random.Range(0, objects.Length);
            GameObject spawnObject = objectPooler.SpawnFromPool(objects[randomIndex].name);
            spawnObject.transform.position = spawnPoint;
            spawnObject.transform.rotation = objects[randomIndex].transform.rotation;
            AlignTransform(spawnObject.transform, mesh.normals[meshPosition]);
            NetworkServer.Spawn(spawnObject);
            terrainItemsList.Add(spawnObject.GetComponent<ItemPickup>());
        }

        serverTerrainGenerator.addChunkItems(chunkCoord, terrainItemsList);
    }

    // Calculate random position
    private Vector3 RandomPosition(Transform player)
    {
        float randomX = Random.Range(player.position.x  - spawnRadius, player.position.x + spawnRadius);
        float randomZ = Random.Range(player.position.z - spawnRadius, player.position.z + spawnRadius);

        // Shoot raycast to find terrain height
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(randomX, 500, randomZ), Vector3.down, out hit, 600))
        {
            return new Vector3(randomX, hit.point.y + 0.01f, randomZ);
        }

        // if raycast did not hit return -1 vector
        return Vector3.down;
    }

    // Rotate objects to align with terrain
    private void AlignTransform(Transform transform, Vector3 normal)
    {
        Vector3 proj = transform.forward - (Vector3.Dot(transform.forward, normal)) * normal;
        transform.rotation = Quaternion.LookRotation(proj, normal);
    }
}
