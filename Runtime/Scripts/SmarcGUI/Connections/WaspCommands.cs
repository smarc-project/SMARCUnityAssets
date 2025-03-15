using System;
using Newtonsoft.Json;
using SmarcGUI.MissionPlanning.Tasks;


namespace SmarcGUI.Connections
{
    //https://api-docs.waraps.org/#/agent_communication/tasks/commands

    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.KebabCaseNamingStrategy))]
    public class BaseCommand
    {
        public string Command;
        public string ComUuid;
        public string Sender = "UnityGUI";

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class PingCommand : BaseCommand
    {
        public PingCommand()
        {
            Command = "ping";
            ComUuid = Guid.NewGuid().ToString();
        }
    }

    public static class WaspSignals
    {
        public static string ENOUGH = "$enough";
        public static string PAUSE = "$pause";
        public static string CONTINUE = "$continue";
        public static string ABORT = "$abort";
    }

    public class SigntalTaskCommand: BaseCommand
    {
        public string Signal;
        public string TaskUuid;

        public SigntalTaskCommand(string signal, string taskUuid)
        {
            Command = "signal-task";
            ComUuid = Guid.NewGuid().ToString();

            Signal = signal;
            TaskUuid = taskUuid;
        }

    }

    public class StartTaskCommand : BaseCommand
    {
        public string ExecutionUnit;
        public Task Task;
        public string TaskUuid;

        public StartTaskCommand(Task task, string robot_name)
        {
            Command = "start-task";
            ComUuid = Guid.NewGuid().ToString();

            ExecutionUnit = robot_name;
            TaskUuid = Guid.NewGuid().ToString();
            Task = task;    
        }
    }


    // Defined but no docs on this, so ignoring for now.
    // public class QueryStatusCommand : Command
    // {

    // }

    public class StartTSTCommand : BaseCommand
    {
        public string Receiver;
        public TaskSpecTree tst;


        public StartTSTCommand(TaskSpecTree tst, string robot_name)
        {
            //https://api-docs.waraps.org/#/agent_communication/tst/tst_commands/start_tst
            ComUuid = Guid.NewGuid().ToString();
            Command = "start-tst";
            Receiver = robot_name;

            this.tst = tst;
            this.tst.CommonParams["execunit"] = $"/{robot_name}";
            this.tst.CommonParams["node-uuid"] = Guid.NewGuid().ToString();
        }
    }


}