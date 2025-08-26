namespace ROS.Core
{
    public class FrequencyTimer
    {
        double lastUpdate = 0f;
        float frequency = 10f;
        float period => 1.0f / frequency;

        public FrequencyTimer(float frequency)
        {
            this.frequency = frequency;
        }

        public bool ShouldUpdate(double now)
        {
            return now - lastUpdate >= period;
        }

        public void Tick()
        {
            lastUpdate += period;
        }
    }
}