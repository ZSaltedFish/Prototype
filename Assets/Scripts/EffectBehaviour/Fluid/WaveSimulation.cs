using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace EffectBehaviour
{
    public class WaveSimulation : MonoBehaviour
    {
        public float PerlinScale = 0.001f;
        public float HeightScale = 0.01f;
        public MeshFilter @MeshFilter;

        private Mesh _cloneMesh;
        private DateTime _startTime;
        private Vector3[] _tempVertices;

        public void Start()
        {
            _startTime = DateTime.Now;
            _cloneMesh = MeshFilter.mesh;
            MeshFilter.sharedMesh = _cloneMesh;
            _tempVertices = new Vector3[_cloneMesh.vertexCount];
        }

        public void Update()
        {
            TimeSpan time = DateTime.Now - _startTime;
            for (int i = 0; i < _cloneMesh.vertexCount; ++i)
            {
                Vector3 p = _cloneMesh.vertices[i];
                int dTime = time.Milliseconds;
                if (dTime > 500)
                {
                    dTime = 1000 - dTime;
                }
                float perlinValue = Mathf.PerlinNoise(
                    p.x + transform.position.x + dTime * PerlinScale,
                    p.z + transform.position.z + dTime * PerlinScale);
                float lerp = Mathf.Lerp(-HeightScale, HeightScale, perlinValue);
                p.y = lerp;
                _tempVertices[i] = p;
            }

            _cloneMesh.SetVertices(_tempVertices, 0, _tempVertices.Length, MeshUpdateFlags.Default);
        }

        public void OnDestroy()
        {
            if (_cloneMesh != null)
            {
                Destroy(_cloneMesh);
            }
        }
    }
}
