namespace SmarcGUI.MissionPlanning.Tasks
{
    public class AuvDepthMoveTo : Task
    {
        public AuvDepthMoveTo(double lat, double lon, float target_depth, float min_altitude, float rpm, float timeout)
        {
            Name = "auv-depth-move-to";
            Description = "Move to a position and depth";
            Params.Add("lat", lat);
            Params.Add("lon", lon);
            Params.Add("target_depth", target_depth);
            Params.Add("min_altitude", min_altitude);
            Params.Add("rpm", rpm);
            Params.Add("timeout", timeout);
        }
    }

    public class AuvAltitudeMoveTo : Task
    {
        public AuvAltitudeMoveTo(double lat, double lon, float target_altitude, float max_depth, float rpm, float timeout)
        {
            Name = "auv-altitude-move-to";
            Description = "Move to a position and altitude";
            Params.Add("lat", lat);
            Params.Add("lon", lon);
            Params.Add("target_altitude", target_altitude);
            Params.Add("max_depth", max_depth);
            Params.Add("rpm", rpm);
            Params.Add("timeout", timeout);
        }
    }

    public class AuvHydrobaticMoveTo : Task
    {
        public AuvHydrobaticMoveTo(double lat, double lon, float target_depth, float[4] enu_quat_orientation, float timeout)
        {
            Name = "auv-hydrobatic-move-to";
            Description = "Move to a position, depth and orientation";
            Params.Add("lat", lat);
            Params.Add("lon", lon);
            Params.Add("target_depth", target_depth);
            Params.Add("enu_quat_orientation", enu_quat_orientation);
            Params.Add("timeout", timeout);
        }
    }

    public class CruiseDepthAtHeading : Task
    {
        public CruiseDepthAtHeading(float target_depth, float target_heading, float min_altitude, float rpm, float timeout)
        {
            Name = "cruise-depth-at-heading";
            Description = "Cruise at a depth and heading";
            Params.Add("target_depth", target_depth);
            Params.Add("target_heading", target_heading);
            Params.Add("min_altitude", min_altitude);
            Params.Add("rpm", rpm);
            Params.Add("timeout", timeout);
        }
    }

    public class CruiseAltitudeAtHeading : Task
    {
        public CruiseAltitudeAtHeading(float target_altitude, float target_heading, float max_depth, float rpm, float timeout)
        {
            Name = "cruise-depth-at-heading";
            Description = "Cruise at a depth and heading";
            Params.Add("target_altitude", target_altitude);
            Params.Add("target_heading", target_heading);
            Params.Add("max_depth", max_depth);
            Params.Add("rpm", rpm);
            Params.Add("timeout", timeout);
        }
    }

    public class Loiter : Task
    {
        public Loiter(float timeout)
        {
            Name = "loiter";
            Description = "Loiter at current position";
            Params.Add("timeout", timeout);
        }
    }

}