using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    public enum ItemType { None = 0, Coin, Boom, Power }

    public ItemType itemType;
    public float speed = 1f;

    private Coroutine moveCoroutine;
    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
    }

    void OnEnable()
    {
        moveCoroutine = null;
    }

    public void BeginMove()
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move());
    }

    public void StopMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }

    private IEnumerator Move()
    {
        while (true)
        {
            if (!cachedTransform) { moveCoroutine = null; yield break; }

            cachedTransform.Translate(Vector3.down * speed * Time.deltaTime);

            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(cachedTransform.position);
                if (vp.y < -0.1f) { ReturnToPool(); yield break; }
            }
            else if (cachedTransform.position.y < -20f)
            {
                ReturnToPool();
                yield break;
            }

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        StopMove();

        switch (ResolveItemType())
        {
            case ItemType.Coin:
                UIManager.Instance?.AddScore(100);
                break;
            case ItemType.Boom:
                UIManager.Instance?.AddBoom();
                UIManager.Instance?.AddScore(300);
                break;
            case ItemType.Power:
                if (Player.Instance != null)
                    Player.Instance.power = Mathf.Min(Player.Instance.power + 1, 4);
                UIManager.Instance?.AddScore(200);
                break;
        }

        ReturnToPool();
    }

    private ItemType ResolveItemType()
    {
        if (itemType != ItemType.None) return itemType;

        string n = gameObject.name.ToLowerInvariant();
        if (n.Contains("coin"))  return ItemType.Coin;
        if (n.Contains("boom"))  return ItemType.Boom;
        if (n.Contains("power")) return ItemType.Power;
        return ItemType.None;
    }

    private void ReturnToPool() => PoolManager.Release(gameObject);

    void OnDisable()
    {
        StopMove();
    }
}
