using EditorHelper;
using Generator;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(EnvironmentObjectData))]
    public class EnvironmentObjectDataEditor : Editor
    {
        private EnvironmentObjectData _target;

        public void Awake()
        {
            _target = (EnvironmentObjectData)target;
        }

        public override void OnInspectorGUI()
        {

            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("树木参数");
                _target.TreeRarity = EditorGUILayout.Slider("密度", _target.TreeRarity, 0, 0.5f);
                _target.TreeList = EditorDataFields.EditorArrayField("模型", _target.TreeList);
            }

            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("岩石参数");
                _target.RockRarity = EditorGUILayout.Slider("密度", _target.RockRarity, 0, 0.1f);
                _target.RockList = EditorDataFields.EditorArrayField("模型", _target.RockList);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }

        public void OnDestroy()
        {
            _target = null;
        }
    }
}
