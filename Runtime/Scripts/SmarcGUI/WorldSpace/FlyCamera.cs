using UnityEngine;

// Source; https://gist.github.com/FreyaHolmer/650ecd551562352120445513efa1d952
// with some mods.


namespace SmarcGUI.WorldSpace
{
	[RequireComponent( typeof(Camera) )]
	public class FlyCamera : MonoBehaviour {
		public float acceleration = 50; // how fast you accelerate
		public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
		public float lookSensitivity = 1; // mouse look sensitivity
		public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input
		public bool focusOnEnable = true; // whether or not to focus and lock cursor immediately on enable

		Vector3 velocity; // current velocity

		Camera cam;

		GUIState guiState;
		SmoothFollow smoothFollow;

		void Start()
		{
			cam = GetComponent<Camera>();
			guiState = FindFirstObjectByType<GUIState>();
			smoothFollow = GetComponent<SmoothFollow>();
		}

		static bool Focused {
			get => Cursor.lockState == CursorLockMode.Locked;
			set {
				Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = value == false;
			}
		}

		void OnEnable() {
			if( focusOnEnable ) Focused = true;
		}

		void OnDisable() => Focused = false;

		void Update() {
			if(guiState.MouseOnGUI)
			{
				Focused = false;
				return;
			}
			
			// Input
			if( Focused )
				UpdateInput();
			else if(cam.enabled && Input.GetMouseButtonDown( 1 ) )
			{
				Focused = true;
				if(smoothFollow) smoothFollow.target = null;
			}

			// Physics
			velocity = Vector3.Lerp( velocity, Vector3.zero, dampingCoefficient * Time.deltaTime );
			transform.position += velocity * Time.deltaTime;
		}

		void UpdateInput() {
			// Position
			velocity += GetAccelerationVector() * Time.deltaTime;

			// Rotation
			Vector2 mouseDelta = lookSensitivity * new Vector2( Input.GetAxis( "Mouse X" ), -Input.GetAxis( "Mouse Y" ) );
			Quaternion rotation = transform.rotation;
			Quaternion horiz = Quaternion.AngleAxis( mouseDelta.x, Vector3.up );
			Quaternion vert = Quaternion.AngleAxis( mouseDelta.y, Vector3.right );
			transform.rotation = horiz * rotation * vert;

			// Leave cursor lock
			if( Input.GetMouseButtonUp( 1 ) )
				Focused = false;
		}

		Vector3 GetAccelerationVector() {
			Vector3 moveInput = default;

			void AddMovement( KeyCode key, Vector3 dir ) {
				if( Input.GetKey( key ) )
					moveInput += dir;
			}

			AddMovement( KeyCode.W, Vector3.forward );
			AddMovement( KeyCode.S, Vector3.back );
			AddMovement( KeyCode.D, Vector3.right );
			AddMovement( KeyCode.A, Vector3.left );
			AddMovement( KeyCode.E, Vector3.up );
			AddMovement( KeyCode.Q, Vector3.down );
			Vector3 direction = transform.TransformVector( moveInput.normalized );

			if( Input.GetKey( KeyCode.LeftShift ) )
				return direction * ( acceleration * accSprintMultiplier ); // "sprinting"
			return direction * acceleration; // "walking"
		}
	}
}