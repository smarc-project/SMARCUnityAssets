using Newtonsoft.Json;
using System.Collections.Generic;

namespace SmarcGUI.MissionPlanning.Tasks
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class TaskSpecTree
    {
        // the TST definition of waraps is... complex. 99% of the time we(smarc) wont be using the _entire_ spec
        // so im just implementing the basic "list of tasks" here. Which corresponds to their "L3" agents.
        // That means, this is a "linear tree" where the root is a sequence and thats it.
        // if anyone wants the _entire_ L4 spec, i wish you good luck :)
        public Dictionary<string, object> CommonParams = new();
        public string Name{get{return "seq";}}
        public Dictionary<string, object> Params = new();
        public List<Task> Children = new();
        public string TSTUuid;

        public string Description = "A sequence of tasks";

        public TaskSpecTree()
        {
            OnTSTModified();
        }

        public void OnTSTModified()
        {
            TSTUuid = System.Guid.NewGuid().ToString();
        }

        public void RecoverFromJson()
        {
            foreach (var task in Children)
                task.RecoverFromJson();
        }

        public string GetKey()
        {
            return $"{Name}-{Description}";
        }
    }
}