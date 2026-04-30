using UnityEngine;

// PlayerBullet_02/03 의 L, M, R 자식에 붙는 스크립트
public class PlayerBulletChild : MonoBehaviour
{
    [HideInInspector] public int damage = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerBullet>() != null) return;
        if (other.GetComponent<PlayerBulletChild>() != null) return;
        if (other.GetComponent<PlayerBulletContainer>() != null) return;
        if (!other.CompareTag("Enemy")) return;

        ImpactBulletManager.Instance?.DamageEnemy(other, damage);
        PoolManager.Release(gameObject);
    }
}
