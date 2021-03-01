using Localization;
using EditorHelper;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace GameEditor
{
    public static class LocalizationHelper
    {
        private static Dictionary<int, string> _dict;
        public static LocalizationString Edit(this LocalizationString str, string desc, ref string tempData)
        {
            if (_dict == null)
            {
                Init();
            }
            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(desc);

                if (tempData == LocalizationString.LOCAL_NULL_DATA)
                {
                    if (_dict.TryGetValue(str.LocalizationName, out string data))
                    {
                        tempData = data;
                    }
                    else
                    {
                        tempData = "";
                    }
                }

                str.LocalizationName = EditorGUILayout.IntField("索引", str.LocalizationName);
                tempData = EditorGUILayout.TextArea(tempData);
                if (TrySearch(tempData, out int newId))
                {
                    str.LocalizationName = newId;
                }
                using (new EditorHorizontalLayout(EditorStyles.boldLabel))
                {

                    if (GUILayout.Button("创建新词条"))
                    {
                        if (!TrySearch(tempData, out int id))
                        {
                            id = tempData.GetHashCode();
                            str.LocalizationName = id;
                            _dict.Add(id, tempData);
                        }
                    }

                    if (GUILayout.Button("写入文件"))
                    {
                        WriteToFile();
                    }
                }
            }
            return str;
        }

        private static void Init()
        {
            _dict = new Dictionary<int, string>();
            string path = $"Assets/Resources/{LocalizationManager.FILE_NAME}";
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string data = reader.ReadToEnd();
                    var list = JsonUtility.FromJson<LocalizationManager.TempJsonType>(data);
                    _dict = list.ToDict();
                }
            }
            else
            {
                File.Create(path);
                AssetDatabase.Refresh();
            }
        }

        private static void WriteToFile()
        {
            string path = $"Assets/Resources/{LocalizationManager.FILE_NAME}";
            using (StreamWriter writer = new StreamWriter(path))
            {
                LocalizationManager.TempJsonType t = new LocalizationManager.TempJsonType(_dict);
                string json = EditorJsonUtility.ToJson(t);
                writer.Write(json);
            }
            AssetDatabase.Refresh();
        }

        private static bool TrySearch(string data, out int id)
        {
            id = 0;
            foreach (var item in _dict)
            {
                if (item.Value == data)
                {
                    id = item.Key;
                    return true;
                }
            }
            return false;
        }
    }
}
