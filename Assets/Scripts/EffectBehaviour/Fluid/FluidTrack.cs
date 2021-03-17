using System.Collections.Generic;
using UnityEngine;

namespace EffectBehaviour
{
    public class FluidTrack : MonoBehaviour
    {
        public float Length = 0.5f;
        public float MaxHigh;
        public Vector2Int RaycastSize => _size + Vector2Int.one;

        private Vector2 _minPoint;
        private Vector2Int _size;

        public void Start()
        {
        }

        public void OnDestroy()
        {
        }

        private bool TryCastTerrain(Vector3 v, out RaycastHit hit)
        {
            const float UP_VALUE = 100;
            Ray ray = new Ray(v + Vector3.up * UP_VALUE, Vector3.down);
            return Physics.Raycast(ray, out hit, MaxHigh + UP_VALUE, 1 << 8);
        }

        public void Alignment()
        {
            float x = Length * (int)(transform.position.x / Length);
            float y = transform.position.y;
            float z = Length * (int)(transform.position.z / Length);

            transform.position = new Vector3(x, y, z);
            Vector3 localScale = transform.localScale;
            float xDelta = ((int)localScale.x) * 0.5f;
            float zDelta = ((int)localScale.y) * 0.5f;

            _minPoint = new Vector2(x - xDelta, z - zDelta);

            _size = new Vector2Int((int)(localScale.x * 2), (int)localScale.y * 2);
        }

        public float[,] RaycastTerrain()
        {
            float[,] values = new float[_size.x + 1, _size.y + 1];
            for (int x = 0; x <= _size.x; ++x)
            {
                for (int y = 0; y <= _size.y; ++y)
                {
                    float xOff = _minPoint.x + x * Length;
                    float yOff = transform.position.y;
                    float zOff = _minPoint.y + y * Length;

                    if (TryCastTerrain(new Vector3(xOff, yOff, zOff), out RaycastHit hit))
                    {
                        values[x, y] = yOff - hit.point.y;
                    }
                    else
                    {
                        values[x, y] = MaxHigh + 1;
                    }
                }
            }
            return values;
        }
    }
}
