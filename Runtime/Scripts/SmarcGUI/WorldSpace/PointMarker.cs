using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class PointMarker : MonoBehaviour, IWorldDraggable
    {
        IParamHasXZ paramXZ;
        IParamHasY paramY;

        GameObject dragArrows;
        GameObject arrowYup, arrowYdown;

        void Awake()
        {
            dragArrows = transform.Find("DragArrows").gameObject;
            dragArrows.SetActive(false);
            var das = dragArrows.GetComponent<DragArrows>();
            arrowYup = das.PY.gameObject;
            arrowYdown = das.NY.gameObject;
            arrowYup.SetActive(false);
            arrowYdown.SetActive(false);
        }

        public void OnWorldDrag(Vector3 deltaPos)
        {
            transform.position += deltaPos;
        }

        public void OnWorldDragEnd()
        {
            paramXZ?.SetXZ(transform.position.x, transform.position.z);
            paramY?.SetY(transform.position.y);
        }

        public void SetXZParam(IParamHasXZ param)
        {
            paramXZ = param;
            var (x, z) = param.GetXZ();
            var y = 0f;
            if(paramY != null) y = paramY.GetY();
            transform.position = new Vector3(x, y, z);
            dragArrows.SetActive(true);
        }

        public void SetYParam(IParamHasY param)
        {
            paramY = param;
            arrowYup.SetActive(true);
            arrowYdown.SetActive(true);
            if(paramXZ != null)
            {
                var (x, z) = paramXZ.GetXZ();
                transform.position = new Vector3(x, param.GetY(), z);
            }
        }

    }
}