using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Generator
{
    public class GameEventSystem : MonoBehaviour
    {

        private Dictionary<GameEventType, List<Action<GameEventParam>>> _eventList;
        private Dictionary<GameEventType, List<Action<GameEventParam>>> _dynimacEventList;

        private static GameEventSystem _INSTANCE;

        public void Start()
        {
            Type[] allType = GetType().Assembly.GetTypes();
            _eventList = new Dictionary<GameEventType, List<Action<GameEventParam>>>();
            _dynimacEventList = new Dictionary<GameEventType, List<Action<GameEventParam>>>();

            foreach (Type type in allType)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                foreach (MethodInfo info in methods)
                {
                    if (!(info.GetCustomAttribute(typeof(GameEventAttribute)) is GameEventAttribute attr))
                    {
                        continue;
                    }
                    Set(attr.EventType, info);
                }
            }

            _INSTANCE = this;
            ThrowEvent(GameEventType.GameEventInitFinished, new GameEventParam());
        }

        private void Set(GameEventType gameType, MethodInfo info)
        {
            if (!_eventList.TryGetValue(gameType, out List<Action<GameEventParam>> list))
            {
                list = new List<Action<GameEventParam>>();
                _eventList.Add(gameType, list);
            }

            list.Add((Action<GameEventParam>)Delegate.CreateDelegate(typeof(Action<GameEventParam>), info));
        }

        public static void ThrowEvent(GameEventType type, GameEventParam param)
        {
            _INSTANCE.ThrowEventAct(type, param);
        }

        public static void Register(GameEventType type, Action<GameEventParam> action)
        {
            _INSTANCE.RegisterEvent(type, action);
        }

        public static void Unregister(GameEventType type, Action<GameEventParam> action)
        {
            _INSTANCE?.UnregisterEvent(type, action);
        }

        private void RegisterEvent(GameEventType gameType, Action<GameEventParam> action)
        {
            if (!_dynimacEventList.TryGetValue(gameType, out List<Action<GameEventParam>> list))
            {
                list = new List<Action<GameEventParam>>();
                _dynimacEventList.Add(gameType, list);
            }

            list.Add(action);
        }

        private void UnregisterEvent(GameEventType gameType, Action<GameEventParam> action)
        {
            if (_dynimacEventList.TryGetValue(gameType, out List<Action<GameEventParam>> list))
            {
                list.Remove(action);
            }
        }

        private void ThrowEventAct(GameEventType type, GameEventParam param)
        {
            if (_eventList.TryGetValue(type, out List<Action<GameEventParam>> list))
            {
                try
                {
                    foreach (var item in list)
                    {
                        item(param);
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
            }
            if (_dynimacEventList.TryGetValue(type, out List<Action<GameEventParam>> dynList))
            {
                try
                {
                    foreach (var item in dynList)
                    {
                        item(param);
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError(err);
                }
            }
        }

        public void OnDestroy()
        {
            _eventList.Clear();
            _INSTANCE = null;
        }
    }
}
