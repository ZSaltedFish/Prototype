using EditorHelper;
using Generator;
using Localization;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(Biome))]
    public class BiomeEditor : Editor
    {
        private Biome _target;
        private string _tempName = LocalizationString.LOCAL_NULL_DATA;
        public void Awake()
        {
            _target = target as Biome;
            _tempName = LocalizationString.LOCAL_NULL_DATA;
        }
        public override void OnInspectorGUI()
        {
            _target.BiomeName = _target.BiomeName.Edit("生物群落名字", ref _tempName);
            _target.TerrainTextureLayerIndex = EditorDataFields.EditorDataField("基础贴图索引", _target.TerrainTextureLayerIndex);
            _target.SrcTexture = EditorDataFields.EditorDataField("基础图索引", _target.SrcTexture);
            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                using (new EditorHorizontalLayout(EditorStyles.helpBox))
                {
                    _target.MinHigh = EditorGUILayout.FloatField("最小高度", _target.MinHigh);
                    _target.MaxHigh = EditorGUILayout.FloatField("最大高度", _target.MaxHigh);
                }
                EditorGUILayout.MinMaxSlider("高度范围(0-1)", ref _target.MinHigh, ref _target.MaxHigh, 0f, 1f);
            }

            using (new EditorVerticalLayout(EditorStyles.helpBox))
            {
                _target.Complexity = EditorGUILayout.Slider("地形复杂度", _target.Complexity, 0.1f, Biome.BIOME_MAX_RANGE_TIMES);
                _target.TerrainCurve = EditorGUILayout.CurveField("地形放大曲线", _target.TerrainCurve);
            }

            _target.EnviromentData = EditorDataFields.EditorDataField("环境对象", _target.EnviromentData);

            //if (GUILayout.Button("打开测试界面"))
            //{
            //    BiomeTestWindow window = EditorWindow.GetWindow<BiomeTestWindow>();
            //    window.Biome = _target;
            //    window.Show();
            //}

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_target);
            }
        }
    }

    public class BiomeTestWindow : EditorWindow
    {
        public Biome Biome;
        public Terrain Terrain;
        public void OnGUI()
        {
            Biome = EditorDataFields.EditorDataField("地形文件", Biome);
            Terrain = EditorDataFields.EditorDataField("Terrain引用", Terrain);

            if (Biome != null && Terrain != null && GUILayout.Button("测试"))
            {
                int size = Terrain.terrainData.heightmapResolution;
                float[,] heightMap = MapHelper.GetPerlinNoiseMap(size, size, Random.Range(1000, 10000), Random.Range(1000, 1000), Biome.Complexity, Biome.MinHigh, Biome.MaxHigh);
                Terrain.terrainData.SetHeights(0, 0, heightMap);
            }
        }
    }
}
