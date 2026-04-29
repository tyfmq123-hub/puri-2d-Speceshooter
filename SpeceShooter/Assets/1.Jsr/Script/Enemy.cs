using UnityEngine;
using System.Reflection;

public class Enemy : MonoBehaviour
{
    public enum EnemyType
    {
        A,
        B,
        C
    }

    [Header("Enemy Settings")]
    public EnemyType enemyType;
    public float speed = 2f;
    public int health;
    public Sprite[] sprites;
    [SerializeField] private bool useHealthByType = false;
    [SerializeField] private float defaultMoveSpeed = 2f;

    [Header("Enemy C Attack")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 1.2f;

    [Header("Item Drop")]
    [SerializeField, Range(0f, 1f)] private float itemDropChance = 0.3f;
    [SerializeField] private GameObject boomItemPrefab;
    [SerializeField] private GameObject coinItemPrefab;
    [SerializeField] private GameObject powerItemPrefab;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;
    private float lastFireTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
        EnsureFirePointReference();
        EnsureMoveSpeed();

        if (useHealthByType)
        {
            SetHealthByType();
        }
        lastFireTime = Time.time;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.down * speed;
        }
    }

    private void Update()
    {
        ApplyDownwardMovement();
        TryFireEnemyBullet();
    }

    private void OnValidate()
    {
        EnsureFirePointReference();
        EnsureMoveSpeed();
        if (useHealthByType)
        {
            SetHealthByType();
        }
        if (fireInterval < 0.1f)
        {
            fireInterval = 0.1f;
        }
    }

    private void EnsureMoveSpeed()
    {
        if (defaultMoveSpeed < 0.1f)
        {
            defaultMoveSpeed = 0.1f;
        }

        if (speed <= 0f)
        {
            speed = defaultMoveSpeed;
        }
    }

    private void ApplyDownwardMovement()
    {
        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.down * speed;
        }
        else
        {
            transform.position += (Vector3)(Vector2.down * speed * Time.deltaTime);
        }
    }

    private void SetHealthByType()
    {
        switch (enemyType)
        {
            case EnemyType.A:
                health = 50;
                break;
            case EnemyType.B:
                health = 100;
                break;
            case EnemyType.C:
                health = 150;
                break;
        }
    }

    private void OnHit(int damage)
    {
        health -= damage;
        if (spriteRenderer != null && sprites != null && sprites.Length > 1)
        {
            spriteRenderer.sprite = sprites[1];
            Invoke(nameof(ReturnSprite), 0.1f);
        }

        if (health <= 0)
        {
            TryDropItem();
            ReturnEnemyToPool();
        }
    }

    private void ReturnSprite()
    {
        if (spriteRenderer != null && sprites != null && sprites.Length > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("BorderBullet"))
        {
            ReturnEnemyToPool();
        }
        else
        {
            int damage = GetBulletDamage(collision.gameObject);
            if (damage > 0)
            {
                OnHit(damage);
                ReturnBulletToPool(collision.gameObject);
            }
        }
    }

    private int GetBulletDamage(GameObject bulletObject)
    {
        Component playerBulletComponent = bulletObject.GetComponent("PlayerBullet");
        if (playerBulletComponent != null)
        {
            System.Type playerBulletType = playerBulletComponent.GetType();
            FieldInfo playerDamageField = playerBulletType.GetField("damage");
            if (playerDamageField != null && playerDamageField.FieldType == typeof(int))
            {
                return (int)playerDamageField.GetValue(playerBulletComponent);
            }
        }

        Component bulletComponent = bulletObject.GetComponent("Bullet");
        if (bulletComponent == null)
        {
            return 0;
        }

        System.Type bulletType = bulletComponent.GetType();

        FieldInfo damageField = bulletType.GetField("damage");
        if (damageField != null && damageField.FieldType == typeof(int))
        {
            return (int)damageField.GetValue(bulletComponent);
        }

        FieldInfo typoField = bulletType.GetField("dmage");
        if (typoField != null && typoField.FieldType == typeof(int))
        {
            return (int)typoField.GetValue(bulletComponent);
        }

        return 0;
    }

    private void TryFireEnemyBullet()
    {
        if (!IsEnemyCShooter()) return;

        // EnemyBulletManager가 있으면 발사 위임
        if (EnemyBulletManager.Instance != null) return;

        if (enemyBulletPrefab == null) return;
        if (Time.time - lastFireTime < fireInterval) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : Quaternion.identity;

        GameObject bulletObj = Instantiate(enemyBulletPrefab, spawnPosition, spawnRotation);
        EnemyBullet eb = bulletObj.GetComponent<EnemyBullet>();
        if (eb != null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            Vector2 dir = playerObj != null
                ? ((Vector2)playerObj.transform.position - (Vector2)spawnPosition).normalized
                : Vector2.down;
            eb.SetDirection(dir);
        }
        lastFireTime = Time.time;
    }

    private void EnsureFirePointReference()
    {
        if (!IsEnemyCShooter() || firePoint != null)
        {
            return;
        }

        Transform directMatch = transform.Find("FirePoint");
        if (directMatch == null)
        {
            directMatch = transform.Find("FierPoint");
        }
        if (directMatch != null)
        {
            firePoint = directMatch;
            return;
        }

        foreach (Transform child in transform)
        {
            string childName = child.name.ToLowerInvariant();
            if (childName.Contains("fire") || childName.Contains("fier"))
            {
                firePoint = child;
                return;
            }
        }
    }

    private bool IsEnemyCShooter()
    {
        return enemyType == EnemyType.C || gameObject.tag == "EnemyC";
    }

    private void ReturnEnemyToPool()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }

    private void ReturnBulletToPool(GameObject bullet)
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(bullet);
        else
            Destroy(bullet);
    }

    private void TryDropItem()
    {
        if (Random.value > itemDropChance)
        {
            return;
        }

        GameObject[] candidates = { boomItemPrefab, coinItemPrefab, powerItemPrefab };
        int validCount = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return;
        }

        int pick = Random.Range(0, validCount);
        int current = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] == null)
            {
                continue;
            }

            if (current == pick)
            {
                Instantiate(candidates[i], transform.position, Quaternion.identity);
                return;
            }

            current++;
        }
    }
}
