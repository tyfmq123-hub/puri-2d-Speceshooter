using UnityEngine;
using System;

public class PlayerBulletManager : MonoBehaviour
{
    public static PlayerBulletManager Instance;

    public Action<Vector2> OnBulletFired;

    void Awake()
    {
        Instance = this;
    }

    public void Fire(Vector2 position)
    {
        if (PoolManager.Instance == null) return;

        GameObject bullet = PoolManager.Instance.GetPlayerBullet();
        if (bullet == null) return;

        bullet.transform.position = position;
        bullet.SetActive(true);

        OnBulletFired?.Invoke(position);
    }
}
