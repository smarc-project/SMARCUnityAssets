namespace SmarcGUI.MissionPlanning.Tasks
{
    public class CustomTask : Task
    {
        // a task that can be customized with a json string
        // so that ppl can run "only-defined-in-a-vehicle" stuff from the gui
        // we'll implement the task proper if the tests show that this custom thing is useful :)
        public CustomTask(
            string name = "custom-task",
            string description = "A custom task with JSON params",
            string jsonParams = "{}") : base()
        {
            Name = name;
            Description = description;
            Params.Add("json-params", jsonParams);
        }

    }
}