using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public const int MAP_SIZE = 6000;

    public MatchSettings matchSettings;

    [SerializeField]
    private GameObject[] gameWeapons;

    public delegate void OnPlayerKilledCallback(string player, string source);
    public OnPlayerKilledCallback onPlayerKilledCallback;

    private static Dictionary<string, GameObject> weapons = new Dictionary<string, GameObject>();

    public static GameObject GetWeapon(string weaponName)
    {
        return weapons[weaponName];
    }

    #region Terrain Tracking

    /// <summary> Keeps track of terrain chunks around players </summary>
    private static Dictionary<Vector2, int> terrainChunks = new Dictionary<Vector2, int>();

    private ServerTerrainGenerator serverTerrainGenerator;

    public static void AddTerrainChunk(Vector2 chunkCoord)
    {
        if (!terrainChunks.ContainsKey(chunkCoord))
        {
            terrainChunks[chunkCoord] = 1;
            instance.serverTerrainGenerator.UpdateVisibleChunks(chunkCoord, true);
        } else
        {
            terrainChunks[chunkCoord]++;
        }
    }

    public static void RemoveTerrainChunk(Vector2 chunkCoord)
    {
        if (terrainChunks.ContainsKey(chunkCoord))
        {
            terrainChunks[chunkCoord]--;
            if (terrainChunks[chunkCoord] == 0)
            {
                instance.serverTerrainGenerator.UpdateVisibleChunks(chunkCoord, false);
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
            if (weapons.Count == 0)
            {
                for (int i = 0; i < gameWeapons.Length; i++)
                {
                    weapons.Add(gameWeapons[i].name, gameWeapons[i]);
                }
            }
            serverTerrainGenerator = FindObjectOfType<ServerTerrainGenerator>();
        }
    }

    #endregion

    #region Player Tracking

    private static Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    public static void RegisterPlayer(string playerID, GameObject player)
    {
        players.Add(playerID, player);
        player.transform.name = playerID;
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
