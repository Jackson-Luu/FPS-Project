using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] enemies;
    public GameObject[] players;
    public GameObject[] objects;

    // Enemy spawning variables
    private float spawnRadius = 50;
    private float spawnInterval = 15.0f;

    // Object spawning variables (poisson)
    private float radius = 10;
    private Vector2 regionSize = new Vector2(120, 120);
    private int rejectionSamples = 30;
    private int offset = 120;

    private ObjectPooler objectPooler;

    [SerializeField]
    private ServerTerrainGenerator serverTerrainGenerator;

    public override void OnStartServer()
    {
        objectPooler = ObjectPooler.Instance;
        //InvokeRepeating("SpawnEnemy", GameManager.instance.matchSettings.playerLoadTime, spawnInterval);
        Invoke("SpawnObjects", 5.0f);
    }

    void SpawnEnemy()
    {     
        players = GameManager.GetAllPlayers();        
        foreach (GameObject player in players)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemy = objectPooler.SpawnFromPool(enemies[randomIndex].name, RandomPosition(player.transform), enemies[randomIndex].transform.rotation);
            if (enemy != null)
            {
                enemy.GetComponent<EnemyMove>().SetPlayer = player.gameObject;
                NetworkServer.Spawn(enemy);
            }
        }
    }

    void SpawnObjects()
    {
        List<Vector2> points;
        int chunks = serverTerrainGenerator.chunkRadius;

        for (int i = -chunks; i <= chunks; i++)
        {
            for (int j = -chunks; j <= chunks; j++)
            {
                // Generate item spawn points within each chunk
                points = PoissonDiscSample.GeneratePoints(radius, regionSize, rejectionSamples);

                foreach (Vector2 point in points)
                {
                    float xPos = point.x + (i * offset);
                    float zPos = point.y + (j * offset);

                    // Pick random item to spawn
                    GameObject objectPrefab = objects[Random.Range(0, objects.Length)];

                    // Shoot raycast to find terrain height and align item to terrain incline
                    RaycastHit hit;
                    Physics.Raycast(new Vector3(xPos, 300, zPos), Vector3.down, out hit, 600);

                    Vector3 spawnPoint = new Vector3(xPos, hit.point.y, zPos);
                    GameObject spawnObject = Instantiate(objectPrefab, spawnPoint, objectPrefab.transform.rotation);
                    AlignTransform(spawnObject.transform, hit.normal);
                    NetworkServer.Spawn(spawnObject);
                }
                Debug.Log(i + " | " + j);
            }
        }
    }

    // Calculate random position
    private Vector3 RandomPosition(Transform player)
    {
        float randomX = Random.Range(player.position.x  - spawnRadius, player.position.x + spawnRadius);
        float randomZ = Random.Range(player.position.z - spawnRadius, player.position.z + spawnRadius);
        Vector3 position = new Vector3(randomX, 0, randomZ);
        position.y = Terrain.activeTerrain.SampleHeight(position);
        return position;
    }

    // Rotate objects to align with terrain
    private void AlignTransform(Transform transform, Vector3 normal)
    {
        Vector3 proj = transform.forward - (Vector3.Dot(transform.forward, normal)) * normal;
        transform.rotation = Quaternion.LookRotation(proj, normal);
    }
}
