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

            Vector3 vp = cachedCamera.WorldToViewportPoint(transform.position);
            bool outOfBounds = vp.y < -0.1f || vp.y > 1.3f
                            || vp.x < -0.3f || vp.x > 1.3f;

            if (outOfBounds)
            {
                PoolManager.Release(gameObject);
                yield break;
            }
        }
    }
}
