using Generator;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(SceneObject))]
    public class SceneObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("锁定点"))
            {

            }
        }
    }
}
