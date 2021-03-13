using GameToolComponents;
using UnityEngine;

namespace EffectBehaviour
{
    public class RandomScale : MonoBehaviour
    {
        public float Max, Min;

        public void Start()
        {
            SetScale();
        }

        public void OnEnable()
        {
            SetScale();
        }

        private int GetPos()
        {
            Vector3 pos = transform.position;
            return (int)(pos.x * 10000 + pos.z);
        }

        private void SetScale()
        {
            int p = GetPos();
            float scale = ExMath.GetRandom(p, Min, Max);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
