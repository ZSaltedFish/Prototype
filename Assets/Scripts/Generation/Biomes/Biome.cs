using Localization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    [Serializable]
    public class Biome : MonoBehaviour
    {
        public const float BIOME_MAX_RANGE_TIMES = 10;
        #region 可引用参数
        public int TerrainTextureLayerIndex;
        public int BiomeIndex;

        public LocalizationString BiomeName;

        public float PwrValue;
        public float MinHigh;
        public float MaxHigh;

        public float Complexity;
        public float Rarity;
        public float Range;
        public float BiomeBlendingRange = 0.01f;

        public Texture2D SrcTexture;
        public AnimationCurve TerrainCurve;
        public EnvironmentObjectData EnviromentData;
        #endregion
    }
}
