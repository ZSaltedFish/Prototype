using UnityEngine;
using GameToolComponents;
using System.Collections.Generic;
using System;

namespace Generator
{
    public class EnvironmentObjectGenerator : MonoBehaviour
    {
        public float Width, Height, Range;
        public int Size;
        public List<ReferenceCollector> SceneObjects;

        private EnvironmentAreaController _controller;
        private Dictionary<string, GameObject> _registerList;
        public void Start()
        {
            _registerList = new Dictionary<string, GameObject>();
            _controller = new EnvironmentAreaController(Width, Height, Range)
            {
                OnNewAreaLunch = LunchArea
            };
            GameEventSystem.Register(GameEventType.TerrainUpdateFinished, OnTerrainUpdateFinished);
        }

        private void LunchArea(EnvironmentObjectArea area)
        {
            Vector3 pos = area.Index2WorldPoint();

            for (int x = 0; x < Size; ++x)
            {
                for (int y = 0; y < Size; ++y)
                {
                    float xOff = x / (Size - 1f) * Width;
                    float yOff = y / (Size - 1f) * Height;

                    Vector3 offPos = pos + new Vector3(xOff, 0, yOff);
                    if (BiomeGenerator.INSTANCE.TryGetBiomeSpecifiedLocation(offPos, out Biome biome))
                    {
                        if (BiomeGenerator.INSTANCE.TryGetHigh(offPos, out float high))
                        {
                            GameObject rock = biome.EnviromentData.RockList[0];
                            Register(rock);

                            int randomPos = (int)(offPos.x * MapGenerator.INSTANCE.TargetSize + offPos.z);
                            float value = ExMath.GetRandom(randomPos, 0, 1);
                            if (value < biome.EnviromentData.RockRarity)
                            {
                                SaveableObjectManager.INSTANCE.CreateObj(rock.name, offPos + Vector3.up * high);
                            }
                        }
                    }
                }
            }


        }

        private void Register(GameObject go)
        {
            if (!_registerList.ContainsKey(go.name))
            {
                SaveableObjectManager.INSTANCE.Register(go.name, go);
                _registerList.Add(go.name, go);
            }
        }

        private void OnTerrainUpdateFinished(GameEventParam param)
        {
            if (_controller.CoroutineRunning)
            {
                return;
            }

            StartCoroutine(_controller.UpdateEnum(ActorManager.GetPlayerLocation()));
        }
    }
}
