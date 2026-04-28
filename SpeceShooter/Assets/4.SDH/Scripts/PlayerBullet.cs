using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            ImpactBulletManager.Instance.SpawnImpact(transform.position);
            PlayerBulletManager.Instance.ReturnToPool(gameObject);
        }
        else if (other.CompareTag("Border"))
        {
            PlayerBulletManager.Instance.ReturnToPool(gameObject);
        }
    }
}
