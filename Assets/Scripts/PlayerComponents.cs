﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
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

            SetupClientOnlyObjects();
            if (SceneManager.GetActiveScene().name == "Game")
            {
                SceneManager.activeSceneChanged += SceneChange;
            }

            player.SetupPlayer();
        }
    }

    private void SceneChange(Scene current, Scene next)
    {
        if (next.name == "Game")
        {
            GameManager.instance.onSeedGeneratedCallback += SetupClientOnlyObjects;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            StartCoroutine(LoadRoomPlayer());
        }
    }

    private IEnumerator LoadRoomPlayer()
    {
        PlayerController pC = GetComponent<PlayerController>();
        CharacterController cC = GetComponent<CharacterController>();
        pC.enabled = false;
        cC.enabled = false;
        Vector3 position = new Vector3(transform.position.x, 300, transform.position.z);
        transform.position = position;
        CmdMovePlayer(position);
        yield return new WaitForSeconds(GameManager.instance.matchSettings.playerLoadTime);
        pC.enabled = true;
        cC.enabled = true;

        CmdRegisterPlayer(netId.ToString(), gameObject);
    }

    [Command]
    void CmdMovePlayer(Vector3 position)
    {
        transform.position = position;
    }

    void SetupClientOnlyObjects()
    {
        Player player = GetComponent<Player>();

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

        if (GameManager.instance.onSeedGeneratedCallback != null)
        {
            GameManager.instance.onSeedGeneratedCallback -= SetupClientOnlyObjects;
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
