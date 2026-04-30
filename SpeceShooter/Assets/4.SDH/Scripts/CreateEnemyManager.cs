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
        // 0:A 1:B 2:C 중 랜덤
        int index = UnityEngine.Random.Range(0, 3);

        GameObject enemy = null;
        GameObject fallbackPrefab = index switch
        {
            0 => enemyAPrefab,
            1 => enemyBPrefab,
            _ => enemyCPrefab
        };

        if (PoolManager.Instance != null)
        {
            enemy = index switch
            {
                0 => PoolManager.Instance.GetEnemyA(),
                1 => PoolManager.Instance.GetEnemyB(),
                _ => PoolManager.Instance.GetEnemyC()
            };

            // Pool exists but couldn't provide an object (pool empty or unconfigured).
            if (enemy == null && fallbackPrefab != null)
            {
                enemy = Instantiate(fallbackPrefab);
            }
        }
        else
        {
            if (!warnedMissingPool)
            {
                Debug.LogWarning("PoolManager not found. CreateEnemyManager is using prefab instantiate fallback.");
                warnedMissingPool = true;
            }

            if (fallbackPrefab != null)
            {
                enemy = Instantiate(fallbackPrefab);
            }
        }

        if (enemy == null) return;

        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPos = GetSpawnPosition(spawnPoint);
        ConfigureEnemyMoveDirection(enemy, spawnPoint);
        enemy.transform.position = spawnPos;
        enemy.transform.SetParent(null);
        enemy.SetActive(true);
        OnEnemySpawned?.Invoke(enemy);
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int pointIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            return spawnPoints[pointIndex];
        }

        return null;
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
