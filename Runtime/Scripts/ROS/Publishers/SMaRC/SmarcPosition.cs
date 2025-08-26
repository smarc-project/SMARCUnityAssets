using UnityEngine;
using Unity.Robotics.Core;
using ROS.Core;
using RosMessageTypes.Geographic;
using GeoRef;
using DefaultNamespace;


namespace VehicleComponents.ROS.Publishers
{
    class SmarcPosition : ROSBehaviour
    {
        [Header("ROS Publisher")]
        public float frequency = 10f;
        float period => 1.0f/frequency;
        double lastUpdate = 0f;
        bool registered = false;

        Transform base_link;
        GeoPointMsg msg;
        GlobalReferencePoint globalRef;


        protected override void StartROS()
        {
            msg = new GeoPointMsg();

            var robot = Utils.FindParentWithTag(gameObject, "robot", false);
            base_link = Utils.FindDeepChildWithName(robot, "base_link").transform;

            globalRef = FindFirstObjectByType<GlobalReferencePoint>();
            if (globalRef == null)
            {
                Debug.LogError("GlobalReferencePoint not found in the scene.");
                enabled = false;
                return;
            }
            
            if (base_link == null)
            {
                Debug.LogError("base_link not found for smarc position.");
                enabled = false;
                return;
            }
            if (!registered)
            {
                rosCon.RegisterPublisher<GeoPointMsg>(topic);
                registered = true;
            }
        }

        void FixedUpdate()
        {
            if (Clock.Now - lastUpdate < period) return;
            lastUpdate = Clock.Now;

            var (lat, lon) = globalRef.GetLatLonFromUnityXZ(base_link.position.x, base_link.position.z);
            msg.latitude = lat;
            msg.longitude = lon;
            msg.altitude = base_link.position.y;
            
            rosCon.Publish(topic, msg);
        }
    }
}
