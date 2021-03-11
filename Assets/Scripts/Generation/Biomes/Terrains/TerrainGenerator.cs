using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public partial class TerrainGenerator : DynamicAreaController<TerrainArea>, IDisposable
    {

        public const int UPDATE_COUNT = 50000;
        public const float VIEW_DIST = 2000;
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

            OnUpdateFinished = OnUpdateOver;
        }
        #region 初始化
        private void OnUpdateOver()
        {
            GameEventSystem.ThrowEvent(GameEventType.TerrainUpdateFinished, new GameEventParam()
                .Add(GameEventFlag.Terrain_update_location, ActorManager.GetPlayerLocation()));
        }

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

        public bool TryGetHigh(Vector3 pos, out float high)
        {
            high = 0;
            Vector2Int index = WorldPoint2AreaIndex(pos);
            if (!LoadingArea.TryGetValue(index, out TerrainArea area))
            {
                return false;
            }

            high = area.GetHigh(pos);
            return true;
        }

        public bool TryGetBiomeSpecifiedLocation(Vector3 pos, out Biome biome)
        {
            biome = null;
            Vector2Int index = WorldPoint2AreaIndex(pos);
            if (!LoadingArea.TryGetValue(index, out TerrainArea area))
            {
                return false;
            }

            biome = area.GetPosType(pos);
            return true;
        }
        #endregion
    }
}
