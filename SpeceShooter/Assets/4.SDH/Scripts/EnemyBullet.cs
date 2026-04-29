using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewPos.y < 0f || viewPos.x < -0.1f || viewPos.x > 1.1f)
            ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerComp = other.GetComponent<Player>();
            if (UIManager.Instance != null && playerComp != null)
            {
                UIManager.Instance.HandlePlayerHit(other.gameObject, playerComp.GetRespawnPosition(), playerComp.RespawnDelay, damage);
            }
            else if (ImpactBulletManager.Instance != null)
            {
                ImpactBulletManager.Instance.DamagePlayer(playerComp, damage);
            }
            else if (playerComp != null)
            {
                playerComp.life -= damage;
                if (playerComp.life <= 0)
                    Destroy(playerComp.gameObject);
            }

            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}
