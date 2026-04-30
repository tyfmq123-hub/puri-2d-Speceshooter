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
        transform.Translate(Vector2.up * speed * Time.deltaTime, Space.World);

        // 자식이 모두 비활성(적 피격)이면 즉시 풀 반환
        if (AllChildrenInactive())
        {
            ReturnSelf();
            return;
        }

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

    private bool AllChildrenInactive()
    {
        if (transform.childCount == 0) return false;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf) return false;
        }
        return true;
    }

    private void ReturnSelf() => PoolManager.Release(gameObject);
}
