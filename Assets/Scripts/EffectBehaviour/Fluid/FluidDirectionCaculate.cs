using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EffectBehaviour
{
    public class FluidDirectionCaculate
    {
        public static Vector2Int[] DIRE_VALUES = {Vector2Int.up, Vector2Int.one, Vector2Int.right, new Vector2Int(1, -1),
            Vector2Int.down, -Vector2Int.one, Vector2Int.left, new Vector2Int(-1, 1)};
        private enum PointVistedType
        {
            UnVist = 0,
            Visting,
            Visted
        }

        private enum PointType
        {
            Water = 0,
            Fall,
            Edge,
            Land
        }
        private Vector2Int _size;
        private PointVistedType[,] _vistedTypes;
        private PointType[,] _pointType;
        private Vector3[,] _directions;
        private float[,] _stresses;
        private float[,] _mask;
        private float _maxHigh;

        public Vector3[,] Direction => _directions;

        public FluidDirectionCaculate(float[,] map, Vector2Int size, float maxHigh)
        {
            _size = size;
            _mask = map;
            _maxHigh = maxHigh;
        }

        public void Init()
        {
            _stresses = new float[_size.x, _size.y];
            _pointType = new PointType[_size.x, _size.y];
            for (int x = 0; x < _size.x; ++x)
            {
                for (int y = 0; y < _size.y; ++y)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    float maskValue = _mask[x, y];
                    if (maskValue > _maxHigh)
                    {
                        _stresses[x, y] = -1;
                        _pointType[x, y] = PointType.Fall;
                    }
                    else if (maskValue > 0)
                    {
                        _stresses[x, y] = 0;
                        _pointType[x, y] = PointType.Water;
                    }
                    else if (IsValueEdge(pos))
                    {
                        _stresses[x, y] = -0.2f;
                        _pointType[x, y] = PointType.Edge;
                    }
                    else
                    {
                        _stresses[x, y] = 1;
                        _pointType[x, y] = PointType.Land;
                    }
                }
            }
        }

        public void Caculate()
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            _vistedTypes = new PointVistedType[_size.x, _size.y];
            _directions = new Vector3[_size.x, _size.y];

            for (int x = 0; x < _size.x; ++x)
            {
                for (int y = 0; y < _size.y; ++y)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (_pointType[x, y] == PointType.Fall)
                    {
                        queue.Enqueue(pos);
                        _vistedTypes[x, y] = PointVistedType.Visted;
                    }
                }
            }

            while (queue.Count > 0)
            {
                Queue<Vector2Int> vistingQueue = new Queue<Vector2Int>();
                List<Vector2Int> writeList = new List<Vector2Int>();
                List<float> writeDatas = new List<float>();
                List<Vector3> writeDires = new List<Vector3>();
                while (queue.Count > 0)
                {
                    Vector2Int pos = queue.Dequeue();
                    List<Vector2Int> surrond = GetSurroundPoint(pos);

                    if (_pointType[pos.x, pos.y] == PointType.Water)
                    {
                        Cacluate(pos, surrond, out float stress, out Vector3 dire);
                        writeList.Add(pos);
                        writeDatas.Add(stress);
                        writeDires.Add(dire);
                    }
                    for (int i = 0; i < surrond.Count; ++i)
                    {
                        Vector2Int sPos = surrond[i];

                        if (_pointType[sPos.x, sPos.y] == PointType.Water)
                        {
                            if (_vistedTypes[sPos.x, sPos.y] == PointVistedType.UnVist)
                            {
                                _vistedTypes[sPos.x, sPos.y] = PointVistedType.Visting;
                                vistingQueue.Enqueue(sPos);
                            }
                        }
                    }
                }

                for (int i = 0; i < writeList.Count; ++i)
                {
                    Vector2Int writePos = writeList[i];
                    _stresses[writePos.x, writePos.y] = writeDatas[i];
                    _directions[writePos.x, writePos.y] = writeDires[i];
                    _vistedTypes[writePos.x, writePos.y] = PointVistedType.Visted;
                }

                while (vistingQueue.Count > 0)
                {
                    queue.Enqueue(vistingQueue.Dequeue());
                }
            }
        }

        private void Cacluate(Vector2Int pos, List<Vector2Int> surrond, out float stress, out Vector3 dire)
        {
            float minStress = float.MaxValue;
            Vector2 v2Dire = Vector2.zero;
            for (int i = 0; i < surrond.Count; ++i)
            {
                Vector2Int sPos = surrond[i];
                float sStress = _stresses[sPos.x, sPos.y];
                Vector2 vDire = ((Vector2)(pos - sPos)).normalized;
                v2Dire += vDire * sStress;
                minStress = Mathf.Min(minStress, sStress);
            }

            stress = minStress * 0.9f;
            dire = new Vector3(v2Dire.x, 0, v2Dire.y);
        }

        private List<Vector2Int> GetSurroundPoint(Vector2Int pos)
        {
            List<Vector2Int> list = new List<Vector2Int>();
            for (int i = 0; i < DIRE_VALUES.Length; ++i)
            {
                Vector2Int newPos = pos + DIRE_VALUES[i];
                if (0 <= newPos.x && newPos.x < _size.x && 0 <= newPos.y && newPos.y < _size.y)
                {
                    if (_pointType[newPos.x, newPos.y] != PointType.Land)
                    {
                        list.Add(newPos);
                    }
                }
            }
            return list;
        }

        private bool IsValueEdge(Vector2Int pos)
        {
            foreach (Vector2Int dire in DIRE_VALUES)
            {
                Vector2Int np = dire + pos;
                if (0 <= np.x && np.x < _size.x && 0 <= np.y && np.y < _size.y)
                {
                    float value = _mask[np.x, np.y];
                    if (0 < value && value < _maxHigh)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
