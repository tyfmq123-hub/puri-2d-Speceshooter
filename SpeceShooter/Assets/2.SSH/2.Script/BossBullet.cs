using UnityEngine;

public class BossBullet : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("Boss Bullet Settings")]
    public float speed = 7f;
    [SerializeField] private int damage = 1;

    private Vector2 direction = Vector2.down;
    private Camera cachedCamera;
    private Rigidbody2D rb;
    private bool useRigidbody;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        useRigidbody = false;
        direction = Vector2.down;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.down;
            return;
        }

        direction = dir.normalized;
    }

    public void SetForceDirection(Vector2 dir, float force)
    {
        useRigidbody = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if (!useRigidbody)
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        if (cachedCamera == null)
        {
            cachedCamera = Camera.main;
        }

        if (cachedCamera != null)
        {
            Vector3 viewPos = cachedCamera.WorldToViewportPoint(transform.position);
            if (viewPos.y < -0.1f || viewPos.y > 1.1f || viewPos.x < -0.1f || viewPos.x > 1.1f)
            {
                ReturnToPool();
            }
        }
        else if (transform.position.y < -20f || transform.position.y > 20f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag))
        {
            return;
        }

        Player player = other.GetComponent<Player>();
        if (player != null && player.IsInvincible)
        {
            return;
        }

        if (player != null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HandlePlayerHit(other.gameObject, player.GetRespawnPosition(), player.RespawnDelay, damage);
            }
            else
            {
                player.TakeDamage(damage);
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        useRigidbody = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
