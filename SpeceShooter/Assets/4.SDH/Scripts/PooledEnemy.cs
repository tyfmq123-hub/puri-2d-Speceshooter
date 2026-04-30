using UnityEngine;
using System.Collections;

public class PooledEnemy : MonoBehaviour
{
    private Camera cachedCamera;

    void OnEnable()
    {
        cachedCamera = Camera.main;
        StartCoroutine(BoundsCheckRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator BoundsCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (cachedCamera == null) cachedCamera = Camera.main;
            if (cachedCamera == null) continue;

            if (cachedCamera.WorldToViewportPoint(transform.position).y < -0.1f)
            {
                if (PoolManager.Instance != null)
                    PoolManager.Instance.ReturnToPool(gameObject);
                else
                    Destroy(gameObject);
                yield break;
            }
        }
    }
}
