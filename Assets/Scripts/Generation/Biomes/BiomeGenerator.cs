using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Generator
{
    public class BiomeGenerator : MonoBehaviour
    {
        public int UpdateCoroutineCount = 50000;
        public int Seed = 10000;
        public static BiomeGenerator INSTANCE { get; private set; }

        public List<Biome> Biomes = new List<Biome>();
        public int TerrainWidth, TerrainHeight;
        public GameObject DefaultTerrain;
        public TerrainData DefaultTerrainData;
        public float MaxRange = 5000;
        public float UpdateRange = 3500;
        //private MapManager _map;
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
            ExMath.SetRandomSeed(Seed);
        }

        public void Start()
        {
            Debug.Log($"{MaxRange}, {UpdateRange}");
            _terrainGenerator = new TerrainGenerator(DefaultTerrainData, DefaultTerrain,
                TerrainWidth, TerrainHeight, MaxRange, UpdateRange);
            UpdateCoroutineCount = int.MaxValue;
            StartCoroutine(_terrainGenerator.UpdateEnum(ActorManager.GetPlayerLocation()));
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

        public void Update()
        {
            if (!_terrainGenerator.CoroutineRunning)
            {
                UpdateCoroutineCount = 20000;
                StartCoroutine(_terrainGenerator.UpdateEnum(ActorManager.GetPlayerLocation()));
            }
        }

        public bool TryGetBiomeSpecifiedLocation(Vector3 pos, out Biome biome)
        {
            return _terrainGenerator.TryGetBiomeSpecifiedLocation(pos, out biome);
        }

        public bool TryGetHigh(Vector3 pos, out float high)
        {
            return _terrainGenerator.TryGetHigh(pos, out high);
        }

        public bool TryGetDot(Vector3 pos, out float dot)
        {
            return _terrainGenerator.TryGetDotSpecifiedLocation(pos, out dot);
        }
    }
}
