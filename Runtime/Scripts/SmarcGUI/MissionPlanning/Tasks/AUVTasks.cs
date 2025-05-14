using System.Collections.Generic;
using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.MissionPlanning.Tasks
{
    public class AuvDepthMoveTo : Task
    {
        public override void SetParams()
        {
            Name = "auv-depth-move-to";
            Description = "Move to a position at depth";
            Params.Add("waypoint", new AuvDepthPoint());
        }
    }

    public class AuvDepthMovePath : Task
    {
        public override void SetParams()
        {
            Name = "auv-depth-move-path";
            Description = "Move through positions at depths";
            Params.Add("waypoints", new List<AuvDepthPoint>());
        }
    }

    public class AuvAltitudeMoveTo : Task
    {
        public override void SetParams()
        {
            Name = "auv-altitude-move-to";
            Description = "Move to a position at altitude";
            Params.Add("waypoint", new AuvAltitudePoint());
        }
    }

    public class AuvAltitudeMovePath : Task
    {
        public override void SetParams()
        {
            Name = "auv-altitude-move-to";
            Description = "Move through positions at altitudes";
            Params.Add("waypoints", new List<AuvAltitudePoint>());
        }
    }

    public class AuvHydrobaticMoveTo : Task
    {
        public override void SetParams()
        {
            Name = "auv-hydrobatic-move-to";
            Description = "Move to a position, depth and orientation";
            Params.Add("waypoint", new AuvHydrobaticPoint());
        }
    }

    public class AuvHydrobaticMovePath : Task
    {
        public override void SetParams()
        {
            Name = "auv-hydrobatic-move-to";
            Description = "Move to a position, depth and orientation";
            Params.Add("waypoints", new List<AuvHydrobaticPoint>());
        }
    }

    public class CruiseDepthAtHeading : Task
    {
        public override void SetParams()
        {
            Name = "cruise-depth-at-heading";
            Description = "Cruise at a depth and heading";
            Params.Add("target_depth", new Depth());
            Params.Add("target_heading", new Heading());
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
            Params.Add("target_altitude", 5);
            Params.Add("target_heading", new Heading());
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