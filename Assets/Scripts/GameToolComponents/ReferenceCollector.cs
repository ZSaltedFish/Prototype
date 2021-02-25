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

        public List<StringObjectPair> RefList;

        private Dictionary<string, Object> _ref = new Dictionary<string, Object>();
        public void Awake()
        {
            foreach (var item in RefList)
            {
                _ref.Add(item.Key, item.Value);
            }
        }

        public T Get<T>(string name) where T : Object
        {
            return _ref[name] as T;
        }

        public int Count => _ref.Count;

        public List<string> Keys => new List<string>(_ref.Keys);
        public List<Object> Values => new List<Object>(_ref.Values);
    }
}
