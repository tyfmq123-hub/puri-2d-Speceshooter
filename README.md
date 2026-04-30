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

### 2026-04-30 · 1.Jsr

#### ✅ 기능 추가

- **BoosBullet.cs 신규**
  - 보스 총알 이동·방향 제어 (`SetDirection()`)
  - 플레이어 충돌 감지 — 무적 여부 확인 후 데미지 처리
  - 화면 이탈 시 PoolManager 반환

- **Boos Bullet 1·2 프리팹** (`1.Jsr/Prefeb/`)
  - 보스 공격 패턴용 총알 프리팹 2종 신규 추가

#### 🔧 기능 수정

- **Enemy A·C 이동 방향 반전 적용** (`Enemy.cs`)
  - `invertFacingForEnemyA`, `invertFacingForEnemyC` 인스펙터 필드 추가
  - 이동 방향 정렬 시 +180° 반전 — 스프라이트 진행 방향 교정

- **대각선 스폰 EndPoint 검색 버그 수정** (`Enemy.cs`)
  - `Transform.Find("EndPoint")`가 공백 포함 자식 이름에서 실패하는 문제 수정
  - `FindEndPoint()` 리팩터링: 이름 트리밍 + 대소문자 무시 검색
  - 스폰 포인트 5~7 이동 방향 올바르게 인식

---

### 2026-04-30 · 2.SSH

#### ✅ 기능 추가

- **Boss.cs — BossController 신규**
  - 상태머신: Idle / Attack / Die
  - 공격 패턴 4종
    - `FireAround`: 360° 균등 탄막 (홀짝 회차별 총알 수 조절)
    - `FireForward`: 좌우 직진 각 2발
    - `FireShot`: 플레이어 방향 산탄 5발 (랜덤 오차 포함)
    - `FireArc`: 호형 탄막 8발
  - `TakeDamage()`, 사망 시 1000점 추가, 2초 후 비활성화

- **BossBullet123.cs 신규**
  - `SetDirection()`, `SetForceDirection()` 두 가지 이동 방식 제공
  - 화면 이탈 시 PoolManager 반환

#### 🔧 기능 수정

- **Item.cs — Update → 코루틴 전환**
  - `Update()` 이동 → `MoveRoutine()` 코루틴으로 전환

- **Boom-1.anim — 애니메이션 키프레임 수정**
  - 스프라이트 키프레임 누락으로 정지 이미지만 표시되던 문제 수정
  - Boom 0 → 1 → 2 프레임 0.2초 간격 루프 재생

- **UIManager.cs — 미사용 필드 제거**
  - PoolManager 풀링 전환 후 불필요해진 `itemPrefabs` 필드 제거

---

### 2026-04-30 · 3.JH

#### ✅ 기능 추가

- **DataManager.cs 신규**
  - `Resources/stage_data.json` 로드, 싱글톤 패턴 (`DataManager.Instance`)

- **StageData.cs 신규**
  - JSON 역직렬화용 데이터 구조체 (`delay`, `type`, `point` 필드)

- **`Assets/Resources/stage_data.json`**
  - 스테이지 스폰 이벤트 데이터 파일 추가

#### 🔧 기능 수정

- **Player.cs — 피격 무적 + 스프라이트 깜빡임**
  - 피격 후 `invincibilityDuration` 동안 무적 적용
  - 무적 중 알파값 깜빡임 (`invincibleBlinkInterval`, `invincibleAlpha`)
  - 리스폰 무적 `respawnInvincibilityDuration` 연동

- **Player.cs — WASD 이동 키 추가**
  - 기존 방향키 전용 이동 → WASD 병행 지원

- **Player.cs — 화면 이탈 방지 클램프**
  - `ClampPositionToScreen()` 추가 — 카메라 뷰포트 기준 이동 범위 제한

- **Follower.cs — 공격 쿨다운 제거**
  - 기존 2초 쿨다운 코루틴 → `Fire()` 호출 시 즉시 발사

- **DataManager.prefab 신규** (`3.JH/Prefab/`)
  - DataManager 싱글톤 프리팹 추가, PoolManager 씬에 연동

---

### 2026-04-30 · 4.SDH

#### ✅ 기능 추가

- **보스 애니메이션 및 프리팹** (`4.SDH/Animations/`, `4.SDH/Prefabs/`)
  - `Boss.controller`, `Boss_Idle.anim`, `Boss_Hit.anim` 신규
  - `Boss.prefab` 신규

- **BossMove.cs 신규**
  - 보스 등장 시 위→아래 이동 코루틴 (`MoveRoutine`)
  - `OnEnable` 시작 / `OnDisable` 정지 / `Invoke()` 미사용 (CODING_RULES 준수)

- **EnemyD 풀 슬롯 추가** (`PoolManager.cs`)
  - 기존 Boss 풀 → EnemyD 로 이름 통일
  - `enemyDPrefab`, `enemyDCount`, `GetEnemyD()` 제공

#### 🔧 기능 수정

- **PlayerBullet 독립 분리** (`PlayerBullet.cs`, `PlayerBulletManager.cs`)
  - L/M/R 총알 컨테이너 방식 제거 → 각각 독립 `PlayerBullet`으로 전환
  - 빠른 이동 시 총알 간 충돌·소멸 문제 해결

- **BoomManager Z키 처리 이전** (`BoomManager.cs`)
  - `Player.HandleBoom()` → `BoomManager.Update()`에서 Z키 직접 처리
  - Boom 효과 2초간 0.1초 간격 반복 실행

- **싱글톤 보호 추가**
  - `PoolManager`, `ImpactBulletManager`, `EnemyBulletManager`, `CreateEnemyManager`
  - 씬 재시작·중복 오브젝트 발생 시 자동 제거

- **코루틴 전환**
  - `PowerItem.Update()` → 코루틴 `MoveRoutine()`
  - `PooledEnemy.Update()` → 코루틴 `BoundsCheckRoutine()` (0.2초 간격)
  - `Enemy.Invoke()` → 코루틴 `FlashSprite()`

- **화면 이탈 방지 체크 확장**
  - `PooledEnemy.cs` — 하단 단방향 → 4방향 bounds 체크 (±0.3 X, +0.3 Y 여유 마진)
  - `PlayerBullet.cs` — 상단 단방향 → 4방향 이탈 체크
  - `EnemyBullet.cs` — 상단 이탈 체크 추가 (기존 하단+좌우 → 4방향)

---

## 기술 스택

- **엔진**: Unity 6
- **언어**: C#
- **렌더 파이프라인**: URP (Universal Render Pipeline)
- **물리**: Physics 2D
- **버전 관리**: Git / GitHub
