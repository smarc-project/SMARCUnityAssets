using DefaultNamespace;
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

    private void DrawArrow(Vector3 arrowStart, Vector3 arrowEnd)
    {
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
            DrawArrow(com, com + Vector3.down * (radius * 2.5f));
        }
        else
        {
            Debug.LogWarning("Total mass is zero, cannot draw center of mass.");
            enabled = false;
        }
    }

    private void DrawCop()
    {
        // no props _right now_ under the object, but likely the props are stored elsewhere, so we go to robot root and search from there
        var robot = Utils.FindParentWithTag(gameObject, "robot", false);
        if (robot == null)
        {
            Debug.LogWarning("No robot found with tag 'robot'.");
            enabled = false;
            return;
        }

        // now find all the props under the robot
        var props = robot.GetComponentsInChildren<VehicleComponents.Actuators.Propeller>();
        if (props.Length == 0)
        {
            Debug.LogWarning("No props found under the robot.");
            enabled = false;
            return;
        }

        // prop objects are link attachments, so we need their attachment points
        Vector3 cop = Vector3.zero;
        var validProps = 0;
        for (int i = 0; i < props.Length; i++)
        {
            var pt = Utils.FindDeepChildWithName(robot, props[i].linkName);
            if (pt == null) continue;
            cop += pt.transform.position;
            validProps++;
        }
        cop /= validProps;

        Gizmos.DrawSphere(cop, radius);
        DrawArrow(cop, cop + Vector3.up * (radius * 2.5f));
    }

    private void OnDrawGizmos()
    {
        if (gizmoType == GizmoType.Position) DrawPosition();
        if (gizmoType == GizmoType.CenterOfMass) DrawCom();
        if (gizmoType == GizmoType.CenterOfProps) DrawCop();
    }
}