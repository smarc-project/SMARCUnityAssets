using System;
using System.Threading;
using SystemTask = System.Threading.Tasks.Task; // to diff from SmarcGUI.MissionPlanning.Tasks.Task

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SmarcGUI.MissionPlanning.Params;
using System.Security.Authentication;

namespace SmarcGUI.Connections
{
    public enum WaspUnitType
    {
        air,
        ground,
        surface,
        subsurface
    }

    public enum WaspLevels
    {
        sensor,
        direct_execution,
        tst_execution,
        delegation
    }


    public class MQTTClientGUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public TMP_InputField ServerAddressInput;
        public TMP_InputField PortInput;
        public TMP_InputField ContextInput;
        public Toggle SubToSimToggle;
        public Toggle SubToRealToggle;
        public Toggle TLSToggle;

        public Button ConnectButton;
        public Button DisconnectButton;

        // mostly a wrapper for: https://github.com/dotnet/MQTTnet/blob/release/4.x.x/Samples/Client/Client_Connection_Samples.cs
        // Notice we use the 4.x branch because dotnet of unity (:

        IMqttClient mqttClient;
        GUIState guiState;

        public string Context => ContextInput.text;

        string ServerAddress => ServerAddressInput.text;
        int ServerPort => int.Parse(PortInput.text);

        MQTTPublisher[] publishers;

        Dictionary<string, RobotGUI> robotsGuis = new();
        Queue<Tuple<string, string>> mqttInbox = new();
        HashSet<string> subscribedTopics = new();

        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            ContextInput.text = "smarcsim";
            publishers = FindObjectsByType<MQTTPublisher>(FindObjectsSortMode.None);
        }

        void Start()
        {
            ServerAddressInput.text = "20.240.40.232";
            PortInput.text = "1884";
            ConnectButton.onClick.AddListener(ConnectToBroker);
            DisconnectButton.onClick.AddListener(DisconnectFromBroker);
            ConnectionInputsInteractable(true);
        }

        void ConnectionInputsInteractable(bool interactable)
        {
            ConnectButton.interactable = interactable;
            ServerAddressInput.interactable = interactable;
            PortInput.interactable = interactable;
            ContextInput.interactable = interactable;
            SubToRealToggle.interactable = interactable;
            SubToSimToggle.interactable = interactable;
            DisconnectButton.interactable = !interactable;
        }


        SystemTask OnMsgReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.ConvertPayloadToString();
            mqttInbox.Enqueue(new Tuple<string, string>(topic, payload));
            return SystemTask.CompletedTask;
        }

        void OnConnetionMade()
        {
            if(SubToRealToggle.isOn) SubToHeartbeats("real");
            if(SubToSimToggle.isOn) SubToHeartbeats("simulation");
            foreach(var publisher in publishers)
            {
                publisher.StartPublishing();
            }
        }

        void OnconnectionLost()
        {
            foreach(var publisher in publishers)
            {
                publisher.StopPublishing();
            }
            subscribedTopics.Clear();
        }


        async void ConnectToBroker()
        {
            var mqttFactory = new MqttFactory();
            mqttClient = mqttFactory.CreateMqttClient();

            var mqttClientOptionsUnbuilt = new MqttClientOptionsBuilder().WithTcpServer(host: ServerAddress, port: ServerPort);

            if(TLSToggle.isOn)
            {
                mqttClientOptionsUnbuilt = mqttClientOptionsUnbuilt.WithTlsOptions(
                    o =>
                    {
                        o.WithCertificateValidationHandler(
                            eventArgs =>
                            {
                                Debug.Log(eventArgs.Certificate.Subject);
                                Debug.Log(eventArgs.Certificate.GetExpirationDateString());
                                Debug.Log(eventArgs.Chain.ChainPolicy.RevocationMode);
                                Debug.Log(eventArgs.Chain.ChainStatus);
                                Debug.Log(eventArgs.SslPolicyErrors);
                                return true;
                            }
                        );

                        // The default value is determined by the OS. Set manually to force version.
                        o.WithSslProtocols(SslProtocols.Tls12);
                    });
            }

            var mqttClientOptions = mqttClientOptionsUnbuilt.Build();

            mqttClient.ApplicationMessageReceivedAsync += OnMsgReceived;

            guiState.Log($"Connecting to {ServerAddress}:{ServerPort} ...");
            MqttClientConnectResult response = null;
            try
            {
                ConnectionInputsInteractable(false);
                response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            }
            catch (MqttCommunicationTimedOutException)
            {
                ConnectionInputsInteractable(true);
                guiState.Log($"Timeout while trying to connect to {ServerAddress}:{ServerPort}");
                return;
            }
            catch (MqttCommunicationException)
            {
                ConnectionInputsInteractable(true);
                guiState.Log($"Communication exception while trying to connect to {ServerAddress}:{ServerPort}");
                return;
            }
            catch (OperationCanceledException)
            {
                ConnectionInputsInteractable(true);
                guiState.Log($"Connection to {ServerAddress}:{ServerPort} was canceled");
                return;
            }

            if(response is null || response.ResultCode != MqttClientConnectResultCode.Success)
            {
                ConnectionInputsInteractable(true);
                guiState.Log($"Failed to connect to {ServerAddress}:{ServerPort}, result code == {response.ResultCode}");
                return;
            }
            guiState.Log($"Connected to broker on {ServerAddress}:{ServerPort}!");

            OnConnetionMade();
        }

        async void DisconnectFromBroker()
        {
            var mqttFactory = new MqttFactory();
            var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();
            try
            {
                await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
            }
            catch (MqttClientNotConnectedException)
            {
                guiState.Log($"Not connected to broker on {ServerAddress}:{ServerPort}!");
                ConnectionInputsInteractable(true);
                return;
            }
            ConnectionInputsInteractable(true);
            guiState.Log($"Disconnected from broker on {ServerAddress}:{ServerPort}!");
            OnconnectionLost();
        }

    
        public async void Publish(string topic, string payload)
        {
            if(mqttClient is null || !mqttClient.IsConnected) return;

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();


            try
            {
                await mqttClient.PublishAsync(message, CancellationToken.None);
            }
            catch (MqttCommunicationTimedOutException)
            {
                guiState.Log($"Timeout while trying to publish message to {ServerAddress}:{ServerPort}");
                return;
            }
            catch (OperationCanceledException)
            {
                guiState.Log($"Publishing message to {ServerAddress}:{ServerPort} was canceled");
                return;
            }
        }

        public async void SubToTopic(string topic)
        {
            Debug.Log($"Subscribing to topic: {topic} ...");
            if(subscribedTopics.Contains(topic))
            {
                Debug.Log($"Already subscribed to topic: {topic}");
                return;
            }

            var mqttFactory = new MqttFactory();
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            subscribedTopics.Add(topic);
            Debug.Log($"MQTT client subscribed to topic: {topic}");
        }

        void SubToHeartbeats(string realism)
        {
            var topic = $"{Context}/unit/+/{realism}/+/heartbeat";
            SubToTopic(topic);
        }

        void HandleMQTTMsg(Tuple<string, string> topicPayload)
        {
            var topic = topicPayload.Item1;
            var payload = topicPayload.Item2;

            // wara stuff is formatted like: smarc/unit/subsurface/simulation/sam1/heartbeat
            // {context}/unit/{air,ground,surface,subsurface}/{real,simulation,playback}/{agentName}/{topic}
            var topicParts = topic.Split('/');
            var context = topicParts[0];
            var domain = topicParts[2];
            var realism = topicParts[3];
            var agentName = topicParts[4];
            var messageType = topicParts[5];

            if(!robotsGuis.ContainsKey(agentName))
            {
                string robotNamespace = $"{context}/unit/{domain}/{realism}/{agentName}/";
                var robotgui = guiState.CreateNewRobotGUI(agentName, InfoSource.MQTT, robotNamespace);
                robotsGuis.Add(agentName, robotgui);
                guiState.Log($"Created new RobotGUI for {agentName}");
            }

            switch(messageType)
            {
                case "heartbeat":
                    robotsGuis[agentName].OnHeartbeatReceived();
                    break;
                case "sensor_info":
                    WaspSensorInfoMsg sensorInfo = new(payload);
                    robotsGuis[agentName].OnSensorInfoReceived(sensorInfo);
                    break;
                case "direct_execution_info":
                    WaspDirectExecutionInfoMsg directExecutionInfo = new(payload);
                    robotsGuis[agentName].OnDirectExecutionInfoReceived(directExecutionInfo);
                    break;
                case "tst_execution_info":
                    WaspTSTExecutionInfoMsg tstExecutionInfo = new(payload);
                    robotsGuis[agentName].OnTSTExecutionInfoReceived(tstExecutionInfo);
                    break;
                case "sensor":
                    // there could be _many_ different kinds of sensors,
                    // some of these, we will have specific ways to visualize, like the basics
                    // of position, heading, course, speed
                    // others, we will have some generic ways... eventually.
                    var sensor_type = topicParts[6];
                    switch(sensor_type)
                    {
                        case "position":
                            GeoPoint pos = new(payload);
                            robotsGuis[agentName].OnPositionReceived(pos);
                            break;
                        case "heading":
                            float heading = float.Parse(payload);
                            robotsGuis[agentName].OnHeadingReceived(heading);
                            break;
                        case "course":
                            float course = float.Parse(payload);
                            robotsGuis[agentName].OnCourseReceived(course);
                            break;
                        case "speed":
                            float velocity = float.Parse(payload);
                            robotsGuis[agentName].OnSpeedReceived(velocity);
                            break;
                        default:
                            guiState.Log($"Received unhandled sensor info from {topic}");
                            break;
                    }
                    break;
                default:
                    guiState.Log($"Received uhandled message on MQTT topic: {topic}. You should add this into MQTTClientGUI.HandleMQTTMsg!");
                    break;
            }
        }
        
        void FixedUpdate()
        {
            if(mqttInbox.Count == 0) return;
            while(mqttInbox.Count > 0) HandleMQTTMsg(mqttInbox.Dequeue());
        }
        

        


    }
        
}