using Entitys;
using UnityEngine;

namespace Generator
{
    public class SceneObject : SaveableObject
    {
        public string SceneObjectID;
        public Vector3 InitPosition;

        public void Start()
        {
            SceneObjectManager.INSTANCE.SetCreated(this);
        }
    }
}
