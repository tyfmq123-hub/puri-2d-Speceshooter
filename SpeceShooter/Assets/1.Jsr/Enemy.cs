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
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();

        SetHealthByType();

        if (rigidBody != null)
        {
            rigidBody.linearVelocity = Vector2.down * speed;
        }
    }

    private void OnValidate()
    {
        SetHealthByType();
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
        else if (collision.CompareTag("PlayerBullet"))
        {
            int damage = GetBulletDamage(collision.gameObject);
            if (damage > 0)
            {
                OnHit(damage);
            }

            Destroy(collision.gameObject);
        }
    }

    private int GetBulletDamage(GameObject bulletObject)
    {
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
}
