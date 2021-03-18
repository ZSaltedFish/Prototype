using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameToolComponents
{
    [Serializable]
    public class SerializableDictionary<T1, T2>
    {
        public int Count => Dict.Count;

        [SerializeField]
        public Dictionary<T1, T2> Dict = new Dictionary<T1, T2>();

        public virtual void Add(T1 a, T2 b)
        {
            Dict.Add(a, b);
        }

        public virtual T3 Get<T3>(T1 a) where T3 : T2
        {
            return (T3)Dict[a];
        }

        public virtual void Remove(T1 a)
        {
            Dict.Remove(a);
        }

        public T2 this[T1 key]
        {
            get { return Dict[key]; }
            set { Dict[key] = value; }
        }

        public virtual List<T1> Keys => new List<T1>(Dict.Keys);
        public virtual List<T2> Values => new List<T2>(Dict.Values);
    }
}
