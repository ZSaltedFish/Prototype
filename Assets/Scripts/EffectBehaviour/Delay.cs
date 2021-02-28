using System.Collections;
using UnityEngine;

namespace EffectBehaviour
{
    public class Delay : MonoBehaviour
    {
        public float DelayTime = 1f;

        public ParticleSystem SrcParticleSystem;

        public void Start()
        {
            if (SrcParticleSystem != null)
            {
                var emission = SrcParticleSystem.emission;
                emission.enabled = false;
            }
            StartCoroutine(DelayFunc());
        }

        private IEnumerator DelayFunc()
        {
            yield return new WaitForSeconds(DelayTime);
            if (SrcParticleSystem != null)
            {
                var emission = SrcParticleSystem.emission;
                emission.enabled = true;
            }
        }
    }
}
