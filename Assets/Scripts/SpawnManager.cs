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

    public override void OnStartServer()
    {
        //InvokeRepeating("PlayersToSpawnEnemy", 15.0f, spawnInterval);
        Invoke("PlayersToSpawnEnemy", 15.0f);
    }

    void PlayersToSpawnEnemy()
    {
        players = GameManager.GetAllPlayers();
        if (GameManager.instance.scene == "Royale")
        {
            foreach (GameObject player in players) {
                if (RoyaleManager.GetStatus(player.name) == Player.PlayerStatus.Alive) { SpawnEnemy(player); }
            }
        } else
        {
            foreach (GameObject player in players) { SpawnEnemy(player); }
        }
    }

    void SpawnEnemy(GameObject player)
    {
        int randomIndex = Random.Range(0, enemies.Length);
        Vector3 randomPosition = RandomPosition(player.transform.position);
        if (randomPosition != Vector3.down)
        {
            GameObject enemy = ObjectPooler.Instance.SpawnFromPool(enemies[randomIndex].name);
            if (enemy != null)
            {
                enemy.GetComponent<NavMeshAgent>().Warp(randomPosition);
                enemy.transform.rotation = enemies[randomIndex].transform.rotation;
                enemy.GetComponent<EnemyMove>().SetPlayer = player.gameObject;
                enemy.SetActive(true);
                NetworkServer.Spawn(enemy);
            }
        }
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
