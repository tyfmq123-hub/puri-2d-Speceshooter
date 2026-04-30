using UnityEngine;
using System.Collections;

public class BossMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float targetY = 2f;

    void OnEnable()
    {
        StartCoroutine(MoveRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator MoveRoutine()
    {
        while (transform.position.y > targetY)
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime);
            yield return null;
        }

        Vector3 pos = transform.position;
        pos.y = targetY;
        transform.position = pos;
    }
}
