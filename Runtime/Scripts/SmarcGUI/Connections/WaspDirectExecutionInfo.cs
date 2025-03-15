using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace SmarcGUI.Connections
{
    
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class TaskSpec
    {
        public string Name;
        public string[] Signals;

        public TaskSpec(string name, string[] signals)
        {
            Name = name;
            Signals = signals;
        }
    }

    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class WaspDirectExecutionInfoMsg
    {
        public string Name;
        public float Rate;
        public string Type = "DirectExecutionInfo";
        public double Stamp;
        public List<TaskSpec> TasksAvailable;
        public List<Dictionary<string, string>> TasksExecuting;

        public string ToJson()
        {
            Stamp = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
            return JsonConvert.SerializeObject(this);
        }

        public WaspDirectExecutionInfoMsg(string payload)
        {
            JsonConvert.PopulateObject(payload, this);
        }

        public WaspDirectExecutionInfoMsg(string name, float rate, List<TaskSpec> tasksAvailable)
        {
            Name = name;
            Rate = rate;
            TasksAvailable = tasksAvailable;
            TasksExecuting = new();
        }

        public WaspDirectExecutionInfoMsg(string name, float rate, List<TaskSpec> tasksAvailable, List<Dictionary<string, string>> tasksExecuting)
        {
            Name = name;
            Rate = rate;
            TasksAvailable = tasksAvailable;
            TasksExecuting = tasksExecuting;
        }


    }

    [RequireComponent(typeof(WaspHeartbeat))]
    public class WaspDirectExecutionInfo : MQTTPublisher
    {
        public float Rate = 0.5f;
        WaspDirectExecutionInfoMsg msg;
        WaspHeartbeat waspHeartbeat;

        void Awake()
        {
            mqttClient = FindFirstObjectByType<MQTTClientGUI>();
            waspHeartbeat = GetComponent<WaspHeartbeat>();
            var tasksAvailable = new List<TaskSpec>(){
                new("move-to", new string[]{"$abort", "$enough", "$pause", "$continue"}),
                new("move-path", new[]{"$abort", "$enough", "$pause", "$continue"}),
                new("do-a-flip", new[]{"$abort", "$enough"})
            };
            var tasksExecuting = new List<Dictionary<string, string>>
            {
                new(){ {"name", "move-to"}, {"uuid", "123"} }, // for testing UI functions
                new(){ {"name", "move-to"}, {"uuid", Guid.NewGuid().ToString()} },
                new(){ {"name", "chilling"}, {"uuid", Guid.NewGuid().ToString()} }
            };
            msg = new(
                name: waspHeartbeat.AgentName,
                rate: Rate,
                tasksAvailable: tasksAvailable,
                tasksExecuting: tasksExecuting
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
                mqttClient.Publish(waspHeartbeat.TopicBase+"direct_execution_info", msg.ToJson());
                yield return new WaitForSeconds(1.0f / Rate);
            }
        }
    }
}