using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sam; // ThrusterAngles, PercentStamped
using RosMessageTypes.Smarc; // ThrusterRPM

namespace DefaultNamespace
{
    public class SamActuatorController : MonoBehaviour
    {
        ISAMControl model;
        float vertical;
        float horizontal;
        float rpm1;
        float rpm2;
        float vbs;
        float lcg;

        public float sleepTime = 0.1f;
        public bool enable = true;
        bool wasEnabled = true;

        public string anglesTopic = "core/thrust_vector_cmd";
        public string rpm1_topic = "core/thruster1_cmd";
        public string rpm2_topic = "core/thruster2_cmd";
        public string vbs_topic = "core/vbs_cmd";
        public string lcg_topic = "core/lcg_cmd";

        float lastCommandTime;

        void Start()
        {
            model = GetComponent<ISAMControl>();
            var ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<ThrusterAnglesMsg>(anglesTopic, SetAngles);
            ros.Subscribe<ThrusterRPMMsg>(rpm1_topic, SetRpm1);
            ros.Subscribe<ThrusterRPMMsg>(rpm2_topic, SetRpm2);
            ros.Subscribe<PercentStampedMsg>(vbs_topic, SetVbs);
            ros.Subscribe<PercentStampedMsg>(lcg_topic, SetLcg);
            lastCommandTime = Time.time;
        }

        void SetAngles(ThrusterAnglesMsg msg)
        {
            vertical = msg.thruster_vertical_radians;
            horizontal = msg.thruster_horizontal_radians;
            lastCommandTime = Time.time;
        }
        void SetRpm1(ThrusterRPMMsg msg)
        {
            rpm1 = msg.rpm;
            lastCommandTime = Time.time;
        }
        void SetRpm2(ThrusterRPMMsg msg)
        {
            rpm2 = msg.rpm;
            lastCommandTime = Time.time;
        }
        void SetVbs(PercentStampedMsg msg)
        {
            vbs = msg.value;
            lastCommandTime = Time.time;
        }
        void SetLcg(PercentStampedMsg msg)
        {
            lcg = msg.value;
            lastCommandTime = Time.time;
        }


        public void Setup(GameObject robot)
        // Called from ROSPrepareRobot to set the topics in awake()
        {
            anglesTopic = "/" + robot.name + "/" + anglesTopic;
            rpm1_topic = "/" + robot.name + "/" + rpm1_topic;
            rpm2_topic = "/" + robot.name + "/" + rpm2_topic;
            vbs_topic = "/" + robot.name + "/" + vbs_topic;
            lcg_topic = "/" + robot.name + "/" + lcg_topic;
        }

        void Update()
        {
            var sinceLastCmd = Time.time - lastCommandTime;
            if(sinceLastCmd > sleepTime) return;
            if(!enable)
            {
                if(wasEnabled)
                {
                    wasEnabled = false;
                    model.SetRpm(0, 0);
                    model.SetElevatorAngle(0);
                    model.SetRudderAngle(0);
                }
                return;
            }
            wasEnabled = enable;

            model.SetRpm(rpm1, rpm2);
            model.SetElevatorAngle(vertical);
            model.SetRudderAngle(horizontal);
            model.SetWaterPump(vbs);
            model.SetBatteryPack(lcg);
        }
    }

}