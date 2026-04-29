# 코딩 규칙 및 게임 설계 기준

이 문서는 puri-2d-Speceshooter 프로젝트에서 공통으로 적용되는 코딩 패턴과 게임 수치 기준을 정리한 것입니다.
각 팀원은 자신의 폴더 안 스크립트를 작성할 때 이 규칙을 따라주세요.

---

## 1. 필수 적용 패턴 3가지

### 1-1. 싱글톤 (Singleton)

매니저 클래스는 반드시 아래 형태로 작성합니다.
중복 인스턴스가 생겨도 자동으로 제거됩니다.

```csharp
public class MyManager : MonoBehaviour
{
    public static MyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
}
```

**싱글톤 적용 대상:** PoolManager, UIManager, PlayerBulletManager, BoomManager,
ImpactBulletManager, EnemyBulletManager, CreateEnemyManager, GameMain, Player

---

### 1-2. 오브젝트 풀링 (Object Pooling)

Instantiate / Destroy 를 직접 사용하지 않습니다.
PoolManager 를 통해 오브젝트를 빌리고 반납합니다.

**가져오기**
```csharp
GameObject obj = PoolManager.Instance.GetPlayerBullet01();
obj.transform.position = spawnPos;
obj.SetActive(true);
```

**반납하기**
```csharp
PoolManager.Instance.ReturnToPool(gameObject);
```

**풀 목록 (PoolManager 에 등록된 항목)**

| 메서드 | 대상 |
|---|---|
| `GetPlayerBullet01()` | 플레이어 총알 (레벨 공통) |
| `GetEnemyBullet()` | 적 총알 |
| `GetEnemyA/B/C()` | 적 A/B/C |
| `GetCoinItem()` | 코인 아이템 |
| `GetPowerItem()` | 파워 아이템 |
| `GetBoomItem()` | 붐 아이템 |

**풀 소진 시 자동 확장** — 풀이 비어도 prefab 으로 새 인스턴스를 만들어 풀에 추가합니다.

---

### 1-3. 코루틴 (Coroutine)

`Update()` 안에서 타이머나 이동 로직을 처리하지 않습니다.
코루틴으로 분리해서 작성합니다.

**기본 패턴**
```csharp
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
    while (true)
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
        yield return null; // 매 프레임
    }
}
```

**일정 시간 후 처리**
```csharp
private IEnumerator FlashSprite()
{
    yield return new WaitForSeconds(0.1f);
    // 처리 내용
}
```

**반복 스폰/발사**
```csharp
private IEnumerator SpawnRoutine()
{
    while (true)
    {
        yield return new WaitForSeconds(interval);
        SpawnEnemy();
    }
}
```

`Invoke()` 는 사용하지 않습니다. 코루틴으로 대체합니다.

---

## 2. 풀링 오브젝트 수명 주기 규칙

풀에서 재사용되는 오브젝트는 `Awake` 가 한 번만 호출됩니다.
**상태 초기화는 반드시 `OnEnable` 에서** 합니다.

```csharp
void Awake()
{
    // 컴포넌트 캐싱만 (한 번만 실행)
    sr = GetComponent<SpriteRenderer>();
}

void OnEnable()
{
    // 상태 리셋 (풀에서 꺼낼 때마다 실행)
    isDead = false;
    health = maxHealth;
}

void OnDisable()
{
    // 코루틴 정리
    StopAllCoroutines();
}
```

---

## 3. 적(Enemy) 관련 규칙

### HP 기준

| 타입 | HP |
|---|---|
| Enemy A | 30 |
| Enemy B | 100 |
| Enemy C | 150 |

HP 는 코드(`SetHealthByType`)에서 자동 적용됩니다. Inspector 에서 직접 수정하지 않습니다.

### 이중 처리 방지 (isDead 플래그)

적이 같은 프레임에 총알 여러 발에 맞아도 아이템 드롭과 사망 처리가 한 번만 실행됩니다.

```csharp
private void OnHit(int damage)
{
    if (isDead) return; // 중복 방지

    health -= damage;
    if (health <= 0)
    {
        isDead = true;
        TryDropItem();
        ReturnEnemyToPool();
    }
}
```

### 아이템 드롭 확률

| 아이템 | 확률 |
|---|---|
| None | 30% |
| Coin | 30% |
| Power | 30% |
| Boom | 10% |

```csharp
int rand = Random.Range(0, 10);
if (rand < 3)      return;                              // None
else if (rand < 6) item = PoolManager.Instance.GetCoinItem();
else if (rand < 9) item = PoolManager.Instance.GetPowerItem();
else               item = PoolManager.Instance.GetBoomItem();
```

---

## 4. 플레이어(Player) 관련 규칙

### 파워 레벨

| Power | 총알 패턴 | 팔로워 수 |
|---|---|---|
| 0 | 기본 1발 (데미지 10) | 0 |
| 1 | L/M/R 3발 기본 (10/15/10) | 1 |
| 2 | L/R 기본 + M 파란 (15/20/15) | 2 |
| 3 | Power 2 와 동일 | 3 |
| 4 | Power 2 와 동일 + 팔로워 파란 총알 | 3 |

파워 최대치 = **4**

```csharp
player.power = Mathf.Min(player.power + 1, 4);
```

### 팔로워 총알 데미지

| 상태 | 총알 | 데미지 |
|---|---|---|
| Power 1~3 | 기본 총알 | 10 |
| Power 4 | 파란 총알 | 20 |

### 리스폰 무적

플레이어 리스폰 후 **2초** 동안 모든 데미지 무시, 적 몸통박치기도 무효.

```csharp
// 리스폰 직후 호출
player.ActivateRespawnInvincibility(2f);
```

무적 판정은 `Player.IsInvincible` 프로퍼티로 확인합니다.
적 충돌, 적 총알 양쪽에서 반드시 이 프로퍼티를 확인합니다.

```csharp
Player player = other.GetComponent<Player>();
if (player != null && player.IsInvincible) return;
```

---

## 5. 점수 기준

| 이벤트 | 점수 |
|---|---|
| Enemy A 처치 | 100 |
| Enemy B 처치 | 200 |
| Enemy C 처치 | 300 |
| Coin 획득 | 100 |
| Power 획득 | 200 |
| Boom 획득 | 300 |

---

## 6. 금지 사항

- `Invoke()` 사용 금지 → 코루틴으로 대체
- `Instantiate` / `Destroy` 직접 사용 금지 → PoolManager 사용
- 싱글톤에 중복 인스턴스 방지 없이 `Instance = this` 단독 사용 금지
- `Update()` 안에서 `Camera.main` 매 프레임 호출 금지 → 캐싱 사용
- `Debug.Log` 프로덕션 코드에 남기지 않기

---

## 7. Inspector 할당 필수 항목

| 오브젝트 | 필드 | 할당 대상 |
|---|---|---|
| PoolManager | Player Bullet 01 Prefab | PlayerBullet_01.prefab |
| PoolManager | Coin / Power / Boom Item Prefab | 각 아이템 프리팹 |
| PlayerBulletManager | Player Bullet 03 Prefab | PlayerBullet_03.prefab (파란 스프라이트 자동 추출) |

---

*작성 기준: 4.SDH 환경 씬 기준, 2026-04-30*
