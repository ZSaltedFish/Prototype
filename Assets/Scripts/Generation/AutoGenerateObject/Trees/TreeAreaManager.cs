using GameToolComponents;
using UnityEngine;

namespace Generator
{
    public class TreeAreaManager : DynamicAreaController<TreeArea>
    {
        public int Size;
        public TreeAreaManager(float width, float height, float maxRange) : base(width, height, maxRange, maxRange)
        {
        }

        protected override TreeArea OnCreate(Vector2Int wIndex)
        {
            return new TreeArea(Size, Width, Height, wIndex);
        }
    }
}
