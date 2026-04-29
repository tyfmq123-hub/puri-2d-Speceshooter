using UnityEngine;

public class PlayerBulletContainer : MonoBehaviour
{
    [HideInInspector] public float speed = 10f;

    private Camera cachedCamera;

    void OnEnable()
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        if (cachedCamera == null) cachedCamera = Camera.main;

        if (cachedCamera != null)
        {
            Vector3 vp = cachedCamera.WorldToViewportPoint(transform.position);
            if (vp.y > 1.2f)
                ReturnSelf();
        }
        else if (transform.position.y > 20f)
        {
            ReturnSelf();
        }
    }

    private void ReturnSelf()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.ReturnToPool(gameObject);
        else
            Destroy(gameObject);
    }
}
