using UnityEngine;
using System.Collections;

public class CharacterWalker : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float turnSpeed = 120f;
    public float waypointRadius = 0.3f;

    [Header("Idle")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Floor Snapping")]
    public float floorRaycastDistance = 2f;
    public LayerMask floorLayer;

    [Header("Waypoints")]
    public Transform[] waypoints;   // Assign in Inspector, or leave empty for random patrol

    private Animator animator;
    private int currentWaypointIndex;
    private Vector3 targetPosition;

    private enum State { Walking, Turning, Idle }
    private State state = State.Walking;

    // Animator parameter hashes
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIdle = Animator.StringToHash("Idle");

    void Start()
    {
        animator = GetComponent<Animator>();
        PickNextWaypoint();
    }

    void Update()
    {
        SnapToFloor();

        switch (state)
        {
            case State.Walking: UpdateWalking(); break;
            case State.Turning: UpdateTurning(); break;
                // Idle is handled by coroutine
        }
    }

    // ── Movement ──────────────────────────────────────────────

    void UpdateWalking()
    {
        Vector3 flatTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        Vector3 dir = flatTarget - transform.position;

        if (dir.magnitude <= waypointRadius)
        {
            // Reached waypoint
            float roll = Random.value;
            if (roll < 0.3f)
                StartCoroutine(DoIdle());
            else
                BeginTurning();
            return;
        }

        // Move forward
        transform.position += transform.forward * walkSpeed * Time.deltaTime;

        // Smoothly face the target
        Quaternion lookRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, turnSpeed * Time.deltaTime);

        SetAnimatorWalking(true);
    }

    void BeginTurning()
    {
        PickNextWaypoint();
        state = State.Turning;
    }

    void UpdateTurning()
    {
        Vector3 dir = new Vector3(targetPosition.x - transform.position.x, 0f,
                                  targetPosition.z - transform.position.z).normalized;
        if (dir == Vector3.zero) { state = State.Walking; return; }

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * 1.5f * Time.deltaTime);

        float angle = Quaternion.Angle(transform.rotation, targetRot);
        if (angle < 5f)
            state = State.Walking;
    }

    IEnumerator DoIdle()
    {
        state = State.Idle;
        SetAnimatorWalking(false);
        if (animator) animator.SetBool(HashIdle, true);

        yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

        if (animator) animator.SetBool(HashIdle, false);
        BeginTurning();
    }

    // ── Waypoints ─────────────────────────────────────────────

    void PickNextWaypoint()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            // Cycle through assigned waypoints
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            targetPosition = waypoints[currentWaypointIndex].position;
        }
        else
        {
            // Random patrol — WalkingManager sets bounds via SetPatrolBounds()
            targetPosition = patrolCenter + new Vector3(
                Random.Range(-patrolRange, patrolRange), 0,
                Random.Range(-patrolRange, patrolRange));
        }
    }

    // Called by WalkingManager for random patrol
    private Vector3 patrolCenter;
    private float patrolRange = 5f;

    public void SetPatrolBounds(Vector3 center, float range)
    {
        patrolCenter = center;
        patrolRange = range;
        PickNextWaypoint();
    }

    // ── Floor snapping ────────────────────────────────────────

    void SnapToFloor()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down,
                            out RaycastHit hit, floorRaycastDistance, floorLayer))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
    }

    // ── Animator helpers ──────────────────────────────────────

    void SetAnimatorWalking(bool walking)
    {
        if (animator == null) return;
        animator.SetFloat(HashSpeed, walking ? 1f : 0f);
    }
}