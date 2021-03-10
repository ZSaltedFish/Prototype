using GameToolComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Generator
{
    public class TerrainArea : DynamicArea
    {
        public float TargetSize = 10000;
        public List<Biome> Biomes;
        public Terrain SrcTerrain;
        public int AlphaMapCount;

        public Vector2 MaskOffset;

        private int _size;
        private int _count = 0;
        private TerrainAreaData[,] _datas;

        public TerrainArea(int count, GameObject terrainGo, TerrainData data, float width, float height, Vector2Int index)
            : base(width, height, index)
        {
            GameObject cloneGameObject = Object.Instantiate(terrainGo);
            cloneGameObject.name = $"{TerrainGenerator.CLONE_TERRAIN_NAME}_{++count}";
            Terrain terrain = cloneGameObject.GetComponent<Terrain>();
            cloneGameObject.transform.SetParent(terrainGo.transform.parent);
            SrcTerrain = terrain;
            terrain.terrainData = Object.Instantiate(data);
            terrain.terrainData.name = TerrainGenerator.CLONE_TERRAIN_NAME;
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

        private IEnumerator ExMap(Vector3 wp, List<Biome> biomes)
        {
            SrcTerrain.transform.position = WorldPoint;

            RectInt rect = MapGenerator.INSTANCE.GetDataRect(new Vector2(wp.x, wp.z), Width, Height);
            int width = rect.width + 1;
            int height = rect.height + 1;
            TerrainAreaData[,] biomesData = new TerrainAreaData[width, height];

            for (int x = rect.xMin; x <= rect.xMax; ++x)
            {
                for (int y = rect.yMin; y <= rect.yMax; ++y)
                {
                    biomesData[x - rect.xMin, y - rect.yMin] = new TerrainAreaData()
                    {
                        Biomes = MapGenerator.INSTANCE.Biomes,
                        FixDatas = MapGenerator.INSTANCE.GetDatas(x, y)
                    };
                    if (IsEnuerator())
                    {
                        yield return null;
                    }
                }
            }
            yield return ExpandCoroutine(new Vector2Int(_size, _size), biomesData, wp, new Vector2Int(width, height));
        }

        public IEnumerator ExpandCoroutine(Vector2Int dest, TerrainAreaData[,] biomeDatas, Vector3 wp, Vector2Int size)
        {
            float[,] highs = new float[_size, _size];
            float[,,] alphaMap = new float[_size, _size, AlphaMapCount];

            if (_datas == null)
            {
                _datas = new TerrainAreaData[dest.x, dest.y];
            }

            for (int x = 0; x < dest.x; ++x)
            {
                float xFlerp = x * (size.x - 1f) / (dest.x - 1f);
                int xIntLerp = (int)xFlerp;
                int fixX = xIntLerp < size.x - 1 ? xIntLerp + 1 : xIntLerp;
                float xOff = x / (dest.x - 1f) * Width;
                float lerpX = xFlerp - xIntLerp;

                for (int y = 0; y < dest.y; ++y)
                {
                    float yFlerp = y * (size.y - 1f) / (dest.y - 1f);
                    int yIntLerp = (int)yFlerp;
                    int fixY = yIntLerp < size.y - 1 ? yIntLerp + 1 : yIntLerp;
                    float yOff = y / (dest.y - 1f) * Height;
                    float lerpY = yFlerp - yIntLerp;

                    TerrainAreaData q00 = biomeDatas[xIntLerp, yIntLerp];
                    TerrainAreaData q10 = biomeDatas[fixX, yIntLerp];
                    TerrainAreaData q01 = biomeDatas[xIntLerp, fixY];
                    TerrainAreaData q11 = biomeDatas[fixX, fixY];

                    Vector3 offWp = wp + new Vector3(xOff, 0, yOff);
                    TerrainAreaData tData = _datas[x, y];
                    if (tData == null)
                    {
                        tData = new TerrainAreaData();
                        _datas[x, y] = tData;
                    }
                    BlendData(q00, q10, q01, q11, lerpX, lerpY, ref tData);

                    if (IsEnuerator())
                    {
                        yield return null;
                    }
                    Vector3 noisePoint = FromPos2PerlinPoint(offWp, 20f);
                    float noise = (Mathf.PerlinNoise(noisePoint.x, noisePoint.y) * 2 - 1) * 0.001f;

                    alphaMap[y, x, tData.CurBiome().TerrainTextureLayerIndex] = 1;

                    float perlinValue = 0;
                    for (int i = 0; i < tData.Biomes.Count; ++i)
                    {
                        Biome biome = tData.Biomes[i];
                        float lerpValue = tData.FixDatas[i];
                        Vector2 perlinPoint = FromPos2PerlinPoint(offWp, biome.Complexity);
                        float value = Mathf.PerlinNoise(perlinPoint.x, perlinPoint.y);
                        float high = Mathf.Lerp(biome.MinHigh, biome.MaxHigh, value);
                        perlinValue = Mathf.Lerp(perlinValue, high, lerpValue);
                    }
                    highs[y, x] = perlinValue + noise;
                }
            }

            SrcTerrain.terrainData.SetAlphamaps(0, 0, alphaMap);
            SrcTerrain.terrainData.SetHeights(0, 0, highs);
        }

        private void BlendData(TerrainAreaData q00, TerrainAreaData q10, TerrainAreaData q01, TerrainAreaData q11, float lerpX, float lerpY, ref TerrainAreaData data)
        {
            lerpX = ExMath.PerlinLerp(0, 1, lerpX);
            lerpY = ExMath.PerlinLerp(0, 1, lerpY);

            data.Biomes = q00.Biomes;
            data.FixDatas = new float[q00.Biomes.Count];

            for (int i = 0; i < data.Biomes.Count; i++)
            {
                float v00, v10, v01, v11;
                v00 = q00.FixDatas[i];
                v10 = q10.FixDatas[i];
                v01 = q01.FixDatas[i];
                v11 = q11.FixDatas[i];
                data.FixDatas[i] = ExMath.DoubleLinearLerp(v00, v10, v01, v11, lerpX, lerpY);
            }
            data.UpdateCurBiome();
        }

        private bool IsEnuerator()
        {
            if (_count == TerrainGenerator.UPDATE_COUNT)
            {
                _count = 0;
                return true;
            }
            ++_count;
            return false;
        }

        public Biome GetPosType(Vector3 pos)
        {
            Vector3 p = Index2WorldPoint();
            Vector3 delta = pos - p;
            int x = (int)(delta.x / Width * _size);
            int y = (int)(delta.z / Height * _size);

            return _datas[x, y].CurBiome();
        }

        private Vector2 FromPos2PerlinPoint(Vector3 worldPoint, float scale)
        {
            return new Vector2(worldPoint.x / Width * scale, worldPoint.z / Height * scale);
        }

        public override void OnAreaRelease()
        {
            if (SrcTerrain.terrainData.name == TerrainGenerator.CLONE_TERRAIN_NAME)
            {
                Object.Destroy(SrcTerrain.terrainData);
            }
        }

        public override IEnumerator OnAreaLoadEnum()
        {
            yield return ExMap(WorldPoint, Biomes);
        }

        public float GetHigh(Vector3 pos)
        {
            Vector3 p = Index2WorldPoint();
            Vector3 delta = pos - p;
            int x = (int)(delta.x / Width * _size);
            int y = (int)(delta.z / Height * _size);

            return SrcTerrain.terrainData.GetHeight(x, y);
        }
    }
}
