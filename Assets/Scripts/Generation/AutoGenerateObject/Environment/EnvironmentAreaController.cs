using GameToolComponents;
using System;
using UnityEngine;

namespace Generator
{
    public class EnvironmentAreaController : DynamicAreaController<EnvironmentObjectArea>
    {
        public Action<EnvironmentObjectArea> OnNewAreaLunch;
        public EnvironmentAreaController(float width, float height, float maxRange) 
            : base(width, height, maxRange, maxRange)
        {
        }

        protected override EnvironmentObjectArea OnCreate(Vector2Int wIndex)
        {
            EnvironmentObjectArea area = new EnvironmentObjectArea(Width, Height, wIndex);
            try
            {
                Vector2 pos = AreaIndex2WorldPoint(wIndex);
                OnNewAreaLunch?.Invoke(area);
            }
            catch (Exception err)
            {
                Debug.LogError(err);
            }
            return area;
        }
    }
}
