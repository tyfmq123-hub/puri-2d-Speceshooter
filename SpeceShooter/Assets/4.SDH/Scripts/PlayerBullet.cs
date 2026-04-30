using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    private Camera cachedCamera;
    private SpriteRenderer sr;
    private Sprite defaultSprite;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) defaultSprite = sr.sprite;
    }

    // 풀 반환 시 스프라이트를 기본값으로 복원
    void OnDisable()
    {
        if (sr != null && defaultSprite != null)
            sr.sprite = defaultSprite;
    }

    public void SetSprite(Sprite sprite)
    {
        if (sr != null && sprite != null)
            sr.sprite = sprite;
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime, Space.World);

        if (cachedCamera == null) cachedCamera = Camera.main;

        if (cachedCamera != null)
        {
            Vector3 viewPos = cachedCamera.WorldToViewportPoint(transform.position);
            if (viewPos.y > 1f)
                ReturnToPool();
        }
        else if (transform.position.y > 20f)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerBullet>() != null) return;
        if (!other.CompareTag("Enemy")) return;

        if (ImpactBulletManager.Instance != null)
            ImpactBulletManager.Instance.DamageEnemy(other, damage);
        else
            other.SendMessage("OnHit", damage, SendMessageOptions.DontRequireReceiver);

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
