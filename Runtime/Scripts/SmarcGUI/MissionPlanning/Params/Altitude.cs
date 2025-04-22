using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct Altitude
    {
        public float altitude{get; set;}
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Altitude(string json)
        {
            var d = JsonConvert.DeserializeObject<Altitude>(json);
            altitude = d.altitude;
        }
    }
}
