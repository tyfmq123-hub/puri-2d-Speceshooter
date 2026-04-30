using UnityEngine;

public class BossController : MonoBehaviour
{
    //#. 보스 상태 종류
    enum BossState
    {
        Idle,    //#. 대기
        Attack,  //#. 공격
        Die      //#. 사망
    }

    //#. 현재 상태 (시작은 Idle)
    BossState currentState = BossState.Idle;

    //#. 이전 상태 (상태 변경 감지용)
    BossState prevState = BossState.Idle;

    //#. 컴포넌트
    private Animator anim;
    private Transform player;

    //#. 체력
    [Header("Health Settings")]
    public int maxHealth = 10;       //#. 최대 체력
    public int health;              //#. 현재 체력

    //#. 총알 프리팹
    [Header("Bullet Prefabs")]
    public GameObject bossBulletAPrefab;  //#. FireForward, FireArc 용
    public GameObject bossBulletBPrefab;  //#. FireAround, FireShot 용

    //#. 총알 속도
    [Header("Bullet Speed")]
    public float bulletASpeed = 5f;  //#. BulletA 속도
    public float bulletBSpeed = 5f;  //#. BulletB 속도

    //#. FireAround 설정
    [Header("FireAround Settings")]
    public int aroundBulletCountA = 50;  //#. 짝수 회차 총알 수 (밀도 조절)
    public int aroundBulletCountB = 40;  //#. 홀수 회차 총알 수 (밀도 조절)

    //#. 발사 딜레이
    [Header("Attack Settings")]
    public float attackInterval    = 2f;    //#. 첫 공격까지 대기 시간
    public float aroundDelay       = 0.7f;  //#. FireAround 반복 딜레이
    public float forwardDelay      = 2f;    //#. FireForward 반복 딜레이
    public float shotDelay         = 3.5f;  //#. FireShot 반복 딜레이
    public float arcDelay          = 0.15f; //#. FireArc 반복 딜레이
    public float nextPatternDelay  = 3f;    //#. 다음 패턴까지 대기 시간

    //#. 패턴 관련 변수
    private int curPatternCount = 0;                 //#. 현재 패턴 반복 횟수
    private int patternIndex    = 0;                 //#. 현재 패턴 인덱스
    private int[] maxPatternCount = { 3, 5, 4, 8 }; //#. 패턴별 최대 반복 횟수

    void Start()
    {
        //#. 체력 초기화
        health = maxHealth;

        //#. 컴포넌트 초기화
        anim = GetComponent<Animator>();

        //#. 플레이어 위치 찾기
        player = GameObject.FindWithTag("Player").transform;

        //#. 시작 상태 설정
        currentState = BossState.Idle;

        //#. 시작 로그
        Debug.Log($"[Boss] 등장 / 체력: {health} / 상태: {currentState}");

        //#. 첫 패턴 시작
        Invoke("Think", attackInterval);
    }

    void Update()
    {
        //#. 상태가 바뀔 때만 로그 출력
        if (currentState != prevState)
        {
            Debug.Log($"[Boss] 상태 변경: {prevState} → {currentState}");
            prevState = currentState;
        }

        //#. 상태머신
        switch (currentState)
        {
            case BossState.Idle:   Idle();   break;
            case BossState.Attack: break;    //#. 공격은 Invoke로 처리
            case BossState.Die:    Die();    break;
        }
    }

    //#. 외부에서 데미지 받을 때 호출
    public void TakeDamage(int damage)
    {
        //#. 이미 죽은 상태면 무시
        if (currentState == BossState.Die)
            return;

        //#. 체력 감소
        health -= damage;

        //#. 피격 로그
        Debug.Log($"[Boss] 피격 / 데미지: {damage} / 남은 체력: {health}");

        //#. 피격 애니메이션
        anim.Play("Boss_Hit");

        //#. 체력 0 이하면 사망 상태로 전환
        if (health <= 0)
        {
            health = 0;
            Debug.Log("[Boss] 체력 0 → 사망 상태로 전환");
            currentState = BossState.Die;
        }
    }

    void Idle()
    {
        //#. Idle 애니메이션 재생
        anim.Play("Boss_Idle");
    }

    void Think()
    {
        //#. 죽은 상태면 패턴 실행 안 함
        if (currentState == BossState.Die)
            return;

        //#. 패턴 카운트 초기화
        curPatternCount = 0;

        //#. 패턴 랜덤 선택 (0~3)
        patternIndex = Random.Range(0, 4);

        //#. 패턴 선택 로그
        string[] patternNames = { "FireAround", "FireForward", "FireShot", "FireArc" };
        Debug.Log($"[Boss] 패턴 선택: {patternNames[patternIndex]} (index: {patternIndex})");

        //#. 공격 상태로 전환
        currentState = BossState.Attack;

        //#. 선택된 패턴 실행
        switch (patternIndex)
        {
            case 0: FireAround();  break;
            case 1: FireForward(); break;
            case 2: FireShot();    break;
            case 3: FireArc();     break;
        }
    }

    void FireAround()
    {
        //#. 죽은 상태면 멈추기
        if (currentState == BossState.Die)
            return;

        //#. 회차 및 총알 수 로그
        int roundNum = (curPatternCount % 2 == 0) ? aroundBulletCountA : aroundBulletCountB;
        Debug.Log($"[Boss] FireAround / 회차: {curPatternCount} / 총알 수: {roundNum}");

        //#. 360도 균등하게 방향 계산
        for (int i = 0; i < roundNum; i++)
        {
            //#. 보스 중심 기준 균등 각도 계산
            float angle = Mathf.PI * 2 * i / roundNum;

            //#. 사방팔방 방향 벡터
            Vector2 dirVec = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            //#. 회전값 계산
            Vector3 rotVec = Vector3.forward * 360 * i / roundNum + Vector3.forward * 90;

            //#. 보스 중심에서 총알 생성
            GameObject bullet = Instantiate(bossBulletBPrefab, transform.position, Quaternion.identity);
            bullet.transform.Rotate(rotVec);
            bullet.SetActive(true);

            //#. AddForce 방식으로 방향 전달
            BoosBullet123 bb = bullet.GetComponent<BoosBullet123>();
            if (bb != null)
                bb.SetForceDirection(dirVec, bulletBSpeed);
        }

        //#. Pattern Counting
        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireAround", aroundDelay);   //#. 패턴 반복
        else
            Invoke("Think", nextPatternDelay);   //#. 다음 패턴으로
    }

    void FireForward()
    {
        //#. 죽은 상태면 멈추기
        if (currentState == BossState.Die)
            return;

        //#. 발사 로그
        Debug.Log($"[Boss] FireForward / 회차: {curPatternCount}");

        //#. 오른쪽 2발
        SpawnBulletA(transform.position + Vector3.right * 0.3f,  Vector2.down);
        SpawnBulletA(transform.position + Vector3.right * 0.45f, Vector2.down);

        //#. 왼쪽 2발
        SpawnBulletA(transform.position + Vector3.left * 0.3f,   Vector2.down);
        SpawnBulletA(transform.position + Vector3.left * 0.45f,  Vector2.down);

        //#. Pattern Counting
        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireForward", forwardDelay);  //#. 패턴 반복
        else
            Invoke("Think", nextPatternDelay);    //#. 다음 패턴으로
    }

    void FireShot()
    {
        //#. 죽은 상태면 멈추기
        if (currentState == BossState.Die)
            return;

        //#. 발사 로그
        Debug.Log($"[Boss] FireShot / 회차: {curPatternCount}");

        //#. 5발 랜덤 산탄 발사
        for (int i = 0; i < 5; i++)
        {
            //#. 플레이어 방향 계산
            Vector2 dir = player.position - transform.position;

            //#. 랜덤 오차 추가
            Vector2 ranVec = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 2f));
            dir += ranVec;

            //#. 총알 생성
            SpawnBulletB(transform.position, dir.normalized);
        }

        //#. Pattern Counting
        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireShot", shotDelay);       //#. 패턴 반복
        else
            Invoke("Think", nextPatternDelay);   //#. 다음 패턴으로
    }

    void FireArc()
    {
        //#. 죽은 상태면 멈추기
        if (currentState == BossState.Die)
            return;

        //#. 발사 로그
        Debug.Log($"[Boss] FireArc / 회차: {curPatternCount}");

        //#. 호형 방향 계산
        Vector2 dir = new Vector2(
            Mathf.Cos(Mathf.PI * 10 * curPatternCount / maxPatternCount[patternIndex]),
            Mathf.Sin(Mathf.PI * 10 * curPatternCount / maxPatternCount[patternIndex])
        );

        //#. 총알 생성
        SpawnBulletA(transform.position, dir.normalized);

        //#. Pattern Counting
        curPatternCount++;
        if (curPatternCount < maxPatternCount[patternIndex])
            Invoke("FireArc", arcDelay);         //#. 패턴 반복
        else
            Invoke("Think", nextPatternDelay);   //#. 다음 패턴으로
    }

    void SpawnBulletA(Vector3 pos, Vector2 dir)
    {
        //#. BulletBossA 생성 공통 함수

        //#. 방향에 맞게 회전값 계산
        float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, rotAngle);

        //#. 총알 생성
        GameObject bullet = Instantiate(bossBulletAPrefab, pos, rot);
        bullet.SetActive(true);

        //#. 속도 및 방향 전달
        BoosBullet123 bb = bullet.GetComponent<BoosBullet123>();
        if (bb != null)
        {
            bb.speed = bulletASpeed; //#. 속도 설정
            bb.SetDirection(dir);    //#. 방향 설정
        }
    }

    void SpawnBulletB(Vector3 pos, Vector2 dir)
    {
        //#. BulletBossB 생성 공통 함수

        //#. 방향에 맞게 회전값 계산
        float rotAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, rotAngle);

        //#. 총알 생성
        GameObject bullet = Instantiate(bossBulletBPrefab, pos, rot);
        bullet.SetActive(true);

        //#. 속도 및 방향 전달
        BoosBullet123 bb = bullet.GetComponent<BoosBullet123>();
        if (bb != null)
        {
            bb.speed = bulletBSpeed; //#. 속도 설정
            bb.SetDirection(dir);    //#. 방향 설정
        }
    }

    void Die()
    {
        //#. 사망 애니메이션 재생
        anim.Play("Boss_Hit");

        //#. 모든 Invoke 취소
        CancelInvoke();

        //#. 사망 로그
        Debug.Log("[Boss] 사망 / 점수 1000점 추가 / 2초 후 비활성화");

        //#. 보스 처치 점수 1000점 추가
        if (UIManager.Instance != null)
            UIManager.Instance.AddScore(1000);

        //#. 2초 후 비활성화
        Invoke("Deactivate", 2f);
    }

    void Deactivate()
    {
        //#. 비활성화 로그
        Debug.Log("[Boss] 비활성화");

        //#. 보스 오브젝트 비활성화
        gameObject.SetActive(false);
    }
}