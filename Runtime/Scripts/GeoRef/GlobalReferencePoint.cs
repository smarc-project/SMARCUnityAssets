using CoordinateSharp;
using UnityEngine;


namespace GeoRef
{
    public enum OriginMode
    {
        LatLon,
        UTM
    }

    public class GlobalReferencePoint: MonoBehaviour
    {
        [Tooltip("Use Lat/Lon or UTM as the origin and set the other values accordingly")]
        public OriginMode originMode = OriginMode.LatLon;

        [Header("Lat/lon in decimal degrees")]
        public double lon = 17.596178; // asko bottom left defaults
        public double lat = 58.811481;

        [Header("UTM properties")]
        public string band;
        public int zone;
        public double easting;
        public double northing;

        void OnValidate()
        {
            var refpoints = FindObjectsByType<GlobalReferencePoint>(FindObjectsSortMode.None);
            if(refpoints.Length > 1)
            {
                Debug.LogWarning("Found too many GPSReferencePoints in the scene, there should only be one!");
            }

            if(originMode == OriginMode.LatLon)
            {
                var latlon = new Coordinate(lat, lon);
                easting = latlon.UTM.Easting;
                northing = latlon.UTM.Northing;
                band = latlon.UTM.LatZone;  //str
                zone = latlon.UTM.LongZone; //int
            }
            else
            {
                (lat, lon) = GetLatLonFromUTM(easting, northing);
            }
        }

        public (double, double) GetLatLonFromUTM(double easting, double northing)
        {
                var utm = new UniversalTransverseMercator(band, zone, easting, northing);
                var latlon = UniversalTransverseMercator.ConvertUTMtoLatLong(utm);
                return (latlon.Latitude.ToDouble(), latlon.Longitude.ToDouble());
        }

        public (double easting, double northing, double lat, double lon) GetUTMLatLonOfObject(GameObject o)
        {
            var posDiff = o.transform.position - transform.position;
            var xDiff = posDiff.x;
            var zDiff = posDiff.z;
            // +z = north
            // +x = east
            var obj_easting = easting + xDiff;
            var obj_northing = northing + zDiff;
            (var obj_lat, var obj_lon) = GetLatLonFromUTM(obj_easting, obj_northing);
            return (obj_easting, obj_northing, obj_lat, obj_lon);
        }

        public (float x, float z) GetUnityXZFromLatLon(double lat, double lon)
        {
            var latlon = new Coordinate(lat, lon);
            var eastingDiff = latlon.UTM.Easting - easting;
            var northingDiff = latlon.UTM.Northing - northing;
            var unityX = transform.position.x + eastingDiff;
            var unityZ = transform.position.z + northingDiff;
            return ((float)unityX, (float)unityZ);
        }
    }
    
}