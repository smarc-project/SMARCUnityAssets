using System;
using Newtonsoft.Json;


namespace SmarcGUI.Connections
{
    //https://api-docs.waraps.org/#/agent_communication/tasks/commands

    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class BaseResponse
    {
        public string AgentUuid;
        public string Response;
        public string ComUuid;
        public string ResponseTo;

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public BaseResponse(){}

        public BaseResponse(string jsonString)
        {
            BaseResponse response = JsonConvert.DeserializeObject<BaseResponse>(jsonString);
            AgentUuid = response.AgentUuid;
            Response = response.Response;
            ComUuid = response.ComUuid;
            ResponseTo = response.ResponseTo;
        }
    }

    
    public class PongResponse : BaseResponse
    {
        public long TimeStamp;
        public long PingDelay;

        public PongResponse(string jsonString)
        {
            JsonConvert.PopulateObject(jsonString, this);
        }

        public PongResponse(PingCommand pingCmd)
        {
            Response = "pong";
            ResponseTo = pingCmd.ComUuid;
            ComUuid = Guid.NewGuid().ToString();
            TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            PingDelay = TimeStamp - pingCmd.TimeStamp;
            
        }

    }

}