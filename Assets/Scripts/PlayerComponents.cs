using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerComponents : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    // Start is called before the first frame update
    void Start()
    {
        DisableComponents();

        RegisterPlayer();
        AssignRemoteLayer();
    }

    void RegisterPlayer()
    {
        string playerID = "Player " + GetComponent<NetworkIdentity>().netId;
        transform.name = playerID;
    }

    void DisableComponents()
    {
        if (!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }


    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }
}
