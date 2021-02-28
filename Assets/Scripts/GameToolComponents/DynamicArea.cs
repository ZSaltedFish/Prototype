using System.Collections;
using UnityEngine;

namespace GameToolComponents
{
    public abstract class DynamicArea
    {
        public readonly float Width, Height;
        public Vector2Int Index;
        public Vector3 WorldPoint;
        public abstract void OnAreaLoad();
        public abstract void OnAreaUnload();
        public abstract void OnAreaRelease();

        public DynamicArea(float width, float height, Vector2Int index)
        {
            Index = index;
            Width = width;
            Height = height;
        }

        public virtual IEnumerator OnAreaLoadEnum()
        {
            OnAreaLoad();
            yield return null;
        }

        public virtual IEnumerator OnAreaUnLoadEnum()
        {
            OnAreaUnload();
            yield return null;
        }

        public Vector2Int WorldPoint2Index(Vector3 worldPoint)
        {
            float xCount = worldPoint.x / Width - (worldPoint.x < 0 ? 1 : 0);
            float yCount = worldPoint.z / Height - (worldPoint.z < 0 ? 1 : 0);
            return new Vector2Int((int)xCount, (int)yCount);
        }

        public Vector3 Index2WorldPoint()
        {
            return new Vector3(Index.x * Width, 0, Index.y * Height);
        }
    }
}
