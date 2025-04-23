using SmarcGUI.WorldSpace;
using TMPro;
using UnityEngine;

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

        Orientation CalculateQuaternionFromEuler(float roll, float pitch, float yaw)
        {
            float rollRad = Mathf.Deg2Rad * roll;
            float pitchRad = Mathf.Deg2Rad * pitch;
            float yawRad = Mathf.Deg2Rad * yaw;

            float cy = Mathf.Cos(yawRad * 0.5f);
            float sy = Mathf.Sin(yawRad * 0.5f);
            float cp = Mathf.Cos(pitchRad * 0.5f);
            float sp = Mathf.Sin(pitchRad * 0.5f);
            float cr = Mathf.Cos(rollRad * 0.5f);
            float sr = Mathf.Sin(rollRad * 0.5f);

            Orientation q = new Orientation();
            q.w = cr * cp * cy + sr * sp * sy;
            q.x = sr * cp * cy - cr * sp * sy;
            q.y = cr * sp * cy + sr * cp * sy;
            q.z = cr * cp * sy - sr * sp * cy;

            return q;
        }
        

        void UpdateQuatFromEuler()
        {
            var ex = exField.text != "" ? float.Parse(exField.text) : 0;
            var ey = eyField.text != "" ? float.Parse(eyField.text) : 0;
            var ez = ezField.text != "" ? float.Parse(ezField.text) : 0;

            
            // pitch = around x
            // yaw = around y
            // roll = around z
            var q = Quaternion.Euler(ex, ey, ez);

            qwField.text = q.w.ToString();
            qxField.text = q.x.ToString();
            qyField.text = q.y.ToString();
            qzField.text = q.z.ToString();
            var v = (Orientation)paramValue;
            var q_enu = q.To<ENU>();
            v.w = q.w;
            v.x = q.x;
            v.y = q.y;
            v.z = q.z;
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
            UpdateTexts();
            exField.onEndEdit.AddListener(OnEulerXChanged);
            eyField.onEndEdit.AddListener(OnEulerYChanged);
            ezField.onEndEdit.AddListener(OnEulerZChanged);

            OnSelectedChange();
        }

        void UpdateTexts()
        {
            var v = (Orientation)paramValue;
            qwField.text = v.w.ToString();
            qxField.text = v.x.ToString();
            qyField.text = v.y.ToString();
            qzField.text = v.z.ToString();

            var q = new Quaternion(v.x, v.y, v.z, v.w);
            var euler = q.eulerAngles;
            exField.text = euler.x.ToString();
            eyField.text = euler.y.ToString();
            ezField.text = euler.z.ToString();
        }


        void OnEulerXChanged(string s)
        {
            try {UpdateQuatFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler X value");
                OnEulerXChanged("0");
                return;
            }
        }

        void OnEulerYChanged(string s)
        {
            try {UpdateQuatFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Y value");
                OnEulerYChanged("0");
                return;
            }
        }

        void OnEulerZChanged(string s)
        {
            try {UpdateQuatFromEuler();}
            catch 
            {
                guiState.Log("Invalid euler Z value");
                OnEulerZChanged("0");
                return;
            }
        }

        public Orientation GetOrientation()
        {
            return (Orientation)paramValue;
        }

        public void SetOrientation(Orientation orientation)
        {
            paramValue = orientation;
        }
    }
}
