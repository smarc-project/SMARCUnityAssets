using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.InputSystem;

using RosMessageTypes.Sensor;

namespace SmarcGUI.Connections
{
    public class ROSClientGUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField ServerAddressInput;
        public TMP_InputField PortInput;
        public Button ConnectButton;
        public Button DisconnectButton;
        public Toggle PublishControllerToRosToggle;
        public string JoyTopic = "/joy";
        InputAction lstick, rstick, lb, rb, lt, rt, north, south, east, west, dpad;

        ROSConnection rosCon;

        string ServerAddress => ServerAddressInput.text;
        int ServerPort => int.Parse(PortInput.text);

        void Awake()
        {
            lstick = InputSystem.actions.FindAction("VirtualJoy/LeftStick");
            rstick = InputSystem.actions.FindAction("VirtualJoy/RightStick");
            lb = InputSystem.actions.FindAction("VirtualJoy/LB");
            rb = InputSystem.actions.FindAction("VirtualJoy/RB");
            lt = InputSystem.actions.FindAction("VirtualJoy/LT");
            rt = InputSystem.actions.FindAction("VirtualJoy/RT");
            north = InputSystem.actions.FindAction("VirtualJoy/North");
            south = InputSystem.actions.FindAction("VirtualJoy/South");
            east = InputSystem.actions.FindAction("VirtualJoy/East");
            west = InputSystem.actions.FindAction("VirtualJoy/West");
            dpad = InputSystem.actions.FindAction("VirtualJoy/Dpad");
        }

        void Start()
        {
            rosCon = ROSConnection.GetOrCreateInstance();
            ServerAddressInput.text = rosCon.RosIPAddress.ToString();
            PortInput.text = rosCon.RosPort.ToString();
            ConnectButton.onClick.AddListener(OnConnect);
            DisconnectButton.onClick.AddListener(OnDisconnect);
            rosCon.ShowHud = false;
        }

        void ConnectionInputsInteractable(bool interactable)
        {
            ConnectButton.interactable = interactable;
            ServerAddressInput.interactable = interactable;
            PortInput.interactable = interactable;
            DisconnectButton.interactable = !interactable;
            PublishControllerToRosToggle.interactable = interactable;
        }

        void OnConnect()
        {
            rosCon.Connect(ServerAddress, ServerPort);
            ConnectionInputsInteractable(false);
            if(PublishControllerToRosToggle.isOn)
            {
                rosCon.RegisterPublisher<JoyMsg>(JoyTopic);
            }
        }

        void OnDisconnect()
        {
            rosCon.Disconnect();
            ConnectionInputsInteractable(true);
        }

        void Update()
        {
            if(PublishControllerToRosToggle.isOn)
            {
                var dpadVal = dpad.ReadValue<Vector2>();
                var leftval = lstick.ReadValue<Vector2>();
                var rightval = rstick.ReadValue<Vector2>();
                rosCon.Publish(JoyTopic, new JoyMsg
                {
                    // https://docs.ros.org/en/iron/p/joy/
                    axes = new float[] 
                    {
                        leftval.x,
                        leftval.y,
                        rightval.x,
                        rightval.y,
                        lt.ReadValue<float>(),
                        rt.ReadValue<float>(),    
                    },
                    buttons = new int[] 
                    {
                        south.ReadValue<int>(),
                        east.ReadValue<int>(),
                        west.ReadValue<int>(),
                        north.ReadValue<int>(),
                        0, // BACK(SELECT)
                        0, // GUIDE (Xbox button?)
                        0, // START
                        0, // Left stick click
                        0, // Right stick click
                        lb.IsPressed()? 1 : 0,
                        rb.IsPressed()? 1 : 0,
                        dpadVal.y > 0? 1 : 0, //dpad up
                        dpadVal.y < 0? 1 : 0, //dpad down
                        dpadVal.x < 0? 1 : 0, //dpad left
                        dpadVal.x > 0? 1 : 0, //dpad right
                        0, // rest is weird stuff like paddles...
                        0,
                        0,
                        0,
                        0,
                        0
                    }
                });
            }
        }

    }
}