using UnityEngine;

public class SimpleGizmo : MonoBehaviour
{
    [Header("Sphere Gizmo")]
    public Color color = Color.red;
    public float radius = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
    }
}