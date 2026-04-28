using UnityEngine;
using System.Collections;
using System;

public class EnemyBulletManager : MonoBehaviour
{
    public static EnemyBulletManager Instance;

    public GameObject bulletPrefab;
    public float fireInterval = 1.5f;
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
                FireBullet(enemy.transform, playerObj.transform.position);
            }

            OnEnemyFired?.Invoke();
        }
    }

    private void FireBullet(Transform enemyTransform, Vector2 playerPos)
    {
        if (bulletPrefab == null || enemyTransform == null) return;

        Vector2 origin = GetFireOrigin(enemyTransform);
        Vector2 direction = (playerPos - origin).normalized;

        GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.identity);
        EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
        if (eb != null)
            eb.SetDirection(direction);
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
