using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    private PlayerBulletContainer parentContainer;
    private Camera cachedCamera;

    void OnEnable()
    {
        // 컨테이너 자식인지 매번 확인 (풀에서 꺼낼 때 부모가 바뀔 수 있음)
        parentContainer = GetComponentInParent<PlayerBulletContainer>();
    }

    void Update()
    {
        // 컨테이너 자식이면 부모가 이동/반환 처리 → 여기서는 아무것도 안 함
        if (parentContainer != null) return;

        transform.Translate(Vector2.up * speed * Time.deltaTime);

        if (cachedCamera == null) cachedCamera = Camera.main;

        if (cachedCamera != null)
        {
            Vector3 viewPos = cachedCamera.WorldToViewportPoint(transform.position);
            if (viewPos.y > 1f)
                ReturnToPool();
        }
        else if (transform.position.y > 20f)
        {
            // Camera.main 없는 씬 대비 위치 기반 폴백
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (parentContainer != null) return;
        if (other.GetComponent<PlayerBullet>() != null) return;
        if (other.GetComponent<PlayerBulletChild>() != null) return;
        if (other.GetComponent<PlayerBulletContainer>() != null) return;
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
