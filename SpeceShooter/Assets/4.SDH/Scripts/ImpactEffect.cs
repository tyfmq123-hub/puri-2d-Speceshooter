using UnityEngine;
using System.Collections;

public class ImpactEffect : MonoBehaviour
{
    public float duration = 0.3f;

    void OnEnable()
    {
        StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        ImpactBulletManager.Instance.ReturnToPool(gameObject);
    }
}
