using System;

namespace Generator
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GameEventAttribute : Attribute
    {
        public readonly GameEventType EventType;
        public GameEventAttribute(GameEventType type)
        {
            EventType = type;
        }
    }
}
