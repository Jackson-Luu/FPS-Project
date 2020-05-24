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

    private bool firstSetup = true;

    [SerializeField]
    private GameObject[] disableGameObjectsOnDeath;

    private PlayerStats playerStats;
    private WeaponManager weaponManager;
    private AudioSource audioSource;

    [SerializeField]
    private Material localPlayerMaterial;
    [SerializeField]
    private Material localZombieMaterial;
    [SerializeField]
    private Renderer playerRenderer;
    [SerializeField]
    private Renderer zombieRenderer;

    public delegate void ZombifyPlayer();
    public ZombifyPlayer zombifyPlayer;

    public delegate void PlayerDied();
    public PlayerDied playerDied;

    [SerializeField]
    private PlayerShoot playerShoot;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponManager = GetComponent<WeaponManager>();
        audioSource = GetComponent<AudioSource>();

        if (isLocalPlayer)
        {
            // Set local player skins
            playerRenderer.material = localPlayerMaterial;
            zombieRenderer.material = localZombieMaterial;

            // Disable username canvas
            transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        }
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Die("Seppuku");
        }
    }

    */

    public void SetupPlayer()
    {
        // De-activate death screen
        if (isLocalPlayer)
        {
            GetComponent<PlayerComponents>().playerUIInstance.deathScreen.SetActive(false);
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
        if (GameManager.instance.scene == "Royale" && status == PlayerStatus.Dead)
        {
            status = PlayerStatus.Undead;
        } else
        {
            status = PlayerStatus.Alive;
        }

        playerStats.SetDefaults();
        weaponManager.currentWeapon.bullets = weaponManager.currentWeapon.maxBullets;

        // Enable player components
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            if (disableOnDeath[i] is PlayerShoot && status == PlayerStatus.Undead) { continue; }
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

        // Recalibrate weapon camera weapon type
        if (isLocalPlayer) { playerShoot.SetupPlayer(); }
    }

    public void Die(string sourceID)
    {
        if (getStatus == PlayerStatus.Dead) {
            return;
        } else if (getStatus == PlayerStatus.Alive)
        {
            if (GameManager.instance.scene == "Royale")
            {
                if (isServer)
                {
                    RoyaleManager.PlayerDied(netId.ToString());
                }

                Zombify();
            }
        }
        getStatus = PlayerStatus.Dead;

        if (isServer && playerDied != null)
        {
            playerDied.Invoke();
        }

        // Show kill feed event
        if (!isServer)
        {
            GameManager.instance.onPlayerKilledCallback.Invoke(gameObject.name, sourceID);
        }

        DisableComponents();

        // Activate death screen
        if (isLocalPlayer)
        {
            GetComponent<PlayerComponents>().playerUIInstance.deathScreen.SetActive(true);
        }

        StartCoroutine(Respawn());
    }

    private void Zombify()
    {
        playerRenderer.gameObject.SetActive(false);
        zombieRenderer.gameObject.SetActive(true);
        Animator playerAnimator = GetComponent<Animator>();
        playerAnimator.SetInteger("WeaponType_int", 0);
        playerAnimator.SetBool("Zombie_b", true);
        zombifyPlayer.Invoke();
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
