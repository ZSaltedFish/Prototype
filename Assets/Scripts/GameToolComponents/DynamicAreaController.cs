using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameToolComponents
{
    public abstract class DynamicAreaController<T> : IDisposable where T : DynamicArea
    {
        public float Width, Height;
        public float MaxRange;
        /// <summary>
        /// 对象池模式
        /// </summary>
        public bool ObjectPoolMode = false;
        protected Dictionary<Vector2Int, T> UnloadArea = new Dictionary<Vector2Int, T>();
        protected Dictionary<Vector2Int, T> LoadingArea = new Dictionary<Vector2Int, T>();

        public Action OnUpdateFinished;

        private ObjectPoolPattern<T> _usingPool;
        public DynamicAreaController(float width, float height, float maxRange)
        {
            Width = width;
            Height = height;
            MaxRange = maxRange;

            _usingPool = new ObjectPoolPattern<T>(PoolCreate);
        }

        private T PoolCreate()
        {
            return OnCreate(new Vector2Int(0, 0));
        }

        private bool OutoffRange(Vector2Int index1, Vector2Int index2)
        {
            float xDist = Mathf.Abs(index1.x - index2.x) * Width;
            float yDist = Mathf.Abs(index1.y - index2.y) * Height;

            return xDist * xDist + yDist * yDist > MaxRange * MaxRange;
        }

        public Vector2Int WorldPoint2AreaIndex(Vector3 worldPoint)
        {
            int xCount = (int)(worldPoint.x / Width) - (worldPoint.x < 0 ? 1 : 0);
            int yCount = (int)(worldPoint.z / Height) - (worldPoint.z < 0 ? 1 : 0);
            return new Vector2Int(xCount, yCount);
        }

        /// <summary>
        /// 更新区块 协程版本
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <returns></returns>
        public IEnumerator UpdateEnum(Vector3 centerPoint)
        {
            Vector2Int centerIndex = WorldPoint2AreaIndex(centerPoint);

            List<Vector2Int> keys = new List<Vector2Int>(LoadingArea.Keys);
            foreach (Vector2Int key in keys)
            {
                T area = LoadingArea[key];
                if (OutoffRange(key, centerIndex))
                {
                    yield return area.OnAreaUnLoadEnum();
                    if (ObjectPoolMode)
                    {
                        _usingPool.Push(area);
                    }
                    else
                    {
                        UnloadArea.Add(key, area);
                    }
                    LoadingArea.Remove(key);
                }
            }

            int xMax = Mathf.FloorToInt(MaxRange / Width);
            int yMax = Mathf.FloorToInt(MaxRange / Height);

            for (int x = -xMax; x <= xMax; ++x)
            {
                for (int y = -yMax; y <= yMax; ++y)
                {
                    Vector2Int wIndex = new Vector2Int(centerIndex.x + x, centerIndex.y + y);
                    if (!OutoffRange(wIndex, centerIndex) && !LoadingArea.ContainsKey(wIndex))
                    {
                        T area;
                        if (!ObjectPoolMode)
                        {
                            if (!UnloadArea.TryGetValue(wIndex, out area))
                            {
                                area = OnCreate(wIndex);
                            }
                            else
                            {
                                UnloadArea.Remove(wIndex);
                            }
                        }
                        else
                        {
                            area = _usingPool.Get();
                            area.Index = wIndex;
                        }
                        LoadingArea.Add(wIndex, area);
                        area.WorldPoint = AreaIndex2WorldPoint(wIndex);
                        yield return area.OnAreaLoadEnum();
                    }
                }
            }

            OnUpdateFinished?.Invoke();
        }

        protected abstract T OnCreate(Vector2Int wIndex);

        /// <summary>
        /// 更新区块
        /// </summary>
        /// <param name="centerPoint"></param>
        public void Update(Vector3 centerPoint)
        {
            Vector2Int centerIndex = WorldPoint2AreaIndex(centerPoint);

            List<Vector2Int> keys = new List<Vector2Int>(LoadingArea.Keys);
            foreach (Vector2Int key in keys)
            {
                T area = LoadingArea[key];
                if (OutoffRange(key, centerIndex))
                {
                    area.OnAreaUnload();
                    if (ObjectPoolMode)
                    {
                        _usingPool.Push(area);
                    }
                    else
                    {
                        UnloadArea.Add(key, area);
                    }
                    LoadingArea.Remove(key);
                }
            }

            int xMax = Mathf.FloorToInt(MaxRange / Width);
            int yMax = Mathf.FloorToInt(MaxRange / Height);

            for (int x = -xMax; x <= xMax; ++x)
            {
                for (int y = -yMax; y <= yMax; ++y)
                {
                    Vector2Int wIndex = new Vector2Int(centerIndex.x + x, centerIndex.y + y);
                    if (!OutoffRange(wIndex, centerIndex) && !LoadingArea.ContainsKey(wIndex))
                    {
                        T area;
                        if (!ObjectPoolMode)
                        {
                            if (!UnloadArea.TryGetValue(wIndex, out area))
                            {
                                area = OnCreate(wIndex);
                            }
                            else
                            {
                                UnloadArea.Remove(wIndex);
                            }
                        }
                        else
                        {
                            area = _usingPool.Get();
                        }
                        LoadingArea.Add(wIndex, area);
                        area.WorldPoint = AreaIndex2WorldPoint(wIndex);
                        area.OnAreaLoad();
                    }
                }
            }
            OnUpdateFinished?.Invoke();
        }

        public Vector3 AreaIndex2WorldPoint(Vector2Int wIndex)
        {
            return new Vector3(wIndex.x * Width, 0, wIndex.y * Height);
        }

        public void ReleaseUnloadArea()
        {
            foreach (var item in UnloadArea.Values)
            {
                item.OnAreaRelease();
            }
            UnloadArea.Clear();
        }

        public void Dispose()
        {
            ReleaseUnloadArea();
            ReleaseLoadingArea();
        }

        public void ReleaseLoadingArea()
        {
            foreach (var item in LoadingArea.Values)
            {
                item.OnAreaRelease();
            }
            LoadingArea.Clear();
        }
    }
}
