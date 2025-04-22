using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct Heading
    {
        public float heading{get; set;}
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Heading(string json)
        {
            var d = JsonConvert.DeserializeObject<Heading>(json);
            heading = d.heading;
        }
    }
}
