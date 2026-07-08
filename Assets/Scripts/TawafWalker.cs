using UnityEngine;

public class TawafWalker : MonoBehaviour
{
    [Header("الكعبة")]
    public Transform kaaba;

    [Header("الإعدادات")]
    public float radius = 5f;
    public float speed = 30f;
    public float startAngle = 0f;

    private float currentAngle;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentAngle = startAngle;
    }

    void Update()
    {
        // عكس عقارب الساعة
        currentAngle += speed * Time.deltaTime;

        float rad = currentAngle * Mathf.Deg2Rad;

        // حساب الموقع على الدائرة
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
        Vector3 targetPos = kaaba.position + offset;
        targetPos.y = -15.48f; // ثبّت الشخصية على هذا الارتفاع
        transform.position = targetPos;

        // اتجاه المشي (tangent) بدون أي offset
        Vector3 dir = new Vector3(Mathf.Sin(rad), 0, -Mathf.Cos(rad));
        // تدوير على Y فقط
        if (dir != Vector3.zero)
        {
            float yAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, yAngle - 90f, 0);
        }
        // تشغيل الأنيميشن
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }
}