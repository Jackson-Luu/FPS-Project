using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public class RoyaleManager : MonoBehaviour
{
    public static RoyaleManager instance;

    private int playersAlive = 0;
    private static Dictionary<string, Player.PlayerStatus> players = new Dictionary<string, Player.PlayerStatus>();
    private System.DateTime startTime;

    #region Singleton

    void Awake()
    {
        // Ensure only one GameManager exists
        if (instance != null)
        {
            Debug.Log("Error: more than one RoyaleManager in scene.");
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        startTime = System.DateTime.Now;
    }
    #endregion

    public static Player.PlayerStatus GetStatus(string player)
    {
        return players[player];
    }

    public static void AddPlayer(string player)
    {
        players[player] = Player.PlayerStatus.Alive;
        instance.playersAlive++;
    }

    public static void PlayerDied(string playerName)
    {
        instance.playersAlive--;
        players[playerName] = Player.PlayerStatus.Undead;
        
        if (instance.playersAlive == 1)
        {
            System.TimeSpan matchTime = System.DateTime.Now - instance.startTime;
            GameManager.instance.sceneLoaded = false;
            foreach (var player in players)
            {
                if (player.Value == Player.PlayerStatus.Alive)
                {
                    GameManager.instance.RpcGameOver(matchTime.Minutes, player.Key);
                    break;
                }
            }
            instance.StartCoroutine(instance.EndGame());
        }
        
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(10f);

        NetworkRoomManager nm = NetworkManager.singleton as NetworkRoomManager;
        nm.ServerChangeScene(nm.RoomScene);
    }
}
