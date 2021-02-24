using GameToolComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public class TerrainGenerator : IDisposable
    {
        public const int UPDATE_COUNT = 22000;
        public const float VIEW_DIST = 1000;
        public const string CLONE_TERRAIN_NAME = "TerrainDefaultClone";
        public readonly TerrainData DefaultTerrainData;

        private ObjectPoolPattern<GameObject> _terrainPool;
        private GameObject _srcTerrain;
        private Dictionary<Vector2Int, Terrain> _terrainGrid = new Dictionary<Vector2Int, Terrain>();

        private Vector2 _maskOffset;
        private int _alphaMapCount;
        private readonly int _width;
        private readonly int _height;

        public TerrainGenerator(TerrainData defaultData, GameObject srcTerrain, int width, int height)
        {
            DefaultTerrainData = defaultData;
            _srcTerrain = srcTerrain;
            _srcTerrain.GetComponent<Terrain>().terrainData = UnityEngine.Object.Instantiate(DefaultTerrainData);
            _srcTerrain.GetComponent<Terrain>().terrainData.name = CLONE_TERRAIN_NAME;
            _srcTerrain.GetComponent<TerrainCollider>().terrainData = _srcTerrain.GetComponent<Terrain>().terrainData;

            _terrainPool = new ObjectPoolPattern<GameObject>(OnTerrainObjectCreate)
            {
                OnObjectDistroy = OnTerrainObjectDistroy,
                OnObjectGottenFromPool = OnObjectGotten,
                OnObjectPushinPool = OnObjectPush
            };
            _terrainPool.Push(srcTerrain);
            _width = width;
            _height = height;

            InitAlphaTexture();
            InitData();
        }

        private void InitData()
        {
            _maskOffset = new Vector2(Random.Range(-2, 2), Random.Range(-2, 2));
        }

        private void InitAlphaTexture()
        {
            int texSize = DefaultTerrainData.heightmapResolution;
            DefaultTerrainData.alphamapResolution = texSize;
            _alphaMapCount = DefaultTerrainData.alphamapLayers;
            Debug.Log($"重定义Alpha地形贴图大小为:{texSize}x{texSize}");
        }

        #region 处理创建与删除
        private void OnObjectGotten(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnObjectPush(GameObject obj)
        {
            obj.SetActive(false);
        }

        private void OnTerrainObjectDistroy(GameObject obj)
        {
            Terrain terrain = obj.GetComponent<Terrain>();
            if (terrain.terrainData.name == CLONE_TERRAIN_NAME)
            {
                UnityEngine.Object.Destroy(terrain.terrainData);
            }
        }

        public void Dispose()
        {
            foreach (Vector2Int v2 in _terrainGrid.Keys)
            {
                Terrain terrain = _terrainGrid[v2];
                if (terrain.terrainData.name == CLONE_TERRAIN_NAME)
                {
                    UnityEngine.Object.Destroy(terrain.terrainData);
                }
            }
            _terrainPool.Dispose();
        }

        private int _count = 0;
        private GameObject OnTerrainObjectCreate()
        {
            GameObject cloneGameObject = UnityEngine.Object.Instantiate(_srcTerrain);
            cloneGameObject.name = $"{CLONE_TERRAIN_NAME}_{++_count}";
            Terrain terrain = cloneGameObject.GetComponent<Terrain>();
            cloneGameObject.transform.SetParent(_srcTerrain.transform.parent);
            terrain.terrainData = UnityEngine.Object.Instantiate(DefaultTerrainData);
            terrain.terrainData.name = CLONE_TERRAIN_NAME;
            terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
            return cloneGameObject;
        }
        #endregion

        #region 基础坐标换算
        private Vector2 FromPos2PerlinPoint(Vector3 worldPoint, float scale)
        {
            return new Vector2(worldPoint.x / _width * scale, worldPoint.z / _height * scale);
        }

        private Vector2Int WorldPoint2TerrainIndex(Vector3 worldPoint)
        {
            int xCount = ((int)worldPoint.x) / _width - (worldPoint.x < 0 ? 1 : 0);
            int yCount = ((int)worldPoint.z) / _height - (worldPoint.z < 0 ? 1 : 0);
            return new Vector2Int(xCount, yCount);
        }

        private Vector3 TerrainIndex2WorldPoint(Vector2Int terrainIndex)
        {
            return new Vector3(_width * terrainIndex.x, 0, _height * terrainIndex.y);
        }
        #endregion

        #region 更新地表
        private IEnumerator SelfGnerate(Terrain terrain, List<Biome> biomes)
        {
            int highMapSize = terrain.terrainData.heightmapResolution;
            float[,] highs = new float[highMapSize, highMapSize];
            float[,,] alphaMap = new float[highMapSize, highMapSize, _alphaMapCount];
            for (int i = 0; i < biomes.Count; ++i)
            {
                int count = 0;
                Biome biome = biomes[i];
                for (int x = 0; x < highMapSize; ++x)
                {
                    for (int y = 0; y < highMapSize; ++y)
                    {
                        float xOff = Mathf.Lerp(0, _width, x / (float)(highMapSize - 1));
                        float yOff = Mathf.Lerp(0, _height, y / (float)(highMapSize - 1));
                        Vector3 worldPoint = terrain.transform.position + new Vector3(xOff, 0, yOff);
                        Vector2 perlinPoint = FromPos2PerlinPoint(worldPoint, biome.Complexity);
                        Vector2 maskPoint = FromPos2PerlinPoint(worldPoint, biome.Range) + _maskOffset * (i + 1);

                        float maskLerp = GetMaskPoint(biome, maskPoint);
                        if (maskLerp > 0.0001f)
                        {
                            float perlinValue = Mathf.Lerp(biome.MinHigh, biome.MaxHigh, Mathf.PerlinNoise(perlinPoint.x, perlinPoint.y));

                            int subTerrainTexutreLayerIndex = biome.TerrainTextureLayerIndex;
                            float subFixPerlinValue = RunSubBiome(worldPoint, biome, perlinValue, ref subTerrainTexutreLayerIndex);
                            float heightValue = highs[y, x];
                            AdjustTexture(x, y, alphaMap, subTerrainTexutreLayerIndex);
                            highs[y, x] = Mathf.Lerp(heightValue, subFixPerlinValue, maskLerp);
                        }
                        ++count;
                        if (count == UPDATE_COUNT)
                        {
                            count = 0;
                            yield return null;
                        }
                    }
                }
            }

            terrain.terrainData.SetAlphamaps(0, 0, alphaMap);
            terrain.terrainData.SetHeights(0, 0, highs);
        }

        private float RunSubBiome(Vector3 world, Biome curBiome, float parentValue, ref int alphaData)
        {
            if (curBiome.SubList == null || curBiome.SubList.Length == 0)
            {
                return parentValue;
            }

            float fixPoint = parentValue;
            for (int i = 0; i < curBiome.SubList.Length; ++i)
            {
                Biome subBiome = curBiome.SubList[i];
                Vector2 perlinPoint = FromPos2PerlinPoint(world, subBiome.Complexity) + _maskOffset * (i + 5);
                Vector2 maskPoint = FromPos2PerlinPoint(world, subBiome.Range) + _maskOffset * (i + 10);

                float maskLerp = GetMaskPoint(subBiome, maskPoint);
                if (maskLerp > 0.0001f)
                {
                    float perlinValue = Mathf.Lerp(subBiome.MinHigh, subBiome.MaxHigh, Mathf.PerlinNoise(perlinPoint.x, perlinPoint.y));

                    int subTerrainTexutreLayerIndex = subBiome.TerrainTextureLayerIndex;
                    float subFixPerlinValue = RunSubBiome(world, subBiome, perlinValue, ref subTerrainTexutreLayerIndex);
                    alphaData = subTerrainTexutreLayerIndex;
                    fixPoint = Mathf.Lerp(parentValue, subFixPerlinValue, maskLerp);
                }
            }
            return fixPoint;
        }

        private void AdjustTexture(int x, int y, float[,,] alphaMap, int cur)
        {
            for (int i = 0; i < _alphaMapCount; ++i)
            {
                alphaMap[y, x, i] = 0;
            }
            alphaMap[y, x, cur] = 1;
        }

        private float GetMaskPoint(Biome biome, Vector2 maskPoint)
        {
            float perlin = Mathf.PerlinNoise(maskPoint.x, maskPoint.y);
            return Mathf.Clamp01((perlin - biome.Rarity) / (1 - biome.Rarity));
        }
        #endregion

        public IEnumerator UpgradeTerrain(List<Biome> biomes, Vector3 centerPoint)
        {
            Vector2Int terrainIndex = WorldPoint2TerrainIndex(centerPoint);
            UnloadTerrain(terrainIndex);
            int xMax = Mathf.FloorToInt(VIEW_DIST / _width);
            int yMax = Mathf.FloorToInt(VIEW_DIST / _height);

            for (int x = -xMax; x <= xMax; ++x)
            {
                for (int y = -yMax; y <= yMax; ++y)
                {
                    Vector2Int wIndex = new Vector2Int(terrainIndex.x + x, terrainIndex.y + y);
                    if (!OutoffRange(wIndex, terrainIndex) && !_terrainGrid.ContainsKey(wIndex))
                    {
                        GameObject terrainObj = _terrainPool.Get();
                        Terrain terrain = terrainObj.GetComponent<Terrain>();
                        terrainObj.transform.position = TerrainIndex2WorldPoint(wIndex);
                        _terrainGrid.Add(wIndex, terrain);
                        yield return SelfGnerate(terrain, biomes);
                    }
                }
            }
        }

        private void UnloadTerrain(Vector2Int centerIndex)
        {
            List<Vector2Int> keys = new List<Vector2Int>(_terrainGrid.Keys);
            foreach (Vector2Int index in keys)
            {
                if (OutoffRange(centerIndex, index))
                {
                    _terrainPool.Push(_terrainGrid[index].gameObject);
                    _terrainGrid.Remove(index);
                }
            }
        }

        private bool OutoffRange(Vector2Int index1, Vector2Int index2)
        {
            float xDist = Mathf.Abs(index1.x - index2.x) * _width;
            float yDist = Mathf.Abs(index1.y - index2.y) * _height;

            return xDist * xDist + yDist * yDist > VIEW_DIST * VIEW_DIST;
        }
    }
}
