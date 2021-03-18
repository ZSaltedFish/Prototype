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
            [SerializeField]
            public string Key;
            [SerializeField]
            public Object Value;

            public StringObjectPair(string key)
            {
                Key = key;
            }
        }

        public List<StringObjectPair> RefList;
        public void Awake()
        {
        }

        public T Get<T>(string name) where T : Object
        {
            return (T)RefList.Find(value => value.Key == name).Value;
        }

        public int Count => RefList.Count;

        public List<string> Keys
        {
            get
            {
                List<string> names = new List<string>();
                foreach (var item in RefList)
                {
                    names.Add(item.Key);
                }
                return names;
            }
        }
        public List<Object> Values
        {
            get
            {
                List<Object> names = new List<Object>();
                foreach (var item in RefList)
                {
                    names.Add(item.Value);
                }
                return names;
            }
        }
    }
}
