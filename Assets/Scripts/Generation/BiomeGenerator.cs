using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public class BiomeGenerator : MonoBehaviour
    {
        public List<Biome> Biomes = new List<Biome>();
        public List<Terrain> Terrains = new List<Terrain>();
        public int TerrainWidth, TerrainHeight;
        public TerrainData DefaultTerrainData;
        public const int TERRAIN_COUNT = 3;
        public GameObject CenterUnit;
        private Dictionary<Vector2Int, Terrain> _terrainGrid = new Dictionary<Vector2Int, Terrain>();

        private Vector2 _maskOffset;
        private Vector3 _centerOffset;
        private int _alphaMapCount;
        public void Start()
        {
            InitAlphaTexture();
            Init();
        }

        private void InitAlphaTexture()
        {
            int texSize = DefaultTerrainData.heightmapResolution;
            DefaultTerrainData.alphamapResolution = texSize;
            _alphaMapCount = DefaultTerrainData.alphamapLayers;
            Debug.Log($"重定义Alpha地形贴图大小为:{texSize}x{texSize}");
        }

        private void Init()
        {
            Random.InitState(DateTime.Now.Millisecond);

            _centerOffset = CaculateCenteroffset();
            _maskOffset = new Vector2(Random.Range(-2, 2), Random.Range(-2, 2));
            for (int tX = 0; tX < TERRAIN_COUNT; ++tX)
            {
                for (int tY = 0; tY < TERRAIN_COUNT; ++tY)
                {
                    Terrain terrain = Terrains[tY * TERRAIN_COUNT + tX];
                    _terrainGrid.Add(new Vector2Int(tX - 1, tY - 1), terrain);
                }
            }

            foreach (Vector2Int pos in _terrainGrid.Keys)
            {
                Terrain terrain = _terrainGrid[pos];
                terrain.terrainData = Instantiate(DefaultTerrainData);
                int terrainX = pos.x;
                int terrainY = pos.y;
                terrain.terrainData.name = $"Terrain({terrainX}, {terrainY})";
                terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

                terrain.transform.position = new Vector3(terrainX * TerrainWidth, 0, terrainY * TerrainHeight) + _centerOffset;
                SelfGnerate(terrain);
            }
        }

        private void SelfGnerate(Terrain terrain)
        {
            int highMapSize = terrain.terrainData.heightmapResolution;
            float[,] highs = new float[highMapSize, highMapSize];
            float[,,] alphaMap = new float[highMapSize, highMapSize, _alphaMapCount];
            for (int i = 0; i < Biomes.Count; ++i)
            {
                Biome biome = Biomes[i];
                for (int x = 0; x < highMapSize; ++x)
                {
                    for (int y = 0; y < highMapSize; ++y)
                    {
                        float xOff = Mathf.Lerp(0, TerrainWidth, x / (float)(highMapSize - 1));
                        float yOff = Mathf.Lerp(0, TerrainHeight, y / (float)(highMapSize - 1));
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
                else
                {
                    int asb = 1;
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

        public void OnDestroy()
        {
            foreach (Terrain terrain in Terrains)
            {
                TerrainData data = terrain.terrainData;
                terrain.terrainData = DefaultTerrainData;
                Destroy(data);
            }
        }

        private Vector2 FromPos2PerlinPoint(Vector3 worldPoint, float scale)
        {
            float worldX = worldPoint.x;
            float worldY = worldPoint.z;                                                                                                        

            return new Vector2(worldX / TerrainWidth * scale, worldY / TerrainHeight * scale);
        }

        public float MaxTimeDelta = 5f;
        private float _curTimeDelta = 0;

        public void Update()
        {
            if (_curTimeDelta > MaxTimeDelta)
            {
                _curTimeDelta = 0;
                Vector2Int curInt = GetCurrentUnitTerrain();
                Debug.Log($"当前位置:{curInt}");
                StartCoroutine("UpgradeTerrain", curInt);
            }
            else
            {
                _curTimeDelta += Time.deltaTime;
            }
        }

        private Vector3 CaculateCenteroffset()
        {
            Vector3 pos = CenterUnit.transform.position;
            //Vector3 centerOffset = new Vector3(TerrainWidth, 0, TerrainHeight) * -0.5f;

            int xCount = ((int)pos.x) / TerrainWidth - (pos.x < 0 ? 1 : 0);
            int yCount = ((int)pos.z) / TerrainHeight - (pos.z < 0 ? 1 : 0);
            return new Vector3(xCount * TerrainWidth, 0, yCount * TerrainHeight);// + centerOffset;
        }

        private Vector2Int GetCurrentUnitTerrain()
        {
            Vector3 pos = CaculateCenteroffset();
            Vector3 delta = pos - _centerOffset;
            int xCount = Mathf.RoundToInt(delta.x / TerrainWidth);
            int yCount = Mathf.RoundToInt(delta.z / TerrainHeight);
            _centerOffset = pos;

            return new Vector2Int(xCount, yCount);
        }

        private IEnumerator UpgradeTerrain(Vector2Int curIndex)
        {
            int xCount = curIndex.x;
            int yCount = curIndex.y;
            Terrain tempTerrain;
            if (xCount != 0)
            {
                Debug.Log("进行X轴替换");
                for (int i = -1; i < TERRAIN_COUNT - 1; ++i)
                {
                    Vector2Int takeOut = new Vector2Int(-xCount, i);
                    Vector2Int center = new Vector2Int(0, i);
                    Vector2Int current = new Vector2Int(xCount, i);
                    tempTerrain = _terrainGrid[takeOut];
                    _terrainGrid[takeOut] = _terrainGrid[center];
                    _terrainGrid[center] = _terrainGrid[current];
                    _terrainGrid[current] = tempTerrain;
                    UpdateTerrainPos(current, _terrainGrid[current]);
                    SelfGnerate(_terrainGrid[current]);
                    yield return null;
                }
            }

            if (yCount != 0)
            {
                Debug.Log("进行Y轴替换");
                for (int i = -1; i < TERRAIN_COUNT - 1; ++i)
                {
                    Vector2Int takeOut = new Vector2Int(i, -yCount);
                    Vector2Int center = new Vector2Int(i, 0);
                    Vector2Int current = new Vector2Int(i, yCount);
                    tempTerrain = _terrainGrid[takeOut];
                    _terrainGrid[takeOut] = _terrainGrid[center];
                    _terrainGrid[center] = _terrainGrid[current];
                    _terrainGrid[current] = tempTerrain;
                    UpdateTerrainPos(current, _terrainGrid[current]);
                    SelfGnerate(_terrainGrid[current]);
                    yield return null;
                }
            }
        }

        private void UpdateTerrainPos(Vector2Int v2, Terrain terrain)
        {
            terrain.transform.position = new Vector3(v2.x * TerrainWidth, 0, v2.y * TerrainHeight) + _centerOffset;
        }
    }
}
