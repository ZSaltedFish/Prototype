using System;
using UnityEngine;
using Random = System.Random;

namespace GameToolComponents
{
    public static class ExMath
    {
        private static Random _RANDOM;
        private static int _SEED;

        private static readonly float[,] _GAUSSIAN_VALUE =
        {
            {0.0453542f, 0.0566406f, 0.0453542f},
            {0.0566406f, 0.0707355f, 0.0566406f},
            {0.0453542f, 0.0566406f, 0.0453542f }
        };
        public static float DoubleLinearLerp(float q00, float q10, float q01, float q11, float lerpX, float lerpY)
        {
            float y0 = Mathf.Lerp(q00, q10, lerpX);
            float y1 = Mathf.Lerp(q01, q11, lerpX);
            return Mathf.Lerp(y0, y1, lerpY);
        }

        public static float DoubleLerp(float q00, float q10, float q01, float q11, float lerpX, float lerpY)
        {
            float y0 = PerlinLerp(q00, q10, lerpX);
            float y1 = PerlinLerp(q01, q11, lerpX);
            return PerlinLerp(y0, y1, lerpY);
        }

        public static float PerlinLerp(float v1, float v2, float lerp)
        {
            float l = 6 * lerp * lerp * lerp * lerp * lerp - 15 * lerp * lerp * lerp * lerp + 10 * lerp * lerp * lerp;
            return Mathf.Lerp(v1, v2, l);
        }

        public static float[,] Expand(Vector2Int src, Vector2Int dist, float[,] map)
        {
            float[,] target = new float[dist.x, dist.y];

            for (int x = 0; x < dist.x; ++x)
            {
                float xFlerp = x * (src.x - 1f) / (dist.x - 1f);
                int xIntLerp = (int)xFlerp;
                int fixX = xIntLerp < src.x - 1 ? xIntLerp + 1 : xIntLerp;
                float lerpX = xFlerp - xIntLerp;

                for (int y = 0; y < dist.y; ++y)
                {
                    float yFlerp = y * (src.y - 1f) / (dist.y - 1f);
                    int yIntLerp = (int)yFlerp;
                    int fixY = yIntLerp < src.y - 1 ? yIntLerp + 1 : yIntLerp;

                    float lerpY = yFlerp - yIntLerp;

                    try
                    {
                        float q00 = map[xIntLerp, yIntLerp];
                        float q10 = map[fixX, yIntLerp];
                        float q01 = map[xIntLerp, fixY];
                        float q11 = map[fixX, fixY];

                        target[x, y] = DoubleLinearLerp(q00, q10, q01, q11, lerpX, lerpY);
                    }
                    catch (IndexOutOfRangeException err)
                    {
                        throw new IndexOutOfRangeException($"({x}, {y})", err);
                    }
                }
            }

            return target;
        }

        public static float Avg(params float[] datas)
        {
            float max = 0;
            for (int i = 0; i < datas.Length; ++i)
            {
                max += datas[i];
            }
            return max / datas.Length;
        }

        public static float GaussianBlur(float[,] temp, int x, int y)
        {
            float value = 0;
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    value += _GAUSSIAN_VALUE[i + 1, j + 1] * temp[x + i, y + j];
                }
            }
            return value;
        }

        private static float[] _PERLIN_VALUE;

        public static void SetRandomSeed(int seed)
        {
            const int PWR_VALUE = 10000;
            _RANDOM = new Random(seed);
            _SEED = seed + _RANDOM.Next();
            _PERLIN_VALUE = new float[256];

            for (int i = 0; i < _PERLIN_VALUE.Length; ++i)
            {
                _PERLIN_VALUE[i] = _RANDOM.Next(-PWR_VALUE, PWR_VALUE) / (float)PWR_VALUE;
            }
        }

        public static float PerlinNoise1D(float x)
        {
            int ix = (int)x;
            if (ix < 0)
            {
                ix += (-ix / _PERLIN_VALUE.Length + 1) * _PERLIN_VALUE.Length;
            }
            try
            {
                float value = _PERLIN_VALUE[ix % _PERLIN_VALUE.Length];
                float value2 = _PERLIN_VALUE[(ix + 1) % _PERLIN_VALUE.Length];

                float lerp = x - (int)x;
                float lerpValue = PerlinLerp(value, value2, lerp);
                return lerpValue;
            }
            catch(IndexOutOfRangeException err)
            {
                Debug.Log($"{ix} out of bound->{_PERLIN_VALUE.Length}");
                throw err;
            }
        }
    }
}
