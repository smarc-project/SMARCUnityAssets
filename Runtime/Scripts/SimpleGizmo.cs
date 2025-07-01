using UnityEngine;


public enum GizmoType
{
    Position,
    CenterOfMass,
    CenterOfProps
}

public class SimpleGizmo : MonoBehaviour
{
    [Header("Gizmo Type")]
    public GizmoType gizmoType = GizmoType.Position;
    public float radius = 0.5f;
    public Color color = Color.red;

    private void DrawPosition()
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, radius);
        return;
    }

    private void DrawCom()
    {
        if (!TryGetComponent<ArticulationBody>(out var ab))
        {
            Debug.LogWarning("No ArticulationBody found on this GameObject.");
            enabled = false;
            return;
        }
        var com = (ab.transform.position + ab.centerOfMass) * ab.mass;
        var totalMass = ab.mass;

        var abs = GetComponentsInChildren<ArticulationBody>();
        foreach (var childAb in abs)
        {
            if (childAb == ab) continue; // Skip the root ArticulationBody
            com += (childAb.transform.position + childAb.centerOfMass) * childAb.mass;
            totalMass += childAb.mass;
        }

        if (totalMass > 0)
        {
            com /= totalMass;
            Gizmos.color = color;
            Gizmos.DrawSphere(com, radius);
            Vector3 arrowStart = com;
            Vector3 arrowEnd = com + Vector3.down * (radius * 2.5f);
            Gizmos.color = color;
            Gizmos.DrawLine(arrowStart, arrowEnd);

            float arrowHeadLength = radius * 0.7f;
            float arrowHeadAngle = 25f;

            // Arrowhead on XY plane
            Vector3 directionXY = Vector3.up;
            Vector3 rightXY = Quaternion.AngleAxis(arrowHeadAngle, Vector3.forward) * directionXY;
            Vector3 leftXY = Quaternion.AngleAxis(-arrowHeadAngle, Vector3.forward) * directionXY;
            Gizmos.DrawLine(arrowEnd, arrowEnd + rightXY.normalized * arrowHeadLength);
            Gizmos.DrawLine(arrowEnd, arrowEnd + leftXY.normalized * arrowHeadLength);

            // Arrowhead on YZ plane
            Vector3 rightYZ = Quaternion.AngleAxis(arrowHeadAngle, Vector3.right) * directionXY;
            Vector3 leftYZ = Quaternion.AngleAxis(-arrowHeadAngle, Vector3.right) * directionXY;
            Gizmos.DrawLine(arrowEnd, arrowEnd + rightYZ.normalized * arrowHeadLength);
            Gizmos.DrawLine(arrowEnd, arrowEnd + leftYZ.normalized * arrowHeadLength);
        }
        else
        {
            Debug.LogWarning("Total mass is zero, cannot draw center of mass.");
            enabled = false;
        }
    }

    private void DrawCop()
    {
        return;
    }

    private void OnDrawGizmos()
    {
        if (gizmoType == GizmoType.Position) DrawPosition();
        if (gizmoType == GizmoType.CenterOfMass) DrawCom();
        if (gizmoType == GizmoType.CenterOfProps) DrawCop();
    }
}