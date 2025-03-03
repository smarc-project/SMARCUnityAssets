
namespace DroneControlScripts
{   
	[System.Serializable]
	public class PIDController  {

		public float Kp = 1.0f;
		public float Ki = 0.0f;
		public float Kd = 1.0f;

		private float _error_t1;
		private float int_error;

		PIDController() {
			_error_t1 = 0.0f;
			int_error = 0.0f;
		}

		public float Update(float error, float dt) {
			float derror = (error - _error_t1)/dt;
			int_error += dt * (error + _error_t1)/2.0f;
			float command = Kp * error + Kd * derror + Ki * int_error;

			_error_t1 = error;

			return command;
		}

	}
}
