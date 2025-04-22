using System.Collections.Generic;
using Newtonsoft.Json;
using SmarcGUI.MissionPlanning.Params;
using System;

namespace SmarcGUI.MissionPlanning.Tasks
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class Task
    {
        public string Name{get; set;}
        public string Description;
        public string TaskUuid;
        public Dictionary<string, object> Params;

        public static Dictionary<string, Type> GetAllKnownTaskTypes()
        {
            var taskType = typeof(Task);
            var d = new Dictionary<string, Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(taskType) && !type.IsAbstract)
                    {
                        d.Add(type.Name, type);
                    }
                }
            }
            return d;
        }

        public virtual void SetParams(){}

        public Task()
        {
            Params = new Dictionary<string, object>();
            SetParams();
            OnTaskModified();
        }

        public void OnTaskModified()
        {
            TaskUuid = Guid.NewGuid().ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void RecoverFromJson()
        {
            // Cant modify a dictionary while iterating over it
            Dictionary<string, object> paramUpdates = new();

            // Dont like this, but we need to deserialize the params into their correct types
            // maybe we can do sth a little more elegant in the future...
            // this (wrongly) assumes that param types and param names are tied together
            // no such thing as "position" that is of type "latlon" etc.
            foreach (var param in Params)
            {
                var paramValue = param.Value;
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
                // AUVTasks
                else if(param.Key == "latlon")
                {
                    var latlon = JsonConvert.DeserializeObject<LatLon>(paramValue.ToString());
                    paramUpdates.Add(param.Key, latlon);
                }
                else if(param.Key == "target_depth")
                {
                    var depth = JsonConvert.DeserializeObject<Depth>(paramValue.ToString());
                    paramUpdates.Add(param.Key, depth);
                }
                else if(param.Key == "target_heading")
                {
                    var heading = JsonConvert.DeserializeObject<Heading>(paramValue.ToString());
                    paramUpdates.Add(param.Key, heading);
                }
                else if(param.Key == "orientation")
                {
                    var orientation = JsonConvert.DeserializeObject<Orientation>(paramValue.ToString());
                    paramUpdates.Add(param.Key, orientation);
                }
                else
                {
                    // nothing specially handled, primitive types are handled by default
                    if (paramValue is string || paramValue is int || paramValue is float || paramValue is bool) continue;
                    
                    // not handled specially, and not a primitive...
                    // We don't know what this is... so we turn it into a string and show it
                    paramUpdates.Add(param.Key, paramValue.ToString());
                }
            }

            foreach (var update in paramUpdates)
                Params[update.Key] = update.Value;
        }
       
    }

    
}