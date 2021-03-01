using System.Collections.Generic;
using UnityEngine;

namespace Generator
{
    public class GameStart : MonoBehaviour
    {
        public List<GameObject> InitList = new List<GameObject>();

        private List<GameObject> _cloneInitList = new List<GameObject>();
        public void Start()
        {
            for (int i = 0; i < InitList.Count; ++i)
            {
                GameObject go = InitList[i];
                _cloneInitList.Add(Instantiate(go));
            }
        }

        public void OnDestroy()
        {
            _cloneInitList.Clear();
        }

        [GameEvent(GameEventType.GameEventInitFinished)]
        public static void GameEventDebug(GameEventParam p)
        {
            Debug.Log("游戏事件系统加载完毕");
        }
    }
}
