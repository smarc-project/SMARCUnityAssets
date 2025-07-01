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
        var com = Utils.GetCenterOfMass(ab);

        if (com == Vector3.zero)
        {
            Debug.LogWarning("Center of Mass is zero!");
            enabled = false;
            return;
        }

        Gizmos.color = color;
        Gizmos.DrawSphere(com, radius);
        DrawArrow(com, com + Vector3.down * (radius * 2.5f));

    }

    private Vector3 GetCoP()
    {
        // no props _right now_ under the object, but likely the props are stored elsewhere, so we go to robot root and search from there
        var robot = Utils.FindParentWithTag(gameObject, "robot", false);
        if (robot == null)
        {
            Debug.LogWarning("No robot found with tag 'robot'.");
            return Vector3.zero;
        }

        // now find all the props under the robot
        var props = robot.GetComponentsInChildren<VehicleComponents.Actuators.Propeller>();
        if (props.Length == 0)
        {
            Debug.LogWarning("No props found under the robot.");
            return Vector3.zero;
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
        return cop;
    }

    private void DrawCop()
    {
        
        var cop = GetCoP();
        if (cop == Vector3.zero)
        {
            Debug.LogWarning("Center of Props is zero!");
            enabled = false;
            return;
        }
        Gizmos.DrawSphere(cop, radius);
        DrawArrow(cop, cop + Vector3.up * (radius * 2.5f));
    }

    private void OnDrawGizmos()
    {
        if (gizmoType == GizmoType.Position) DrawPosition();
        if (gizmoType == GizmoType.CenterOfMass) DrawCom();
        if (gizmoType == GizmoType.CenterOfProps) DrawCop();
    }

    public void CreateObject()
    {
        GameObject obj = new GameObject($"SimpleGizmo_{gizmoType}");
        Vector3 pt;
        switch (gizmoType)
        {
            case GizmoType.Position:
                pt = transform.position;
                break;
            case GizmoType.CenterOfMass:
                if (!TryGetComponent<ArticulationBody>(out var ab))
                {
                    Debug.LogWarning("No ArticulationBody found on this GameObject.");
                    return;
                }
                pt = Utils.GetCenterOfMass(ab);
                if (pt == Vector3.zero)
                {
                    Debug.LogWarning("Center of Mass is zero!");
                    return;
                }
                break;
            case GizmoType.CenterOfProps:
                pt = GetCoP();
                if (pt == Vector3.zero)
                {
                    Debug.LogWarning("Center of Props is zero!");
                    return;
                }
                break;
            default:
                return;
        }
        obj.transform.parent = transform;
        obj.transform.position = pt;
    }
}