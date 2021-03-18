using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameToolComponents
{
    [Serializable]
    public class ReferenceCollector : MonoBehaviour
    {
        public List<string> Names = new List<string>();
        public List<Object> Objects = new List<Object>();
        public void Awake()
        {
        }

        public T Get<T>(string name) where T : Object
        {
            if (TryGetIndex(name, out int index))
            {
                return (T)Objects[index];
            }
            throw new KeyNotFoundException($"This is no key named {name}");
        }

        public int Count => Names.Count;

        public List<string> Keys
        {
            get
            {
                return new List<string>(Names);
            }
        }
        public List<Object> Values
        {
            get
            {
                return new List<Object>(Objects);
            }
        }

        public void Remove(string name)
        {
            if (TryGetIndex(name, out int index))
            {
                Names.Remove(name);
                Objects.RemoveAt(index);
            }
        }

        public void Add(string name, Object value)
        {
            if (!TryGetIndex(name, out int index))
            {
                Names.Add(name);
                Objects.Add(value);
            }
            throw new ArgumentException($"{name} is already existed");
        }

        private bool TryGetIndex(string name, out int index)
        {
            index = Names.IndexOf(name);
            return index != -1;
        }

        public Object this[string name]
        {
            get { return Get<Object>(name); }
            set
            {
                if (TryGetIndex(name, out int index))
                {
                    Objects[index] = value;
                }
                else
                {
                    Names.Add(name);
                    Objects.Add(value);
                }
            }
        }
    }
}
