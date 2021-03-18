using EditorHelper;
using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReferenceCollector))]
    public class ReferenceCollectorEditor : Editor
    {
        private const string KEY_WORLD = "索引字典";
        private ReferenceCollector _target;
        private string _key = string.Empty;

        public void Awake()
        {
            _target = (ReferenceCollector)target;

        }

        public override void OnInspectorGUI()
        {
            EditorDictionaryField();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }

        public void EditorDictionaryField()
        {
            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(KEY_WORLD);
                List<string> keys = _target.Keys;
                foreach (var item in keys)
                {
                    using (new EditorHorizontalLayout("Button"))
                    {
                        EditorGUILayout.TextField(item);
                        Object value = _target[item];
                        value = EditorDataFields.EditorDataField(value);
                        _target[item] = value;

                        if (GUILayout.Button("x"))
                        {
                            _target.Remove(item);
                        }
                    }
                }

                using (new EditorVerticalLayout(EditorStyles.helpBox))
                {
                    using (new EditorHorizontalLayout("Button"))
                    {
                        _key = EditorDataFields.EditorDataField(_key);
                        try
                        {
                            if (GUILayout.Button("Create"))
                            {
                                _target.Add(_key, null);
                            }
                        }
                        catch (Exception err)
                        {
                            Debug.LogError($"对Key({_key})的操作失败\t{err}");
                        }
                    }
                }
            }
            DragAreaGetObject.GetDrawData(_target);
        }
    }
}
