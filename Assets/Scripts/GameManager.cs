using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public const int MAP_SIZE = 6000;

    [SyncVar]
    public int seed = 0;

    public void OnDisable()
    {
        seed = -1;
    }

    [SyncVar]
    public bool sceneLoaded = false;

    [SyncVar]
    public string scene;

    public delegate void OnGameOverCallback(int minutes, string player);
    public OnGameOverCallback onGameOverCallback;

    [ClientRpc]
    public void RpcGameOver(int minutes, string player)
    {
        if (onGameOverCallback != null)
        {
            onGameOverCallback.Invoke(minutes, player);
        }
    }

    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject[] gameWeapons;

    private static Dictionary<string, GameObject> weapons = new Dictionary<string, GameObject>();

    public static GameObject GetWeapon(string weaponName)
    {
        return weapons[weaponName];
    }

    #region Terrain Tracking

    /// <summary> Keeps track of terrain chunks around players </summary>
    private static Dictionary<Vector2, int> terrainChunks = new Dictionary<Vector2, int>();

    /// <summary>
    /// Delegate for players syncing terrain chunks with server
    /// </summary>
    /// <param name="addChunk">True if adding chunk, false otherwise.</param>
    public delegate void ClientChangeTerrainCallback(Vector2 chunkCoord, bool addChunk);
    public ClientChangeTerrainCallback clientChangeTerrainCallback;

    /// <summary>
    /// Delegate for syncing observers with terrain items
    /// </summary>
    /// <param name="observer">Network connectionToClient for observer</param>
    /// <param name="addObserver">True if adding observers, false otherwise</param>
    public delegate void TerrainObserverCallback(Vector2 chunkCoord, NetworkConnection observer, bool addObserver);
    public TerrainObserverCallback terrainObserverCallback;

    public static void AddTerrainChunk(Vector2 chunkCoord, NetworkConnection observer)
    {
        if (!terrainChunks.ContainsKey(chunkCoord))
        {
            terrainChunks[chunkCoord] = 1;
            instance.clientChangeTerrainCallback.Invoke(chunkCoord, true);
        } else
        {
            terrainChunks[chunkCoord]++;
        }
        instance.terrainObserverCallback.Invoke(chunkCoord, observer, true);
    }

    public static void RemoveTerrainChunk(Vector2 chunkCoord, NetworkConnection observer)
    {
        if (terrainChunks.ContainsKey(chunkCoord))
        {
            terrainChunks[chunkCoord]--;
            instance.terrainObserverCallback.Invoke(chunkCoord, observer, false);
            if (terrainChunks[chunkCoord] == 0)
            {
                instance.clientChangeTerrainCallback.Invoke(chunkCoord, false);
                terrainChunks.Remove(chunkCoord);
            }
        }
    }

    public static int GetTerrainPlayerCount(Vector2 coord)
    {
        return terrainChunks[coord];
    }

    #endregion

    #region Singleton

    void Awake ()
    {
        // Ensure only one GameManager exists
        if (instance != null)
        {
            Debug.Log("Error: more than one GameManager in scene.");
            Destroy(gameObject);
            return;
        } else
        {
            instance = this;
            if (terrainChunks.Count > 0)
            {
                terrainChunks.Clear();
            }
            if (weapons.Count == 0)
            {
                for (int i = 0; i < gameWeapons.Length; i++)
                {
                    weapons.Add(gameWeapons[i].name, gameWeapons[i]);
                }
            }
            string[] sceneString = NetworkManager.networkSceneName.Split("/"[0]);
            scene = sceneString[2].Split("."[0])[0];
        }
    }

    public override void OnStartServer()
    {
        // Minimum 1 as zero is used to detect uninitialised seed
        seed = Random.Range(1, 999999);
        sceneLoaded = true;
    }

    #endregion

    #region Player Tracking

    private static Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    public static void RegisterPlayer(string playerID, GameObject player)
    {
        players.Add(playerID, player);
        player.transform.name = playerID;
        Debug.Log(instance.scene);
        if (instance.scene == "Royale")
        {
            RoyaleManager.AddPlayer(playerID);
        }
    }

    public static void UnRegisterPlayer(string playerID)
    {
        players.Remove(playerID);
    }

    public static GameObject GetPlayer(string playerID)
    {
        return players[playerID];
    }

    public static int GetPlayerCount()
    {
        return players.Count;
    }

    public static GameObject[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }

    #endregion
}
