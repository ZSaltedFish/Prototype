using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorHelper
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class EditorDataFieldAttribute : Attribute
    {
        public Type FieldType;
        public MethodInfo UsingFuncion;
        public object DefaultValue;
        public EditorDataFieldAttribute(Type type, object defaultValue)
        {
            FieldType = type;
            DefaultValue = defaultValue;
        }

        public EditorDataFieldAttribute(Type type)
        {
            FieldType = type;
            DefaultValue = Activator.CreateInstance(type);
        }

        public object Invoke(string desc, object data)
        {
            return Invoke(desc, data, FieldType);
        }

        public object Invoke(string desc, object data, Type type)
        {
            if (data == null)
            {
                data = DefaultValue;
            }
            object[] objs = { desc, data, type };
            return UsingFuncion.Invoke(null, objs);
        }
    }
    public partial class EditorDataFields
    {
        private static Dictionary<Type, EditorDataFieldAttribute> _type2Infos = new Dictionary<Type, EditorDataFieldAttribute>();
        [MenuItem("Tools/批量操作/Init输入")]
        public static void Init()
        {
            ExtendEditor.EditorReflect.Init();
            Type t = typeof(EditorDataFields);
            _type2Infos.Clear();
            MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (MethodInfo method in methods)
            {
                try
                {
                    object[] attrs = method.GetCustomAttributes(typeof(EditorDataFieldAttribute), false);
                    foreach (EditorDataFieldAttribute attr in attrs)
                    {
                        attr.UsingFuncion = method;
                        _type2Infos.Add(attr.FieldType, attr);
                    }
                }
                catch(Exception err)
                {
                    Debug.LogError(err);
                }
            }
        }

        [EditorDataField(typeof(bool), false)]
        private static object BoolFeild(string desc, object data, Type type)
        {
            bool value = (bool)data;
            return EditorGUILayout.Toggle(desc, value);
        }

        [EditorDataField(typeof(int), 0)]
        private static object IntFeild(string desc, object data, Type type)
        {
            return EditorGUILayout.IntField(desc, (int)data);
        }

        [EditorDataField(typeof(float), 0f)]
        private static object FloatFeild(string desc, object data, Type type)
        {
            return EditorGUILayout.FloatField(desc, (float)data);
        }

        [EditorDataField(typeof(long), 0L)]
        private static object LongFeild(string desc, object data, Type type)
        {
            return EditorGUILayout.LongField(desc, (long)data);
        }

        [EditorDataField(typeof(double), 0d)]
        private static object DoubleFeild(string desc, double data, Type type)
        {
            return EditorGUILayout.DoubleField(desc, data);
        }

        [EditorDataField(typeof(string), "")]
        private static object StringFeild(string desc, object data, Type type)
        {
            return EditorGUILayout.TextField(desc, (string)data);
        }
        
        [EditorDataField(typeof(Color))]
        private static object ColorFeild(string desc, object data, Type type)
        {
            return EditorGUILayout.ColorField(desc, (Color)data);
        }

        [EditorDataField(typeof(Vector2))]
        private static object Vector2Field(string desc, object data, Type type)
        {
            return EditorGUILayout.Vector2Field(desc, (Vector2)data);
        }

        [EditorDataField(typeof(Vector3))]
        private static object Vector3Field(string desc, object data, Type type)
        {
            return EditorGUILayout.Vector3Field(desc, (Vector3)data);
        }

        [EditorDataField(typeof(Vector4))]
        private static object Vector4Field(string desc, object data, Type type)
        {
            return EditorGUILayout.Vector3Field(desc, (Vector4)data);
        }

        [EditorDataField(typeof(UnityEngine.Object), null)]
        private static object ObjectField(string desc, object data, Type type)
        {
            if (data == null)
            {
                return EditorGUILayout.ObjectField(desc, null, type, true);
            }
            UnityEngine.Object obj = null;
            try
            {
                UnityEngine.Object objData = (UnityEngine.Object)data;
                obj = EditorGUILayout.ObjectField(desc, objData, type, true);
            }
            catch(InvalidCastException e)
            {
                Debug.LogError(e);
            }
            catch(ExitGUIException)
            {
                return data;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            return obj;
        }

        [EditorDataField(typeof(Enum), 0)]
        private static object EnumField(string desc, object data, Type type)
        {
            if (data is string)
            {
                try
                {
                    data = Enum.Parse(type, data as string);
                }
                catch(ArgumentException err)
                {
                    Debug.LogWarning($"{data}不存在于{type}中，已重置数据 | {err}");
                    data = 0;
                }
            }
            if (((int)data) == 0)
            {
                data = Enum.GetValues(type).GetValue(0);
            }
            return EditorGUILayout.EnumPopup(desc, data as Enum);
        }

        public static T EditorDataField<T>(T data)
        {
            return EditorDataField("", data);
        }

        public static object EditorDataField(object obj)
        {
            return EditorDataField("", obj, obj.GetType());
        }

        private static object RunArray(string desc, object data, Type arrType)
        {
            Type type = arrType.GetElementType();
            if (data == null)
            {
                data = Array.CreateInstance(type, 0);
            }
            Array arr = data as Array;
            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                int index = EditorGUILayout.IntField(desc, arr.Length);
                if (index != arr.Length)
                {
                    arr = Array.CreateInstance(type, index);
                }

                for (int i = 0; i < arr.Length; ++i)
                {
                    arr.SetValue(EditorDataField($"索引{i}", arr.GetValue(i), type), i);
                }
            }
            return arr;
        }

        public static T[] EditorArrayField<T>(string desc, T[] data)
        {
            Type type = typeof(T);
            if (data == null)
            {
                data = Array.CreateInstance(type, 0) as T[];
            }
            T[] arr = data;
            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                int index = EditorGUILayout.IntField(desc, arr.Length);
                index = Mathf.Max(0, index);
                if (index != arr.Length)
                {
                    arr = Array.CreateInstance(type, index) as T[];
                }

                for (int i = 0; i < arr.Length; ++i)
                {
                    arr.SetValue(EditorDataField($"索引{i}", arr.GetValue(i), type), i);
                }
            }
            return arr;
        }

        public static object EditorDataField(string desc, object data, Type type)
        {
            if (_type2Infos.Count == 0)
            {
                Init();
            }
            if (type.IsSubclassOf(typeof(Component)))
            {
                GameObject go;
                if (data != null)
                {
                    go = _type2Infos[typeof(UnityEngine.Object)].Invoke(desc, (data as Component).gameObject, typeof(GameObject)) as GameObject;
                }
                else
                {
                    go = _type2Infos[typeof(UnityEngine.Object)].Invoke(desc, null, typeof(GameObject)) as GameObject;
                }
                if (go?.GetComponent(type) != null)
                {
                    return go.GetComponent(type);
                }
                else
                {
                    return null;
                }
            }
            Type objType = typeof(UnityEngine.Object);
            if (type.IsSubclassOf(objType))
            {
                return _type2Infos[objType].Invoke(desc, data, type);
            }

            if (type.IsSubclassOf(typeof(Enum)))
            {
                GUI.SetNextControlName(desc);
                return _type2Infos[typeof(Enum)].Invoke(desc, data, type);
            }
            else if (type.IsArray)
            {
                return RunArray(desc, data, type);
            }
            object obj = null;
            try
            {
                obj = _type2Infos[type].Invoke(desc, data);
            }
            catch(InvalidCastException)
            {

            }
            catch(KeyNotFoundException e)
            {
                EditorGUILayout.LabelField($"不支持的类型:{type} -> {e}");
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            return obj;
        }
        public static T EditorDataField<T>(string desc, T data)
        {
            Type type = typeof(T);
            return (T)EditorDataField(desc, data, type);
        }

        private static int _page = 0;
        private static Vector2 _scroll;
        public static List<T> EditorListDataField<T>(List<T> datas, int pageMax)
        {
            List<T> newList = new List<T>(datas);
            using (new EditorVerticalLayout())
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                for (int i = 0; i < pageMax && i + pageMax * _page < datas.Count; i++)
                {
                    newList[i] = EditorDataField(i.ToString(), datas[i + pageMax * _page]);
                }
                EditorGUILayout.EndScrollView();
                int maxPage = datas.Count / pageMax;
                using (new EditorHorizontalLayout())
                {
                    if (GUILayout.Button("<"))
                    {
                        _page = Mathf.Max(0, _page - 1);
                    }
                    EditorDataField("", $"{_page + 1}/{maxPage + 1}");
                    if (GUILayout.Button(">"))
                    {
                        _page = Mathf.Min(maxPage, _page + 1);
                    }
                }
            }
            return newList;
        }
    }
}
