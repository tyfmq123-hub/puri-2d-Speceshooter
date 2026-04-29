using UnityEngine;

public class PowerItem : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    void Update()
    {
        transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.y < -0.1f)
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnToPool(gameObject);
            else
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
            player.power = Mathf.Min(player.power + 1, 3);

        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}
