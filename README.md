# puri-2d-Speceshooter

Unity 2D 세로형 슈팅 게임 팀 프로젝트

---

## 팀 구성

| 폴더 | 담당자 |
|---|---|
| `1.Jsr/` | JSR — 적(Enemy), Boss |
| `2.SSH/` | SSH — UI, 배경, 아이템 |
| `3.JH/` | JH — 플레이어, 팔로워 |
| `4.SDH/` | SDH — 풀링, 총알, 붐, 매니저 |

---

## 작업 이력

---

### 2026-04-30 · JSR

#### 🔧 기능 수정

- **Enemy A 이동 방향 반전 플래그 추가**
  - `invertFacingForEnemyA` 필드 추가 — Enemy A가 이동 방향에 정렬될 때 +180° 반전 적용
  - 기존 `invertFacingForEnemyB`와 동일한 방식으로 동작

- **대각선 스폰 포인트 EndPoint 검색 버그 수정**
  - `Transform.Find("EndPoint")`가 이름에 공백이 포함된 경우 매칭 실패하는 문제 수정
  - `IsSpawnPoint7()` → `FindEndPoint()` 로 리팩터링: 이름 트리밍 + 대소문자 무시 검색
  - 스폰 포인트 5~7의 실제 이동 방향을 올바르게 인식하도록 개선

- **스폰 포인트 7 Enemy A 회전값 조정**
  - 스폰 포인트 7 진입 시 Enemy A의 이동 방향 회전값 조정

---

### 2026-04-30 · SDH

#### ✅ 기능 추가

- **파워 레벨 4단계 확장**
  - 기존 Power 0~3 → 0~4로 확장
  - Power 4: 팔로워가 파란 총알 발사 (데미지 20)

- **팔로워 공격 기능**
  - 팔로워가 플레이어 발사 시 동시에 총알 발사
  - Power 1~3: 기본 총알 (데미지 10) / Power 4: 파란 총알 (데미지 20)

- **리스폰 무적 시스템**
  - 플레이어 리스폰 후 2초간 모든 데미지 무효
  - 적 몸통박치기, 적 총알 양쪽 모두 무적 판정 적용
  - 무적 중 적 오브젝트가 사라지지 않음

- **아이템 드롭 확률 설정**
  - None 30% / Coin 30% / Power 30% / Boom 10%

- **아이템 획득 점수 추가**
  - Coin 획득 +100점 / Power 획득 +200점 / Boom 획득 +300점

- **오브젝트 풀링 — 아이템 확장**
  - Coin, Power, Boom 아이템을 PoolManager에서 관리
  - Instantiate/Destroy 없이 재사용

- **코딩 규칙 문서 추가**
  - `Assets/4.SDH/CODING_RULES.md` — 싱글톤/풀링/코루틴 패턴 및 수치 기준 정리

#### 🔧 기능 수정

- **Enemy HP 자동 적용**
  - A: 30 / B: 100 / C: 150
  - Inspector 직접 입력 방식 → 코드에서 타입별 자동 세팅으로 변경

- **Boom 애니메이션 수정**
  - `.anim` 파일에 스프라이트 키프레임이 없어 정지 이미지만 표시되던 문제 수정
  - Boom 0 → 1 → 2 프레임 순서로 0.2초 간격 루프 재생

- **Enemy 이중 처리 방지**
  - 같은 프레임에 총알 여러 발 충돌 시 아이템 2개 드롭·이중 점수 문제 수정
  - `isDead` 플래그로 사망 처리 1회만 실행 보장

- **Enemy 풀 재사용 버그 수정**
  - 풀에서 꺼낸 적 오브젝트가 이전 사망 상태(`isDead=true`, 음수 HP)로 재사용되는 문제 수정
  - `Awake` → `OnEnable`로 초기화 이동 (HP, isDead, 스프라이트 매번 리셋)

- **팔로워 공격 쿨다운 제거**
  - 기존 2초 쿨다운으로 팔로워가 실질적으로 공격 안 하는 문제 수정
  - `Fire()` 호출 시 즉시 발사하도록 변경

- **싱글톤 보호 추가**
  - `PoolManager`, `ImpactBulletManager`, `EnemyBulletManager`, `CreateEnemyManager`
  - 씬 재시작 또는 중복 오브젝트 발생 시 자동 제거

- **코루틴 전환**
  - `Enemy.Invoke()` → 코루틴 `FlashSprite()`
  - `Item.Update()` → 코루틴 `MoveRoutine()`
  - `PowerItem.Update()` → 코루틴 `MoveRoutine()`
  - `PooledEnemy.Update()` → 코루틴 `BoundsCheckRoutine()` (0.2초 간격)

- **BoomManager Z키 처리 이전**
  - `Player.HandleBoom()` → `BoomManager.Update()`에서 Z키 직접 감지 및 처리
  - Boom 효과가 2초 동안 0.1초 간격으로 반복 실행 — 애니메이션 도중 스폰된 적도 제거

- **Player 코드 정리**
  - 매 프레임 출력되던 `Debug.Log` 5곳 제거
  - `SyncFollowerCount()` — 파워 변화 없으면 즉시 리턴 (매 프레임 while/for 루프 방지)
  - 사용하지 않는 `deathLogged`, `previousState` 필드 및 `LogStateChanged()` 제거

- **UIManager 정리**
  - PoolManager 풀링 전환 후 사용하지 않는 `itemPrefabs` 필드 제거

#### ❌ 기능 삭제

- **PlayerBulletContainer 방식 제거**
  - L/M/R 총알을 하나의 컨테이너로 묶는 방식 → 각각 독립 `PlayerBullet`으로 분리
  - 기존 방식에서 플레이어 빠른 이동 시 총알끼리 충돌해 사라지는 문제 해결

- **Enemy Item Drop 프리팹 필드 제거**
  - `Enemy.cs`의 `[Item Drop]` 섹션 (boomItemPrefab, coinItemPrefab, powerItemPrefab) 제거
  - PoolManager에서 직접 아이템 관리

- **useHealthByType 토글 제거**
  - Inspector에서 체크 여부에 따라 HP가 적용 안 되는 혼란 제거
  - 항상 타입별 HP 자동 적용

- **팔로워 attackCooldown / isWaiting / FireRoutine 제거**
  - 2초 쿨다운 코루틴 방식 → 즉시 발사 방식으로 대체

---

## 기술 스택

- **엔진**: Unity 6
- **언어**: C#
- **렌더 파이프라인**: URP (Universal Render Pipeline)
- **물리**: Physics 2D
- **버전 관리**: Git / GitHub
