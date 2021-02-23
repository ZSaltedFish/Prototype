using UnityEngine;

namespace Generator
{
    [RequireComponent(typeof(Terrain))]
    public class TerrainDataForge : MonoBehaviour
    {
        public int PixWidth, PixHeight;
        public int seed;
        public float Scale = 0.1f;
        public float HeightScale = 0.1f;

        public Terrain SrcTerrain;
        // Start is called before the first frame update
        public void Start()
        {
            SrcTerrain = GetComponent<Terrain>();

            PixWidth = SrcTerrain.terrainData.heightmapResolution;
            PixHeight = SrcTerrain.terrainData.heightmapResolution;
            float[,] sample = MapHelper.GetPerlinNoiseMap(PixWidth, PixHeight, seed, Scale, 0, HeightScale);
            SrcTerrain.terrainData.SetHeights(0, 0, sample);
        }

        // Update is called once per frame
        public void Update()
        {
        }

        public void TestMethod()
        {
        }
    }
}