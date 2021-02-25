using Entitys;
using System;
using System.Collections.Generic;
using System.Text;
using Tools;
using UnityEngine;

namespace Generator
{
    /// <summary>
    /// 可储存区域
    /// </summary>
    [SerializeField]
    public class SaveableArea
    {
        public Vector2Int AreaIndex;
        public int Width, Height;
        public List<SaveableObject> SaveList;

        public const string PATTERN_TITLE = "|SAVEABLE_DATA_INDEX|";
        public const string SAVEABLE_AREA_NAME_TITLE = "SaveableAreaName";

        private byte[] _preLoadData;

        public SaveableArea(Vector2Int wIndex)
        {
            AreaIndex = wIndex;
        }

        private string GetFileName()
        {
            return $"{Application.dataPath}/{SAVEABLE_AREA_NAME_TITLE}_{AreaIndex.x}_{AreaIndex.y}.save";
        }

        public Dictionary<string, List<byte[]>> Load()
        {
            Dictionary<string, List<byte[]>> dict = new Dictionary<string, List<byte[]>>();
            if (_preLoadData == null || _preLoadData.Length == 0)
            {
                return dict;
            }

            BytesIO reader = new BytesIO(_preLoadData);
            int count = reader.GetInt32();
            for (int i = 0; i < count; ++i)
            {
                string name = reader.GetString();
                byte[] nameData = reader.GetBytes();
                if (!dict.TryGetValue(name, out List<byte[]> list))
                {
                    list = new List<byte[]>();
                    dict.Add(name, list);
                }
                list.Add(nameData);
            }
            Debug.Log($"读取地块包含{count}个单位");
            return dict;
        }

        public void Save(List<GameObject> unloadObjects)
        {
            BytesIO writer = new BytesIO();
            writer.Set(unloadObjects.Count);
            foreach (GameObject go in unloadObjects)
            {
                string name = SaveableObjectManager.GetFromName(go);
                SaveableObject saveObj = go.GetComponent<SaveableObject>();
                byte[] serialize = saveObj.Serialize();
                writer.Set(name).Set(serialize);
            }
            Debug.Log($"保存地块包含{unloadObjects.Count}个单位");

            _preLoadData = writer.ToBytes();
        }
    }
}
