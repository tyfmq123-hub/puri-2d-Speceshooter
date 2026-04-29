using UnityEngine;

public class PooledEnemy : MonoBehaviour
{
    private int initialHealth;
    private Sprite initialSprite;

    void Awake()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy == null) return;

        initialHealth = enemy.health;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) initialSprite = sr.sprite;
    }

    void OnEnable()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.health = initialHealth;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && initialSprite != null)
            sr.sprite = initialSprite;
    }

    void OnDisable()
    {
        // Enemy.cs에서 예약된 Invoke(ReturnSprite 등) 취소
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null) enemy.CancelInvoke();
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        if (viewPos.y < -0.1f)
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnToPool(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
