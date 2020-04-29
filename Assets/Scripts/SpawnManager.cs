using System.Collections.Generic;
using UnityEngine;
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
    private float radius = 40;
    private Vector2 regionSize = new Vector2(1000, 1000);
    private int rejectionSamples = 30;
    private float offset = 1000;

    private ObjectPooler objectPooler;

    [SerializeField]
    private Transform map;

    public override void OnStartServer()
    {
        objectPooler = ObjectPooler.Instance;
        //InvokeRepeating("SpawnEnemy", GameManager.instance.matchSettings.playerLoadTime, spawnInterval);
        Invoke("SpawnObjects", 20.0f);
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
        points = PoissonDiscSample.GeneratePoints(radius, regionSize, rejectionSamples);
        Terrain terrain;
        int k = 0;
        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                terrain = map.GetChild(k).GetComponent<Terrain>();
                foreach (Vector2 point in points)
                {
                    GameObject objectPrefab = objects[Random.Range(0, objects.Length)];
                    Vector3 spawnPoint = new Vector3(point.x + (j * offset), 0, point.y + (i * offset));
                    spawnPoint.y = terrain.SampleHeight(spawnPoint) + 0.1f;
                    GameObject spawnObject = Instantiate(objectPrefab, spawnPoint, objectPrefab.transform.rotation);
                    AlignTransform(spawnObject.transform, terrain);
                    NetworkServer.Spawn(spawnObject);
                }
                k++;
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

    // Find terrain height at a specific world position
    float RaycastHeight(Vector3 position)
    {
        RaycastHit hit;
        //Raycast up to terrain
        if (Physics.Raycast(position, Vector3.up, out hit, 1000f))
        {
            Debug.Log(hit.point.y);
            return hit.point.y;
        }
        return 0;
    }

    // Rotate objects to align with terrain
    private void AlignTransform(Transform transform, Terrain terrain)
    {
        Vector3 sample = SampleNormal(transform.position, terrain);

        Vector3 proj = transform.forward - (Vector3.Dot(transform.forward, sample)) * sample;
        transform.rotation = Quaternion.LookRotation(proj, sample);
    }

    private Vector3 SampleNormal(Vector3 position, Terrain terrain)
    {
        var terrainLocalPos = position - terrain.transform.position;
        var normalizedPos = new Vector2(
            Mathf.InverseLerp(0f, terrain.terrainData.size.x, terrainLocalPos.x),
            Mathf.InverseLerp(0f, terrain.terrainData.size.z, terrainLocalPos.z)
        );
        var terrainNormal = terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.y);

        return terrainNormal;
    }
}
