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

    private float startDelay = 2.0f;
    private float spawnInterval = 4.0f;

    public override void OnStartServer()
    {
        xSpawnRange = zSpawnRange = (GameObject.Find("Ground").GetComponent<Renderer>().bounds.size.x / 2) - edgeMargin;
        InvokeRepeating("SpawnEnemy", startDelay, spawnInterval);
    }

    void SpawnEnemy()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(players.Length + " PLAYERS ONLINE");
        foreach (GameObject player in players)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemy = Instantiate(enemies[randomIndex], RandomPosition(randomIndex), enemies[randomIndex].transform.rotation);
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
