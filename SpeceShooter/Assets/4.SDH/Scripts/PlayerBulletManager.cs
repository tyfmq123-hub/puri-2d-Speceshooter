using UnityEngine;
using System;

public class PlayerBulletManager : MonoBehaviour
{
    public static PlayerBulletManager Instance;

    public GameObject bulletPrefab;

    // 대리자 - 총알 발사 시 호출
    public Action<Vector2> OnBulletFired;

    void Awake()
    {
        Instance = this;
    }

    public void Fire(Vector2 position)
    {
        Instantiate(bulletPrefab, position, Quaternion.identity);
        OnBulletFired?.Invoke(position);
    }
}
