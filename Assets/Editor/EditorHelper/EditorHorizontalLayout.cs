using System;
using UnityEditor;
using UnityEngine;

namespace EditorHelper
{
    public class EditorHorizontalLayout : IDisposable
    {
        public EditorHorizontalLayout(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
        }

        public EditorHorizontalLayout(GUIStyle style, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
