using GameToolComponents;
using UnityEditor;
using UnityEngine;

namespace GameEditor
{
    public class DragAreaGetObject : Editor
    {
        public static void GetDrawData(ReferenceCollector dict,
            string name = "Drag Object here from Project view to get the object")
        {
            GUIContent title = new GUIContent(name);
            var dragArea = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
            GUI.Box(dragArea, title);
            Event @event = Event.current;

            switch (@event.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    if (!dragArea.Contains(@event.mousePosition))
                    {
                        break;
                    }
                    DragAndDrop.AcceptDrag();

                    if (@event.type == EventType.DragPerform)
                    {
                        foreach (var item in DragAndDrop.objectReferences)
                        {
                            if (item != null)
                            {
                                dict.Add(item.name, item);
                            }
                        }
                    }
                    @event.Use();
                    break;
            }
        }
    }
}