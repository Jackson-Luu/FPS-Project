using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] enemies;
    public GameObject[] powerups;
    public GameObject[] players;

    private float spawnRadius = 50;
    private float groundRadius;
    private float edgeMargin = 5;

    private float startDelay = 5.0f;
    private float spawnInterval = 8.0f;

    private ObjectPooler objectPooler;

    public override void OnStartServer()
    {
        objectPooler = ObjectPooler.Instance;
        InvokeRepeating("SpawnEnemy", startDelay, spawnInterval);
    }

    void SpawnEnemy()
    {
        players = GameManager.GetAllPlayers();
        foreach (GameObject player in players)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemy = objectPooler.SpawnFromPool(enemies[randomIndex].name, RandomPosition(player.transform, randomIndex), enemies[randomIndex].transform.rotation);
            if (enemy != null)
            {
                enemy.GetComponent<EnemyMove>().SetPlayer = player.gameObject;
                NetworkServer.Spawn(enemy);
            }
        }
    }

    private Vector3 RandomPosition(Transform player, int enemy)
    {
        float randomX = Random.Range(player.position.x  - spawnRadius, player.position.x + spawnRadius);
        float randomZ = Random.Range(player.position.z - spawnRadius, player.position.z + spawnRadius);
        Vector3 position = new Vector3(randomX, 0, randomZ);
        position.y = Terrain.activeTerrain.SampleHeight(position);
        Debug.Log(position);
        return position;
    }
}
