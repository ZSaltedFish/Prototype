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

        public GameObject[] Trees;
        public float TreeThreshold;

        public Texture2D SrcTexture;
        public AnimationCurve TerrainCurve;
        public Biome[] SubBiomes;
        #endregion

        public Biome Parent { get; private set; }
        public static Biome ROOT;
        public static void InitBiomeTree(Biome root)
        {
            ROOT = root;
            Stack<Biome> stack = new Stack<Biome>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Biome nowRoot = stack.Pop();
                foreach (var item in nowRoot.SubBiomes)
                {
                    item.Parent = nowRoot;
                    stack.Push(item);
                }
            }
        }
    }
}
