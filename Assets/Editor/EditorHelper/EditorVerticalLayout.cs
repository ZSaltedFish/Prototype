using System;
using UnityEditor;
using UnityEngine;

namespace EditorHelper
{
    public class EditorVerticalLayout : IDisposable
    {
        public EditorVerticalLayout(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
        }

        public EditorVerticalLayout(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
