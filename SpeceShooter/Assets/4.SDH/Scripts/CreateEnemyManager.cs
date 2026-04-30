using UnityEngine;
using System.Collections;
using System;

public class CreateEnemyManager : MonoBehaviour
{
    public static CreateEnemyManager Instance;

    [Header("Enemy Prefabs (Fallback when PoolManager is missing)")]
    public GameObject enemyAPrefab;
    public GameObject enemyBPrefab;
    public GameObject enemyCPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    [SerializeField] private Transform[] spawnPoints;

    public Action<GameObject> OnEnemySpawned;
    private bool warnedMissingPool;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CacheSpawnPointsFromChildren();
    }

    void Start()
    {
        if (DataManager.Instance?.StageData?.waves != null)
            StartCoroutine(WaveRoutine());
        else
            StartCoroutine(SpawnRoutine());
    }

    // DataManager 기반 웨이브 순차 스폰
    private IEnumerator WaveRoutine()
    {
        foreach (WaveData wave in DataManager.Instance.StageData.waves)
        {
            foreach (SpawnData spawn in wave.enemies)
            {
                yield return new WaitForSeconds(spawn.delay);
                int typeIndex = (int)spawn.enemyType;
                if (typeIndex == 3)
                    SpawnBoss();
                else
                    SpawnEnemyAtPoint(typeIndex, spawn.point);
            }
        }
    }

    // 랜덤 스폰 (DataManager 없을 때 fallback)
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemyAtPoint(UnityEngine.Random.Range(0, 3), -1);
        }
    }

    private void SpawnEnemyAtPoint(int typeIndex, int pointIndex)
    {
        GameObject enemy = GetEnemyFromPool(typeIndex);
        if (enemy == null) return;

        Transform spawnPoint = pointIndex >= 0 ? GetSpawnPointByIndex(pointIndex) : GetRandomSpawnPoint();
        Vector3 spawnPos = GetSpawnPosition(spawnPoint);
        ConfigureEnemyMoveDirection(enemy, spawnPoint);
        enemy.transform.position = spawnPos;
        enemy.transform.SetParent(null);
        enemy.SetActive(true);
        OnEnemySpawned?.Invoke(enemy);
    }

    private void SpawnBoss()
    {
        if (PoolManager.Instance == null) return;
        GameObject boss = PoolManager.Instance.GetEnemyD();
        if (boss == null) return;

        Camera cam = Camera.main;
        Vector3 spawnPos = cam != null
            ? cam.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, 0f))
            : new Vector3(0f, 10f, 0f);
        spawnPos.z = 0f;

        boss.transform.position = spawnPos;
        boss.transform.SetParent(null);
        boss.SetActive(true);
    }

    private GameObject GetEnemyFromPool(int typeIndex)
    {
        GameObject fallbackPrefab = typeIndex switch
        {
            0 => enemyAPrefab,
            1 => enemyBPrefab,
            _ => enemyCPrefab
        };

        GameObject enemy = null;

        if (PoolManager.Instance != null)
        {
            enemy = typeIndex switch
            {
                0 => PoolManager.Instance.GetEnemyA(),
                1 => PoolManager.Instance.GetEnemyB(),
                _ => PoolManager.Instance.GetEnemyC()
            };
            if (enemy == null && fallbackPrefab != null)
                enemy = Instantiate(fallbackPrefab);
        }
        else
        {
            if (!warnedMissingPool)
            {
                Debug.LogWarning("PoolManager not found. CreateEnemyManager is using prefab instantiate fallback.");
                warnedMissingPool = true;
            }
            if (fallbackPrefab != null)
                enemy = Instantiate(fallbackPrefab);
        }

        return enemy;
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        return null;
    }

    private Transform GetSpawnPointByIndex(int index)
    {
        if (spawnPoints != null && index >= 0 && index < spawnPoints.Length)
            return spawnPoints[index];
        return GetRandomSpawnPoint();
    }

    private Vector3 GetSpawnPosition(Transform spawnPoint)
    {
        if (spawnPoint != null)
        {
            Vector3 pointPos = spawnPoint.position;
            pointPos.z = 0f;
            return pointPos;
        }

        // Fallback when no spawn points are configured.
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float randomX = UnityEngine.Random.Range(0.05f, 0.95f);
            Vector3 cameraPos = mainCamera.ViewportToWorldPoint(new Vector3(randomX, 1.1f, 0f));
            cameraPos.z = 0f;
            return cameraPos;
        }

        return transform.position;
    }

    private void ConfigureEnemyMoveDirection(GameObject enemyObj, Transform spawnPoint)
    {
        Enemy enemy = enemyObj != null ? enemyObj.GetComponent<Enemy>() : null;
        if (enemy == null || spawnPoint == null)
        {
            return;
        }

        Vector2 moveDir = Vector2.down;
        bool alignRotation = false;
        float extraRotation = 0f;
        bool isDiagonalPoint = IsRightDiagonalSpawnPoint(spawnPoint.name) || IsLeftDiagonalSpawnPoint(spawnPoint.name);
        Transform endPoint = FindEndPoint(spawnPoint);

        if (!isDiagonalPoint)
        {
            // Spawn points 1~4: always move down without rotation.
            moveDir = Vector2.down;
            alignRotation = false;
            enemyObj.transform.rotation = Quaternion.identity;
            enemy.SetMoveDirection(moveDir, alignRotation, extraRotation);
            return;
        }

        // Spawn points 5~8: move toward EndPoint and look at that direction.
        if (endPoint != null)
        {
            Vector2 towardEndPoint = (Vector2)endPoint.position - (Vector2)spawnPoint.position;
            if (towardEndPoint.sqrMagnitude > 0.0001f)
            {
                moveDir = towardEndPoint.normalized;
                alignRotation = true;
                enemy.SetMoveDirection(moveDir, alignRotation, extraRotation);
                return;
            }
        }

        // Fallback when EndPoint is missing on 5~8.
        if (IsRightDiagonalSpawnPoint(spawnPoint.name))
        {
            moveDir = new Vector2(0.6f, -1f).normalized;
            alignRotation = true;
        }
        else if (IsLeftDiagonalSpawnPoint(spawnPoint.name))
        {
            moveDir = new Vector2(-0.6f, -1f).normalized;
            alignRotation = true;
        }

        enemy.SetMoveDirection(moveDir, alignRotation, extraRotation);
    }

    private bool IsRightDiagonalSpawnPoint(string pointName)
    {
        if (string.IsNullOrEmpty(pointName))
        {
            return false;
        }

        return pointName.Contains("5") || pointName.Contains("6");
    }

    private bool IsLeftDiagonalSpawnPoint(string pointName)
    {
        if (string.IsNullOrEmpty(pointName))
        {
            return false;
        }

        return pointName.Contains("7") || pointName.Contains("8");
    }

    /// <summary>
    /// Prefab may name the child "EndPoint", "EndPoint ", etc. Transform.Find("EndPoint") only matches exactly.
    /// </summary>
    private static Transform FindEndPoint(Transform spawnPoint)
    {
        if (spawnPoint == null || spawnPoint.childCount == 0)
        {
            return null;
        }

        for (int i = 0; i < spawnPoint.childCount; i++)
        {
            Transform child = spawnPoint.GetChild(i);
            if (string.Equals(child.name.Trim(), "EndPoint", System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }

        return null;
    }

    private void CacheSpawnPointsFromChildren()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return;
        }

        int childCount = transform.childCount;
        if (childCount == 0)
        {
            return;
        }

        spawnPoints = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            spawnPoints[i] = transform.GetChild(i);
        }
    }
}
