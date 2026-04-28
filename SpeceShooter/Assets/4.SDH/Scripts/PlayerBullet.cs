using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
        if (viewPos.y > 1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (ImpactBulletManager.Instance != null)
            {
                ImpactBulletManager.Instance.SpawnImpact(transform.position);
            }
            Destroy(gameObject);
        }
    }
}
