using System;
using System.Collections.Generic;

namespace GameToolComponents
{
    public class ObjectPoolPattern<T> : IDisposable
    {
        private readonly Queue<T> _poolQueue;

        public Func<T> OnCreateInstance;
        public Action<T> OnObjectGottenFromPool;
        public Action<T> OnObjectPushinPool;
        public Action<T> OnObjectDistroy;

        public ObjectPoolPattern(Func<T> createInstance)
        {
            _poolQueue = new Queue<T>();
            OnCreateInstance = createInstance;
        }

        public void Dispose()
        {
            while (_poolQueue.Count > 0)
            {
                OnObjectDistroy(_poolQueue.Dequeue());
            }
        }

        public T Get()
        {
            if (_poolQueue.Count == 0)
            {
                _poolQueue.Enqueue(OnCreateInstance());
            }
            T obj = _poolQueue.Dequeue();
            OnObjectGottenFromPool?.Invoke(obj);
            return obj;
        }

        public void Push(T obj)
        {
            OnObjectPushinPool?.Invoke(obj);
            _poolQueue.Enqueue(obj);
        }
    }
}
