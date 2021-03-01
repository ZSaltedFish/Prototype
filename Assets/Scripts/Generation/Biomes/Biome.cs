using Localization;
using System;
using UnityEngine;

namespace Generator
{
    [Serializable]
    public class Biome : MonoBehaviour
    {
        public const float BIOME_MAX_RANGE_TIMES = 10;
        public int TerrainTextureLayerIndex;
        public int BiomeIndex;

        public LocalizationString BiomeName;

        public float BaseHigh;
        public float MinHigh;
        public float MaxHigh;

        public float Complexity;
        public float Rarity;
        public float Range;
        public Biome[] SubList;

        public GameObject[] Trees;
        public float TreeThreshold;
    }
}
