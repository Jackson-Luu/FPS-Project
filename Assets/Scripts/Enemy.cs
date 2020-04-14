using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour
{
    private ObjectPooler objectPooler;
    private EnemyStats enemyStats;

    [SerializeField]
    private GameObject enemyGFX;

    private bool firstSetup = true;

    void Start()
    {
        if (isServer)
        {
            objectPooler = ObjectPooler.Instance;
        }
        enemyStats = GetComponent<EnemyStats>();
    }

    // Enable used instead of due to object pooling reusing units
    private void OnEnable()
    {
        Enable();

        if (isServer)
        {
            RpcEnable();
        }
    }

    [ClientRpc]
    private void RpcEnable()
    {
        gameObject.SetActive(true);
    }

    private void Enable()
    {
        // Initial enable interaction with object pooler enabling bug
        if (firstSetup) {
            firstSetup = false;
            return;
        }

        // Enable enemy graphics and collider
        enemyGFX.SetActive(true);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        enemyStats.SetDefaults();
    }

    // Enemy disabled on clientrpc call then returned to object pool
    public void Die(string sourceID)
    {
        Disable();
        if (isServer)
        {
            RpcDie();
            objectPooler.ReturnToPool(gameObject);
        }
    }

    [ClientRpc]
    public void RpcDie()
    {
        Disable();
        gameObject.SetActive(false);
    }

    public void Disable()
    {
        // Disable enemy graphics and collider
        enemyGFX.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}
