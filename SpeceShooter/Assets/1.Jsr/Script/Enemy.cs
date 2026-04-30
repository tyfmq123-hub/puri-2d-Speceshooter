using UnityEngine;
using System.Collections;
using System.Reflection;

public class Enemy : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string BorderBulletTag = "BorderBullet";
    private const string EnemyCTag = "EnemyC";

    public enum EnemyType { A, B, C }

    [Header("Enemy Settings")]
    public EnemyType enemyType;
    public float speed = 2f;
    public int health;
    public Sprite[] sprites;
    [SerializeField] private float defaultMoveSpeed = 2f;
    [SerializeField] private float enemyASpeed = 2f;
    [SerializeField] private float enemyBSpeed = 2.5f;
    [SerializeField] private float enemyCSpeed = 1.6f;
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float rotationOffset = -90f;
    [SerializeField] private bool invertFacingForEnemyA = true;
    [SerializeField] private bool invertFacingForEnemyB = true;
    [SerializeField] private bool invertFacingForEnemyC = true;

    [Header("Enemy C Attack")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 1.2f;

    [Header("Player Collision Damage")]
    [SerializeField] private int collisionDamage = 1;

    [Header("Item Drop")]
    [SerializeField, Range(0f, 1f)] private float noDropChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float coinDropChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float powerDropChance = 0.3f;
    [SerializeField, Range(0f, 1f)] private float boomDropChance = 0.1f;
    [SerializeField] private GameObject coinItemPrefab;
    [SerializeField] private GameObject powerItemPrefab;
    [SerializeField] private GameObject boomItemPrefab;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;
    private float lastFireTime;
    private Vector2 moveDirection = Vector2.down;
    private bool isDead = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        EnsureFirePointReference();
    }

    private void OnEnable()
    {
        isDead = false;
        ApplySpeedByType();
        EnsureMoveSpeed();
        SetHealthByType();
        lastFireTime = Time.time;

        if (spriteRenderer != null && sprites != null && sprites.Length > 0)
            spriteRenderer.sprite = sprites[0];

        if (rigidBody != null)
            rigidBody.linearVelocity = moveDirection * speed;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        ApplyDownwardMovement();
        TryFireEnemyBullet();
    }

    private void OnValidate()
    {
        EnsureFirePointReference();
        ApplySpeedByType();
        EnsureMoveSpeed();
        SetHealthByType();
        if (fireInterval < 0.1f) fireInterval = 0.1f;
        NormalizeDropChances();
    }

    private void EnsureMoveSpeed()
    {
        if (defaultMoveSpeed < 0.1f) defaultMoveSpeed = 0.1f;
        if (speed <= 0f) speed = defaultMoveSpeed;
    }

    private void ApplySpeedByType()
    {
        switch (enemyType)
        {
            case EnemyType.A: speed = enemyASpeed; break;
            case EnemyType.B: speed = enemyBSpeed; break;
            case EnemyType.C: speed = enemyCSpeed; break;
        }
    }

    private void SetHealthByType()
    {
        switch (enemyType)
        {
            case EnemyType.A: health = 30;  break;
            case EnemyType.B: health = 100; break;
            case EnemyType.C: health = 150; break;
        }
    }

    private void ApplyDownwardMovement()
    {
        if (rigidBody != null)
            rigidBody.linearVelocity = moveDirection * speed;
        else
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    public void SetMoveDirection(Vector2 direction, bool alignRotation = false, float extraRotation = 0f)
    {
        if (direction.sqrMagnitude <= 0.0001f) { moveDirection = Vector2.down; return; }

        moveDirection = direction.normalized;

        if (alignRotation && rotateToMoveDirection)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg + rotationOffset;
            if (invertFacingForEnemyA && enemyType == EnemyType.A) angle += 180f;
            if (invertFacingForEnemyB && enemyType == EnemyType.B) angle += 180f;
            if (invertFacingForEnemyC && enemyType == EnemyType.C) angle += 180f;
            angle += extraRotation;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // Shared damage entrypoint used by PlayerBullet/ImpactBulletManager via SendMessage.
    public void OnHit(int damage)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            isDead = true;
            UIManager.Instance?.AddScoreByEnemyType(enemyType);
            TryDropItem();
            ReturnEnemyToPool();
            return;
        }

        if (spriteRenderer != null && sprites != null && sprites.Length > 1)
        {
            spriteRenderer.sprite = sprites[1];
            StartCoroutine(FlashSprite());
        }
    }

    private IEnumerator FlashSprite()
    {
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null && sprites != null && sprites.Length > 0)
            spriteRenderer.sprite = sprites[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(PlayerTag))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null && player.IsInvincible) return;
            DamagePlayer(collision.gameObject);
            ReturnEnemyToPool();
            return;
        }

        if (collision.CompareTag(BorderBulletTag))
        {
            ReturnEnemyToPool();
        }
        else
        {
            int damage = GetBulletDamage(collision.gameObject);
            if (damage > 0)
            {
                ApplyDamage(damage);
                ReturnBulletToPool(collision.gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(PlayerTag)) return;
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null && player.IsInvincible) return;
        DamagePlayer(collision.gameObject);
        ReturnEnemyToPool();
    }

    private void DamagePlayer(GameObject playerObject)
    {
        if (playerObject == null) return;
        Player playerComp = playerObject.GetComponent<Player>();
        if (playerComp == null) return;

        int appliedDamage = Mathf.Max(1, collisionDamage);
        if (UIManager.Instance != null)
            UIManager.Instance.HandlePlayerHit(playerObject, playerComp.GetRespawnPosition(), playerComp.RespawnDelay, appliedDamage);
        else if (ImpactBulletManager.Instance != null)
            ImpactBulletManager.Instance.DamagePlayer(playerComp, appliedDamage);
        else
            playerComp.life = Mathf.Max(0, playerComp.life - appliedDamage);
    }

    private int GetBulletDamage(GameObject bulletObject)
    {
        Component playerBulletComponent = bulletObject.GetComponent("PlayerBullet");
        if (playerBulletComponent != null)
        {
            FieldInfo f = playerBulletComponent.GetType().GetField("damage");
            if (f != null && f.FieldType == typeof(int))
                return (int)f.GetValue(playerBulletComponent);
        }

        Component bulletComponent = bulletObject.GetComponent("Bullet");
        if (bulletComponent == null) return 0;

        FieldInfo damageField = bulletComponent.GetType().GetField("damage");
        if (damageField != null && damageField.FieldType == typeof(int))
            return (int)damageField.GetValue(bulletComponent);

        FieldInfo typoField = bulletComponent.GetType().GetField("dmage");
        if (typoField != null && typoField.FieldType == typeof(int))
            return (int)typoField.GetValue(bulletComponent);

        return 0;
    }

    private void TryFireEnemyBullet()
    {
        if (!IsEnemyCShooter()) return;
        if (EnemyBulletManager.Instance != null) return;
        if (Time.time - lastFireTime < fireInterval) return;

        Vector2 spawnPosition = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;

        GameObject bulletObj = PoolManager.Instance != null
            ? PoolManager.Instance.GetEnemyBullet()
            : (enemyBulletPrefab != null ? Instantiate(enemyBulletPrefab, spawnPosition, Quaternion.identity) : null);

        if (bulletObj == null) return;

        bulletObj.transform.position = spawnPosition;
        EnemyBullet eb = bulletObj.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            GameObject playerObj = GameObject.FindWithTag(PlayerTag);
            Vector2 dir = playerObj != null
                ? ((Vector2)playerObj.transform.position - spawnPosition).normalized
                : Vector2.down;
            eb.SetDirection(dir);
        }
        bulletObj.SetActive(true);
        lastFireTime = Time.time;
    }

    private void EnsureFirePointReference()
    {
        if (!IsEnemyCShooter() || firePoint != null) return;

        Transform directMatch = transform.Find("FirePoint") ?? transform.Find("FierPoint");
        if (directMatch != null) { firePoint = directMatch; return; }

        foreach (Transform child in transform)
        {
            string n = child.name.ToLowerInvariant();
            if (n.Contains("fire") || n.Contains("fier")) { firePoint = child; return; }
        }
    }

    private bool IsEnemyCShooter()
    {
        return enemyType == EnemyType.C || gameObject.CompareTag(EnemyCTag);
    }

    private void ReturnEnemyToPool()
    {
        if (PoolManager.Instance != null) PoolManager.Instance.ReturnToPool(gameObject);
        else Destroy(gameObject);
    }

    private void ReturnBulletToPool(GameObject bullet)
    {
        if (PoolManager.Instance != null) PoolManager.Instance.ReturnToPool(bullet);
        else Destroy(bullet);
    }

    private void TryDropItem()
    {
        float roll = Random.value;
        float noDropThreshold = noDropChance;
        float coinThreshold = noDropThreshold + coinDropChance;
        float powerThreshold = coinThreshold + powerDropChance;

        if (roll < noDropThreshold)
        {
            return;
        }

        GameObject item = null;
        GameObject fallbackPrefab = null;

        if (roll < coinThreshold)
        {
            if (PoolManager.Instance != null) item = PoolManager.Instance.GetCoinItem();
            fallbackPrefab = coinItemPrefab;
        }
        else if (roll < powerThreshold)
        {
            if (PoolManager.Instance != null) item = PoolManager.Instance.GetPowerItem();
            fallbackPrefab = powerItemPrefab;
        }
        else
        {
            if (PoolManager.Instance != null) item = PoolManager.Instance.GetBoomItem();
            fallbackPrefab = boomItemPrefab;
        }

        if (item == null && fallbackPrefab != null)
        {
            item = Instantiate(fallbackPrefab, transform.position, Quaternion.identity);
        }

        if (item == null)
        {
            return;
        }

        item.transform.position = transform.position;
        item.SetActive(true);
        item.GetComponent<Item>()?.BeginMove();
    }

    private void NormalizeDropChances()
    {
        noDropChance = Mathf.Clamp01(noDropChance);
        coinDropChance = Mathf.Clamp01(coinDropChance);
        powerDropChance = Mathf.Clamp01(powerDropChance);
        boomDropChance = Mathf.Clamp01(boomDropChance);

        float total = noDropChance + coinDropChance + powerDropChance + boomDropChance;
        if (total <= 0.0001f)
        {
            noDropChance = 0.3f;
            coinDropChance = 0.3f;
            powerDropChance = 0.3f;
            boomDropChance = 0.1f;
            return;
        }

        noDropChance /= total;
        coinDropChance /= total;
        powerDropChance /= total;
        boomDropChance /= total;
    }
}
