using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct LatLon
    {
        public double latitude{get; set;}
        public double longitude{get; set;}

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public LatLon(string json)
        {
            var ll = JsonConvert.DeserializeObject<LatLon>(json);
            latitude = ll.latitude;
            longitude = ll.longitude;
        }
    }
}
