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

    private List<GameObject> playerBulletPool = new List<GameObject>();
    private List<GameObject> enemyBulletPool = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        InitPool(playerBulletPool, playerBulletPrefab, playerBulletCount);
        InitPool(enemyBulletPool, enemyBulletPrefab, enemyBulletCount);
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

    GameObject GetFromPool(List<GameObject> pool)
    {
        foreach (var go in pool)
        {
            if (!go.activeInHierarchy)
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
}
