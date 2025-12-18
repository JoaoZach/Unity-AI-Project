using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.8f);

    void Awake()
    {
        // Make all colliders on this object and its children triggers so they don't apply physics forces
        var cols = GetComponentsInChildren<Collider2D>();
        foreach (var c in cols)
        {
            if (c != null)
                c.isTrigger = true;
        }

        // Ensure there is at least one Rigidbody2D and make all present kinematic
        var rbs = GetComponentsInChildren<Rigidbody2D>();
        if (rbs.Length == 0)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            foreach (var rb in rbs)
            {
                if (rb != null)
                    rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            GetComponent<Enemy>()?.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            if (col is CircleCollider2D circle)
            {
                Vector3 center = transform.TransformPoint(circle.offset);
                float radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                Gizmos.DrawWireSphere(center, radius);
                return;
            }

            if (col is BoxCollider2D box)
            {
                Vector3 center = transform.TransformPoint(box.offset);
                Vector3 size = Vector3.Scale(box.size, transform.lossyScale);
                Gizmos.DrawWireCube(center, size);
                return;
            }

            if (col is PolygonCollider2D poly)
            {
                for (int p = 0; p < poly.pathCount; p++)
                {
                    var points = poly.GetPath(p);
                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector3 a = transform.TransformPoint(points[i]);
                        Vector3 b = transform.TransformPoint(points[(i + 1) % points.Length]);
                        Gizmos.DrawLine(a, b);
                    }
                }
                return;
            }
        }

        // fallback: draw sprite bounds if no collider
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Bounds b = sr.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}