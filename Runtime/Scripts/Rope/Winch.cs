using UnityEngine;

using Force;
using Unity.Robotics.ROSTCPConnector;
using StdMessages = RosMessageTypes.Std;
namespace Rope
{
    public class Winch : RopeSystemBase
    {
        [Header("Hanging Load")]
        [Tooltip("Due to how ABs are solved, the AB will be converted to an RB when its attached to the winch for stability.")]
        public ArticulationBody LoadAB;
        public Rigidbody LoadRB;
        MixedBody loadBody;
    
        SpringJoint ropeJoint;
        LineRenderer lineRenderer;

        [Header("Winch Controls")]
        public float TargetLength = 0.5f;
        public float WinchSpeed = 0.5f;

        [Header("Winch")]
        public float CurrentRopeSpeed;
        public float CurrentLength = 0.5f;
        public float MinLength = 0.1f;

        [Header("Debug")]
        public float ActualDistance;

        ROSConnection ros;

        
        void WinchControlTestCallback(StdMessages.Float32MultiArrayMsg msg)
        {
            if (msg.data.Length >= 2)
            {
                float testTargetLength = msg.data[0];
                float testWinchSpeed = msg.data[1];
                Debug.Log($"Received test Float32MultiArray: target_length={testTargetLength}, winch_speed={testWinchSpeed}");

                TargetLength = Mathf.Clamp(testTargetLength, MinLength, RopeLength);
                WinchSpeed = testWinchSpeed;
            }
            else
            {
                Debug.LogWarning("Received Float32MultiArray with insufficient data.");
            }
        }
        

        
        public void AttachLoad(GameObject load)
        {
            LoadAB = load.GetComponent<ArticulationBody>();
            LoadRB = load.GetComponent<Rigidbody>();
        }

        protected override void SetupEnds()
        {
            loadBody = new MixedBody(LoadAB, LoadRB);
            ropeJoint = AttachBody(loadBody);
            lineRenderer = ropeJoint.gameObject.GetComponent<LineRenderer>();
            CurrentRopeSpeed = 0;
            ropeJoint.maxDistance = CurrentLength;
            setup = true;
            Update();
            FixedUpdate();
        }
        

        void OnValidate()
        {
            TargetLength = Mathf.Clamp(TargetLength, MinLength, RopeLength);
        }
        
        void Awake()
        {
            ros = ROSConnection.GetOrCreateInstance();
            Debug.Log("Subscribing to /winch_control_test");
            ros.Subscribe<StdMessages.Float32MultiArrayMsg>("/winch_control_test", WinchControlTestCallback);


            if(loadBody == null) loadBody = new MixedBody(LoadAB, LoadRB);
        }

        void Update()
        {
            if(!setup) return;
            ActualDistance = Vector3.Distance(loadBody.position, transform.position);
            bool ropeSlack = ActualDistance < CurrentLength;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, loadBody.position);
            lineRenderer.startColor = ropeSlack ? Color.green : Color.red;
            lineRenderer.endColor = lineRenderer.startColor;
        }

        void FixedUpdate()
        {   
            if(!setup) return;
            

            // simple speed control
            var lenDiff = TargetLength - CurrentLength;
            if(Mathf.Abs(lenDiff) > 0.025)
            {
                CurrentRopeSpeed = lenDiff > 0 ? WinchSpeed : -WinchSpeed;
            }
            else
            {
                CurrentRopeSpeed = 0;
                return;
            }

            CurrentLength += CurrentRopeSpeed * Time.fixedDeltaTime;
            CurrentLength = Mathf.Clamp(CurrentLength, MinLength, RopeLength);
            if(CurrentLength == MinLength || CurrentLength == RopeLength)
            {
                CurrentRopeSpeed = 0;
                return;
            }
            ropeJoint.maxDistance = CurrentLength;
        }

    }
}