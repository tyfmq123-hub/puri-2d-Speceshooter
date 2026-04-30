using UnityEngine;
using System.Collections;
using System;

public class EnemyBulletManager : MonoBehaviour
{
    public static EnemyBulletManager Instance;

    public float fireInterval = 1.5f;
    [SerializeField] private string shooterTag = "EnemyC";

    public Action OnEnemyFired;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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

            // 1.5초 후 위치 한 번 감지
            Vector2 playerPos = playerObj.transform.position;

            GameObject[] enemies;
            try
            {
                enemies = GameObject.FindGameObjectsWithTag(shooterTag);
            }
            catch (UnityException)
            {
                continue;
            }

            // 1발
            foreach (GameObject enemy in enemies)
                FireBullet(enemy.transform, playerPos);

            yield return new WaitForSeconds(0.2f);

            // 2발 (같은 감지 위치로)
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                    FireBullet(enemy.transform, playerPos);
            }

            OnEnemyFired?.Invoke();
        }
    }

    private void FireBullet(Transform enemyTransform, Vector2 playerPos)
    {
        if (PoolManager.Instance == null || enemyTransform == null) return;

        GameObject bullet = PoolManager.Instance.GetEnemyBullet();
        if (bullet == null) return;

        Vector2 origin = GetFireOrigin(enemyTransform);
        Vector2 direction = (playerPos - origin).normalized;

        bullet.transform.position = origin;
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
            eb.SetDirection(direction);

        bullet.SetActive(true);
    }

    private Vector2 GetFireOrigin(Transform enemyTransform)
    {
        Transform firePoint = enemyTransform.Find("FirePoint");
        if (firePoint == null) firePoint = enemyTransform.Find("FierPoint");

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
