using System.Collections.Generic;
using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.MissionPlanning.Tasks
{

    public class AlarsSearch : Task
    {
        public override void SetParams()
        {
            Name = "alars-search";
            Description = "Search for an AUV in the water";
            Params.Add("search_center", new GeoPoint());
            Params.Add("search_radius", 10.0f);
        }
    }


    public class AlarsFollow : Task
    {
        public override void SetParams()
        {
            Name = "alars-follow";
            Description = "Follow an AUV in the water";
            Params.Add("timeout", 60);
        }
    }



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