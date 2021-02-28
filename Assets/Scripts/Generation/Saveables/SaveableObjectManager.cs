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

        private SaveableAreaManager _areaManager;
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
            _areaManager = new SaveableAreaManager(AreaWidth, AreaHeight, MaxActiveRange);
            _areaManager.Update(CenterObject.transform.position);
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

        public List<int> GetIds()
        {
            return new List<int>(_activityObject.Keys);
        }

        public GameObject Get(int id)
        {
            return _activityObject[id];
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
                _areaManager.Update(CenterObject.transform.position);
                _deltaTime = 0;
            }
            else
            {
                _deltaTime += Time.deltaTime;
            }
        }
    }
}
