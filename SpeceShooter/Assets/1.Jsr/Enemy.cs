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

    [Header("Enemy C Attack")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 1.2f;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;
    private float lastFireTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();

        SetHealthByType();
        lastFireTime = Time.time;

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.down * speed;
        }
    }

    private void Update()
    {
        TryFireEnemyBullet();
    }

    private void OnValidate()
    {
        SetHealthByType();
        if (fireInterval < 0.1f)
        {
            fireInterval = 0.1f;
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
            default:
                health = 50;
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
            Destroy(gameObject);
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
            Destroy(gameObject);
        }
        else
        {
            int damage = GetBulletDamage(collision.gameObject);
            if (damage > 0)
            {
                OnHit(damage);
                Destroy(collision.gameObject);
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
        if (enemyType != EnemyType.C)
        {
            return;
        }

        if (enemyBulletPrefab == null)
        {
            return;
        }

        if (Time.time - lastFireTime < fireInterval)
        {
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : Quaternion.identity;

        Instantiate(enemyBulletPrefab, spawnPosition, spawnRotation);
        lastFireTime = Time.time;
    }
}
