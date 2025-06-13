using Newtonsoft.Json;
using NUnit.Framework.Constraints;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct AuvHydrobaticPoint
    {
        public double latitude{get; set;}
        public double longitude{get; set;}
        public float target_depth{get; set;}
        public Orientation orientation{get; set;}
        public float timeout{get; set;}
        public float tolerance { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public AuvHydrobaticPoint(string json)
        {
            var ll = JsonConvert.DeserializeObject<AuvHydrobaticPoint>(json);
            latitude = ll.latitude;
            longitude = ll.longitude;
            target_depth = ll.target_depth;
            orientation = ll.orientation;
            timeout = ll.timeout;
            tolerance = ll.tolerance;
        }
    }
}
