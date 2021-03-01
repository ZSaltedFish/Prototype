using GameToolComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public partial class TerrainGenerator
    {
        public class TerrainArea : DynamicArea
        {
            public List<Biome> Biomes;
            public Terrain SrcTerrain;
            public int AlphaMapCount;

            public Vector2 MaskOffset;

            private int _size;
            public TerrainArea(int count, GameObject terrainGo, TerrainData data, float width, float height, Vector2Int index) 
                : base(width, height, index)
            {
                GameObject cloneGameObject = Object.Instantiate(terrainGo);
                cloneGameObject.name = $"{CLONE_TERRAIN_NAME}_{++count}";
                Terrain terrain = cloneGameObject.GetComponent<Terrain>();
                cloneGameObject.transform.SetParent(terrainGo.transform.parent);
                SrcTerrain = terrain;
                terrain.terrainData = Object.Instantiate(data);
                terrain.terrainData.name = CLONE_TERRAIN_NAME;
                terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
                SrcTerrain.gameObject.SetActive(true);
                _size = SrcTerrain.terrainData.heightmapResolution;
            }
            public override void OnAreaLoad()
            {

            }
            public override void OnAreaUnload()
            {
            }

            public override IEnumerator OnAreaLoadEnum()
            {
                int count = 0;
                float[,] highs = new float[_size, _size];
                float[,,] alphaMap = new float[_size, _size, AlphaMapCount];
                SrcTerrain.transform.position = WorldPoint;
                for (int i = 0; i < Biomes.Count; ++i)
                {
                    Biome biome = Biomes[i];
                    for (int x = 0; x < _size; ++x)
                    {
                        for (int y = 0; y < _size; ++y)
                        {
                            float xOff = Mathf.Lerp(0, Width, x / (float)(_size - 1));
                            float yOff = Mathf.Lerp(0, Height, y / (float)(_size - 1));
                            Vector3 worldPoint = WorldPoint + new Vector3(xOff, 0, yOff);
                            Vector2 perlinPoint = FromPos2PerlinPoint(worldPoint, biome.Complexity);
                            Vector2 maskPoint = FromPos2PerlinPoint(worldPoint, biome.Range) + MaskOffset * (i + 1);

                            float maskLerp = GetMaskPoint(biome, maskPoint);
                            if (maskLerp > 0.0001f)
                            {
                                float perlinValue = Mathf.Lerp(biome.MinHigh, biome.MaxHigh, Mathf.PerlinNoise(perlinPoint.x, perlinPoint.y));

                                int subTerrainTexutreLayerIndex = biome.TerrainTextureLayerIndex;
                                float subFixPerlinValue = RunSubBiome(worldPoint, biome, perlinValue, new Vector2Int(x, y), ref subTerrainTexutreLayerIndex);
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

                SrcTerrain.terrainData.SetAlphamaps(0, 0, alphaMap);
                SrcTerrain.terrainData.SetHeights(0, 0, highs);
            }

            public float GetHigh(Vector3 pos)
            {
                Vector3 p = Index2WorldPoint();
                Vector3 delta = pos - p;
                int x = (int)(delta.x / Width * _size);
                int y = (int)(delta.z / Height * _size);

                return SrcTerrain.terrainData.GetHeight(x, y);
            }

            private void AdjustTexture(int x, int y, float[,,] alphaMap, int cur)
            {
                for (int i = 0; i < AlphaMapCount; ++i)
                {
                    alphaMap[y, x, i] = 0;
                }
                alphaMap[y, x, cur] = 1;
            }

            private float RunSubBiome(Vector3 world, Biome curBiome, float parentValue, Vector2Int cp, ref int alphaData)
            {
                if (curBiome.SubList == null || curBiome.SubList.Length == 0)
                {
                    return parentValue;
                }

                float fixPoint = parentValue;
                for (int i = 0; i < curBiome.SubList.Length; ++i)
                {
                    Biome subBiome = curBiome.SubList[i];
                    Vector2 perlinPoint = FromPos2PerlinPoint(world, subBiome.Complexity) + MaskOffset * (i + 5);
                    Vector2 maskPoint = FromPos2PerlinPoint(world, subBiome.Range) + MaskOffset * (i + 10);

                    float maskLerp = GetMaskPoint(subBiome, maskPoint);
                    if (maskLerp > 0.0001f)
                    {
                        float perlinValue = Mathf.Lerp(subBiome.MinHigh, subBiome.MaxHigh, Mathf.PerlinNoise(perlinPoint.x, perlinPoint.y));

                        int subTerrainTexutreLayerIndex = subBiome.TerrainTextureLayerIndex;
                        float subFixPerlinValue = RunSubBiome(world, subBiome, perlinValue, cp, ref subTerrainTexutreLayerIndex);
                        alphaData = subTerrainTexutreLayerIndex;
                        fixPoint = Mathf.Lerp(parentValue, subFixPerlinValue, maskLerp);
                    }
                }
                return fixPoint;
            }

            private float GetMaskPoint(Biome biome, Vector2 maskPoint)
            {
                float perlin = Mathf.PerlinNoise(maskPoint.x, maskPoint.y);
                return Mathf.Clamp01((perlin - biome.Rarity) / (1 - biome.Rarity));
            }

            private Vector2 FromPos2PerlinPoint(Vector3 worldPoint, float scale)
            {
                return new Vector2(worldPoint.x / Width * scale, worldPoint.z / Height * scale);
            }

            public override void OnAreaRelease()
            {
                if (SrcTerrain.terrainData.name == CLONE_TERRAIN_NAME)
                {
                    Object.Destroy(SrcTerrain.terrainData);
                }
            }
        }
    }
}
