using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
    // 아이템 종류
    public enum ItemType
    {
        None = 0,
        Coin,
        Boom,
        Power
    }

    public ItemType itemType;
    public float speed = 1f;

    // 코루틴 참조 저장 (나중에 멈출 때 사용)
    private Coroutine moveCoroutine;

    // transform 캐싱 (파괴된 후 접근 오류 방지)
    private Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;
    }

    // 외부에서 호출 : 이동 시작
    public void BeginMove()
    {
        if (moveCoroutine == null)
            moveCoroutine = StartCoroutine(Move());
    }

    // 외부에서 호출 : 이동 멈춤 (플레이어가 먹었을 때)
    public void StopMove()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }

    // 코루틴 = 매 프레임마다 조금씩 실행되는 함수
    // yield return null = 여기서 잠깐 멈추고 다음 프레임에 이어서 실행
    private IEnumerator Move()
    {
        while (this)
        {
            if (!cachedTransform)
            {
                moveCoroutine = null;
                yield break;
            }

            // 아래로 이동
            cachedTransform.Translate(Vector3.down * speed * Time.deltaTime);

            // AreaDrawer 경계 밖으로 나가면 삭제
            if (AreaDrawer.Instance != null && AreaDrawer.Instance.IsOutOfBounds(cachedTransform.position))
                break;

            yield return null;
        }

        moveCoroutine = null;
        Destroy(gameObject);
    }

    void OnDisable()
    {
        StopMove();
    }

    void OnDestroy()
    {
        moveCoroutine = null;
    }
}