using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public class BiomeGenerator : MonoBehaviour
    {
        public int Seed = 10000;
        public static BiomeGenerator INSTANCE { get; private set; }

        public List<Biome> Biomes = new List<Biome>();
        public int TerrainWidth, TerrainHeight;
        public GameObject DefaultTerrain;
        public TerrainData DefaultTerrainData;
        public GameObject CenterUnit;
        private TerrainGenerator _terrainGenerator;
        public void Awake()
        {
            if (INSTANCE != null)
            {
                throw new InvalidOperationException("This component can only be added at once.");
            }
            INSTANCE = this;
            InitBiomes();
            Random.InitState(Seed);
        }

        public void Start()
        {
            _terrainGenerator = new TerrainGenerator(DefaultTerrainData, DefaultTerrain,
                TerrainWidth, TerrainHeight, Biomes);
            StartCoroutine(_terrainGenerator.UpdateEnum(CenterUnit.transform.position));

            TreeGenerator.INSTANCE.Run();
        }

        private void InitBiomes()
        {
            for (int i = 0; i < Biomes.Count; ++i)
            {
                Biomes[i].BiomeIndex = i;
            }
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
                StartCoroutine(_terrainGenerator.UpdateEnum(CenterUnit.transform.position));
            }
            else
            {
                _curTimeDelta += Time.deltaTime;
            }
        }

        public Biome GetBiomeSpecifiedLocation(Vector3 pos)
        {
            return _terrainGenerator.GetBiomeSpecifiedLocation(pos);
        }
    }
}
