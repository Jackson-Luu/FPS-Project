using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerComponents : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayer = "DontDraw";

    [SerializeField]
    GameObject playerGraphics;

    [SerializeField]
    GameObject playerUIPrefab;
    [HideInInspector]
    public GameObject playerUIInstance;

    [SerializeField]
    private GameObject terrainGeneratorPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        } else
        {
            Player player = GetComponent<Player>();

            // Hide player model graphics from the player themselves
            Util.SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayer));

            // Create Player UI
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            // Configure Player UI
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if (ui == null) { Debug.LogError("No PlayerUI on PlayerUI Prefab."); }

            ui.SetPlayer(player);

            // Create Terrain Generator
            GameObject terrainObject = Instantiate(terrainGeneratorPrefab);
            TerrainGenerator terrainGenerator = terrainObject.GetComponent<TerrainGenerator>();
            terrainGenerator.viewer = gameObject.transform;
            terrainGenerator.SetPlayer(player);
            terrainGenerator.enabled = true;
            
            player.SetupPlayer();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        string netID = GetComponent<NetworkIdentity>().netId.ToString();

        GameManager.RegisterPlayer(netID, gameObject);
        if (isLocalPlayer)
        {
            CmdRegisterPlayer(netID, gameObject);
        }
    }

    [Command]
    void CmdRegisterPlayer(string netID, GameObject player)
    {
        GameManager.RegisterPlayer(netID, player);
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void OnDisable()
    {
        Destroy(playerUIInstance);

        GameManager.UnRegisterPlayer(transform.name);
    }
}
