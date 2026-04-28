using UnityEngine;
using System;

public class ImpactBulletManager : MonoBehaviour
{
    public static ImpactBulletManager Instance;

    // 대리자 - 충돌 데미지 발생 시 호출
    public Action<int> OnPlayerDamaged;
    public Action<int> OnEnemyDamaged;

    void Awake()
    {
        Instance = this;
    }

    // 적 총알이 player에 맞았을 때
    public void DamagePlayer(Player player, int damage)
    {
        if (player == null) return;

        player.life -= damage;
        OnPlayerDamaged?.Invoke(damage);

        if (player.life <= 0)
            Destroy(player.gameObject);
    }

    // player 총알이 적에 맞았을 때 (Enemy.cs의 OnHit 호출)
    public void DamageEnemy(Collider2D enemyCol, int damage)
    {
        if (enemyCol == null) return;

        enemyCol.SendMessage("OnHit", damage, SendMessageOptions.DontRequireReceiver);
        OnEnemyDamaged?.Invoke(damage);
    }
}
