using UnityEngine;

namespace SmarcGUI.MissionPlanning.Tasks
{
    public class AuvDepthMoveTo : Task
    {
        public override void SetParams()
        {
            Name = "auv-depth-move-to";
            Description = "Move to a position and depth";
            Params.Add("lat", 0);
            Params.Add("lon", 0);
            Params.Add("target_depth", 0);
            Params.Add("min_altitude", 0);
            Params.Add("rpm", 0);
            Params.Add("timeout", 0);
        }
    }

    public class AuvAltitudeMoveTo : Task
    {
        public override void SetParams()
        {
            Name = "auv-altitude-move-to";
            Description = "Move to a position and altitude";
            Params.Add("lat", 0);
            Params.Add("lon", 0);
            Params.Add("target_altitude", 0);
            Params.Add("max_depth", 0);
            Params.Add("rpm", 0);
            Params.Add("timeout", 0);
        }
    }

    public class AuvHydrobaticMoveTo : Task
    {
        public override void SetParams()
        {
            Name = "auv-hydrobatic-move-to";
            Description = "Move to a position, depth and orientation";
            Params.Add("lat", 0);
            Params.Add("lon", 0);
            Params.Add("target_depth", 0);
            Params.Add("qx", 0);
            Params.Add("qy", 0);
            Params.Add("qz", 0);
            Params.Add("qw", 1);
            Params.Add("timeout", 0);
        }
    }

    public class CruiseDepthAtHeading : Task
    {
        public override void SetParams()
        {
            Name = "cruise-depth-at-heading";
            Description = "Cruise at a depth and heading";
            Params.Add("target_depth", -1);
            Params.Add("target_heading", 0);
            Params.Add("min_altitude", 0);
            Params.Add("rpm", 0);
            Params.Add("timeout", 0);
        }
    }

    public class CruiseAltitudeAtHeading : Task
    {
        public override void SetParams()
        {
            Name = "cruise-depth-at-heading";
            Description = "Cruise at a depth and heading";
            Params.Add("target_altitude", -1);
            Params.Add("target_heading", 0);
            Params.Add("max_depth", 0);
            Params.Add("rpm", 0);
            Params.Add("timeout", 0);
        }
    }

    public class Loiter : Task
    {
        public override void SetParams()
        {
            Name = "loiter";
            Description = "Loiter at current position";
            Params.Add("timeout", 0);
        }
    }

}