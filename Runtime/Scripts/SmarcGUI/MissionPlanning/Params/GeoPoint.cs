using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct GeoPoint
    {
        public double latitude{get; set;}
        public double longitude{get; set;}
        public float altitude{get; set;}
        public readonly string rostype{ get{return "GeoPoint";} }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public GeoPoint(string json)
        {
            var gp = JsonConvert.DeserializeObject<GeoPoint>(json);
            latitude = gp.latitude;
            longitude = gp.longitude;
            altitude = gp.altitude;
        }
    }
}
