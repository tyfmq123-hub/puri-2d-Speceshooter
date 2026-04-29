using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Player Bullet")]
    public GameObject playerBulletPrefab;
    public int playerBulletCount = 30;

    [Header("Enemy Bullet")]
    public GameObject enemyBulletPrefab;
    public int enemyBulletCount = 30;

    [Header("Enemies")]
    public GameObject enemyAPrefab;
    public int enemyACount = 10;
    public GameObject enemyBPrefab;
    public int enemyBCount = 10;
    public GameObject enemyCPrefab;
    public int enemyCCount = 10;

    private List<GameObject> playerBulletPool = new List<GameObject>();
    private List<GameObject> enemyBulletPool = new List<GameObject>();
    private List<GameObject> enemyAPool = new List<GameObject>();
    private List<GameObject> enemyBPool = new List<GameObject>();
    private List<GameObject> enemyCPool = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        InitPool(playerBulletPool, playerBulletPrefab, playerBulletCount);
        InitPool(enemyBulletPool, enemyBulletPrefab, enemyBulletCount);
        InitEnemyPool(enemyAPool, enemyAPrefab, enemyACount);
        InitEnemyPool(enemyBPool, enemyBPrefab, enemyBCount);
        InitEnemyPool(enemyCPool, enemyCPrefab, enemyCCount);
    }

    void InitPool(List<GameObject> pool, GameObject prefab, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pool.Add(go);
        }
    }

    void InitEnemyPool(List<GameObject> pool, GameObject prefab, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            if (go.GetComponent<PooledEnemy>() == null)
                go.AddComponent<PooledEnemy>();
            go.SetActive(false);
            pool.Add(go);
        }
    }

    GameObject GetFromPool(List<GameObject> pool)
    {
        foreach (var go in pool)
        {
            if (go != null && !go.activeInHierarchy)
                return go;
        }
        return null;
    }

    public void ReturnToPool(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
    }

    public GameObject GetPlayerBullet() => GetFromPool(playerBulletPool);
    public GameObject GetEnemyBullet() => GetFromPool(enemyBulletPool);
    public GameObject GetEnemyA() => GetFromPool(enemyAPool);
    public GameObject GetEnemyB() => GetFromPool(enemyBPool);
    public GameObject GetEnemyC() => GetFromPool(enemyCPool);
}
