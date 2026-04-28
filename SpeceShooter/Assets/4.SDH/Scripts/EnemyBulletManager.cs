using UnityEngine;
using System.Collections.Generic;

public class EnemyBulletManager : MonoBehaviour
{
    public static EnemyBulletManager Instance;

    public GameObject bulletPrefab;
    public int poolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void SpawnBullet(Vector2 position)
    {
        GameObject bullet = pool.Count > 0 ? pool.Dequeue() : Instantiate(bulletPrefab);
        bullet.transform.position = position;
        bullet.SetActive(true);
    }

    public void ReturnToPool(GameObject bullet)
    {
        bullet.SetActive(false);
        pool.Enqueue(bullet);
    }
}
