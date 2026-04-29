using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    private Vector2 direction;
    private Camera cachedCamera;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (cachedCamera == null) cachedCamera = Camera.main;

        if (cachedCamera != null)
        {
            Vector3 viewPos = cachedCamera.WorldToViewportPoint(transform.position);
            if (viewPos.y < 0f || viewPos.x < -0.1f || viewPos.x > 1.1f)
                ReturnToPool();
        }
        else if (transform.position.y < -20f)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
            player.TakeDamage(damage);

        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}
