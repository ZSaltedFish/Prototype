using UnityEngine;

namespace EffectBehaviour
{
    public class ScaleHolder : MonoBehaviour
    {
        public void Update()
        {
            if (transform.parent != null)
            {
                float x = 1 / transform.parent.localScale.x;
                float y = 1 / transform.parent.localScale.y;
                float z = 1 / transform.parent.localScale.z;

                transform.localScale = new Vector3(x, y, z);
            }
        }
    }
}
