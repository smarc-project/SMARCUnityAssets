using Newtonsoft.Json;

namespace SmarcGUI.MissionPlanning.Params
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public struct Orientation
    {
        public float w{get; set;}
        public float x{get; set;}
        public float y{get; set;}
        public float z{get; set;}


        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public Orientation(string json)
        {
            var o = JsonConvert.DeserializeObject<Orientation>(json);
            w = o.w;
            x = o.x;
            y = o.y;
            z = o.z;
        }
    }
}