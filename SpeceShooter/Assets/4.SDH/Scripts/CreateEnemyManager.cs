using UnityEngine;
using System.Collections;
using System;

public class CreateEnemyManager : MonoBehaviour
{
    public static CreateEnemyManager Instance;

    [Header("Enemy Prefabs")]
    public GameObject enemyAPrefab;
    public GameObject enemyBPrefab;
    public GameObject enemyCPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;

    // 대리자 - 적 생성 시 호출
    public Action<GameObject> OnEnemySpawned;

    private GameObject[] enemyPrefabs;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        enemyPrefabs = new GameObject[] { enemyAPrefab, enemyBPrefab, enemyCPrefab };
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
        int index = UnityEngine.Random.Range(0, enemyPrefabs.Length);
        if (enemyPrefabs[index] == null) return;

        // 화면 상단 랜덤 X 위치 (1920x1080 기준 viewport 사용)
        float randomX = UnityEngine.Random.Range(0.05f, 0.95f);
        Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(randomX, 1.1f, 0f));
        spawnPos.z = 0f;

        GameObject enemy = Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);
        OnEnemySpawned?.Invoke(enemy);
    }
}
