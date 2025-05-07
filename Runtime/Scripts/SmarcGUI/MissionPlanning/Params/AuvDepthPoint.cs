using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct AuvDepthPoint
    {
        public double latitude{get; set;}
        public double longitude{get; set;}
        public float depth{get; set;}
        public float min_altitude{get; set;}
        public float rpm{get; set;}
        public float timeout{get; set;}

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public AuvDepthPoint(string json)
        {
            var ll = JsonConvert.DeserializeObject<AuvDepthPoint>(json);
            latitude = ll.latitude;
            longitude = ll.longitude;
            depth = ll.depth;
            min_altitude = ll.min_altitude;
            rpm = ll.rpm;
            timeout = ll.timeout;
        }
    }
}
