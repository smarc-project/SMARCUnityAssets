namespace SmarcGUI.MissionPlanning.Tasks
{
    public class CustomTask : Task
    {
        // a task that can be customized with a json string
        // so that ppl can run "only-defined-in-a-vehicle" stuff from the gui
        // we'll implement the task proper if the tests show that this custom thing is useful :)
        public override void SetParams()
        {
            Name = "custom-task";
            Description = "A custom task with JSON params";
            Params.Add("json-params", "{}");
        }

    }
}