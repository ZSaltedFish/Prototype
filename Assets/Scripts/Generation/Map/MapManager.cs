using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public class MapManager : IMap
    {
        public float TargetSize = 10000;
        public float MapSize = 1000f;
        public MapBlockData[,] Datas => _datas;
        public Vector2Int Size => _size;

        private List<Biome> _biomes;
        private Vector2 MaskOffset;
        private Dictionary<MapBiomeTag, Biome> _dictBiomes;
        private MapBlockData[,] _datas;
        private Vector2Int _size;


        public static MapManager INSTANCE { get; private set; }
        public MapManager(List<Biome> biomes)
        {
            _biomes = biomes;
            _dictBiomes = new Dictionary<MapBiomeTag, Biome>
            {
                { MapBiomeTag.Plain, _biomes[0] },
                { MapBiomeTag.Hill, _biomes[1] },
                { MapBiomeTag.Peaks, _biomes[2] },
                { MapBiomeTag.Plateau, _biomes[3] },
                { MapBiomeTag.Mountains, _biomes[2] }
            };
            MaskOffset = new Vector2(Random.Range(0, 4), Random.Range(0, 4));
            INSTANCE = this;
        }

        public void Generate()
        {
            _size = new Vector2Int(100, 100);
            _datas = new MapBlockData[_size.x, _size.y];

            for (int x = 0; x < _size.x; ++x)
            {
                for (int y = 0; y < _size.y; ++y)
                {
                    MapBlockData data = new MapBlockData();
                    _datas[x, y] = data;
                    var biome = _dictBiomes[MapBiomeTag.Plain];
                    data.BiomeType = (byte)biome.BiomeIndex;
                    //data.BlendValue = biome.PwrValue;
                }
            }

            AddBiome(_dictBiomes[MapBiomeTag.Hill], (byte)_dictBiomes[MapBiomeTag.Plain].BiomeIndex);
            Expand(new Vector2Int(500, 500));
            AddBiome(_dictBiomes[MapBiomeTag.Peaks], (byte)_dictBiomes[MapBiomeTag.Hill].BiomeIndex);
            AddBiome(_dictBiomes[MapBiomeTag.Plateau], (byte)_dictBiomes[MapBiomeTag.Hill].BiomeIndex);
            Expand(new Vector2Int(1000, 1000));
            EdgeStackCalculate();
        }

        public void WriteBitmap(string path)
        {
#if UNITY_EDITOR
            using (System.IO.FileStream file = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                Texture2D tex = new Texture2D(_size.x, _size.y);
                for (int x = 0; x < _size.x; x++)
                {
                    for (int y = 0; y < _size.y; y++)
                    {
                        Color color;
                        switch (_datas[x, y].BiomeType)
                        {
                            case 0: color = Color.gray; break;
                            case 1: color = Color.black; break;
                            case 2: color = Color.blue; break;
                            case 3: color = Color.green; break;
                            default: color = Color.red;break;
                        }

                        tex.SetPixel(x, y, color);
                    }
                }
                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                file.Write(bytes, 0, bytes.Length);
            }
#endif
        }

        public void AddBiome(Biome biome, byte srcBiome)
        {
            for (int x = 0; x < _size.x; ++x)
            {
                float xV = x / (_size.x - 1f) * MapSize;
                for (int y = 0; y < _size.y; ++y)
                {
                    float yV = y / (_size.y - 1f) * MapSize;
                    Vector2 maskPoint = FromPos2PerlinPoint(new Vector2(xV, yV), biome.Range) + MaskOffset * (biome.BiomeIndex + 1);
                    float maskValue = Mathf.PerlinNoise(maskPoint.x, maskPoint.y);

                    MapBlockData data = _datas[x, y];
                    if (maskValue > biome.Rarity && data.BiomeType == srcBiome)
                    {
                        data.BiomeType = (byte)biome.BiomeIndex;
                    }
                }
            }
        }

        public void Expand(Vector2Int destSize)
        {
            MapBlockData[,] tempData = new MapBlockData[destSize.x, destSize.y];
            Vector2Int src = _size;

            for (int x = 0; x < destSize.x; ++x)
            {
                float xFlerp = x * (src.x - 1f) / (destSize.x - 1f);
                int xIntLerp = (int)xFlerp;
                int fixX = xIntLerp < src.x - 1 ? xIntLerp + 1 : xIntLerp;
                float lerpX = xFlerp - xIntLerp;
                for (int y = 0; y < destSize.y; ++y)
                {
                    float yFlerp = y * (src.y - 1f) / (destSize.y - 1f);
                    int yIntLerp = (int)yFlerp;
                    int fixY = yIntLerp < src.y - 1 ? yIntLerp + 1 : yIntLerp;
                    float lerpY = yFlerp - yIntLerp;

                    MapBlockData q00 = _datas[xIntLerp, yIntLerp];
                    MapBlockData q10 = _datas[fixX, yIntLerp];
                    MapBlockData q01 = _datas[xIntLerp, fixY];
                    MapBlockData q11 = _datas[fixX, fixY];

                    tempData[x, y] = Blend(q00, q10, q01, q11, new Vector2(xFlerp, yFlerp) * 2f, lerpX, lerpY);
                }
            }

            _datas = tempData;
            _size = destSize;
        }

        private MapBlockData Blend(MapBlockData q00, MapBlockData q10, MapBlockData q01, MapBlockData q11, Vector2 perTex,
            float lerpX, float lerpY)
        {
            lerpX = ExMath.PerlinLerp(0, 1, lerpX);
            lerpY = ExMath.PerlinLerp(0, 1, lerpY);
            float perlinX = (ExMath.PerlinNoise1D(perTex.x) + 1);
            float perlinY = (ExMath.PerlinNoise1D(perTex.y) + 1);
            byte qx0, qx1;
            qx0 = lerpX > perlinY ? q10.BiomeType : q00.BiomeType;
            qx1 = lerpX > perlinY ? q11.BiomeType : q01.BiomeType;

            MapBlockData data = new MapBlockData
            {
                BiomeType = lerpY > perlinX ? qx1 : qx0,
            };
            return data;
        }

        private void EdgeStackCalculate()
        {
            for (int i = MapBlockData.MBD_MIN; i < MapBlockData.MBD_MAX; ++i)
            {
                for (int x = 0; x < _size.x; ++x)
                {
                    for (int y = 0; y < _size.y; ++y)
                    {
                        byte curType = _datas[x, y].BiomeType;
                        int minByte = MinAround(x, y, curType);
                        if (minByte + 1 == i)
                        {
                            _datas[x, y].BlendValue = (byte)i;
                        }
                    }
                }
            }
        }

        private int MinAround(int x, int y, byte nowType)
        {
            return 0;
        }

        private Vector2 FromPos2PerlinPoint(Vector2 worldPoint, float scale)
        {
            return new Vector2(worldPoint.x / MapSize * scale, worldPoint.y / MapSize * scale);
        }

        public RectInt GetDataRect(Vector2 pos, float width, float height)
        {
            int xStart = (int)(pos.x / TargetSize * Size.x);
            int yStart = (int)(pos.y / TargetSize * Size.y);
            xStart = Mathf.Clamp(xStart, 0, Size.x - 1);
            yStart = Mathf.Clamp(yStart, 0, Size.y - 1);
            int xEnd = Mathf.FloorToInt((pos.x + width) / TargetSize * Size.x);
            int yEnd = Mathf.FloorToInt((pos.y + height) / TargetSize * Size.y);
            xEnd = Mathf.Clamp(xEnd, 0, Size.x - 1);
            yEnd = Mathf.Clamp(yEnd, 0, Size.y - 1);

            return new RectInt(xStart, yStart, xEnd - xStart, yEnd - yStart);
        }
    }
}
