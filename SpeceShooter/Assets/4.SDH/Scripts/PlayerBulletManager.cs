using UnityEngine;
using System;

public class PlayerBulletManager : MonoBehaviour
{
    public static PlayerBulletManager Instance { get; private set; }

    [Header("Bullet Prefabs")]
    public GameObject playerBullet01Prefab;
    public GameObject playerBullet02Prefab;
    public GameObject playerBullet03Prefab;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("PlayerBulletManager");
            Instance = go.AddComponent<PlayerBulletManager>();
        }
    }

    // Player.power를 직접 읽어서 총알 결정
    // power 0 → PlayerBullet_01
    // power 1 (아이템 1개) → PlayerBullet_02
    // power 2+ (아이템 2개) → PlayerBullet_03
    public void Fire(Vector2 position)
    {
        int powerLevel = Player.Instance != null ? Mathf.Clamp(Player.Instance.power, 0, 2) : 0;

        if (powerLevel >= 2 && CanFireMulti(3))
            FireMulti(position, 3);
        else if (powerLevel >= 1 && CanFireMulti(2))
            FireMulti(position, 2);
        else
            FireSingle(position);

        OnBulletFired?.Invoke(position);
    }

    public void FireSingleAt(Vector2 position)
    {
        FireSingle(position);
    }

    private bool CanFireMulti(int level)
    {
        if (level == 2)
            return playerBullet02Prefab != null ||
                   (PoolManager.Instance != null && PoolManager.Instance.playerBullet02Prefab != null);
        if (level == 3)
            return playerBullet03Prefab != null ||
                   (PoolManager.Instance != null && PoolManager.Instance.playerBullet03Prefab != null);
        return false;
    }

    private void FireSingle(Vector2 position)
    {
        GameObject bullet = PoolManager.Instance != null
            ? PoolManager.Instance.GetPlayerBullet01()
            : null;

        // 풀이 비어있으면 직접 생성
        if (bullet == null && playerBullet01Prefab != null)
            bullet = Instantiate(playerBullet01Prefab, position, Quaternion.identity);

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
        GameObject container = null;

        if (PoolManager.Instance != null)
        {
            container = level == 2
                ? PoolManager.Instance.GetPlayerBullet02()
                : PoolManager.Instance.GetPlayerBullet03();
        }

        // 풀이 비어있으면 직접 생성
        if (container == null)
        {
            GameObject prefab = level == 2 ? playerBullet02Prefab : playerBullet03Prefab;
            if (prefab != null)
                container = Instantiate(prefab, position, Quaternion.identity);
        }

        if (container == null) return;

        PlayerBulletContainer pbc = container.GetComponent<PlayerBulletContainer>();
        if (pbc != null) pbc.speed = bulletSpeed;

        SetupChild(container, "L", new Vector3(-spacing, 0f, 0f),
            level == 2 ? bullet02DamageL : bullet03DamageL);
        SetupChild(container, "M", new Vector3(0f, midForwardOffset, 0f),
            level == 2 ? bullet02DamageM : bullet03DamageM);
        SetupChild(container, "R", new Vector3(spacing, 0f, 0f),
            level == 2 ? bullet02DamageR : bullet03DamageR);

        container.transform.position = position;
        container.SetActive(true);
    }

    private void SetupChild(GameObject container, string suffix, Vector3 localPos, int damage)
    {
        Transform found = null;
        foreach (Transform child in container.transform)
        {
            // "L" 또는 "_L" 로 끝나는 이름 모두 허용 (예: L, PlayerBullet_02_L)
            string upper = child.name.ToUpper();
            if (upper == suffix || upper.EndsWith("_" + suffix))
            {
                found = child;
                break;
            }
        }
        if (found == null) return;

        found.localPosition = localPos;

        PlayerBulletChild pbc = found.GetComponent<PlayerBulletChild>();
        if (pbc != null) pbc.damage = damage;
    }
}
