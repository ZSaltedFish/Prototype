using GameToolComponents;
using System.Collections;
using UnityEngine;

namespace Generator
{
    public class TreeArea : DynamicArea
    {
        public int MaxCount = 500;
        private int _count;
        private int _detialSize;
        private System.Random _rad;
        public TreeArea(int detialSize, float width, float height, Vector2Int index) : base(width, height, index)
        {
            int seed = index.x * 100 * index.y * 100;
            _rad = new System.Random(seed);
            _detialSize = detialSize;
        }

        public override void OnAreaLoad()
        {
            
        }

        public override void OnAreaRelease()
        {
        }

        public override void OnAreaUnload()
        {
            TreeGenerator.INSTANCE.RecycleTree(Index);
        }

        public override IEnumerator OnAreaLoadEnum()
        {
            for (int x = 0; x < _detialSize; ++x)
            {
                for (int y = 0; y < _detialSize; ++y)
                {
                    
                    float xOff = Width * x / _detialSize;
                    float yOff = Height * y / _detialSize;
                    Vector3 worldPoint = Index2WorldPoint() + new Vector3(xOff, 0, yOff);
                    TreeGenerator.INSTANCE.Generate(worldPoint, _rad);

                    if (_count == MaxCount)
                    {
                        _count = 0;
                        yield return null;
                    }
                    else
                    {
                        ++_count;
                    }
                }
            }
        }
    }
}
