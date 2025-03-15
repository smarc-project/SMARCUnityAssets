using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public interface IWorldDraggable
    {
        public void OnWorldDrag(Vector3 newPos);
        public void OnWorldDragEnd();

    }
}