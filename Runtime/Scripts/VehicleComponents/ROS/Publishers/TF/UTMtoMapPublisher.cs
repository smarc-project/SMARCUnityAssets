using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Tf2;
using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

using GPSRef = GeoRef.GlobalReferencePoint;


namespace VehicleComponents.ROS.Publishers
{
    public class UTMtoMapPublisher: MonoBehaviour
    {
        public float frequency = 1f;
        float period => 1.0f/frequency;
        double lastTime;
        GPSRef gpsRef;
        ROSConnection ros;
        string topic = "/tf";
        TFMessageMsg ROSMsg;


        void Start()
        {
            var utmpubs = FindObjectsByType<UTMtoMapPublisher>(FindObjectsSortMode.None);
            if(utmpubs.Length > 1)
            {
                Debug.LogWarning("Found too many UTM->Map_gt publishers in the scene, there should only be one!");
            }

            var gpsRefs = FindObjectsByType<GPSRef>(FindObjectsSortMode.None);
            if(gpsRefs.Length < 1)
            {
                Debug.LogWarning("[UTM->Map pub] No Global Reference Point found in the scene. There must be at least one! Disabling UTM->Map publisher.");
                enabled = false;
                return;
            }
            else gpsRef = gpsRefs[0];

            // make sure this is in the origin
            // why origin? so that we can tell all other tf publishers
            // in the scene to publish a "global" frame that is map_gt
            // and they wont need to do any origin shenanigans that way
            transform.localPosition = Vector3.zero;
            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<TFMessageMsg>(topic);
        }

        void CreateMsg()
        {
            string utm_zone_band = $"utm_{gpsRef.zone}_{gpsRef.band}";
            // this is the position of unity-world in utm coordinates
            double lat, lon, originEasting, originNorthing;
            (originEasting, originNorthing, lat, lon) = gpsRef.GetUTMLatLonOfObject(gameObject);
            // create transform message from utm to map_gt
            var tf = new TransformMsg();
            tf.translation.x = (float)originEasting;
            tf.translation.y = (float)originNorthing;
            var utmToMap = new TransformStampedMsg
            (
                new HeaderMsg(new TimeStamp(Clock.time), "utm"), //header
                "map_gt", //child frame_id
                tf //transform
            );

            // also create a dummy utm_Z_B -> utm tf for people
            // that do not care about actual global location...
            var utmZBToUtm = new TransformStampedMsg
            (
                new HeaderMsg(new TimeStamp(Clock.time), utm_zone_band), //header
                "utm", //child frame_id
                new TransformMsg() // 0-transform
            );

            var tfMessageList = new List<TransformStampedMsg>
            {
                utmZBToUtm,
                utmToMap
            };

            // These transforms never change during play mode
            // so we can publish the same message all the time
            ROSMsg = new TFMessageMsg(tfMessageList.ToArray());
        }

        void FixedUpdate()
        {
            var deltaTime = Clock.NowTimeInSeconds - lastTime;
            if(deltaTime < period) return;

            CreateMsg();

            ros.Publish(topic, ROSMsg);
            lastTime = Clock.NowTimeInSeconds;
        }
    }
}