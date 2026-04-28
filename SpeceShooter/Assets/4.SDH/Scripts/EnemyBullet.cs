using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewPos.y < 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ImpactBulletManager.Instance != null)
                ImpactBulletManager.Instance.SpawnImpact(transform.position);

            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.life -= damage;
                if (player.life <= 0)
                    Destroy(other.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
