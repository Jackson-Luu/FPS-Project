using UnityEngine;
using Mirror;

public class PauseMenu : MonoBehaviour
{
    public static bool isOn = false;

    public void LeaveGame()
    {
        NetworkManager.singleton.StopClient();
    }
}
