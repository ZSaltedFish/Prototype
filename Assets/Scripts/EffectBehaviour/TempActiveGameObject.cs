using System.Collections;
using UnityEngine;

namespace EffectBehaviour
{
    public class TempActiveGameObject : MonoBehaviour
    {
        public GameObject[] SrcGameObject = new GameObject[0];
        public float DelayTime = 5;
        public bool Switch = true;

        public void Start()
        {
            StartCoroutine(RunDelay());
        }

        private IEnumerator RunDelay()
        {
            yield return new WaitForSeconds(DelayTime);
            SetObject();
        }

        public void SetObject()
        {
            foreach (var item in SrcGameObject)
            {
                item.SetActive(Switch);
            }
        }
    }
}
