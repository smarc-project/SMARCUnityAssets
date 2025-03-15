using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;


namespace SmarcGUI.Connections
{

    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class WaspHeartbeatMsg
    {
        public string AgentType;
        public string AgentUuid;
        public string[] Levels;
        public string Name;
        public float Rate;
        public double Stamp;
        public string Type = "Heartbeat";

        public WaspHeartbeatMsg(string agentType, string agentUuid, string[] levels, string name, float rate)
        {
            AgentType = agentType;
            AgentUuid = agentUuid;
            Levels = levels;
            Name = name;
            Rate = rate;
        }

        public string ToJson()
        {
            Stamp = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            return JsonConvert.SerializeObject(this);
        }
    }

    public class WaspHeartbeat : MQTTPublisher
    {
        public WaspUnitType UnitType;
        public string Context = "smarc";
        public string AgentType => UnitType.ToString();
        public string AgentName => $"{Environment.UserName}__{transform.root.name}";
        public string TopicBase => $"{Context}/unit/{AgentType}/simulation/{AgentName}/";

        WaspHeartbeatMsg msg;

        public float HeartbeatRate = 1.0f;
        public string AgentUUID{get; private set;}

        public bool HasPublihed = false;

        void Awake()
        {
            AgentUUID = Guid.NewGuid().ToString();
            mqttClient = FindFirstObjectByType<MQTTClientGUI>();

            msg = new WaspHeartbeatMsg(
                agentType: AgentType,
                agentUuid: AgentUUID,
                levels: new string[]{WaspLevels.sensor.ToString(), WaspLevels.direct_execution.ToString(), WaspLevels.tst_execution.ToString()},
                name: AgentName,
                rate: HeartbeatRate);            
        }

        void Start()
        {
            Context = mqttClient.Context;
        }

        void OnEnable()
        {
            Context = mqttClient.Context;
        }

        public override void StartPublishing()
        {
            Context = mqttClient.Context;
            publish = true;
            StartCoroutine(HeartbeatCoroutine());
            HasPublihed = true;
        }

        public override void StopPublishing()
        {
            publish = false;
        }

        IEnumerator HeartbeatCoroutine()
        {
            while (publish)
            {
                mqttClient.Publish(TopicBase + "heartbeat", msg.ToJson());
                yield return new WaitForSeconds(HeartbeatRate);
            }
        }
    }
}