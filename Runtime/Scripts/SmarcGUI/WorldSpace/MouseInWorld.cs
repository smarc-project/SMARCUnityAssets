using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class MouseInWorld : MonoBehaviour
    {
        private Camera mainCamera;

        void Update()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera not found");
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                transform.position = hit.point;
            }
        }
    }
}