using UnityEngine;
using System.Collections.Generic;

public class NPCPathFollower : MonoBehaviour
{
    public List<Transform> waypoints;
    public float speed = 2f;
    public float rotationSpeed = 5f;

    private int index = 0;

    void Update()
    {
        if (waypoints.Count == 0) return;

        Transform target = waypoints[index];

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;

        // Move
        transform.position += dir * speed * Time.deltaTime;

        // Rotate
        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }

        // Next waypoint
        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            index = (index + 1) % waypoints.Count;
        }
    }
}