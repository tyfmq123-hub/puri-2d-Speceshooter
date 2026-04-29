using UnityEngine;

// PlayerBullet_02/03 의 부모 오브젝트에 붙는 스크립트 (이동 + 화면 이탈 반환)
public class PlayerBulletContainer : MonoBehaviour
{
    [HideInInspector] public float speed = 10f;

    void OnEnable()
    {
        // 이전에 맞아서 비활성화된 자식들 전부 재활성화
        foreach (Transform child in transform)
            child.gameObject.SetActive(true);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.y > 1.2f)
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnToPool(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
