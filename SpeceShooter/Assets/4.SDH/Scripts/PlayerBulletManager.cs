using UnityEngine;
using System;

public class PlayerBulletManager : MonoBehaviour
{
    public static PlayerBulletManager Instance;

    [Header("Bullet Speed")]
    public float bulletSpeed = 10f;

    [Header("Multi-Bullet Formation")]
    public float spacing = 0.3f;
    public float midForwardOffset = 0.15f;

    [Header("Bullet 01 Damage")]
    public int bullet01Damage = 10;

    [Header("Bullet 02 Damages (L / M / R)")]
    public int bullet02DamageL = 10;
    public int bullet02DamageM = 15;
    public int bullet02DamageR = 10;

    [Header("Bullet 03 Damages (L / M / R)")]
    public int bullet03DamageL = 15;
    public int bullet03DamageM = 20;
    public int bullet03DamageR = 15;

    public Action<Vector2> OnBulletFired;

    void Awake()
    {
        Instance = this;
    }

    public void Fire(Vector2 position, int powerLevel)
    {
        if (PoolManager.Instance == null) return;

        if (powerLevel >= 3)
            FireMulti(position, 3);
        else if (powerLevel == 2)
            FireMulti(position, 2);
        else
            FireSingle(position);

        OnBulletFired?.Invoke(position);
    }

    private void FireSingle(Vector2 position)
    {
        GameObject bullet = PoolManager.Instance.GetPlayerBullet01();
        if (bullet == null) return;

        PlayerBullet pb = bullet.GetComponent<PlayerBullet>();
        if (pb != null)
        {
            pb.damage = bullet01Damage;
            pb.speed = bulletSpeed;
        }

        bullet.transform.position = position;
        bullet.SetActive(true);
    }

    private void FireMulti(Vector2 position, int level)
    {
        GameObject container = level == 2
            ? PoolManager.Instance.GetPlayerBullet02()
            : PoolManager.Instance.GetPlayerBullet03();

        if (container == null) return;

        PlayerBulletContainer pbc = container.GetComponent<PlayerBulletContainer>();
        if (pbc != null) pbc.speed = bulletSpeed;

        // 자식 위치 및 데미지 설정 (SetActive 전에 설정)
        SetupChild(container, "L", new Vector3(-spacing, 0f, 0f),
            level == 2 ? bullet02DamageL : bullet03DamageL);
        SetupChild(container, "M", new Vector3(0f, midForwardOffset, 0f),
            level == 2 ? bullet02DamageM : bullet03DamageM);
        SetupChild(container, "R", new Vector3(spacing, 0f, 0f),
            level == 2 ? bullet02DamageR : bullet03DamageR);

        container.transform.position = position;
        container.SetActive(true);
    }

    private void SetupChild(GameObject container, string childName, Vector3 localPos, int damage)
    {
        Transform child = container.transform.Find(childName);
        if (child == null) return;

        child.localPosition = localPos;

        PlayerBulletChild pbc = child.GetComponent<PlayerBulletChild>();
        if (pbc != null) pbc.damage = damage;
    }
}
