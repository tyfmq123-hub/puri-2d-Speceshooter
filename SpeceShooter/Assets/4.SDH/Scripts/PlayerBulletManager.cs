using UnityEngine;
using System;

public class PlayerBulletManager : MonoBehaviour
{
    public static PlayerBulletManager Instance { get; private set; }

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
    public int bullet03DamageL  = 15;
    public int bullet03DamageM  = 20;
    public int bullet03DamageR  = 15;

    [Header("Bullet 03 Source (PlayerBullet_03 프리팹 할당 → M 자식에서 파란 스프라이트 자동 추출)")]
    public GameObject playerBullet03Prefab;

    // 런타임에서 M 자식으로부터 자동 추출됨 (직접 할당도 가능)
    public Sprite bullet03CenterSprite;

    public Action<Vector2> OnBulletFired;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ExtractCenterSprite();
    }

    // PlayerBullet_03.prefab 의 M 자식 SpriteRenderer 에서 파란 스프라이트 추출
    private void ExtractCenterSprite()
    {
        if (bullet03CenterSprite != null) return;   // 이미 할당된 경우 스킵
        if (playerBullet03Prefab == null) return;

        foreach (Transform child in playerBullet03Prefab.transform)
        {
            string upper = child.name.ToUpper();
            if (upper == "M" || upper.EndsWith("_M"))
            {
                var sr = child.GetComponent<SpriteRenderer>();
                if (sr != null) { bullet03CenterSprite = sr.sprite; return; }
            }
        }
    }

    // power 0 → 1발 (기본)
    // power 1 → 3발 L/M/R (기본 스프라이트)
    // power 2 → 3발 L/M/R (좌·우 기본, 가운데 파란 스프라이트)
    public void Fire(Vector2 position)
    {
        int power = Player.Instance != null ? Mathf.Clamp(Player.Instance.power, 0, 2) : 0;

        switch (power)
        {
            case 2:  FireLevel3(position); break;
            case 1:  FireLevel2(position); break;
            default: SpawnBullet(position, bullet01Damage); break;
        }

        OnBulletFired?.Invoke(position);
    }

    public void FireSingleAt(Vector2 position) => SpawnBullet(position, bullet01Damage);

    // 2단계: 기본 총알 3발
    private void FireLevel2(Vector2 position)
    {
        SpawnBullet(position + new Vector2(-spacing, 0f),           bullet02DamageL);
        SpawnBullet(position + new Vector2(0f, midForwardOffset),   bullet02DamageM);
        SpawnBullet(position + new Vector2(spacing,  0f),           bullet02DamageR);
    }

    // 3단계: 좌·우 기본 총알, 가운데 파란 총알
    private void FireLevel3(Vector2 position)
    {
        SpawnBullet(position + new Vector2(-spacing, 0f),           bullet03DamageL);
        SpawnBullet(position + new Vector2(0f, midForwardOffset),   bullet03DamageM, bullet03CenterSprite);
        SpawnBullet(position + new Vector2(spacing,  0f),           bullet03DamageR);
    }

    private void SpawnBullet(Vector2 position, int damage, Sprite overrideSprite = null)
    {
        if (PoolManager.Instance == null) return;
        GameObject bullet = PoolManager.Instance.GetPlayerBullet01();
        if (bullet == null) return;

        PlayerBullet pb = bullet.GetComponent<PlayerBullet>();
        if (pb != null)
        {
            pb.damage = damage;
            pb.speed  = bulletSpeed;
            if (overrideSprite != null)
                pb.SetSprite(overrideSprite);
        }

        bullet.transform.position = position;
        bullet.SetActive(true);
    }
}
