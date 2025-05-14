using System.Collections.Generic;
using SmarcGUI.MissionPlanning.Params;

namespace SmarcGUI.MissionPlanning.Tasks
{
    public struct MoveSpeed
    {
        public static string FAST{ get{return "fast";} }
        public static string STANDARD{ get{return "standard";} }
        public static string SLOW{ get{return "slow";} }
    }

    public class MoveTo : Task
    {
        public override void SetParams()
        {
            Name = "move-to";
            Description = "Move to a WGS84 point at speed";
            Params.Add("speed", MoveSpeed.STANDARD);
            Params.Add("waypoint", new GeoPoint());
        }
    }

    public class MovePath : Task
    {
        public override void SetParams()
        {
            Name = "move-path";
            Description = "Move through a list of WGS84 points at speed";
            Params.Add("speed", MoveSpeed.STANDARD);
            Params.Add("waypoints", new List<GeoPoint>());
        }
    }
}