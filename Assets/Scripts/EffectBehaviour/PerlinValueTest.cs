using GameToolComponents;
using UnityEngine;
using UnityEngine.UI;

namespace EffectBehaviour
{
    public class PerlinValueTest : MonoBehaviour
    {
        public RawImage SrcImage;

        private Texture2D _tex;
        public void Start()
        {
            Vector2 size = SrcImage.rectTransform.sizeDelta;
            ExMath.SetRandomSeed(1000);
            Texture2D tex = new Texture2D((int)size.x, (int)size.y);
            _tex = tex;

            for (int x = 0; x < size.x; ++x)
            {
                float xF = x / size.x * 5;
                float lerp = (ExMath.PerlinNoise1D(xF) + 1) / 2f;
                int pos = (int)Mathf.Lerp(0, size.y, lerp);

                for (int y = 0; y < size.y; ++y)
                {
                    Color color = pos < y ? Color.white : Color.red;
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            SrcImage.texture = _tex;
        }

        public void OnDestroy()
        {
            SrcImage.texture = null;
            if (_tex != null)
            {
                Destroy(_tex);
            }
        }
    }
}