using EffectBehaviour;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(FluidTrack))]
    public class FluidTrackEditor : Editor
    {
        public static Vector2Int[] DIRE_VALUES = {Vector2Int.up, Vector2Int.one, Vector2Int.right, new Vector2Int(1, -1),
            Vector2Int.down, -Vector2Int.one, Vector2Int.left, new Vector2Int(-1, 1)};
        private FluidTrack _target;

        public void Awake()
        {
            _target = (FluidTrack)target;
        }

        public override void OnInspectorGUI()
        {
            _target.MaxHigh = EditorGUILayout.FloatField("最大检测高度", _target.MaxHigh);
            if (GUILayout.Button("测试"))
            {
                _target.Alignment();
                Track();
            }
        }

        private void Track()
        {
            Vector2Int size = _target.RaycastSize;
            Texture2D tex = new Texture2D(size.x, size.y);
            float[,] values = _target.RaycastTerrain();
            FluidDirectionCaculate fdc = new FluidDirectionCaculate(values, size, _target.MaxHigh);
            fdc.Init();
            fdc.Caculate();

            Vector3[,] normals = fdc.Direction;

            for (int x = 0; x < size.x; ++x)
            {
                for (int y = 0; y < size.y; ++y)
                {
                    Vector3 normal = normals[x, y];
                    Color color = NormalToColor(normal);

                    tex.SetPixel(x, y, color);
                }
            }

            byte[] bytes = tex.EncodeToTGA();
            using (FileStream file = new FileStream("L:\\Fluid.tga", FileMode.Create))
            {
                file.Write(bytes, 0, bytes.Length);
            }
            if (Application.isPlaying)
            {
                Destroy(tex);
            }
            else
            {
                DestroyImmediate(tex);
            }
        }

        public Color NormalToColor(Vector3 v)
        {
            Vector3 nv = (v + Vector3.one) / 2;
            return new Color(nv.x, 0, nv.z);
        }
    }
}
