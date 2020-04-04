using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerComponents : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    // Start is called before the first frame update
    void Start()
    {
        if(!isLocalPlayer)
        {
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
