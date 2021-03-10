using System.Collections.Generic;

namespace Generator
{
    public class TerrainAreaData
    {
        public float[] FixDatas;
        public List<Biome> Biomes;

        public Biome CurBiome()
        {
            for (int i = Biomes.Count - 1; i >= 0; --i)
            {
                if (FixDatas[i] > 0.01f)
                {
                    return Biomes[i];
                }
            }
            return Biomes[0];
        }
    }
}
