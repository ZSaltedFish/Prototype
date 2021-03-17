using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameToolComponents
{
    [Serializable]
    public class ReferenceCollector : MonoBehaviour
    {
        [Serializable]
        public class StringObjectPair
        {
            public string Key;
            public Object Value;

            public StringObjectPair(string key)
            {
                Key = key;
            }
        }

        public List<T> GetAll<T>() where T : Object
        {
            List<T> list = new List<T>();
            foreach (T item in _ref.Values)
            {
                list.Add(item);
            }
            return list;
        }

        public List<StringObjectPair> RefList;

        private Dictionary<string, Object> _ref = null;
        private bool _isInited => _ref != null;
        public void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (_isInited) return;
            _ref = new Dictionary<string, Object>();
            foreach (var item in RefList)
            {
                _ref.Add(item.Key, item.Value);
            }
        }

        public T Get<T>(string name) where T : Object
        {
            Init();
            return _ref[name] as T;
        }

        public int Count => RefList.Count;

        public List<string> Keys
        {
            get
            {
                Init();
                return new List<string>(_ref.Keys);
            }
        }
        public List<Object> Values
        {
            get
            {
                Init();
                return new List<Object>(_ref.Values);
            }
        }
    }
}
