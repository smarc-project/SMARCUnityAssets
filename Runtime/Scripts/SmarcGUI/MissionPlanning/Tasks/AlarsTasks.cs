using System.Collections.Generic;
using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.MissionPlanning.Tasks
{
    public class AlarsRecover : Task
    {
        public override void SetParams()
        {
            Name = "alars-recover";
            Description = "Hook a rope in the water";
            Params.Add("rope_points", new List<GeoPoint>());
            Params.Add("min_height_above_water", 3.0f);
            Params.Add("swoop_vertical", 5.0f);
            Params.Add("swoop_horizontal", 3.0f);
            Params.Add("straight_before_rope", 1.0f);
            Params.Add("straight_distance", 3.0f);
            Params.Add("raise_horizontal", 1.0f);
            Params.Add("raise_vertical", 10.0f);
        }
    }

    

}