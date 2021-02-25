using EditorHelper;
using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEditor
{
    [CustomEditor(typeof(ReferenceCollector))]
    public class ReferenceCollectorEditor : Editor
    {
        private ReferenceCollector _target;
        private string _key = string.Empty;

        public void Awake()
        {
            _target = (ReferenceCollector)target;

        }

        public override void OnInspectorGUI()
        {
            _target.RefList = EditorDictionaryField("索引字典", _target.RefList, ref _key);
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }

        public static List<ReferenceCollector.StringObjectPair> EditorDictionaryField(string desc, List<ReferenceCollector.StringObjectPair> dict, ref string controlKey)
        {
            if (dict == null)
            {
                dict = new List<ReferenceCollector.StringObjectPair>();
            }

            int deleteIndex = -1;
            int count = 0;
            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(desc);
                foreach (var item in dict)
                {
                    using (new EditorHorizontalLayout("Button"))
                    {
                        item.Key = EditorGUILayout.TextField(item.Key);
                        Object value = item.Value;
                        value = EditorDataFields.EditorDataField(value);
                        item.Value = value;

                        if (GUILayout.Button("x"))
                        {
                            deleteIndex = count;
                        }
                        ++count;
                    }
                }

                if (deleteIndex != -1)
                {
                    dict.RemoveAt(deleteIndex);
                }

                using (new EditorVerticalLayout(EditorStyles.helpBox))
                {
                    using (new EditorHorizontalLayout("Button"))
                    {
                        controlKey = EditorDataFields.EditorDataField(controlKey);
                        try
                        {
                            if (GUILayout.Button("新建"))
                            {
                                dict.Add(new ReferenceCollector.StringObjectPair(controlKey));
                            }
                        }
                        catch (Exception err)
                        {
                            Debug.LogError($"对Key({controlKey})的操作失败\t{err}");
                        }
                    }
                }
            }
            return dict;
        }
    }
}
