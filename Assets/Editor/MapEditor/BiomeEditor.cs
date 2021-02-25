using EditorHelper;
using Generator;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    [CustomEditor(typeof(Biome))]
    public class BiomeEditor : Editor
    {
        private Biome _target;
        public void Awake()
        {
            _target = target as Biome;
        }
        public override void OnInspectorGUI()
        {
            //_target.BiomeIndex = EditorDataFields.EditorDataField("生物群落索引", _target.BiomeIndex);
            _target.BiomeName = EditorDataFields.EditorDataField("群落名字", _target.BiomeName);
            _target.TerrainTextureLayerIndex = EditorDataFields.EditorDataField("基础贴图索引", _target.TerrainTextureLayerIndex);
            //_target.BaseHigh = EditorGUILayout.Slider("基础高度", _target.BaseHigh, 0, 1);
            EditorGUILayout.MinMaxSlider("高度范围(0-1)", ref _target.MinHigh, ref _target.MaxHigh, 0f, 1f);

            _target.Complexity = EditorGUILayout.Slider("地形复杂度", _target.Complexity, 0.1f, Biome.BIOME_MAX_RANGE_TIMES);
            _target.Rarity = EditorGUILayout.Slider("地形遮罩范围", _target.Rarity, 0f, 1f);
            _target.Range = EditorGUILayout.Slider("地形碎裂度（越小代表越地形范围越连贯）", _target.Range, 0.01f, 5f);
            _target.SubList = EditorDataFields.EditorArrayField("子群落", _target.SubList);

            if (GUILayout.Button("打开测试界面"))
            {
                BiomeTestWindow window = EditorWindow.GetWindow<BiomeTestWindow>();
                window.Biome = _target;
                window.Show();
            }

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
