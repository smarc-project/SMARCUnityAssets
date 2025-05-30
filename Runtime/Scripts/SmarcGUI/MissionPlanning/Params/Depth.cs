using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct Depth
    {
        public float depth{get; set;}
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Depth(string json)
        {
            var d = JsonConvert.DeserializeObject<Depth>(json);
            depth = d.depth;
        }
    }
}
