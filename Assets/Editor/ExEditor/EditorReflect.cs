using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace ExtendEditor
{
    public static class EditorReflect
    {
        private static Dictionary<Type, Type> _t2t = new Dictionary<Type, Type>();

        public static Type GetEditorType(Type srcType)
        {
            if (_t2t.Count == 0)
            {
                Init();
            }

            return _t2t[srcType];
        }

        public static void Init()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in assemblies)
            {
                Type[] types = ass.GetTypes();
                foreach (Type type in types)
                {
                    CustomEditor ce = type.GetCustomAttribute<CustomEditor>();
                    if (ce != null)
                    {
                        Type destType = GetCustomEditorAttrType(ce);
                        _t2t[destType] = type;
                    }
                }
            }
        }

        public static Type GetCustomEditorAttrType(CustomEditor data)
        {
            FieldInfo ceInfo = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Type destType = ceInfo.GetValue(data) as Type;
            return destType;
        }

        //public static void TextCustomEditor()
        //{
        //    Type type = typeof(CustomEditor);
        //    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //    foreach (FieldInfo field in fields)
        //    {
        //        Log.Debug(field.Name);
        //    }
        //}
    }
}
