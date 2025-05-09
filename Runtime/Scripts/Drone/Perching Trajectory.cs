using UnityEngine;

// Place the drone position a little bit far from the SAM (for example at (1310,4,1165)) position to make sure the drone do not crash before starting (Teleporting issue, will fix later)
// Controller Mode: Tracking Control  

public class PerchingTrajectory : MonoBehaviour
{
    [Header("Trajectory Parameters")]

    [Tooltip("The target position the drone is trying to reach (For ex RopeSegment_14).")]
    public Transform target;

    [Tooltip("Vertical offset above the target point where the drone will aim to perch avoiding water.")]
    public float heightOffset = 0.3f;

    [Tooltip("Total duration (in seconds) of the perching phase from start to contact.")]
    public float totalTime = 35f;

    [Tooltip("Shape parameter for how distance decreases over time; higher values make approach sharper.")]
    public float k = 3f;

    [Tooltip("Shape parameter for how pitch angle decays over time during approach.")]
    public float kd_alpha = 0.4f;

    [Tooltip("Initial velocity (in m/s) at which the drone begins its perching maneuver.")]
    public float initialVelocity = 4.5f;

    [Tooltip("Initial pitch angle (in radians) between the drone and the target direction (default: 45°).")]
    public float alpha0 = Mathf.PI / 4;

    [Header("Perch Velocity Control")]

    [Tooltip("Minimum forward velocity (in m/s) the drone maintains while approaching the perch.")]
    public float minPerchVelocity = 0.5f;

    [Header("Departure Parameters")]

    [Tooltip("Time (in seconds) the drone travels straight forward at perch height before climbing.")]
    public float flatDuration = 3f;

    [Tooltip("Climb-out angle (in degrees) after perching is complete.")]
    public float exitAngleDeg = 30f;

    [Tooltip("Speed (in m/s) while traveling flat before beginning the climb.")]
    public float flatSpeed = 0.5f;

    [Tooltip("Climb-out speed (in m/s) as the drone ascends after flat movement.")]
    public float exitSpeed = 2.5f;


    private Vector3 p0;                       // starting position
    private Vector3 raisedTarget;             // target + heightOffset up
    private Vector3 hUnit;                    // horizontal forward unit vector
    private Vector3 departureDir;             // unit vector of climb‐out direction
    private float startTime;                  // when this script began

    void Start()
    {
        p0 = transform.position;
        if (target == null)
        {
            Debug.LogError("Assign a target Transform for perching.");
            enabled = false;
            return;
        }

        // compute the “raised” perch point once
        raisedTarget = target.position + Vector3.up * heightOffset;

        // record when we started
        startTime = Time.time;

        // precompute horizontal approach direction (in XZ plane)
        Vector3 h = raisedTarget - p0;
        h.y = 0f;
        hUnit = h.normalized;

        // convert exit angle to radians
        float phiExit = exitAngleDeg * Mathf.Deg2Rad;

        // build the 3D climb‐out unit vector
        departureDir = new Vector3(
            hUnit.x * Mathf.Cos(phiExit),
            Mathf.Sin(phiExit),
            hUnit.z * Mathf.Cos(phiExit)
        );
    }

    void Update()
    {
        float elapsed = Time.time - startTime;

        if (elapsed <= totalTime)
        {
            // === Phase 1: Perching with minimum speed ===

            // 1) characteristic time to close straight‐line gap
            float dist0 = Vector3.Distance(raisedTarget, p0);
            float tau0 = dist0 / initialVelocity;

            // 2) original distance‐gap d_orig(t)
            float frac = 1f - elapsed / totalTime;
            float d_orig = initialVelocity * tau0 * Mathf.Pow(frac, 1f / k);

            // 3) compute fallback gap so speed never drops below minPerchVelocity
            float remainingTime = totalTime - elapsed;
            float fallbackDist = minPerchVelocity * remainingTime;

            // 4) enforce minimum approach speed
            float d_t = Mathf.Max(d_orig, fallbackDist);

            // 5) angular‐gap α(t)
            float alpha_t = alpha0 * Mathf.Pow(d_t / (initialVelocity * tau0), 1f / kd_alpha);

            // 6) interpolation weights
            float sin0 = Mathf.Sin(alpha0);
            float sin_t = Mathf.Sin(alpha_t);
            float M1 = (sin0 - sin_t) / sin0;
            float M2 = sin_t / sin0;

            // 7) vertical offset along Unity’s Y axis
            Vector3 M3 = new Vector3(0f, d_t * sin_t, 0f);

            // 8) final perching position
            Vector3 p_t = M1 * raisedTarget + M2 * p0 + M3;
            transform.position = p_t;
        }
        else if (elapsed <= totalTime + flatDuration)
        {
            // === Phase 2: Straight‐ahead at same height ===

            float deltaFlat = elapsed - totalTime;
            transform.position = raisedTarget + hUnit * flatSpeed * deltaFlat;
        }
        else
        {
            // === Phase 3: Climb‐out ===

            float deltaClimb = elapsed - (totalTime + flatDuration);
            // start point of climb‐out = end of flat run
            Vector3 flatEndPos = raisedTarget + hUnit * flatSpeed * flatDuration;
            transform.position = flatEndPos + departureDir * exitSpeed * deltaClimb;
        }
    }
}