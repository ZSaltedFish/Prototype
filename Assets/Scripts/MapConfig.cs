using UnityEngine;

namespace Generator
{
    public class MapConfig : MonoBehaviour
    {
        private static MapConfig _INSTANCE;
        public float MapSizeIns = 1000 * 1000;
        public int SeedIns;

        public static float MapSize => _INSTANCE.MapSizeIns;
        public static int Seed => _INSTANCE.SeedIns;
        public void Awake()
        {
            _INSTANCE = this;
        }

        public void OnDestroy()
        {
            _INSTANCE = null;
        }
    }
}
