using EffectBehaviour;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(TerrainDataExport))]
    public class TerrainDataExportEditor : Editor
    {
        private TerrainDataExport _target;
        private Terrain _terrain;

        public void Awake()
        {
            _target = (TerrainDataExport)target;
            _terrain = _target.GetComponent<Terrain>();
        }

        public override void OnInspectorGUI()
        {
            int size = _terrain.terrainData.heightmapResolution;

            if (GUILayout.Button("导出"))
            {
                float[,] highs = _terrain.terrainData.GetHeights(0, 0, size, size);
                float max = 0;

                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        max = Mathf.Max(highs[y, x], max);
                    }
                }
                Color color = Color.gray;
                Texture2D tex2D = new Texture2D(size, size);

                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        float alpha = highs[y, x] / max;
                        color.a = alpha;
                        tex2D.SetPixel(x, y, color);
                    }
                }
                tex2D.Apply();
                byte[] bytes = tex2D.EncodeToPNG();

                using (FileStream file = new FileStream("L:\\Tex2D.png", FileMode.Create))
                {
                    file.Write(bytes, 0, bytes.Length);
                }
                DestroyImmediate(tex2D);
            }

            if (GUILayout.Button("Reset"))
            {
                float[,] highs = new float[size, size];
                for (int x = 0; x < size; ++x)
                {
                    for (int y = 0; y < size; ++y)
                    {
                        highs[y, x] = 0;
                    }
                }

                _terrain.terrainData.SetHeights(0, 0, highs);
            }

            if (GUILayout.Button("Detail"))
            {
                int dsize = _terrain.terrainData.detailResolution;
                int[,] sizes = _terrain.terrainData.GetDetailLayer(0, 0, dsize, dsize, 1);
                Debug.Log(sizes.Length);
            }
        }
        Texture2D ToTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }

        public void OnDestroy()
        {
            _target = null;
        }
    }
}
