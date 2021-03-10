using GameToolComponents;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public class TreeGenerator : MonoBehaviour
    {
        public float UpdateTime = 5f;
        public GameObject CenterObj;
        public static TreeGenerator INSTANCE { get; private set; }
        public float Width, Height, Range;
        public int DetialSize;
        private Dictionary<string, ObjectPoolPattern<GameObject>> _objTreePools;
        private Dictionary<int, GameObject> _activityTrees = new Dictionary<int, GameObject>();
        private TreeAreaManager _manager;

        public void Awake()
        {
            if (INSTANCE != null)
            {
                throw new InvalidOperationException("This component can only be added at once.");
            }
            INSTANCE = this;
            _objTreePools = new Dictionary<string, ObjectPoolPattern<GameObject>>();
        }

        public void Start()
        {
            _manager = new TreeAreaManager(Width, Height, Range)
            {
                ObjectPoolMode = true,
                Size = DetialSize
            };
            GameEventSystem.Register(GameEventType.TerrainUpdateFinished, OnTerrainFishished);
        }

        public void OnDestroy()
        {
            GameEventSystem.Unregister(GameEventType.TerrainUpdateFinished, OnTerrainFishished);
        }

        private void OnTerrainFishished(GameEventParam obj)
        {
            StartCoroutine(_manager.UpdateEnum(obj.Get<Vector3>(GameEventFlag.Terrain_update_location)));
        }

        public void Generate(Vector3 worldPoint, System.Random rad)
        {
            if (!BiomeGenerator.INSTANCE.TryGetBiomeSpecifiedLocation(worldPoint, out Biome curBiome))
            {
                return;
            }
            if (BiomeGenerator.INSTANCE.TryGetHigh(worldPoint, out float high))
            {
                worldPoint.y = high;
            }

            if (curBiome == null || curBiome.Trees.Length == 0 || IsBlocked(worldPoint))
            {
                return;
            }

            float swapRate = TreeRate(worldPoint);
            float thres = swapRate * curBiome.TreeThreshold;
            float randomValue = rad.Next(0, 1000) / 1000f;
            if (thres < randomValue)
            {
                return;
            }

            GameObject[] trees = curBiome.Trees;
            int index = rad.Next(0, trees.Length);
            GameObject tree = trees[index];
            GameObject treeIndstance = GetTree(tree);
            treeIndstance.transform.position = worldPoint;
            float scale = rad.Next(500, 5000) / 1000f;
            treeIndstance.transform.localScale = new Vector3(scale, scale, scale);
            treeIndstance.transform.SetParent(transform);
            _activityTrees.Add(treeIndstance.GetInstanceID(), treeIndstance);
        }

        public void RecycleTree(Vector2Int index)
        {
            float minWidth = Width * index.x;
            float maxWidth = Width + minWidth;
            float minHeight = Height * index.y;
            float maxHeight = Height + minHeight;

            List<int> ids = new List<int>(_activityTrees.Keys);
            for (int i = 0; i < ids.Count; ++i)
            {
                int id = ids[i];
                GameObject activityTree = _activityTrees[id];
                Vector3 v = activityTree.transform.position;
                if (maxWidth < v.x || v.x < minWidth || maxHeight < v.z || v.z < minHeight)
                {
                    continue;
                }
                else
                {
                    PushTree(_activityTrees[id]);
                    _activityTrees.Remove(id);
                }
            }
        }

        private bool IsBlocked(Vector2 wp)
        {
            return false;
        }

        private float TreeRate(Vector2 worldPoint)
        {
            Vector2 pos = worldPoint / Width;
            return Mathf.PerlinNoise(pos.x, pos.y);
        }

        private GameObject GetTree(GameObject go)
        {
            string name = go.name;
            if (!_objTreePools.TryGetValue(name, out ObjectPoolPattern<GameObject> pool))
            {
                pool = new ObjectPoolPattern<GameObject>(() =>
                {
                    GameObject clone = Instantiate(go);
                    clone.name = CloneName(go, clone.GetInstanceID());
                    return clone;
                })
                {
                    OnObjectGottenFromPool = OnTreeGet,
                    OnObjectPushinPool = OnTreePush
                };
                _objTreePools.Add(name, pool);
            }

            return pool.Get();
        }

        private void PushTree(GameObject go)
        {
            string srcName = go.name.Split('~')[0];
            _objTreePools[srcName].Push(go);
        }

        private void OnTreePush(GameObject go)
        {
            go.SetActive(false);
        }

        private void OnTreeGet(GameObject go)
        {
            go.SetActive(true);
        }

        private string CloneName(GameObject go, int id)
        {
            return $"{go.name}~{id}";
        }
    }
}
