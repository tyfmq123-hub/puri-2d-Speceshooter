using System.Collections.Generic;
using UnityEngine;

public class PoolManager1 : MonoBehaviour
{
    public static PoolManager1 Instance { get; private set; }

    [Header("Player Bullet (모든 레벨 공통)")]
    public GameObject playerBullet01Prefab;
    public int playerBullet01Count = 3;

    [Header("Enemy Bullet")]
    public GameObject enemyBulletPrefab;
    public int enemyBulletCount = 3;

    [Header("Enemies")]
    public GameObject enemyAPrefab;
    public int enemyACount = 10;
    public GameObject enemyBPrefab;
    public int enemyBCount = 10;
    public GameObject enemyCPrefab;
    public int enemyCCount = 10;

    [Header("Boss")]
    public GameObject bossPrefab;
    public int bossCount = 1;

    [Header("Items")]
    public GameObject coinItemPrefab;
    public int coinItemCount = 5;
    public GameObject powerItemPrefab;
    public int powerItemCount = 5;
    public GameObject boomItemPrefab;
    public int boomItemCount = 3;

    private List<GameObject> playerBullet01Pool = new List<GameObject>();
    private List<GameObject> enemyBulletPool    = new List<GameObject>();
    private List<GameObject> enemyAPool         = new List<GameObject>();
    private List<GameObject> enemyBPool         = new List<GameObject>();
    private List<GameObject> enemyCPool         = new List<GameObject>();
    private List<GameObject> bossPool           = new List<GameObject>();
    private List<GameObject> coinItemPool       = new List<GameObject>();
    private List<GameObject> powerItemPool      = new List<GameObject>();
    private List<GameObject> boomItemPool       = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        InitPool(playerBullet01Pool, playerBullet01Prefab, playerBullet01Count);
        InitPool(enemyBulletPool,    enemyBulletPrefab,    enemyBulletCount);
        InitEnemyPool(enemyAPool, enemyAPrefab, enemyACount);
        InitEnemyPool(enemyBPool, enemyBPrefab, enemyBCount);
        InitEnemyPool(enemyCPool, enemyCPrefab, enemyCCount);
        InitEnemyPool(bossPool,   bossPrefab,   bossCount);
        InitPool(coinItemPool,  coinItemPrefab,  coinItemCount);
        InitPool(powerItemPool, powerItemPrefab, powerItemCount);
        InitPool(boomItemPool,  boomItemPrefab,  boomItemCount);
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

    GameObject GetFromPool(List<GameObject> pool, GameObject prefab)
    {
        foreach (var go in pool)
        {
            if (go != null && !go.activeInHierarchy)
                return go;
        }
        if (prefab == null) return null;
        var newGo = Instantiate(prefab, transform);
        newGo.SetActive(false);
        pool.Add(newGo);
        return newGo;
    }

    GameObject GetFromEnemyPool(List<GameObject> pool, GameObject prefab)
    {
        foreach (var go in pool)
        {
            if (go != null && !go.activeInHierarchy)
                return go;
        }
        if (prefab == null) return null;
        var newGo = Instantiate(prefab, transform);
        if (newGo.GetComponent<PooledEnemy>() == null)
            newGo.AddComponent<PooledEnemy>();
        newGo.SetActive(false);
        pool.Add(newGo);
        return newGo;
    }

    public void ReturnToPool(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
    }

    public GameObject GetPlayerBullet01() => GetFromPool(playerBullet01Pool, playerBullet01Prefab);
    public GameObject GetEnemyBullet()    => GetFromPool(enemyBulletPool,    enemyBulletPrefab);
    public GameObject GetEnemyA()         => GetFromEnemyPool(enemyAPool, enemyAPrefab);
    public GameObject GetEnemyB()         => GetFromEnemyPool(enemyBPool, enemyBPrefab);
    public GameObject GetEnemyC()         => GetFromEnemyPool(enemyCPool, enemyCPrefab);
    public GameObject GetBoss()           => GetFromEnemyPool(bossPool,   bossPrefab);
    public GameObject GetCoinItem()       => GetFromPool(coinItemPool,  coinItemPrefab);
    public GameObject GetPowerItem()      => GetFromPool(powerItemPool, powerItemPrefab);
    public GameObject GetBoomItem()       => GetFromPool(boomItemPool,  boomItemPrefab);
}
