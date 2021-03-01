using System;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        [Serializable]
        public class TempJsonType
        {
            public int[] Ids;
            public string[] Datas;
            public TempJsonType(Dictionary<int, string> dict)
            {
                int count = 0;
                Ids = new int[dict.Count];
                Datas = new string[dict.Count];
                foreach (var item in dict)
                {
                    Ids[count] = item.Key;
                    Datas[count] = item.Value;
                    ++count;
                }
            }

            public Dictionary<int, string> ToDict()
            {
                Dictionary<int, string> dict = new Dictionary<int, string>();
                for (int i = 0; i < Ids.Length; i++)
                {
                    dict.Add(Ids[i], Datas[i]);
                }
                return dict;
            }
        }
        public TextAsset LocalizationFile;

        private static LocalizationManager _INSTANCE;
        private Dictionary<int, string> _data;

        public const string FILE_NAME = "localization.txt";

        public void Awake()
        {
            _INSTANCE = this;
        }

        public void Start()
        {
            TempJsonType list = JsonUtility.FromJson<TempJsonType>(LocalizationFile.text);
            _data = list.ToDict();
        }

        public static string GetData(int localizationName)
        {
            return _INSTANCE._data[localizationName];
        }
    }
}
