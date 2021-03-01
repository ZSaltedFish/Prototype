using System.Collections.Generic;

namespace Generator
{
    public class GameEventParam
    {
        private Dictionary<string, object> _dict = new Dictionary<string, object>();

        public GameEventParam Add(string key, object value)
        {
            _dict.Add(key, value);
            return this;
        }

        public T Get<T>(string key)
        {
            return (T)_dict[key];
        }
    }
}