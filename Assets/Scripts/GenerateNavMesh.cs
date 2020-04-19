using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class GenerateNavMesh : NetworkBehaviour
{
    private MapMagic.MapMagic mapMagic;

    [SerializeField]
    private GameObject cameras;

    // Generate map seed and enable terrain generator (server)
    public override void OnStartServer()
    {
        mapMagic = GetComponent<MapMagic.MapMagic>();
        mapMagic.seed = (int)(System.DateTime.Now.Ticks % 1000000);
        //mapMagic.totalMeshes = cameras.transform.childCount * 4;
        mapMagic.totalMeshes = 1;
        mapMagic.enabled = true;
        //MapMagic.MapMagic.OnApplyCompleted += BuildMesh;
    }

    // Enable terrain generator (client)
    public override void OnStartClient()
    {
        GetComponent<MapMagic.MapMagic>().enabled = true;
    }

    public void BuildMesh()
    {
        mapMagic.enabled = false;
        cameras.SetActive(false);
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}


