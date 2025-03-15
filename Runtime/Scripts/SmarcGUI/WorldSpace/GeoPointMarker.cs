using GeoRef;
using SmarcGUI.MissionPlanning.Params;
using UnityEngine;

namespace SmarcGUI.WorldSpace
{

    public class GeoPointMarker : MonoBehaviour, IWorldDraggable
    {
        GeoPointParamGUI gppgui;

        public GameObject draggingObject;
        GlobalReferencePoint globalReferencePoint;
        public LineRenderer Circle, SurfacePointer, CrossOne, CrossTwo;
        public float circleRadius = 1;
        public int numPtsOnCircle = 50;
        public float lineThickness = 0.1f;
        public Material lineMaterial;
        public Color SurfaceColor = Color.white;
        public Color UnderwaterColor = new(1f,0.4f,0f); //orange
        public Color InAirColor = Color.cyan;


        Vector3[] circlePoints, pointerPoints, crossOnePoints, crossTwoPoints;


        GUIState guiState;

        void Awake()
        {
            globalReferencePoint = FindFirstObjectByType<GlobalReferencePoint>();
            if(globalReferencePoint == null)
            {
                Debug.LogError("GlobalReferencePoint not found, GeoPointMarker will not work, destroying it");
                Destroy(gameObject);
                return;
            }
            guiState = FindFirstObjectByType<GUIState>();
            circlePoints = new Vector3[numPtsOnCircle];
            pointerPoints = new Vector3[2];
            crossOnePoints = new Vector3[2];
            crossTwoPoints = new Vector3[2];
        }

        public void SetGeoPointParamGUI(GeoPointParamGUI gppgui)
        {
            this.gppgui = gppgui;
            if(gppgui.altitude == 0 && gppgui.latitude == 0 && gppgui.longitude == 0)
            {
                gameObject.SetActive(false);
                guiState.Log($"GeoPoint {gppgui.name} is not set, hiding the marker");
                return;
            }

            CreateLines();
            OnGUILatLonChanged();
            OnGUIAltChanged();
            UpdateLines();
        }

        void SetLRColor(LineRenderer lr)
        {
            if(transform.position.y < 0) lr.startColor = lr.endColor = UnderwaterColor;
            else if(transform.position.y > 0) lr.startColor = lr.endColor = InAirColor;
            else lr.startColor = lr.endColor = SurfaceColor;
        }

        void SetCircleSizes(LineRenderer lr)
        {
            lr.startWidth = lineThickness;
            lr.endWidth = lineThickness;
            lr.material = lineMaterial;
        }

        void SetLineSizes(LineRenderer lr)
        {
            lr.startWidth = lineThickness/3;
            lr.endWidth = lineThickness/3;
            lr.material = lineMaterial;
        }

        void CreateLines()
        {
            if(circlePoints == null) return;
            if(pointerPoints == null) return;
            
            for(int i=0; i<numPtsOnCircle; i++)
            {
                float rad = i * 2*Mathf.PI / (numPtsOnCircle-1);
                var x = circleRadius * Mathf.Cos(rad);
                var z = circleRadius * Mathf.Sin(rad);
                circlePoints[i] = new Vector3(x, 0, z);
            }
            Circle.positionCount = circlePoints.Length;
            Circle.SetPositions(circlePoints);
            SetLRColor(Circle);
            SetCircleSizes(Circle);

            pointerPoints[0] = new Vector3(0,0,0);
            pointerPoints[1] = new Vector3(0,0,0);
            SurfacePointer.positionCount = pointerPoints.Length;
            SurfacePointer.SetPositions(pointerPoints);
            SetLRColor(SurfacePointer);
            SetLineSizes(SurfacePointer);

            crossOnePoints[0] = circlePoints[0];
            crossOnePoints[1] = circlePoints[numPtsOnCircle/2];
            CrossOne.positionCount = crossOnePoints.Length;
            CrossOne.SetPositions(crossOnePoints);
            SetLRColor(CrossOne);
            SetLineSizes(CrossOne);

            crossTwoPoints[0] = circlePoints[numPtsOnCircle/4];
            crossTwoPoints[1] = circlePoints[numPtsOnCircle*3/4];
            CrossTwo.positionCount = crossTwoPoints.Length;
            CrossTwo.SetPositions(crossTwoPoints);
            SetLRColor(CrossTwo);
            SetLineSizes(CrossTwo);
        }

        public void UpdateLines()
        {            
            for(int i=0; i<numPtsOnCircle; i++)
            {
                Circle.SetPosition(i, circlePoints[i] + transform.position);
            }
            SurfacePointer.SetPosition(0, new Vector3(transform.position.x, 0, transform.position.z));
            SurfacePointer.SetPosition(1, transform.position);

            CrossOne.SetPosition(0, Circle.GetPosition(0));
            CrossOne.SetPosition(1, Circle.GetPosition(numPtsOnCircle/2));

            CrossTwo.SetPosition(0, Circle.GetPosition(numPtsOnCircle/4));
            CrossTwo.SetPosition(1, Circle.GetPosition(numPtsOnCircle*3/4));

            SetLRColor(Circle);
            SetLRColor(SurfacePointer);
            SetLRColor(CrossOne);
            SetLRColor(CrossTwo);
        }

        public void OnWorldDrag(Vector3 motion)
        {
            transform.position += motion;
            UpdateLines();
        }
        

        public void OnWorldDragEnd()
        {
            var (lat, lon) = globalReferencePoint.GetLatLonFromUnityXZ(transform.position.x, transform.position.z);
            gppgui.latitude = lat;
            gppgui.longitude = lon;
            gppgui.altitude = transform.position.y;
        }

        public void OnGUILatLonChanged()
        {
            var (tx,tz) = globalReferencePoint.GetUnityXZFromLatLon(gppgui.latitude, gppgui.longitude);
            transform.position = new Vector3((float)tx, transform.position.y, (float)tz);
            UpdateLines();
        }

        public void OnGUIAltChanged()
        {
            transform.position = new Vector3(transform.position.x, gppgui.altitude, transform.position.z);
            UpdateLines();
        }

        public void ToggleDraggable(bool draggable)
        {
            draggingObject.SetActive(draggable);
        }

    }
}