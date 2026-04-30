using UnityEngine;
using System.Collections;

public class PowerItem : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    private Camera cachedCamera;

    void OnEnable()
    {
        cachedCamera = Camera.main;
        StartCoroutine(MoveRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);

            if (cachedCamera == null) cachedCamera = Camera.main;

            if (cachedCamera != null)
            {
                if (cachedCamera.WorldToViewportPoint(transform.position).y < -0.1f)
                {
                    ReturnToPool();
                    yield break;
                }
            }
            else if (transform.position.y < -20f)
            {
                ReturnToPool();
                yield break;
            }

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
            player.power = Mathf.Min(player.power + 1, 4);

        UIManager.Instance?.AddScore(200);
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}
