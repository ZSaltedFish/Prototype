using GameToolComponents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public class SceneObjectArea : DynamicArea
    {
        private int _iEnum = 0;
        private const int _MAX_IEUM = 5;

        public List<SceneObject> ObjectList;
        public SceneObjectArea(float width, float height, Vector2Int index) : base(width, height, index)
        {
        }

        public override void OnAreaLoad()
        {
        }

        public override void OnAreaRelease()
        {
        }

        public override void OnAreaUnload()
        {
        }

        public override IEnumerator OnAreaLoadEnum()
        {
            foreach (SceneObject sceneObject in ObjectList)
            {
                if (!SceneObjectManager.INSTANCE.IsCreated(sceneObject.SceneObjectID))
                {
                    SaveableObjectManager.INSTANCE.CreateObj(sceneObject.SceneObjectID, sceneObject.InitPosition);
                    if (IsEnumerator())
                    {
                        yield return null;
                    }
                }
            }
        }

        private bool IsEnumerator()
        {
            if (_iEnum >= _MAX_IEUM)
            {
                _iEnum = 0;
                return true;
            }
            ++_iEnum;
            return false;
        }
    }
}
