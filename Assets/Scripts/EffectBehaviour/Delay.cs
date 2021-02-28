using UnityEngine;
using System.Collections;

namespace EffectBehaviour
{
    public class Delay : MonoBehaviour
    {

        public float DelayTime = 1.0f;


        void OnDisable()
        {
            _takeEffect = false;
        }
        void DelayFunc()
        {

            gameObject.SetActive(true);

        }
        private bool _takeEffect = false;
        public void Update()
        {
            if (!_takeEffect)
            {
                Invoke("DelayFunc", DelayTime);
                gameObject.SetActive(false);
                _takeEffect = true;
            }
        }
    }
}
