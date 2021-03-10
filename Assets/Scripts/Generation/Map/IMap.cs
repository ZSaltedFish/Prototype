using UnityEngine;

namespace Generator
{
    public interface IMap
    {
        void Generate();
        RectInt GetDataRect(Vector2 pos, float width, float height);
    }
}