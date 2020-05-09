using UnityEngine;
using System.Collections;
using Mirror;

public class RoyaleManager : MonoBehaviour
{
    public static RoyaleManager instance;

    private int playersAlive = 0;

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
            playersAlive = NetworkManager.singleton.numPlayers;
        }
    }

    private void Start()
    {
        GameManager.instance.scene = "Royale";
    }
    #endregion

    public static void PlayerDied()
    {
        instance.playersAlive--;
        if (instance.playersAlive == 0)
        {
            Debug.Log("Game Over");
            instance.StartCoroutine(instance.EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(5f);

        NetworkRoomManager nm = NetworkManager.singleton as NetworkRoomManager;
        nm.ServerChangeScene(nm.RoomScene);
    }
}
