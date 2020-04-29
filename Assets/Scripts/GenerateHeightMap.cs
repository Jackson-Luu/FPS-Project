using UnityEngine;
using System.IO;

public class GenerateHeightMap : MonoBehaviour
{
    [SerializeField]
    Transform mapMagic;

    // Start is called before the first frame update
    void Start()
    {
        string path = "Assets/Mapmagic.txt";

        //Write heights array to text file
        StreamWriter writer = new StreamWriter(path, true);

        MapMagic.GeneratorsAsset mapMagicGens = MapMagic.MapMagic.instance.gens; //finding current graph asset
        MapMagic.Chunk.Results results = new MapMagic.Chunk.Results(); //preparing results
        MapMagic.Chunk.Size size = new MapMagic.Chunk.Size(MapMagic.MapMagic.instance.resolution, MapMagic.MapMagic.instance.terrainSize, MapMagic.MapMagic.instance.terrainHeight);
        mapMagicGens.Calculate(0, 0, 1000, results, size, 12345);

        for (int i = 0; i < results.heights.count; i++)
        {
            writer.WriteLine(results.heights.array[i] * 300f);
        }
        writer.Close();

        Invoke("SampleTerrainHeights", 1f);
    }

    void SampleTerrainHeights()
    {
        StreamWriter writer2 = new StreamWriter("Assets/SampleHeights.txt", true);
        for (int i = 0; i < 1000; i++)
        {
            for (int j = 0; j < 1000; j++)
            {
                writer2.WriteLine(Terrain.activeTerrain.SampleHeight(new Vector3(j, 0, i)));
            }
        }
        writer2.Close();
        Debug.Log("Sampling done.");
    }
}
