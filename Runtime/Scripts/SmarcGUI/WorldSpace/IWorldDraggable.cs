using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public interface IWorldDraggable
    {
        public void OnWorldDrag(Vector3 deltaPos);
        public void OnWorldDragEnd(DragConstraint dragConstraint);

    }
}