using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public class SceneObjectController : DynamicAreaController<SceneObjectArea>
    {
        private Dictionary<Vector2Int, List<SceneObject>> _dict;
        public SceneObjectController(float width, float height, float maxRange,
            ListCollector collector) : base(width, height, maxRange, maxRange)
        {
            _dict = new Dictionary<Vector2Int, List<SceneObject>>();
            foreach (GameObject sGo in collector.Objects)
            {
                if (sGo != null)
                {
                    SceneObject so = sGo.GetComponent<SceneObject>();
                    if (so != null)
                    {
                        Vector2Int index = WorldPoint2AreaIndex(so.InitPosition);

                        if (!_dict.TryGetValue(index, out List<SceneObject> list))
                        {
                            list = new List<SceneObject>();
                            _dict.Add(index, list);
                        }
                        list.Add(so);
                    }
                }
            }
        }

        protected override SceneObjectArea OnCreate(Vector2Int wIndex)
        {
            SceneObjectArea area = new SceneObjectArea(Width, Height, wIndex);

            if (_dict.TryGetValue(wIndex, out List<SceneObject> value))
            {
                area.ObjectList = value;
            }
            else
            {
                area.ObjectList = new List<SceneObject>();
            }
            return area;
        }
    }
}
