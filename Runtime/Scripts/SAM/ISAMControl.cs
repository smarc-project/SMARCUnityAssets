namespace Force
{
    public interface ISAMControl
    {
        
        public void SetRpm1(double rpm);
        public void SetRpm2(double rpm);
        public void SetRpm(double rpm1, double rpm2);


        public void SetRudderAngle(float dr);


        public void SetElevatorAngle(float de);

        public void SetBatteryPack(double lcg);

        public void SetWaterPump(float vbs);
        float d_rudder { get; }
        float d_aileron { get; }
        double rpm1 { get; }
        double rpm2 { get; }
        double lcg { get; }
        double vbs { get; }
        
        SAMParameters parameters { get; set; }
    }
}