using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public class BiomeGenerator : MonoBehaviour
    {
        public List<Biome> Biomes = new List<Biome>();
        public List<Terrain> Terrains = new List<Terrain>();
        public int TerrainWidth, TerrainHeight;
        public TerrainData DefaultTerrainData;
        public const int TERRAIN_COUNT = 3;
        public GameObject CenterUnit;
        private TerrainGenerator _terrainGenerator;
        public void Start()
        {
            Random.InitState(DateTime.Now.Millisecond);
            _terrainGenerator = new TerrainGenerator(DefaultTerrainData, transform.GetChild(0).gameObject,
                TerrainWidth, TerrainHeight);

            StartCoroutine(_terrainGenerator.UpgradeTerrain(Biomes, CenterUnit.transform.position));
        }

        public void OnDestroy()
        {
            _terrainGenerator.Dispose();
        }

        public float MaxTimeDelta = 5f;
        private float _curTimeDelta = 0;

        public void Update()
        {
            if (_curTimeDelta > MaxTimeDelta)
            {
                _curTimeDelta = 0;
                StartCoroutine(_terrainGenerator.UpgradeTerrain(Biomes, CenterUnit.transform.position));
            }
            else
            {
                _curTimeDelta += Time.deltaTime;
            }
        }
    }
}
