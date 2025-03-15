using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;


namespace SmarcGUI.Connections
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class WaspSensorInfoMsg
    {
        public string Name;
        public float Rate;
        public string[] SensorDataProvided;
        public double Stamp;
        public string Type = "SensorInfo";

        public WaspSensorInfoMsg(string name, float rate, string[] sensorDataProvided)
        {
            Name = name;
            Rate = rate;
            SensorDataProvided = sensorDataProvided;
        }

        public WaspSensorInfoMsg(string payload)
        {
            JsonConvert.PopulateObject(payload, this);
        }

        public string ToJson()
        {
            Stamp = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            return JsonConvert.SerializeObject(this);
        }
    }

    [RequireComponent(typeof(WaspHeartbeat))]
    public class WaspSensorInfo : MQTTPublisher
    {
        WaspHeartbeat waspHeartbeat;
        public float SensorInfoRate = 1.0f;

        WaspSensorInfoMsg msg;

        void Awake()
        {
            waspHeartbeat = GetComponent<WaspHeartbeat>();
            mqttClient = FindFirstObjectByType<MQTTClientGUI>();

            msg = new WaspSensorInfoMsg(
                name: waspHeartbeat.AgentName,
                rate: SensorInfoRate,
                // https://api-docs.waraps.org/#/agent_communication/topics/sensor
                sensorDataProvided: new string[]{"position", "heading", "course", "speed"}
            );   
        }
        public override void StartPublishing()
        {
            publish = true;
            StartCoroutine(HeartbeatCoroutine());
        }

        public override void StopPublishing()
        {
            publish = false;
        }

        IEnumerator HeartbeatCoroutine()
        {
            while (publish)
            {
                mqttClient.Publish(waspHeartbeat.TopicBase+"sensor_info", msg.ToJson());
                yield return new WaitForSeconds(SensorInfoRate);
            }
        }


    }
}
