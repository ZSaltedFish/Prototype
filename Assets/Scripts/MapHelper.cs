using UnityEngine;

namespace Generator
{
    public static class MapHelper
    {
        /// <summary>
        /// 通过柏林算法获取高度图
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="seed">Seed</param>
        /// <param name="scale">放大倍数</param>
        /// <returns>高度图</returns>
        public static float[,] GetPerlinNoiseMap(int width ,int height, float xOffset, float yOffset, float scale, float min = 0, float max = 1)
        {
            float[,] map = new float[width, height];

            for (float y = 0; y < height; ++y)
            {
                for (float x = 0; x < width; ++x)
                {
                    float xCoord = (xOffset + x) / width * scale;
                    float yCoord = (yOffset + y) / height * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    sample = Mathf.Lerp(min, max, sample);
                    map[(int)x, (int)y] = sample;
                }
            }
            return map;
        }

        /// <summary>
        /// 通过柏林函数生成一张贴图
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="seed"></param>
        /// <param name="scale"></param>
        public static void SetTextureWidthPerlinNoise(Texture2D texture, int seed, float scale)
        {
            float width = texture.width;
            float height = texture.height;

            int xOffset = (int)(seed * width);
            int yOffset = (int)(seed * height);

            for (float y = 0; y < height; y++)
            {
                for (float x = 0; x < width; x++)
                {
                    float xCoord = xOffset + x / width * scale;
                    float yCoord = yOffset + y / height * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    Color color = new Color(sample, sample, sample);
                    texture.SetPixel((int)x, (int)y, color);
                }
            }
            texture.Apply();
        }
    }
}