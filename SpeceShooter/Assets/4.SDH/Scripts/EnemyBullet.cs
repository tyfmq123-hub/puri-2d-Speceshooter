using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);

        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewPos.y < 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ImpactBulletManager.Instance.SpawnImpact(transform.position);
            Destroy(gameObject);
        }
    }
}
