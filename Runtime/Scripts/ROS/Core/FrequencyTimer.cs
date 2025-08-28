namespace ROS.Core
{
    public class FrequencyTimer
    {
        double lastUpdate = 0f;
        float frequency = 10f;
        float period => 1.0f / frequency;

        /// <summary>
        /// A simple class that simply keeps track of number of periods passed and
        /// provides a way to check if it's time to update.
        /// </summary>
        /// <param name="frequency"></param>
        public FrequencyTimer(float frequency)
        {
            this.frequency = frequency;
        }

        public bool NeedsTick(double now)
        {
            return now - lastUpdate >= period;
        }

        public void Tick()
        {
            lastUpdate += period;
        }
    }
}