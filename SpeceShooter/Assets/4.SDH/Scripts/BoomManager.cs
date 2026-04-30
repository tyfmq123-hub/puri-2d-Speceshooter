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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            UseBoom();
    }

    public void UseBoom()
    {
        if (UIManager.Instance == null || !UIManager.Instance.UseBoom()) return;
        StartCoroutine(BoomRoutine());
    }

    private IEnumerator BoomRoutine()
    {
        if (boomAnimationPrefab != null)
        {
            Vector3 pos = Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
            Destroy(Instantiate(boomAnimationPrefab, pos, Quaternion.identity), 2f);
        }

        // 2초 동안 0.1초마다 씬 전체 적·총알 제거 (도중에 스폰된 적도 처리)
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            foreach (var enemy in FindObjectsByType<Enemy>())
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                enemy.OnHit(9999);
            }

            foreach (var bullet in FindObjectsByType<EnemyBullet>())
            {
                if (bullet == null || !bullet.gameObject.activeInHierarchy) continue;
                PoolManager.Release(bullet.gameObject);
            }

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
