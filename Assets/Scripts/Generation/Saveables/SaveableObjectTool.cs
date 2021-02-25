using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Generator
{
    public class SaveableObjectPools
    {
        private Dictionary<string, ObjectPoolPattern<GameObject>> _pools;
        private Dictionary<string, GameObject> _goDict;
        private string _name;

        public SaveableObjectPools(List<string> names, List<Object> objs, Action<GameObject> onGet, Action<GameObject> onPush)
        {
            _pools = new Dictionary<string, ObjectPoolPattern<GameObject>>();
            _goDict = new Dictionary<string, GameObject>();
            for (int i = 0; i < names.Count; ++i)
            {
                ObjectPoolPattern<GameObject> pool = new ObjectPoolPattern<GameObject>(OnCreate)
                {
                    OnObjectGottenFromPool = onGet,
                    OnObjectPushinPool = onPush
                };
                _goDict.Add(names[i], (GameObject)objs[i]);
                _pools.Add(names[i], pool);
            }
        }

        private GameObject OnCreate()
        {
            return Object.Instantiate(_goDict[_name]);
        }

        public GameObject Get(string name)
        {
            _name = name;
            return _pools[name].Get();
        }

        public void Push(string name, GameObject go)
        {
            _pools[name].Push(go);
        }
    }
}
