using UnityEngine;

namespace DefaultNamespace
{
    public class SAMThrusterWiggler : MonoBehaviour
    {
        private ISAMControl _samForceModel;
        private Transform yaw_link;
        private Transform front_prop_link;
        private Transform back_prop_link;



        public void Setup(GameObject robot)
        {
            // We will read the thruster angles from the force model
            // so that we dont care what is controlling that model
            var sam_motion_model = robot.transform.parent.gameObject;
            _samForceModel = sam_motion_model.GetComponent<ISAMControl>();

            // Need access to the thruster_yaw_link object
            // Normally the transform.Find method would work for this, but
            // since we change its name to suit ROS needs, we dont really know 
            // the name of our child... but it IS tagged as "robot" by the URDF importer
            // so we can use that to get the "sam_auv" object
            // var sam_auv_tf = transform.FindWithTag("robot");
            string p = robot.transform.name;

            // then find the yaw link from that tf
            var yaw_link_name = $"{p}_base_link/{p}_thruster_yaw_link";
            yaw_link = robot.transform.Find(yaw_link_name);
            if(yaw_link == null) Debug.Log($"yaw link is null! : {yaw_link_name}");

            // and the props are children of the yaw link
            front_prop_link = yaw_link.Find($"{p}_front_prop_link");
            back_prop_link = yaw_link.Find($"{p}_back_prop_link");
            if(front_prop_link == null || back_prop_link == null) Debug.Log("Prop link is null!");
        }

        void Update()
        {
            if(yaw_link == null) return;
            else{
                // The target angle for the thruster link as defined by underlying model
                var yawRad = _samForceModel.d_rudder;
                var pitchRad = _samForceModel.d_aileron;
                yaw_link.transform.localRotation = Quaternion.Euler(pitchRad*Mathf.Rad2Deg, -yawRad*Mathf.Rad2Deg, 0);
            }


            if(front_prop_link == null || back_prop_link == null) return;
            else{
                // These guys can just turn and stop whereever.
                front_prop_link.Rotate(Vector3.forward * (float)(_samForceModel.rpm1*60) * Time.deltaTime);
                back_prop_link.Rotate(Vector3.forward * -(float)(_samForceModel.rpm2*60) * Time.deltaTime);
            }
        }
    }

}
