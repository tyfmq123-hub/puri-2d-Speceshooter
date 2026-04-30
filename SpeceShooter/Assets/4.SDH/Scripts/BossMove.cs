using UnityEngine;
using System.Collections;

public class BossMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float attackStartDelay = 0.5f;

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
        // Inspector 미할당 시 씬에서 이름으로 찾기
        if (startPoint == null)
        {
            GameObject sp = GameObject.Find("Boss_StartPoint");
            if (sp != null) startPoint = sp.transform;
        }
        if (endPoint == null)
        {
            GameObject ep = GameObject.Find("Boss_EndPoint");
            if (ep != null) endPoint = ep.transform;
        }

        // 스타트 포인트로 즉시 이동
        if (startPoint != null)
        {
            Vector3 pos = startPoint.position;
            pos.z = 0f;
            transform.position = pos;
        }

        // BossController.Start()가 실행되도록 한 프레임 대기
        yield return null;

        // Start()가 예약한 Invoke("Think") 취소 — 이동 중 공격 방지
        BossController bc = GetComponent<BossController>();
        if (bc != null) bc.CancelInvoke();

        // 엔드 포인트까지 이동
        if (endPoint != null)
        {
            Vector3 target = endPoint.position;
            target.z = 0f;

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }

        // 도착 후 attackStartDelay 만큼 대기 후 공격 시작
        if (bc != null)
            bc.Invoke("Think", attackStartDelay);
    }
}
