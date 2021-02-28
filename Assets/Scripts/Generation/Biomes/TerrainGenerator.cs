using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public partial class TerrainGenerator : DynamicAreaController<TerrainGenerator.TerrainArea>, IDisposable
    {

        public const int UPDATE_COUNT = 22001;
        public const float VIEW_DIST = 1000;
        public const string CLONE_TERRAIN_NAME = "TerrainDefaultClone";
        public readonly TerrainData DefaultTerrainData;

        //private ObjectPoolPattern<GameObject> _terrainPool;
        private GameObject _srcTerrain;

        private int _alphaMapCount;
        private List<Biome> _biomes;
        private int _count = 0;
        private Vector2 _maskOffset;

        public TerrainGenerator(TerrainData defaultData, GameObject srcTerrain, int width, int height, List<Biome> biomes) 
            : base(width, height, VIEW_DIST)
        {
            ObjectPoolMode = true;
            _maskOffset = new Vector2(Random.Range(-2, 2), Random.Range(-2, 2));
            DefaultTerrainData = defaultData;
            InitAlphaTexture();
            _srcTerrain = srcTerrain;
            _srcTerrain.SetActive(false);
            _biomes = biomes;
        }
        #region 初始化

        private void InitAlphaTexture()
        {
            int texSize = DefaultTerrainData.heightmapResolution;
            DefaultTerrainData.alphamapResolution = texSize;
            _alphaMapCount = DefaultTerrainData.alphamapLayers;
        }
        #endregion

        #region 处理创建与删除

        protected override TerrainArea OnCreate(Vector2Int arg)
        {
            TerrainArea data = new TerrainArea(
                _count, _srcTerrain, DefaultTerrainData, Width, Height, arg)
            {
                Biomes = _biomes,
                AlphaMapCount = _alphaMapCount,
                MaskOffset = _maskOffset
            };
            ++_count;
            return data;
        }

        public Biome GetBiomeSpecifiedLocation(Vector3 wp)
        {
            return GetLocBiome(wp, _biomes.ToArray(), _biomes[0]);
        }

        private Biome GetLocBiome(Vector3 worldPoint, Biome[] biomes, Biome upB)
        {
            for (int i = biomes.Length - 1; i >= 0; --i)
            {
                Biome biome = biomes[i];
                Vector2 maskPoint = FromPos2PerlinPoint(worldPoint, biome.Range) + _maskOffset * (i + 1);

                float maskLerp = GetMaskPoint(biome, maskPoint);
                if (maskLerp > 0.0001f)
                {
                    return GetLocBiome(worldPoint, biome.SubList, upB);
                }
            }

            return upB;
        }
        #endregion

        private float GetMaskPoint(Biome biome, Vector2 maskPoint)
        {
            float perlin = Mathf.PerlinNoise(maskPoint.x, maskPoint.y);
            return Mathf.Clamp01((perlin - biome.Rarity) / (1 - biome.Rarity));
        }

        private Vector2 FromPos2PerlinPoint(Vector3 worldPoint, float scale)
        {
            return new Vector2(worldPoint.x / Width * scale, worldPoint.z / Height * scale);
        }
    }
}
