using System.Collections.Generic;
using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class PointMarker : MonoBehaviour, IWorldDraggable, IParamChangeListener, IPathInWorld
    {
        IParamHasXZ paramXZ;
        IParamHasY paramY;
        IParamHasHeading paramHeading;
        IParamHasOrientation paramOrientation;

        GameObject dragArrows;
        GameObject arrowYup, arrowYdown;
        Transform headingCone;
        Transform orientationModel;

        LineRenderer lineToShadow;
        Transform shadowMarker;

        void Awake()
        {
            dragArrows = transform.Find("DragArrows").gameObject;
            dragArrows.SetActive(false);
            var das = dragArrows.GetComponent<DragArrows>();
            arrowYup = das.PY.gameObject;
            arrowYdown = das.NY.gameObject;
            arrowYup.SetActive(false);
            arrowYdown.SetActive(false);

            headingCone = transform.Find("Heading");
            headingCone.gameObject.SetActive(false);

            orientationModel = transform.Find("OrientationModel");
            orientationModel.gameObject.SetActive(false);

            shadowMarker = transform.Find("ShadowOnWater");
            shadowMarker.gameObject.SetActive(false);
            lineToShadow = shadowMarker.GetComponent<LineRenderer>();
            lineToShadow.enabled = false;
        }

        public void OnWorldDrag(Vector3 deltaPos)
        {
            transform.position += deltaPos;
        }

        public void OnWorldDragEnd(DragConstraint dragConstraint)
        {
            if(dragConstraint == DragConstraint.XZ ||
               dragConstraint == DragConstraint.X  || 
               dragConstraint == DragConstraint.Z)
            {
                paramXZ?.SetXZ(transform.position.x, transform.position.z);
            }
            else if(dragConstraint == DragConstraint.Y)
            {
                paramY?.SetY(transform.position.y);
            }
        }

        public void SetXZParam(IParamHasXZ param)
        {
            if(param == null) return;
            paramXZ = param;
            var (x, z) = param.GetXZ();
            transform.position = new Vector3(x, transform.position.y, z);
            dragArrows.SetActive(true);
        }

        public void SetYParam(IParamHasY param)
        {
            if(param == null) return;
            paramY = param;
            arrowYup.SetActive(true);
            arrowYdown.SetActive(true);
            var y = param.GetY();
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            if(Mathf.Abs(y) > 1)
            {
                shadowMarker.gameObject.SetActive(true);
                lineToShadow.enabled = true;
                shadowMarker.position = new Vector3(transform.position.x, 0, transform.position.z);
                lineToShadow.startWidth = 0.1f;
                lineToShadow.endWidth = 0.1f;
                lineToShadow.startColor = Color.yellow;
                lineToShadow.endColor = Color.yellow;
                lineToShadow.positionCount = 2;
                lineToShadow.SetPosition(0, transform.position);
                lineToShadow.SetPosition(1, shadowMarker.position);
            }
            else
            {
                shadowMarker.gameObject.SetActive(false);
                lineToShadow.enabled = false;
            }
        }

        public void SetHeadingParam(IParamHasHeading param)
        {
            if(param == null) return;
            paramHeading = param;
            headingCone.localEulerAngles = new Vector3(0, param.GetHeading(), 0);
            headingCone.gameObject.SetActive(paramXZ != null);
        }

        public void SetOrientationParam(IParamHasOrientation param)
        {
            if(param == null) return;
            paramOrientation = param;
            var o = param.GetOrientation();
            var q = new Quaternion(o.x, o.y, o.z, o.w);
            orientationModel.localRotation = q;
            orientationModel.gameObject.SetActive(paramXZ != null);
        }

        public void OnParamChanged()
        {
            SetYParam(paramY);
            SetXZParam(paramXZ);
            SetHeadingParam(paramHeading);
            SetOrientationParam(paramOrientation);
        }

        public List<Vector3> GetWorldPath()
        {
            var l = new List<Vector3>();
            if(paramXZ != null) l.Add(transform.position);
            return l;
        }
    }
}