using GameToolComponents;
using UnityEngine;

namespace Generator
{
    public class SaveableAreaManager : DynamicAreaController<SaveableArea>
    {
        public SaveableAreaManager(float width, float height, float range) : base(width, height, range)
        {

        }

        protected override SaveableArea OnCreate(Vector2Int wIndex)
        {
            return new SaveableArea(Width, Height, wIndex);
        }
    }
}
