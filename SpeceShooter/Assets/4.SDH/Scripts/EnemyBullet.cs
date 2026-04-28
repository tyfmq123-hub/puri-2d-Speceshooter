using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    private Transform player;

    void OnEnable()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // player가 살아있으면 실시간으로 방향 업데이트
        if (player != null)
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(Vector2.down * speed * Time.deltaTime);
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewPos.y < 0f || viewPos.x < -0.1f || viewPos.x > 1.1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerComp = other.GetComponent<Player>();

            if (ImpactBulletManager.Instance != null)
            {
                ImpactBulletManager.Instance.DamagePlayer(playerComp, damage);
            }
            else if (playerComp != null)
            {
                // Fallback when manager is missing in scene.
                playerComp.life -= damage;
                if (playerComp.life <= 0)
                {
                    Destroy(playerComp.gameObject);
                }
            }

            Destroy(gameObject);
        }
    }
}
