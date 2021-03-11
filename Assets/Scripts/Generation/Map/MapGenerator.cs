using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public class MapGenerator : MonoBehaviour, IMap
    {
        public const float FRAMGE_VALUE = 0.001f;
        public float TargetSize = 10000f;
        public static MapGenerator INSTANCE { get; private set; }
        public List<Biome> Biomes;
        public float[,,] BiomeDatas;
        public Vector2Int Size => _size;
        private Vector2Int _size;

        public void Start()
        {
            INSTANCE = this;
            Generate();
        }

        public void Generate()
        {
            int maxWidth = 0, maxHeight = 0;
            foreach (Biome biome in Biomes)
            {
                maxWidth = Math.Max(biome.SrcTexture.width, maxWidth);
                maxHeight = Math.Max(biome.SrcTexture.height, maxHeight);
            }

            _size = new Vector2Int(maxWidth, maxHeight);
            BiomeDatas = new float[Biomes.Count, _size.x, _size.y];

            for (int i = 0; i < Biomes.Count; ++i)
            {
                for (int x = 0; x < _size.x; ++x)
                {
                    for (int y = 0; y < _size.y; ++y)
                    {
                        Biome biome = Biomes[i];
                        float value = biome.SrcTexture.GetPixel(x, y).a;
                        BiomeDatas[i, x, y] = value;
                    }
                }
            }

            GameEventSystem.ThrowEvent(GameEventType.TextureMapInitFinished);
        }

        public void OnDestroy()
        {
            INSTANCE = null;
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

        public Biome[] GetBiome(int x, int y)
        {
            return Biomes.ToArray();
        }

        public float[] GetDatas(int x, int y)
        {
            float[] values = new float[Biomes.Count];
            for (int i = 0; i < Biomes.Count; ++i)
            {
                float value = BiomeDatas[i, x, y];
                values[i] = value;
            }
            return values;
        }

        public float[] GetDatas(Vector2Int pos)
        {
            return GetDatas(pos.x, pos.y);
        }
    }
}
