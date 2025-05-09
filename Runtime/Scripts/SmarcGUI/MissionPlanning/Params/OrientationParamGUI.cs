using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;
using System;

using Unity.Robotics.ROSTCPConnector.ROSGeometry;


namespace SmarcGUI.MissionPlanning.Params
{
    public class OrientationParamGUI : ParamGUI, IParamHasOrientation
    {
        [Header("OrientationParamGUI")]
        public TMP_InputField qwField, qxField, qyField, qzField;
        public TMP_InputField exField, eyField, ezField;


        // given roll, pitch yaw in the order of x, y, z
        // where roll is around the x axis, pitch is around the y axis and yaw is around the z axis
        // the quaternion is given by:
        // q = [cos(roll/2)cos(pitch/2)cos(yaw/2) - sin(roll/2)sin(pitch/2)sin(yaw/2),
        //      sin(roll/2)cos(pitch/2)cos(yaw/2) + cos(roll/2)sin(pitch/2)sin(yaw/2),
        //      cos(roll/2)sin(pitch/2)cos(yaw/2) - sin(roll/2)cos(pitch/2)sin(yaw/2),
        //      cos(roll/2)cos(pitch/2)sin(yaw/2) + sin(roll/2)sin(pitch/2)cos(yaw/2)]
        // where roll, pitch and yaw are in radians
        // and the quaternion is given by:
        // q = [qx, qy, qz, qw]

        Orientation RPYtoRosOrientation(float ex, float ey, float ez)
        {
            // In X forward, Y left, Z up coordinate system!
            // roll = around x
            // pitch = around y
            // yaw = around z
            float exRad = Mathf.Deg2Rad * ex;
            float eyRad = Mathf.Deg2Rad * ey;
            float ezRad = Mathf.Deg2Rad * ez;

            float cy = Mathf.Cos(ezRad * 0.5f);
            float sy = Mathf.Sin(ezRad * 0.5f);
            float cp = Mathf.Cos(eyRad * 0.5f);
            float sp = Mathf.Sin(eyRad * 0.5f);
            float cr = Mathf.Cos(exRad * 0.5f);
            float sr = Mathf.Sin(exRad * 0.5f);

            Orientation q = new Orientation();
            q.w = cr * cp * cy + sr * sp * sy;
            q.x = sr * cp * cy - cr * sp * sy;
            q.y = cr * sp * cy + sr * cp * sy;
            q.z = cr * cp * sy - sr * sp * cy;

            return q;
        }

        private static double CopySign(double magnitude, double sign)
        {
            return Math.Abs(magnitude) * Math.Sign(sign);
        }

        Vector3 ROSOrientationToRPY(Orientation q)
        {
            var angles = new Vector3();

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
            double cosr_cosp = 1 - 2 * (q.x * q.x + q.y * q.y);
            angles.x = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.w * q.y - q.z * q.x);
            if (Math.Abs(sinp) >= 1)
            {
                angles.y = (float)CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.y = (float)Math.Asin(sinp);
            }

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.w * q.z + q.x * q.y);
            double cosy_cosp = 1 - 2 * (q.y * q.y + q.z * q.z);
            angles.z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
        

        void UpdateOrientationFromEuler()
        {
            var ex = exField.text != "" ? float.Parse(exField.text) : 0;
            var ey = eyField.text != "" ? float.Parse(eyField.text) : 0;
            var ez = ezField.text != "" ? float.Parse(ezField.text) : 0;

            var o = RPYtoRosOrientation(ex, ey, ez);

            qwField.text = o.w.ToString();
            qxField.text = o.x.ToString();
            qyField.text = o.y.ToString();
            qzField.text = o.z.ToString();

            var v = (Orientation)paramValue;
            v.w = o.w;
            v.x = o.x;
            v.y = o.y;
            v.z = o.z;
            paramValue = v;
            NotifyPathChange();
        }


        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            qwField.interactable = false;
            qxField.interactable = false;
            qyField.interactable = false;
            qzField.interactable = false;
        }

        protected override void SetupFields()
        {
            var rosOri = (Orientation)paramValue;
            if(rosOri.x == 0 && rosOri.y == 0 && rosOri.z == 0 && rosOri.w == 0)
            {
                rosOri.x = 0;
                rosOri.y = 0;
                rosOri.z = 0;
                rosOri.w = 1;
            }
            paramValue = rosOri;

            qwField.text = rosOri.w.ToString();
            qxField.text = rosOri.x.ToString();
            qyField.text = rosOri.y.ToString();
            qzField.text = rosOri.z.ToString();

            var euler = ROSOrientationToRPY(rosOri);
            exField.text = euler.x.ToString();
            eyField.text = euler.y.ToString();
            ezField.text = euler.z.ToString();

            exField.onEndEdit.AddListener(OnEulerXChanged);
            eyField.onEndEdit.AddListener(OnEulerYChanged);
            ezField.onEndEdit.AddListener(OnEulerZChanged);

            fields.Add(exField.GetComponent<RectTransform>());
            fields.Add(eyField.GetComponent<RectTransform>());
            fields.Add(ezField.GetComponent<RectTransform>());

            OnSelectedChange();
        }



        void OnEulerXChanged(string s)
        {
            try {UpdateOrientationFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler X value");
                OnEulerXChanged("0");
                return;
            }
        }

        void OnEulerYChanged(string s)
        {
            try {UpdateOrientationFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Y value");
                OnEulerYChanged("0");
                return;
            }
        }

        void OnEulerZChanged(string s)
        {
            try {UpdateOrientationFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Z value");
                OnEulerZChanged("0");
                return;
            }
        }

        public Orientation GetROSOrientation()
        {
            return (Orientation)paramValue;
        }

        public void SetROSOrientation(Orientation orientation)
        {
            paramValue = orientation;
        }

        public Quaternion GetUnityQuaternion()
        {
            var o = (Orientation)paramValue;
            var unityOri = ENU.ConvertToRUF(
                        new Quaternion(
                            (float)o.x,
                            (float)o.y,
                            (float)o.z,
                            (float)o.w));

            return unityOri;
        }

        public void SetUnityQuaternion(Quaternion q)
        {
            // might be nice to have later, when there is a gui widget for setting the orientation
            // in-world
            Debug.Log("SetUnityQuat not implemented");
        }
    }
}
