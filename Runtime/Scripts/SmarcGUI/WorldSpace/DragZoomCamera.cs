using UnityEngine;
using UnityEngine.InputSystem;

namespace SmarcGUI.WorldSpace
{
	[RequireComponent( typeof(Camera) )]
	public class DragZoomCamera : MonoBehaviour {
        Camera cam;
        GUIState guiState;

        public float panSpeed = 1f;

        InputAction dragAction;
        InputAction zoomAction;

        void Awake()
        {
            cam = GetComponent<Camera>();
            guiState = FindFirstObjectByType<GUIState>();

            dragAction = InputSystem.actions.FindAction("CameraControls/MoveCam");
            zoomAction = InputSystem.actions.FindAction("CameraControls/ZoomCam");
        }

        void LateUpdate()
        {
            if(guiState.MouseDragging) return;

            if(dragAction.triggered)
            {
                var mouseDelta = dragAction.ReadValue<Vector2>();
                var camOrthoSize = cam.orthographicSize;

                transform.Translate(-mouseDelta.x * camOrthoSize/1000, -mouseDelta.y * camOrthoSize/1000, 0);
            }

            
            var zoomDelta = zoomAction.ReadValue<Vector2>();
            cam.orthographicSize = Mathf.Max(1, cam.orthographicSize - zoomDelta.y * 0.5f);
            
        }

    }
}