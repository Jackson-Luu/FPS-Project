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

    #region Player Tracking Stuff (Click to Expand)

    private const string PLAYER_PREFIX = "Player ";

    public static Dictionary<string, Player> players = new Dictionary<string, Player>();

    /*public Dictionary<string, Player> GetPlayers {
        get {
            return players; }
    }*/

    public static void RegisterPlayer(string netID, Player player)
    {
        string playerID = PLAYER_PREFIX + netID;
        players.Add(playerID, player);
        player.transform.name = playerID;
        Debug.Log("HI" + players.Count + " PLAYER(S)");
    }

    public static void UnRegisterPlayer(string playerID)
    {
        Debug.Log("NOOO");
        players.Remove(playerID);
    }

    public static Player GetPlayer(string playerID)
    {
        return players[playerID];
    }

    public static int GetPlayerCount()
    {
        return players.Count;
    }

    public static Player[] GetAllPlayers()
    {
        return players.Values.ToArray();
    }

    #endregion
}
