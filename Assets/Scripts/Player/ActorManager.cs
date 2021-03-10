using Control;
using UnityEngine;

namespace Generator
{
    public class ActorManager : MonoBehaviour
    {
        public GameObject TempGo;
        public GameObject CameraObj;

        private GameObject _actorUnit;
        private static ActorManager _INSTANCE;
        private Vector3 _localPos = new Vector3(750, 0, 750);
        private GameObject _cameraObject;

        public void Awake()
        {
            _INSTANCE = this;
        }

        public void Start()
        {
            GameEventSystem.Register(GameEventType.TerrainUpdateFinished, InitObject);
        }

        private void InitObject(GameEventParam param)
        {
            _actorUnit = Instantiate(TempGo);
            if (BiomeGenerator.INSTANCE.TryGetHigh(_actorUnit.transform.position, out float high))
            {
                Vector3 pos = _actorUnit.transform.position;
                pos.y = high;
                _actorUnit.transform.position = pos;
            }
            _cameraObject = Instantiate(CameraObj);
            _cameraObject.GetComponent<ThirdCamera>().LockUpObject = _actorUnit;
            GameEventSystem.ThrowEvent(GameEventType.PlayerUnitCreated, new GameEventParam().Add("PlayerObject", _actorUnit));
            GameEventSystem.Unregister(GameEventType.TerrainUpdateFinished, InitObject);
        }

        public static GameObject GetPlayerUnit()
        {
            return _INSTANCE._actorUnit;
        }

        public static Vector3 GetPlayerLocation()
        {
            if (_INSTANCE._actorUnit == null)
            {
                return _INSTANCE._localPos;
            }
            return _INSTANCE._actorUnit.transform.position;
        }
    }
}