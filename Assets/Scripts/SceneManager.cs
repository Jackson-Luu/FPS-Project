using UnityEngine;
using Mirror;

public class SceneManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject gameManager;

    public override void OnStartServer()
    {
        GameObject game = Instantiate(gameManager);
        NetworkServer.Spawn(game);
    }
}
