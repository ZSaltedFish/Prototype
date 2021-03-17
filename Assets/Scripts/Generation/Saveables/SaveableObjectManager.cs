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
            _areaManager = new SaveableAreaManager(AreaWidth, AreaHeight, MaxActiveRange)
            {
                OnUpdateFinished = OnUpdateFinished
            };

            GameEventSystem.Register(GameEventType.TerrainUpdateFinished, OnTerrainUpdateFinished);
            GameEventSystem.ThrowEvent(GameEventType.SaveableObjectRegister);
        }

        private void OnUpdateFinished()
        {
            GameEventSystem.ThrowEvent(GameEventType.SaveableObjectUpdateFinished);
        }

        private void OnTerrainUpdateFinished(GameEventParam obj)
        {
            _areaManager.Update(obj.Get<Vector3>(GameEventFlag.Terrain_update_location));
        }

        private void OnGet(GameObject obj)
        {
            obj.SetActive(true);
        }

        private void OnPush(GameObject obj)
        {
            obj.SetActive(false);
        }

        public void OnDestroy()
        {
            GameEventSystem.Unregister(GameEventType.TerrainUpdateFinished, OnTerrainUpdateFinished);
        }

        #region Object管理
        public void CreateObj(string name, byte[] initData)
        {
            GameObject go = _objPools.Get(name);
            go.transform.SetParent(transform);
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
            go.transform.SetParent(transform);
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

        public void Register(string name, GameObject obj)
        {
            try
            {
                _objPools.Add(name, obj, OnGet, OnPush);
            }
            catch (Exception err)
            {
                Debug.LogError(err);
            }
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
    }
}
