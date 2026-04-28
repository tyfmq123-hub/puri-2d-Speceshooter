using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ImpactBulletManager.Instance.SpawnImpact(transform.position);
            EnemyBulletManager.Instance.ReturnToPool(gameObject);
        }
        else if (other.CompareTag("Border"))
        {
            EnemyBulletManager.Instance.ReturnToPool(gameObject);
        }
    }
}
