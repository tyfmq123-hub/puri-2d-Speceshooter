using UnityEngine;
using System.Collections;
using System;

public class EnemyBulletManager : MonoBehaviour
{
    public static EnemyBulletManager Instance;

    public GameObject bulletPrefab;
    public float fireInterval = 2f;
    public float bulletSpread = 0.3f;
    [SerializeField] private string shooterTag = "EnemyC";

    // 대리자 - 적 총알 발사 시 호출
    public Action OnEnemyFired;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireInterval);

            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj == null) continue;

            if (string.IsNullOrEmpty(shooterTag))
            {
                continue;
            }

            GameObject[] enemies;
            try
            {
                enemies = GameObject.FindGameObjectsWithTag(shooterTag);
            }
            catch (UnityException)
            {
                continue;
            }

            foreach (GameObject enemy in enemies)
            {
                FireBullets(enemy.transform);
            }

            OnEnemyFired?.Invoke();
        }
    }

    private void FireBullets(Transform enemyTransform)
    {
        if (bulletPrefab == null || enemyTransform == null)
        {
            return;
        }

        Vector2 origin = GetFireOrigin(enemyTransform);

        // 적 발사 위치 기준으로 2발 spread 발사
        for (int i = 0; i < 2; i++)
        {
            float offsetX = (i == 0 ? -bulletSpread : bulletSpread);
            Vector2 spawnPos = new Vector2(origin.x + offsetX, origin.y);
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
    }

    private Vector2 GetFireOrigin(Transform enemyTransform)
    {
        Transform firePoint = enemyTransform.Find("FirePoint");
        if (firePoint == null)
        {
            firePoint = enemyTransform.Find("FierPoint");
        }

        if (firePoint == null)
        {
            foreach (Transform child in enemyTransform)
            {
                string nameLower = child.name.ToLowerInvariant();
                if (nameLower.Contains("fire") || nameLower.Contains("fier"))
                {
                    firePoint = child;
                    break;
                }
            }
        }

        return firePoint != null ? (Vector2)firePoint.position : (Vector2)enemyTransform.position;
    }
}
