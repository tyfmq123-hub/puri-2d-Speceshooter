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

        if (ImpactBulletManager.Instance != null)
        {
            ImpactBulletManager.Instance.DamageEnemy(other, damage);
        }
        else
        {
            // Fallback when manager object is missing in scene.
            other.SendMessage("OnHit", damage, SendMessageOptions.DontRequireReceiver);
        }
        PoolManager.Release(gameObject);
    }
}
