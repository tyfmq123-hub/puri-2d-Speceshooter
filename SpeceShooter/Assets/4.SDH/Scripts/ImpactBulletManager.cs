using UnityEngine;
using System.Collections.Generic;

public class ImpactBulletManager : MonoBehaviour
{
    public static ImpactBulletManager Instance;

    public GameObject impactPrefab;
    public int poolSize = 15;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(impactPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void SpawnImpact(Vector2 position)
    {
        GameObject impact = pool.Count > 0 ? pool.Dequeue() : Instantiate(impactPrefab);
        impact.transform.position = position;
        impact.SetActive(true);
    }

    public void ReturnToPool(GameObject impact)
    {
        impact.SetActive(false);
        pool.Enqueue(impact);
    }
}
