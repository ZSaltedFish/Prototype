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
            StartCoroutine(_terrainGenerator.UpdateEnum(Vector3.zero));
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
                StartCoroutine(_terrainGenerator.UpdateEnum(ActorManager.GetPlayerLocation()));
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

        public bool TryGetHigh(Vector3 pos, out float high)
        {
            return _terrainGenerator.TryGetHigh(pos, out high);
        }
    }
}
