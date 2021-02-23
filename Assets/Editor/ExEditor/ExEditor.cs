using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ExtendEditor
{
    public class ExEditor<T> : Editor where T : MonoBehaviour
    {
        protected T Target;

        private static readonly object[] EMPTY_ARRAY = new object[0];
        private Type _exType;
        private Editor _editorInstance;
        private Dictionary<string, MethodInfo> _destMethods = new Dictionary<string, MethodInfo>();

        public void Awake()
        {
            Target = (T)target;
        }
        protected Editor EditorInstance
        {
            get
            {
                if (_editorInstance == null && targets != null && targets.Length > 0)
                {
                    _editorInstance = CreateEditor(targets, _exType);
                }

                return _editorInstance;
            }
        }

        public ExEditor()
        {
            _exType = EditorReflect.GetEditorType(typeof(T));
        }

        public void OnDisable()
        {
            if (_editorInstance != null)
            {
                DestroyImmediate(_editorInstance);
            }
        }

        protected void CallInspectorMethod(string methodName)
        {
            if (!_destMethods.TryGetValue(methodName, out MethodInfo method))
            {
                var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

                method = _exType.GetMethod(methodName, flags);

                if (method != null)
                {
                    _destMethods[methodName] = method;
                }
                else
                {
                    Debug.LogError($"Could not find method {methodName}");
                }
            }
            else
            {
                method = _destMethods[methodName];
            }

            if (method != null)
            {
                method.Invoke(EditorInstance, EMPTY_ARRAY);
            }
        }

        //public void OnSceneGUI()
        //{
        //    CallInspectorMethod("OnSceneGUI");
        //}

        protected override void OnHeaderGUI()
        {
            CallInspectorMethod("OnHeaderGUI");
        }

        public override void OnInspectorGUI()
        {
            EditorInstance.OnInspectorGUI();
        }

        public override void DrawPreview(Rect previewArea)
        {
            EditorInstance.DrawPreview(previewArea);
        }

        public override string GetInfoString()
        {
            return EditorInstance.GetInfoString();
        }

        public override GUIContent GetPreviewTitle()
        {
            return EditorInstance.GetPreviewTitle();
        }

        public override bool HasPreviewGUI()
        {
            return EditorInstance.HasPreviewGUI();
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            EditorInstance.OnInteractivePreviewGUI(r, background);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            EditorInstance.OnPreviewGUI(r, background);
        }

        public override void OnPreviewSettings()
        {
            EditorInstance.OnPreviewSettings();
        }

        public override void ReloadPreviewInstances()
        {
            EditorInstance.ReloadPreviewInstances();
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            return EditorInstance.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        public override bool RequiresConstantRepaint()
        {
            return EditorInstance.RequiresConstantRepaint();
        }

        public override bool UseDefaultMargins()
        {
            return EditorInstance.UseDefaultMargins();
        }
    }
}
