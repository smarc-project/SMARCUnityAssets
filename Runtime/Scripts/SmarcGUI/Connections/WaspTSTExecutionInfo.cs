using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;


namespace SmarcGUI.Connections
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class WaspTSTExecutionInfoMsg
    {
        public string Name;
        public float Rate;
        public string Type = "TSTExecutionInfo";
        public double Stamp;

        public string ToJson()
        {
            Stamp = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            return JsonConvert.SerializeObject(this);
        }

        public WaspTSTExecutionInfoMsg(string payload)
        {
            JsonConvert.PopulateObject(payload, this);
        }

        public WaspTSTExecutionInfoMsg(string name, float rate)
        {
            Name = name;
            Rate = rate;
        }

    }

    [RequireComponent(typeof(WaspHeartbeat))]
    public class WaspTSTExecutionInfo : MQTTPublisher
    {
        public float Rate = 0.5f;
        WaspTSTExecutionInfoMsg msg;
        WaspHeartbeat waspHeartbeat;

        void Awake()
        {
            mqttClient = FindFirstObjectByType<MQTTClientGUI>();
            waspHeartbeat = GetComponent<WaspHeartbeat>();
            msg = new(
                name: waspHeartbeat.AgentName,
                rate: Rate
            );
        }

        public override void StartPublishing()
        {
            publish = true;
            StartCoroutine(PublishCoroutine());
        }

        public override void StopPublishing()
        {
            publish = false;
        }

        IEnumerator PublishCoroutine()
        {
            while (publish)
            {
                mqttClient.Publish(waspHeartbeat.TopicBase+"tst_execution_info", msg.ToJson());
                yield return new WaitForSeconds(1.0f / Rate);
            }
        }
    }
}