using UnityEngine;

public class WaypointWalker : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints;      // Drag your empty GameObjects (points) here
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float waypointThreshold = 0.1f;
    public bool loop = true;

    [Header("Animation")]
    public Animator animator;          // Drag your character's Animator component
    public string isWalkingParam = "IsWalking";

    private int currentIndex = 0;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // ignore height difference for rotation/movement

        float distance = direction.magnitude;

        if (distance > waypointThreshold)
        {
            // Move toward target
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;

            // Rotate smoothly to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (animator != null)
                animator.SetBool(isWalkingParam, true);
        }
        else
        {
            // Reached waypoint, move to next
            currentIndex++;

            if (currentIndex >= waypoints.Length)
            {
                if (loop)
                    currentIndex = 0;
                else
                {
                    currentIndex = waypoints.Length - 1;
                    if (animator != null)
                        animator.SetBool(isWalkingParam, false);
                }
            }
        }
    }
}