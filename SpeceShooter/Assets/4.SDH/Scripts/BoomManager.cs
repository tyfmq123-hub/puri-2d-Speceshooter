using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoomManager : MonoBehaviour
{
    public static BoomManager Instance { get; private set; }

    [Header("Boom Effect")]
    public GameObject boomAnimationPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update() { }

    public void UseBoom()
    {
        UIManager.Instance?.UseBoom();
        StartCoroutine(BoomRoutine());
    }

    private IEnumerator BoomRoutine()
    {
        // 애니메이션 (없어도 실행)
        if (boomAnimationPrefab != null)
        {
            Vector3 pos = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
            Destroy(Instantiate(boomAnimationPrefab, pos, Quaternion.identity), 2f);
        }

        // 씬의 모든 적 처리 - OnHit(9999)으로 일반 사망 플로우 실행 (아이템 드랍 포함)
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            enemy.gameObject.SendMessage("OnHit", 9999, SendMessageOptions.DontRequireReceiver);
        }

        // 씬의 모든 적 총알 제거
        foreach (var bullet in FindObjectsOfType<EnemyBullet>())
        {
            if (bullet == null || !bullet.gameObject.activeInHierarchy) continue;
            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnToPool(bullet.gameObject);
            else
                bullet.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(2f);
    }
}
