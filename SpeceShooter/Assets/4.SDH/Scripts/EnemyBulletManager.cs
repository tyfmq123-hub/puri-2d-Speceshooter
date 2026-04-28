using UnityEngine;
using System.Collections;
using System;

public class EnemyBulletManager : MonoBehaviour
{
    public static EnemyBulletManager Instance;

    public GameObject bulletPrefab;
    public float fireInterval = 2f;
    public float bulletSpread = 0.3f;

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

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                FireBullets(enemy.transform.position, playerObj.transform.position);
            }

            OnEnemyFired?.Invoke();
        }
    }

    private void FireBullets(Vector2 enemyPos, Vector2 playerPos)
    {
        // 플레이어 X 기준으로 2발 spread 발사
        for (int i = 0; i < 2; i++)
        {
            float offsetX = (i == 0 ? -bulletSpread : bulletSpread);
            Vector2 spawnPos = new Vector2(playerPos.x + offsetX, enemyPos.y);
            Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
    }
}
