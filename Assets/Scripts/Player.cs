using System.Collections;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerComponents))]
public class Player : NetworkBehaviour
{
    [SyncVar]
    private bool dead = false;
    public bool isDead
    {
        get { return dead; }
        protected set { dead = value; }
    }

    [SerializeField]
    private float maxHealth = 100f;

    [SyncVar]
    private float currentHealth;

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    private bool firstSetup = true;

    public void SetupPlayer()
    {
        // De-activate death screen
        if (isLocalPlayer)
        {
            GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>().deathScreen.SetActive(false);
        }

        CmdBroadcastNewPlayerSetup();
    }

    
    void Update()
    {
    	if (!isLocalPlayer)
    		return;

    	if (Input.GetKeyDown(KeyCode.K))
    	{
    		RpcTakeDamage(99999);
    	}
    }
    

    [Command]
    private void CmdBroadcastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }

            firstSetup = false;
        }

        setDefaults();
    }

    public void setDefaults()
    {
        isDead = false;
        currentHealth = maxHealth;

        // Enable player components
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        // Enable player gameObjects
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
        }

        // Enable collider (colliders are not behaviours hence the special case)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(float amount)
    {
        if (isDead) { return; }

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Disable components
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        // Disable player gameObjects
        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Activate death screen
        if (isLocalPlayer)
        {
            GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>().deathScreen.SetActive(true);
        }

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        // Reactivate playerController after position has been set to avoid reverting to pre-death position
        yield return new WaitForSeconds(0.1f);
        SetupPlayer();
    }
}
