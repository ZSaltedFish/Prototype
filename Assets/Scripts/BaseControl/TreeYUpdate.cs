using UnityEngine;

namespace Control
{
    public class TreeYUpdate : MonoBehaviour
    {
        public float UpdateTime = 5;
        private float _deltaTime = 0;

        public void Update()
        {
            if (_deltaTime > UpdateTime)
            {
                _deltaTime = 0;
                CastUpdate();
            }
            else
            {
                _deltaTime += Time.deltaTime;
            }
        }

        public void CastUpdate()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 1000, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                transform.position = hit.point + Vector3.up * 0.5f;
            }
        }
    }
}
