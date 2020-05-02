using UnityEngine;
using Mirror;

public class InitialiseTerrain : NetworkBehaviour
{
    [SerializeField]
    private TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    { 
        if (!isServer)
        {
        }
        Debug.Log("WTF");
    }
}
