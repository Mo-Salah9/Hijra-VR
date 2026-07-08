using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;
    public float waypointReachRadius = 0.2f;

    [Header("Loop")]
    public bool loop = true;
    public bool pingPong = false;

    [Header("Rotation Fix")]
    [Tooltip("If character faces wrong way, try 0, 90, 180, -90")]
    public float rotationOffset = 0f;

    [Header("Start Position")]
    [Tooltip("Which waypoint index this character starts from")]
    public int startWaypointIndex = 0;

    [Header("Animation")]
    [Tooltip("Name of the float parameter in your Animator")]
    public string animSpeedParam = "Speed";
    [Tooltip("Smooths the animation transition so it doesn't snap")]
    public float animDampTime = 0.1f;

    private int currentIndex = 0;
    private int direction = 1;
    private Animator animator;
    private Vector3 lastPosition;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (waypoints.Length == 0) { enabled = false; return; }

        currentIndex = Mathf.Clamp(startWaypointIndex, 0, waypoints.Length - 1);
        transform.position = waypoints[currentIndex].position;

        lastPosition = transform.position;
    }

    void Update()
    {
        if (waypoints.Length < 2) return;

        MoveToWaypoint();
        RotateTowardNextWaypoint();
        UpdateAnimation();

        // Check reached — flat distance only (ignore Y)
        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(waypoints[currentIndex].position.x, 0f, waypoints[currentIndex].position.z)
        );

        if (dist < waypointReachRadius)
            AdvanceWaypoint();
    }

    void MoveToWaypoint()
    {
        Vector3 targetPos = waypoints[currentIndex].position;
        targetPos.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }

    void RotateTowardNextWaypoint()
    {
        int nextIndex = GetNextIndex(currentIndex);
        Vector3 dir = waypoints[nextIndex].position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        targetRot *= Quaternion.Euler(0f, rotationOffset, 0f);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // ✅ Measure how far we ACTUALLY moved this frame
        float actualSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // ✅ Feed real speed into animator with smooth damping (no snapping)
        animator.SetFloat(animSpeedParam, actualSpeed, animDampTime, Time.deltaTime);
    }

    int GetNextIndex(int fromIndex)
    {
        if (pingPong)
            return Mathf.Clamp(fromIndex + direction, 0, waypoints.Length - 1);

        return (fromIndex + 1) % waypoints.Length;
    }

    void AdvanceWaypoint()
    {
        if (pingPong)
        {
            currentIndex += direction;
            if (currentIndex >= waypoints.Length - 1 || currentIndex <= 0)
                direction *= -1;
        }
        else if (loop)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
        else
        {
            currentIndex = Mathf.Min(currentIndex + 1, waypoints.Length - 1);
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.15f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }

        int safeStart = Mathf.Clamp(startWaypointIndex, 0, waypoints.Length - 1);
        if (waypoints[safeStart] != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(waypoints[safeStart].position, 0.3f);
        }

        if (Application.isPlaying && waypoints[currentIndex] != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[currentIndex].position, 0.25f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
    }
}