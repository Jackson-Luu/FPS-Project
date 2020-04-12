using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public MatchSettings matchSettings;

    void Awake ()
    {
        // Ensure only one GameManager exists
        if (instance != null)
        {
            Debug.Log("Error: more than one GameManager in scene.");
        } else
        {
            instance = this;
            Debug.Log(this + " " + instance);
        }
    }

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
}
