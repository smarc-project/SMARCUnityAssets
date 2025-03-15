using System.Collections.Generic;
using Newtonsoft.Json;
using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.MissionPlanning.Tasks
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class Task
    {
        public string Name{get; set;}
        public string Description;
        public string TaskUuid;
        public Dictionary<string, object> Params = new();

        public Task()
        {
            OnTaskModified();
        }

        public void OnTaskModified()
        {
            TaskUuid = System.Guid.NewGuid().ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void RecoverFromJson()
        {
            // Cant modify a dictionary while iterating over it
            Dictionary<string, object> paramUpdates = new();

            foreach (var param in Params)
            {
                var paramValue = param.Value;
                if (paramValue is string || paramValue is int || paramValue is float || paramValue is bool) continue;
                if(Name == "move-to" && param.Key == "waypoint")
                {
                    var geoPoint = JsonConvert.DeserializeObject<GeoPoint>(paramValue.ToString());
                    paramUpdates.Add(param.Key, geoPoint);
                }
                else if(Name == "move-path" && param.Key == "waypoints")
                {
                    var geoPoints = JsonConvert.DeserializeObject<List<GeoPoint>>(paramValue.ToString());
                    paramUpdates.Add(param.Key, geoPoints);
                }
                else
                {
                    // We don't know what this is... so we turn it into a string and show it
                    paramUpdates.Add(param.Key, paramValue.ToString());
                }
                // Add other known stuff like this...
            }

            foreach (var update in paramUpdates)
                Params[update.Key] = update.Value;
        }
       
    }

    
}