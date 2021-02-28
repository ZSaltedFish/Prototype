using UnityEngine;

namespace EffectBehaviour
{
    [ExecuteInEditMode]
    public class BindingPoint : MonoBehaviour
    {
        public Vector3 LocalScaleLock;

        public void Update()
        {
            if (transform.parent != null)
            {
                float x = LocalScaleLock.x / transform.parent.localScale.x;
                float y = LocalScaleLock.y / transform.parent.localScale.y;
                float z = LocalScaleLock.z / transform.parent.localScale.z;

                transform.localScale = new Vector3(x, y, z);
            }
        }
    }
}
