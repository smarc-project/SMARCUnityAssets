using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct AuvAltitudePoint
    {
        public double latitude{get; set;}
        public double longitude{get; set;}
        public float target_altitude{get; set;}
        public float max_depth{get; set;}
        public float rpm{get; set;}
        public float timeout{get; set;}

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public AuvAltitudePoint(string json)
        {
            var ll = JsonConvert.DeserializeObject<AuvAltitudePoint>(json);
            latitude = ll.latitude;
            longitude = ll.longitude;
            target_altitude = ll.target_altitude;
            max_depth = ll.max_depth;
            rpm = ll.rpm;
            timeout = ll.timeout;
        }
    }
}
