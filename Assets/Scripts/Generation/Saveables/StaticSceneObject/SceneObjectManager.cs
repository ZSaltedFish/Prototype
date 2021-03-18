using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public class SceneObjectManager : MonoBehaviour
    {
        public ListCollector RefCol;
        public Dictionary<Vector2Int, List<SceneObject>> PosObjList;

        private Dictionary<string, SceneObject> _createdList;
        private SceneObjectController _ctrl;
        public static SceneObjectManager INSTANCE;
        public void Start()
        {
            RegisterData();
            _createdList = new Dictionary<string, SceneObject>();
            GameEventSystem.Register(GameEventType.SaveableObjectUpdateFinished, UpdateData);

            int width = SaveableObjectManager.INSTANCE.AreaWidth;
            int Height = SaveableObjectManager.INSTANCE.AreaHeight;
            float range = SaveableObjectManager.INSTANCE.MaxActiveRange;

            _ctrl = new SceneObjectController(width, Height, range, RefCol);
            INSTANCE = this;
        }

        private void RegisterData()
        {
            foreach (GameObject sGo in RefCol.Objects)
            {
                if (sGo == null)
                {
                    continue;
                }
                SceneObject sObj = sGo.GetComponent<SceneObject>();
                if (sObj != null)
                {
                    SaveableObjectManager.INSTANCE.Register(sObj.SceneObjectID, sObj.gameObject);
                }
            }
        }

        public void OnDestroy()
        {
            INSTANCE = null;
            GameEventSystem.Unregister(GameEventType.SaveableObjectUpdateFinished, UpdateData);
        }

        private void UpdateData(GameEventParam obj)
        {
            if (!_ctrl.CoroutineRunning)
            {
                StartCoroutine(_ctrl.UpdateEnum(ActorManager.GetPlayerLocation()));
            }
        }

        public void Create(SceneObject so)
        {
            _createdList.Add(so.SceneObjectID, so);
        }

        public List<SceneObject> GetList(Vector2Int index)
        {
            List<SceneObject> objs = new List<SceneObject>();
            if (PosObjList.TryGetValue(index, out List<SceneObject> list))
            {
                foreach (SceneObject sceneObject in list)
                {
                    if (!_createdList.ContainsKey(sceneObject.SceneObjectID))
                    {
                        objs.Add(sceneObject);
                    }
                }
            }
            return objs;
        }

        public void SetCreated(SceneObject sObj)
        {
            _createdList.Add(sObj.SceneObjectID, sObj);
        }

        public bool IsCreated(string name)
        {
            return _createdList.ContainsKey(name);
        }
    }
}
