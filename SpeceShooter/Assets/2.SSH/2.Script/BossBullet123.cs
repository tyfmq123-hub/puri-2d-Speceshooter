using UnityEngine;

public class BoosBullet123 : MonoBehaviour
{
    [Header("Boss Bullet Settings")]
    public float speed = 7f;
    [SerializeField] private int damage = 1;

    private Vector2 direction = Vector2.down;
    private Camera cachedCamera;
    private Rigidbody2D rb;        //#. Rigidbody2D 추가
    private bool useRigidbody = false; //#. AddForce 방식 여부

    public void SetDirection(Vector2 dir)
    {
        //#. SetDirection 방식 (FireForward, FireShot, FireArc 용)
        if (dir.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.down;
            return;
        }
        direction = dir.normalized;
    }

    public void SetForceDirection(Vector2 dir, float force)
    {
        //#. AddForce 방식 (FireAround 용)
        useRigidbody = true;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
    }

    private void Update()
    {
        //#. SetDirection 방식일 때만 직접 이동
        if (!useRigidbody)
            transform.Translate(direction * speed * Time.deltaTime, Space.World);

        //#. 카메라 캐싱
        if (cachedCamera == null)
            cachedCamera = Camera.main;

        //#. 화면 밖으로 나가면 삭제
        if (cachedCamera != null)
        {
            Vector3 viewPos = cachedCamera.WorldToViewportPoint(transform.position);
            if (viewPos.y < -0.1f || viewPos.y > 1.1f || viewPos.x < -0.1f || viewPos.x > 1.1f)
                ReturnToPool();
        }
        else if (transform.position.y < -20f || transform.position.y > 20f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player player = other.GetComponent<Player>();
        if (player != null && player.IsInvincible)
            return;

        if (player != null)
        {
            if (UIManager.Instance != null)
                UIManager.Instance.HandlePlayerHit(other.gameObject, player.GetRespawnPosition(), player.RespawnDelay, damage);
            else
                player.TakeDamage(damage);
        }

        ReturnToPool();
    }

    private void OnEnable()
    {
        useRigidbody = false;
        direction = Vector2.down;
    }

    private void ReturnToPool() => PoolManager.Release(gameObject);
    
}