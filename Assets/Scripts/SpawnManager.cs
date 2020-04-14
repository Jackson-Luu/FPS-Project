using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] enemies;
    public GameObject[] powerups;
    public GameObject[] players;

    private float xSpawnRange;
    private float zSpawnRange;
    private float groundRadius;
    private float edgeMargin = 5;

    private float startDelay = 5.0f;
    private float spawnInterval = 8.0f;

    private ObjectPooler objectPooler;

    public override void OnStartServer()
    {
        objectPooler = ObjectPooler.Instance;
        xSpawnRange = zSpawnRange = (GameObject.Find("Ground").GetComponent<Renderer>().bounds.size.x / 2) - edgeMargin;
        InvokeRepeating("SpawnEnemy", startDelay, spawnInterval);
    }

    void SpawnEnemy()
    {
        players = GameManager.GetAllPlayers();
        foreach (GameObject player in players)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemy = objectPooler.SpawnFromPool(enemies[randomIndex].name, RandomPosition(randomIndex), enemies[randomIndex].transform.rotation);
            enemy.GetComponent<EnemyMove>().SetPlayer = player.gameObject;
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
