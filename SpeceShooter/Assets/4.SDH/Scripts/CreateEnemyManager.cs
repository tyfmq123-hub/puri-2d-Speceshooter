using UnityEngine;
using System.Collections;
using System;

public class CreateEnemyManager : MonoBehaviour
{
    public static CreateEnemyManager Instance;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;

    public Action<GameObject> OnEnemySpawned;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (PoolManager.Instance == null) return;

        // 0:A 1:B 2:C 중 랜덤
        int index = UnityEngine.Random.Range(0, 3);
        GameObject enemy = index switch
        {
            0 => PoolManager.Instance.GetEnemyA(),
            1 => PoolManager.Instance.GetEnemyB(),
            _ => PoolManager.Instance.GetEnemyC()
        };

        if (enemy == null) return;

        float randomX = UnityEngine.Random.Range(0.05f, 0.95f);
        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, 1.1f, 0f));
        spawnPos.z = 0f;

        enemy.transform.position = spawnPos;
        enemy.transform.SetParent(null);
        enemy.SetActive(true);

        OnEnemySpawned?.Invoke(enemy);
    }
}
