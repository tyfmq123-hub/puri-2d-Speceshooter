using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Player Bullet 01 (단일)")]
    public GameObject playerBullet01Prefab;
    public int playerBullet01Count = 30;

    [Header("Player Bullet 02 (L/M/R)")]
    public GameObject playerBullet02Prefab;
    public int playerBullet02Count = 15;

    [Header("Player Bullet 03 (L/M/R)")]
    public GameObject playerBullet03Prefab;
    public int playerBullet03Count = 15;

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

    private List<GameObject> playerBullet01Pool = new List<GameObject>();
    private List<GameObject> playerBullet02Pool = new List<GameObject>();
    private List<GameObject> playerBullet03Pool = new List<GameObject>();
    private List<GameObject> enemyBulletPool = new List<GameObject>();
    private List<GameObject> enemyAPool = new List<GameObject>();
    private List<GameObject> enemyBPool = new List<GameObject>();
    private List<GameObject> enemyCPool = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        InitPool(playerBullet01Pool, playerBullet01Prefab, playerBullet01Count);
        InitPool(playerBullet02Pool, playerBullet02Prefab, playerBullet02Count);
        InitPool(playerBullet03Pool, playerBullet03Prefab, playerBullet03Count);
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

    public GameObject GetPlayerBullet01() => GetFromPool(playerBullet01Pool);
    public GameObject GetPlayerBullet02() => GetFromPool(playerBullet02Pool);
    public GameObject GetPlayerBullet03() => GetFromPool(playerBullet03Pool);
    public GameObject GetEnemyBullet()    => GetFromPool(enemyBulletPool);
    public GameObject GetEnemyA()         => GetFromPool(enemyAPool);
    public GameObject GetEnemyB()         => GetFromPool(enemyBPool);
    public GameObject GetEnemyC()         => GetFromPool(enemyCPool);
}
