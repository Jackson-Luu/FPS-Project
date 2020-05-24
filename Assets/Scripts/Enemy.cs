using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour
{
    private ObjectPooler objectPooler;
    private EnemyStats enemyStats;

    [SerializeField]
    private GameObject enemyGFX;

    private bool firstSetup = true;

    void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
    }

    // Enable used instead of due to object pooling reusing units
    private void OnEnable()
    {
        Enable();
    }

    void Start()
    {
        objectPooler = ObjectPooler.Instance;
    }

    private void Enable()
    {
        enemyStats.SetDefaults();
    }

    // Enemy disabled on clientrpc call then returned to object pool
    [Server]
    public void Die(string sourceID)
    {
        objectPooler.ReturnToPool(gameObject);
        NetworkServer.UnSpawn(gameObject);
    }
}
