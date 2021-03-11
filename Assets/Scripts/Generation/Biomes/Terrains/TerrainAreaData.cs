using System.Collections.Generic;

namespace Generator
{
    public class TerrainAreaData
    {
        public float[] FixDatas;
        public Biome[] Biomes;

        private Biome _curBiome;

        public Biome CurBiome()
        {
            return _curBiome;
        }

        public void UpdateCurBiome()
        {
            for (int i = Biomes.Length - 1; i >= 0; --i)
            {
                if (FixDatas[i] > 0.01f)
                {
                    _curBiome = Biomes[i];
                    break;
                }
            }
        }
    }
}
