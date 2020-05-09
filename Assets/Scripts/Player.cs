using System.Collections;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerComponents))]
public class Player : NetworkBehaviour
{
    [SyncVar]
    private PlayerStatus status = PlayerStatus.Alive;
    public PlayerStatus getStatus
    {
        get { return status; }
        protected set { status = value; }
    }

    [SerializeField]
    private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    private bool firstSetup = true;

    private PlayerStats playerStats;
    private WeaponManager weaponManager;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponManager = GetComponent<WeaponManager>();
    }

    public void SetupPlayer()
    {
        // De-activate death screen
        if (isLocalPlayer)
        {
            GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>().deathScreen.SetActive(false);
        }

        if (!isServer)
        {
            CmdBroadcastNewPlayerSetup();
        }
    }

    // Setup remote players on server
    [Command]
    private void CmdBroadcastNewPlayerSetup()
    {
        RpcSetupPlayerOnAllClients();
        SetupRemotePlayer();
    }

    // Setup remote players on other clients
    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        SetupRemotePlayer();
    }

    private void SetupRemotePlayer()
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
        getStatus = PlayerStatus.Alive;
        playerStats.SetDefaults();
        weaponManager.GetCurrentWeapon().bullets = weaponManager.GetCurrentWeapon().maxBullets;

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

    public void Die(string sourceID)
    {
        if (getStatus == PlayerStatus.Dead) {
            return;
        } else if (getStatus == PlayerStatus.Alive)
        {
            if (isServer && GameManager.instance.scene == "Royale")
            {
                RoyaleManager.PlayerDied();
            }
        }
        getStatus = PlayerStatus.Dead;

        // Show kill feed event
        if (!isServer)
        {
            GameManager.instance.onPlayerKilledCallback.Invoke(gameObject.name, sourceID);
        }

        DisableComponents();

        // Activate death screen
        if (isLocalPlayer)
        {
            GetComponent<PlayerComponents>().playerUIInstance.GetComponent<PlayerUI>().deathScreen.SetActive(true);
        }

        StartCoroutine(Respawn());
    }

    public void DisableComponents()
    {
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
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        // Reactivate playerController after position has been set to avoid reverting to pre-death position
        // yield return new WaitForSeconds(0.1f);

        SetupPlayer();
    }

    public void TakeItem(GameObject itemObject)
    {
        CmdPickup(itemObject);
    }

    [Command]
    void CmdPickup(GameObject itemObject)
    {
        if (itemObject != null)
        {
            ItemPickup pickup = itemObject.GetComponent<ItemPickup>();
            if (GetComponent<Inventory>().Add(pickup.item))
            {
                RpcAddToInventory(itemObject);
                pickup.Despawn();
            }
        }
    }

    [ClientRpc]
    public void RpcAddToInventory(GameObject itemObject)
    {
        GetComponent<Inventory>().Add(itemObject.GetComponent<ItemPickup>().item);
    }

    [Command]
    public void CmdAddTerrainChunk(Vector2 coord)
    {
        GameManager.AddTerrainChunk(coord, connectionToClient);
    }

    [Command]
    public void CmdRemoveTerrainChunk(Vector2 coord)
    {
        GameManager.RemoveTerrainChunk(coord, connectionToClient);
    }

    public enum PlayerStatus { Alive, Dead, Undead }
}
