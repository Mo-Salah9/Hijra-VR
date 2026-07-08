using UnityEngine;

public class SaayWalker : MonoBehaviour
{
    [Header("النقاط")]
    public Transform[] points; // اسحب كل النقاط هنا بالترتيب

    [Header("الإعدادات")]
    public float speed = 1.5f;
    public int totalLaps = 7;
    public int startPointIndex = 0; // 0 الى 19 لتوزيع الشخصيات

    private int currentPoint = 0;
    private int lapCount = 0;
    private bool finished = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentPoint = startPointIndex % points.Length;
        transform.position = points[currentPoint].position;
    }

    void Update()
    {
        if (finished) return;

        Transform target = points[currentPoint];

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            float yAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, yAngle + 90f, 0);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentPoint++;

            if (currentPoint >= points.Length)
            {
                currentPoint = 0;
                lapCount++;

                if (lapCount >= totalLaps)
                {
                    finished = true;
                    animator.SetFloat("Speed", 0);
                    return;
                }
            }
        }

        animator.SetFloat("Speed", speed);
    }
}