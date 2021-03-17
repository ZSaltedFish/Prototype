using Entitys;
using GameToolComponents;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Generator
{
    /// <summary>
    /// 可储存区域
    /// </summary>
    [SerializeField]
    public class SaveableArea : DynamicArea
    {
        public List<SaveableObject> SaveList;

        public const string SAVEABLE_AREA_NAME_TITLE = "SaveableAreaName";

        private byte[] _preLoadData;

        public SaveableArea(float width, float height, Vector2Int wIndex) : base(width, height, wIndex)
        {
        }

        private Dictionary<string, List<byte[]>> Load()
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
            return dict;
        }

        private void Save(List<GameObject> unloadObjects)
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

            _preLoadData = writer.ToBytes();
        }

        public override void OnAreaLoad()
        {
            Dictionary<string, List<byte[]>> loads = Load();
            foreach (string name in loads.Keys)
            {
                foreach (byte[] item in loads[name])
                {
                    SaveableObjectManager.INSTANCE.CreateObj(name, item);
                }
            }
        }

        public override void OnAreaUnload()
        {
            List<int> ids = SaveableObjectManager.INSTANCE.GetIds();
            List<GameObject> unloadObjects = new List<GameObject>();
            foreach (int id in ids)
            {
                GameObject go = SaveableObjectManager.INSTANCE.Get(id);
                if (WorldPoint2Index(go.transform.position) == Index)
                {
                    unloadObjects.Add(go);
                    SaveableObjectManager.INSTANCE.Remove(go);
                }
            }
            Save(unloadObjects);
        }

        public override void OnAreaRelease()
        {
            //保存区域到文件
        }
    }
}
