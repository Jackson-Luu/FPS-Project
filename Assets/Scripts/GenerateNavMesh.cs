using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class GenerateNavMesh : NetworkBehaviour
{
    private void OnEnable()
    {
        if (isServer)
        {
            MapMagic.MapMagic.OnApplyCompleted += BuildMesh;
        }
    }

    public void BuildMesh(Terrain terrain)
    {
        Debug.Log("BUILDING MESH @ " + Time.deltaTime);
        GetComponent<NavMeshSurface>().BuildNavMesh();
        Debug.Log("MESH FINISHED @ " + Time.deltaTime);
    }

    private void OnDisable()
    {
        if (isServer)
        {
            MapMagic.MapMagic.OnApplyCompleted -= BuildMesh;
        }
    }
}


