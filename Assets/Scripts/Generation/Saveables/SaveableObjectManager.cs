using Entitys;
using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    [RequireComponent(typeof(ReferenceCollector))]
    public class SaveableObjectManager : MonoBehaviour
    {
        public float MaxActiveRange = 1000f;
        public float UpdateTime = 5f;
        public int AreaWidth, AreaHeight;
        public GameObject CenterObject;
        public static SaveableObjectManager INSTANCE { get; private set; }

        private Dictionary<Vector2Int, SaveableArea> _unloadArea = new Dictionary<Vector2Int, SaveableArea>();
        private Dictionary<Vector2Int, SaveableArea> _loadingArea = new Dictionary<Vector2Int, SaveableArea>();

        private SaveableObjectPools _objPools;
        private Dictionary<int, GameObject> _activityObject = new Dictionary<int, GameObject>();

        public void Start()
        {
            if (INSTANCE != null)
            {
                throw new InvalidOperationException("This component can only be added at once.");
            }
            INSTANCE = this;
            ReferenceCollector refCol = GetComponent<ReferenceCollector>();
            _objPools = new SaveableObjectPools(refCol.Keys, refCol.Values, OnGet, OnPush);

            UpdateArea();
        }

        private void OnGet(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnPush(GameObject obj)
        {
            obj.SetActive(false);
        }

        #region Object管理
        public void CreateObj(string name, byte[] initData)
        {
            GameObject go = _objPools.Get(name);
            SaveableObject saveComponent = go.GetComponent<SaveableObject>();

            if (saveComponent != null)
            {
                saveComponent.Deserialize(initData);
            }
            SetName(name, go);
            _activityObject.Add(go.GetInstanceID(), go);
        }

        public void CreateObj(string name, Vector3 pos)
        {
            GameObject go = _objPools.Get(name);
            go.transform.position = pos;
            SetName(name, go);
            _activityObject.Add(go.GetInstanceID(), go);
        }

        public void Remove(int id)
        {
            GameObject go = _activityObject[id];
            Remove(go);
        }

        public void Remove(GameObject go)
        {
            string srcName = GetFromName(go);
            _objPools.Push(srcName, go);
            _activityObject.Remove(go.GetInstanceID());
        }

        private static void SetName(string name, GameObject go)
        {
            go.name = $"{name}~{go.GetInstanceID()}";
        }

        public static string GetFromName(GameObject go)
        {
            return go.name.Split('~')[0];
        }
        #endregion

        #region 活动区块管理

        private void UpdateArea()
        {
            Vector3 pos = CenterObject.transform.position;
            Vector2Int centerIndex = WorldPoint2AreaIndex(pos);

            int xMax = Mathf.FloorToInt(MaxActiveRange / AreaWidth);
            int yMax = Mathf.FloorToInt(MaxActiveRange / AreaHeight);

            List<Vector2Int> keys = new List<Vector2Int>(_loadingArea.Keys);
            foreach (Vector2Int key in keys)
            {
                SaveableArea area = _loadingArea[key];
                if (OutoffRange(area.AreaIndex, centerIndex))
                {
                    List<GameObject> unloadObjects = new List<GameObject>();
                    List<int> actkeys = new List<int>(_activityObject.Keys);
                    foreach (int id in actkeys)
                    {
                        GameObject go = _activityObject[id];
                        if (WorldPoint2AreaIndex(go.transform.position) == area.AreaIndex)
                        {
                            unloadObjects.Add(go);
                            Remove(go);
                        }
                    }
                    area.Save(unloadObjects);

                    _unloadArea.Add(key, area);
                    _loadingArea.Remove(key);
                }
            }

            for (int x = -xMax; x <= xMax; ++x)
            {
                for (int y = -yMax; y <= yMax; ++y)
                {
                    Vector2Int wIndex = new Vector2Int(centerIndex.x + x, centerIndex.y + y);
                    if (!OutoffRange(wIndex, centerIndex) && !_loadingArea.ContainsKey(wIndex))
                    {
                        if (!_unloadArea.TryGetValue(wIndex, out SaveableArea area))
                        {
                            area = new SaveableArea(wIndex);
                        }
                        else
                        {
                            _unloadArea.Remove(wIndex);
                        }

                        var nameList = area.Load();
                        foreach (string name in nameList.Keys)
                        {
                            foreach (byte[] item in nameList[name])
                            {
                                CreateObj(name, item);
                            }
                        }

                        _loadingArea.Add(wIndex, area);
                    }
                }
            }
        }
        #endregion

        #region 坐标计算
        private bool OutoffRange(Vector2Int index1, Vector2Int index2)
        {
            float xDist = Mathf.Abs(index1.x - index2.x) * AreaWidth;
            float yDist = Mathf.Abs(index1.y - index2.y) * AreaHeight;

            return xDist * xDist + yDist * yDist > MaxActiveRange * MaxActiveRange;
        }

        private Vector2Int WorldPoint2AreaIndex(Vector3 worldPoint)
        {
            int xCount = ((int)worldPoint.x) / AreaWidth - (worldPoint.x < 0 ? 1 : 0);
            int yCount = ((int)worldPoint.y) / AreaHeight - (worldPoint.y < 0 ? 1 : 0);
            return new Vector2Int(xCount, yCount);
        }
        #endregion

        private float _deltaTime = 0;
        public void Update()
        {
            if (_deltaTime > UpdateTime)
            {
                Debug.Log("Update Area");
                UpdateArea();
                _deltaTime = 0;
            }
            else
            {
                _deltaTime += Time.deltaTime;
            }
        }
    }
}
