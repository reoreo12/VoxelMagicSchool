# 설계 문서 — 게임플레이 프로토타입 2 (2D 횡스크롤 복도 + 기본 도형 캐릭터)

> 상태: **제작 중** (2026-11-02 "제작 시작" 승인. 1단계 복도 맵, 2단계 캐릭터(모델·상태 머신·이동·애니메이션·HUD) 완료. 사용자 플레이 확인 완료. 12번 아바타 적용(파이프라인·애니메이션 랩·복도 교체) 완료, 사용자 확인 대기)
> 기준 문서: `CORRIDOR_SIDESCROLL_SPEC.md`
> 작성일: 2026-11-02

---

## 0. 프로젝트 환경 (파일로 확인, 읽기만 함)

| 항목 | 확인 결과 | 이번 작업에 주는 영향 |
|---|---|---|
| Unity | 6000.0.38f1 | — |
| 렌더 파이프라인 | URP 17.0.3, `PC_Renderer` (Universal, 3D용 렌더러) | 3D 렌더러에서도 SpriteRenderer는 그대로 그려진다. 2D 렌더러로 바꾸지 않는다 (설정 변경 없음) |
| 입력 | Both (구 Input Manager + Input System 1.13.0) | 새 Input System 그대로 사용 |
| 입력 에셋 | `_Project/Input/GameControls.inputactions` — `Gameplay` 맵 1개 (Move 2D WASD·방향키, Interact E, Slot1~5, Scroll, ToggleTracker Q, Cancel Esc) | 아래 2-5에서 새 액션 맵 추가 제안 |
| 어셈블리 | `VoxelMagic.Core` / `Gameplay`(→Core) / `UI`(→Core) / `Editor` / `Tests`(→Core) | **새 asmdef 없이** 하위 폴더만 추가 |
| 정렬 레이어 (Sorting Layer) | `Default` 하나뿐 | 정렬 레이어를 새로 만들면 프로젝트 설정 변경이라, **`Default` + 정렬 순서 숫자만** 쓴다 |
| 한글 폰트 | `Fonts/Pretendard SDF.asset` (고정 아틀라스) | 그대로 사용. 새 문구에 목록 밖 글자가 있으면 `VoxelMagic/한글 폰트 다시 굽기` |
| 프로토타입 1 재사용 후보 | `ControlsHintView`(조작법 표시), `IGameContextBinder` 방식 조립, `UIPanel` | 2-6 참고 |

### 확인 결과 (2026-11-02)
- **MCP 연결**: `VoxelMagicSchool@2d926515` 지정, `Application.dataPath` = `C:/Unity_Project/NCAI/VoxelMagicSchool/Assets` 확인.
- **이미지 5개** (파일을 직접 읽어 확인, 임포트 설정은 아직 기본값 그대로):

| 파일 | 크기 | 알파 | 비고 |
|---|---|---|---|
| `tile_window_glass.png` | 1536×864 | 반투명 픽셀 384,796개, 알파 90 (=0.35) | 반투명은 위에서 95–530px 안에만 있음 (유리 영역) |
| `tile_door_glass.png` | 1536×864 | 반투명 108,066개, 알파 0.35 | 같음 |
| `tile_locker_glass.png` | 1536×864 | 반투명 278,167개, 알파 0.35 | 같음 |
| `tile_wall.png` | 1536×864 | 모두 불투명 | — |
| `bg_day_loop.png` | **2688×864** | 모두 불투명 | 오른쪽 끝 열과 왼쪽 첫 열의 색 차이(19.1)가 이웃한 두 열의 차이(18.3)와 비슷 → 이음매 없이 반복됨 |

- 완전히 투명한 픽셀(알파 0)은 없다. 유리 밖(벽·바닥)은 모두 불투명.
- **주의**: 현재 임포트 최대 크기가 기본 2048이라 **배경(폭 2688)이 줄어들어 들어온다.** 임포트 설정에서 최대 크기를 **4096**으로 올린다 (아래 4번에 반영).

### 결정 사항 (2026-11-02 사용자)
| 항목 | 결정 |
|---|---|
| 이미지 기준점 | **Bottom Center** |
| 입력 | `GameControls.inputactions`에 **`SideScroll` 맵 추가 승인**. 기존 `Gameplay` 맵은 건드리지 않음 |
| 조작법 안내 | **새 컴포넌트** `SideScrollHintView`. 프로토타입 1 안내와 합치는 방법은 9번 확장 지점에 적음 |
| 정렬 레이어 | 추가하지 않음. `Default` + 정렬 순서 숫자 |
| 폴더 | `Animations/SideScroll/` 추가 승인 |
| 필터 | **Point로 시작.** 깜빡임이 보이면 ① 배경만 Bilinear ② 복도까지 Bilinear 두 경우를 비교해 보고 (필터는 이미지별 임포트 설정이라 코드 수정 없이 바꿈) |

---

## 0-1. 구현하며 바뀐 점

| 변경 | 이유 |
|---|---|
| `ParallaxBackdrop`은 `ISideScrollBinder`가 아니고 카메라를 직접 참조 | 필요한 값이 카메라 위치·화면 폭뿐이고, 에디터 미리보기에서도 같은 코드로 위치를 정하려면 컨텍스트 없이 동작해야 한다 |
| `CorridorConfig`에 필터(복도·배경 따로)와 최대 텍스처 크기 추가 | Point/Bilinear 비교를 코드 수정 없이 설정값 + 메뉴(`VoxelMagic/복도/이미지 임포트 설정 적용`)로 하기 위해 |
| `SideScrollContext`는 지금 설정·복도 범위만 가짐 | 상태 머신·이동 계산기는 캐릭터 단계에서 추가 |
| `CorridorBounds` 구조체 추가 (Core) | 카메라 제한과 캐릭터 이동 범위가 같은 값을 쓰도록 |
| 상태들이 함께 쓰는 `CharacterBody`(이동 계산기·설정·이번 프레임 입력) 추가 | `ICharacterState.Tick(dt)`가 입력을 인자로 받지 않으므로, 상태 머신이 입력을 `CharacterBody.Intent`에 넣고 상태는 거기서 읽는다 |
| `CharacterAnimationConfig`는 상태별 `LimbMotion`(흔들림 각도·몸통 오르내림·주기·기울기) 묶음 | Idle/Walk/Run 값을 같은 모양으로 다뤄 클립 생성 코드를 하나로. 섞기 시간은 플레이 중 바로 반영, 클립 값은 메뉴로 다시 굽기 |
| Idle 클립은 다리를 흔들지 않음 | 서 있을 때 다리가 움직이면 어색하다. 팔 ±2°, 몸통 숨쉬기만 |
| 눈은 머리 **옆면** 앞쪽에 붙임 (설계: 앞면) | 카메라가 옆모습을 보므로 앞면의 눈은 보이지 않는다. 양 옆면에 하나씩 → 어느 쪽을 봐도 눈 하나가 보임 |
| 화면 구석 UI에 반투명 어두운 판 | 밝은 베이지 벽 위에서 흰 글자가 안 읽혀서 |
| 상태 이름: Idle = "서 있기" | 화면 표시용 한국어 |
| (아바타) 랩 이름 `AvatarTest` → **`AnimationLab`**, 코드 위치 `Scripts/Gameplay/Sandbox/` → **`Scripts/Sandbox/` + 새 asmdef** | 사용자 요청으로 툴 형태(목록·재생 조작·측정)가 되면서 커졌다. 게임 코드와 완전히 분리하고 폴더째 지울 수 있게 |
| (아바타) 랩의 키·제자리 판정: 렌더러 경계 상자 → 메시를 구워 정점 높이, 골반 최대 거리 → 골반 앞뒤 이동 **폭**(허용 0.2m, 설정값) | 경계 상자는 여유가 붙어 1.72로 크게 나왔고, 걸음마다 생기는 골반 흔들림(약 6cm)까지 "In Place 아님"으로 잘못 표시했다 |
| (아바타) 재생 속도 제안은 지금 클립이 연결된 상태(걷기/달리기)만 | 걷기 클립의 측정값으로 달리기 제안까지 보여 헷갈렸다 |
| (아바타) 큐브 전용 .anim 클립은 랩 목록에 "이 아바타에 쓸 수 없음"으로 표시 | 모델 파일이 아니라 Humanoid로 다시 임포트할 수 없다 |
| (아바타) 랩 UI 생성 시 글자보다 폰트를 먼저 지정 | 기본 폰트로 한글을 먼저 그리면 `Assets/TextMesh Pro`의 기본 폴백 폰트 파일이 바뀐다 (한 번 바뀐 것은 git으로 되돌림) |
| 씬 생성기는 새 씬을 연 **뒤에** 데이터 에셋을 읽음 | 새 씬을 열면 쓰지 않는 에셋이 메모리에서 내려가 미리 읽은 참조가 풀렸다 (설정 참조가 비어 복도가 안 만들어지던 문제) |

## 1. 단위와 좌표 (모든 숫자는 설정 에셋에서 바뀜)

- **PPU(Pixels Per Unit, 1유닛에 들어가는 픽셀 수) = 300** → 타일 1536×864px = **5.12 × 2.88 유닛**.
- 복도 7칸 = **35.84 유닛**. 복도 바닥 왼쪽 끝을 월드 원점 (x=0, y=0)으로 둔다.
- 타일 세로 띠를 바닥 기준(y, 유닛)으로 바꾸면:

| 띠 | 이미지 px (위에서) | 월드 y (유닛) |
|---|---|---|
| 바닥 띠 | 788–864 | 0 – 0.253 |
| 걸레받이 | 743–788 | 0.253 – 0.403 |
| 징두리 | 541–743 | 0.403 – 1.077 |
| 베이지 벽 | 33–541 | 1.077 – 2.770 |
| 천장 띠 | 0–33 | 2.770 – 2.880 |
| 창 유리 영역 | 100–523 | 1.137 – 2.547 |

- 캐릭터 발 높이 **y = 0.12** (바닥 띠 가운데쯤. 벽 앞 바닥을 딛고 선 것처럼 보임) → `CharacterMoveConfig.GroundY`.
- 방향: 카메라는 **+Z를 바라본다** (복도 정면). 화면 오른쪽 = +X, 위 = +Y, 멀리 = +Z.

---

## 2. 구조

### 2-1. 클래스 목록

**Core** (`Scripts/Core/SideScroll/`, 일반 C# + 데이터 SO, Unity 입력 장치를 모름)

| 이름 | 종류 | 한 줄 역할 |
|---|---|---|
| `MoveIntent` | 구조체 | 한 프레임의 입력 값: 방향(-1/0/1), 달리기 여부. 입력 장치와 상태 머신 사이의 약속 |
| `Facing` | 열거형 | `Left` / `Right` 두 값만 |
| `CharacterStateId` | 열거형 | `Idle`, `Walk`, `Run` (나중에 `Jump`, `Interact`, `CastSpell` 추가) |
| `ICharacterState` | 인터페이스 | `Id`, `Enter()`, `Tick(dt)`, `Exit()`, `Evaluate(intent)` → 다음 상태 Id |
| `CharacterStateBase` | 추상 클래스 | 상태 공통: 이동 계산기·설정 참조, 기본 Enter/Exit(빈 동작) |
| `IdleState` | 클래스 | 감속해 멈춤. 이동 입력이 오면 Walk/Run으로 |
| `MoveStateBase` | 추상 클래스 | Walk/Run 공통: 목표 속도로 가속. 목표 속도만 자식이 정함 |
| `WalkState` / `RunState` | 클래스 | 걷기 속도 / 달리기 속도. 서로 Shift로 오감 |
| `CharacterStateMachine` | 클래스 | 상태 등록·전환. `StateChanged(이전, 다음)`, `FacingChanged(방향)` 이벤트 |
| `SideScrollMotor` | 클래스 | x 위치·속도만 가진 이동 계산기. 가속·감속·양 끝 제한. **Y·Z를 아예 갖지 않음** |
| `SideScrollMath` | 정적 클래스 | 패럴랙스 위치, 배경 반복 배치, 카메라 x 제한, 직교 크기 계산 (테스트 대상) |
| `SideScrollContext` | 클래스 | 위 객체 묶음. 부트스트랩이 한 번 만들어 나눠 줌 (프로토타입 1의 `GameContext`와 같은 방식) |
| `ISideScrollBinder` | 인터페이스 | `Bind(SideScrollContext)` — 컨텍스트를 받는 컴포넌트 표시 |
| `SideScrollTexts` | 정적 클래스 | 조작법·상태 이름 한국어 문구 (프로토타입 1의 `GameTexts`는 건드리지 않음) |
| `CorridorLayout` | ScriptableObject | 타일 목록: 항목마다 `Sprite`, `FlipX` |
| `BackdropSet` | ScriptableObject | 창밖 배경 세트(낮/밤): 스프라이트, 하늘 대체 색 |
| `CorridorConfig` | ScriptableObject | PPU, 레이어 Z·정렬 순서, 패럴랙스 계수, 배경 세로 오프셋·배율, 복사본 수, 양 끝 여유, 카메라 따라가기 시간, 상태 표시 켜기 |
| `CharacterMoveConfig` | ScriptableObject | 걷기·달리기 속도, 가속·감속, 발 높이(GroundY), 캐릭터 Z, 좌우 회전 각도(±90) |
| `CharacterAnimationConfig` | ScriptableObject | 상태별 팔·다리 각도, 한 주기 시간, 몸통 오르내림, 달리기 기울기, 전환 섞기 시간 |

**Gameplay** (`Scripts/Gameplay/SideScroll/`, MonoBehaviour — Unity와 연결만)

| 이름 | 한 줄 역할 |
|---|---|
| `CorridorBootstrap` | 조립 지점. 설정·데이터를 읽어 `SideScrollContext`를 만들고 `ISideScrollBinder`들에게 나눠 줌 |
| `SideScrollInputReader` | 새 Input System의 `SideScroll` 맵을 읽어 `MoveIntent`로 바꾸는 유일한 곳. W/S는 읽지 않음 |
| `SideScrollCharacter` | 캐릭터 루트. 매 프레임 입력 → 상태 머신 → x 위치 적용. 루트 회전을 ±90°로만 정함 |
| `CharacterAnimatorView` | `StateChanged`를 받아 Animator 상태를 섞어 바꾸기만 함 (상태 결정 안 함) |
| `CorridorBuilder` | `CorridorLayout`을 읽어 타일 SpriteRenderer를 타일 폭 간격으로 배치. 복도 범위(Bounds) 계산 |
| `ParallaxBackdrop` | `BackdropSet` 스프라이트 복사본 N장을 패럴랙스 계수로 움직이고 화면 밖 장을 반대편으로 옮김 |
| `SideScrollCamera` | 직교 크기를 복도 높이에 맞추고, x만 부드럽게 따라가며 복도 범위로 제한 |

**UI** (`Scripts/UI/SideScroll/`)

| 이름 | 한 줄 역할 |
|---|---|
| `StateDebugView` | 현재 상태 이름을 화면 구석에 표시 (`StateChanged` 구독, 설정으로 끔) |
| `SideScrollHintView` | 왼쪽 아래 조작법 안내. 문구는 `SideScrollTexts.ControlsHint` (프로토타입 1의 `ControlsHintView`와 같은 모양) |

**Editor** (`Scripts/Editor/SideScroll/`)

| 이름 | 한 줄 역할 |
|---|---|
| `CorridorSpriteImportTool` | 메뉴: Art/Corridor·Art/Backdrop 이미지에 임포트 설정 일괄 적용 (PPU는 `CorridorConfig`에서 읽음) |
| `SideScrollCharacterBuilder` | 메뉴: 기본 도형 캐릭터 프리팹 생성 (이미 있으면 덮어쓰지 않음) |
| `CharacterAnimationBuilder` | 메뉴: `CharacterAnimationConfig`로 Idle/Walk/Run 클립과 Animator Controller 생성·갱신 |
| `CorridorSceneBuilder` | 메뉴: `Corridor_SideScroll` 씬 생성 (프로토타입 1의 씬 생성기와 같은 방식) |

**Tests** (`Tests/SideScroll/`, 기존 `VoxelMagic.Tests` 어셈블리에 폴더만 추가)

| 이름 | 확인 내용 |
|---|---|
| `CharacterStateMachineTests` | Idle↔Walk↔Run 전이, 방향 즉시 전환, 양 끝에 막히면 Idle, 이벤트 발생 순서 |
| `SideScrollMotorTests` | 가속·감속, 최고 속도, 양 끝 제한 |
| `SideScrollMathTests` | 패럴랙스 계수 0/0.3/1, 배경 반복 배치에 빈틈 없음, 카메라 x 제한, 복도보다 화면이 넓을 때 |

### 2-2. 상속·구현 구조도

```
ICharacterState (인터페이스)
└─ CharacterStateBase (추상)                 ← 깊이 1
   ├─ IdleState
   └─ MoveStateBase (추상)                    ← 깊이 2 (최대)
      ├─ WalkState
      └─ RunState

ScriptableObject
├─ CorridorLayout, BackdropSet
└─ CorridorConfig, CharacterMoveConfig, CharacterAnimationConfig

MonoBehaviour
├─ CorridorBootstrap
├─ SideScrollInputReader
├─ SideScrollCharacter          : ISideScrollBinder
├─ CharacterAnimatorView        : ISideScrollBinder
├─ CorridorBuilder
├─ ParallaxBackdrop             : ISideScrollBinder (카메라 위치를 받기 위해)
├─ SideScrollCamera             : ISideScrollBinder
└─ StateDebugView (UI)          : ISideScrollBinder
```

### 2-3. 상태 머신

**전이 다이어그램** (`dir` = 이동 방향 -1/0/1, `run` = Shift 누름, "막힘" = 복도 끝에서 그 방향으로 더 못 감)

```
              dir≠0, run 아님                  run
   ┌────────┐ ─────────────────▶ ┌────────┐ ─────────▶ ┌────────┐
   │  Idle  │                    │  Walk  │            │  Run   │
   └────────┘ ◀───────────────── └────────┘ ◀───────── └────────┘
       ▲  │      dir=0 또는 막힘                run 뗌        │
       │  │                                                  │
       │  └───────────── dir≠0, run ─────────────────────────▶│
       └──────────────── dir=0 또는 막힘 ──────────────────────┘

[Idle] --이동 입력 있음, Shift 아님--> [Walk]
[Idle] --이동 입력 있음, Shift 누름--> [Run]     (Walk를 한 프레임 거치면 애니메이션이 깜빡여서 바로 감)
[Walk] --Shift 누름--> [Run]
[Run]  --Shift 뗌--> [Walk]
[Walk/Run] --이동 입력 없음 또는 막힘--> [Idle]
```

**방향 전환**은 상태가 아니다. 상태 머신이 매 프레임 상태 판단 전에 처리한다: `dir≠0`이고 지금 방향과 다르면 즉시 `Facing`을 바꾸고 `FacingChanged`를 보낸다 → 루트 Y 회전이 그 프레임에 ±90°로 바뀐다 (보간 없음). Idle에서 A를 눌러도 같은 프레임에 돌고 Walk로 간다.

**한 프레임 처리 순서** (`CharacterStateMachine.Update(intent, dt)`)
1. 막힘 판정: 복도 끝에서 바깥쪽으로 누르면 `dir`을 0으로 본다 (방향 전환은 1단계 전에 원래 `dir`로 처리 → 끝에서도 돌아설 수 있음).
2. 방향 전환 (위 설명).
3. `next = Current.Evaluate(intent)` → 다르면 `Current.Exit()` → `next.Enter()` → `StateChanged` 이벤트.
4. `Current.Tick(dt)` → 이동 계산기가 x를 갱신.

**상태별 Enter / Tick / Exit**

| 상태 | Enter | Tick(dt) | Exit |
|---|---|---|---|
| Idle | (없음) — 애니메이션은 이벤트를 받은 View가 바꿈 | 이동 계산기를 감속값으로 0까지 줄임 | (없음) |
| Walk | (없음) | 목표 속도 = 방향 × 걷기 속도, 가속값으로 다가감 | (없음) |
| Run | (없음) | 목표 속도 = 방향 × 달리기 속도, 가속값으로 다가감 | (없음) |

Enter/Exit가 지금은 비어 있지만 남겨 두는 이유: Jump(Enter에서 위로 속도 주기), CastSpell(Enter에서 이동 잠그기, Exit에서 풀기)처럼 다음 상태들이 바로 쓴다.

**A와 D를 동시에 누르면**: 서 있는다 (스펙 기본안). Input System의 1D Axis 합성 바인딩이 "둘 다 누르면 0"을 기본으로 지원해서 코드가 필요 없다. "마지막에 누른 키 따르기"는 키 순서를 따로 기록해야 해서 이번에는 넣지 않는다.

### 2-4. 이동

- `SideScrollMotor`는 x와 x 속도만 가진다. `SideScrollCharacter`는 매 프레임 `position = (motor.X, config.GroundY, config.Z)`로 **덮어써서** Y·Z가 바뀔 틈이 없다.
- 회전도 매 프레임 `rotation = Euler(0, facing == Right ? +90 : -90, 0)`로 덮어쓴다. 다른 축은 코드 어디에서도 건드리지 않는다.
- 이동 범위: `[복도 왼쪽 + 끝 여유, 복도 오른쪽 - 끝 여유]`. 끝 여유 기본 2.56 (= 문 타일 가운데, 문 앞에서 멈춤).

### 2-5. 입력

`GameControls.inputactions`에 새 액션 맵 `SideScroll` 추가 (기존 `Gameplay` 맵은 그대로):

| 액션 | 종류 | 바인딩 |
|---|---|---|
| `Move` | Value(축 1개) | 1D Axis: 음수 A, ←  / 양수 D, → (둘 다 누르면 0) |
| `Run` | Button | Left Shift (누르는 동안) |

W/S/↑/↓는 이 맵에 **바인딩 자체가 없다** → "읽지 않는다"가 구조로 보장된다.

### 2-6. 화면 구석 UI

- 조작법 안내 (왼쪽 아래, 프로토타입 1과 같은 위치·크기·폰트): `A / ← 왼쪽` · `D / → 오른쪽` · `Shift 누르는 동안 달리기`
- 상태 이름 (오른쪽 위, 작게): `상태: 걷기` — `CorridorConfig.ShowStateDebug`로 끔

---

## 3. 씬 계층 구조와 핵심 컴포넌트

```
Corridor_SideScroll.unity
├─ CorridorBootstrap       [CorridorBootstrap] 설정·데이터 연결, 컨텍스트 조립·배포
├─ Main Camera             [Camera: 직교, 배경색=하늘 대체 색] [SideScrollCamera] x 따라가기·범위 제한
├─ Directional Light       [Light] 카메라 쪽에서 비스듬히 → 캐릭터의 보이는 옆면·윗면이 밝음. 그림자 끔
├─ Input                   [SideScrollInputReader]
├─ Backdrop  (z=+5)        [ParallaxBackdrop] 복사본 생성·이동
│   └─ Copy_0..2           [SpriteRenderer] 정렬 순서 -10, 조명 없는 스프라이트 머테리얼
├─ Corridor  (z=+1)        [CorridorBuilder] 플레이 시작 때 타일 생성 + 에디터 미리보기(씬에 저장 안 함)
│   └─ Tile_0..6           [SpriteRenderer] 정렬 순서 0, 조명 없는 스프라이트 머테리얼, 마지막 문은 FlipX
├─ Character (z=0)         프리팹 인스턴스
└─ HUD Canvas              [Canvas: Screen Space Overlay] [CanvasScaler]
    ├─ ControlsHint        [TMP 텍스트] [SideScrollHintView]
    └─ StateLabel          [TMP 텍스트] [StateDebugView]
```

**캐릭터 프리팹** (`Prefabs/SideScroll/SideScrollCharacter.prefab`) — 로직 루트와 모델 자식을 분리

```
SideScrollCharacter (루트)  [SideScrollCharacter] [CharacterAnimatorView] [CapsuleCollider 하나만]
└─ Model                    [Animator: Controller=SideScrollCharacter, Apply Root Motion 끔]  ← 복셀 모델로 교체할 부분
   ├─ Hips (관절 피벗, y=0.46)
   │   ├─ LegL_Pivot → Leg(회색) → Shoe(흰색)
   │   └─ LegR_Pivot → Leg(회색) → Shoe(흰색)
   └─ Body (피벗, y=0.46 — 숨쉬기 오르내림·달리기 기울기는 여기만)
       ├─ Torso(남색 블레이저) · Shirt(흰 얇은 판) · Tie(짙은 빨강 얇은 판)
       ├─ ArmL_Pivot (어깨) → Arm(남색) → Hand(살구색)
       ├─ ArmR_Pivot (어깨) → Arm(남색) → Hand(살구색)
       └─ Head_Pivot (목) → Head(살구색) · Hair(검정, 위·뒤) · EyeL · EyeR(검정)
```

- 모델은 로컬 **+Z를 앞**으로 만든다. 루트 Y 회전 +90° → 오른쪽(+X)을 봄, -90° → 왼쪽(-X)을 봄.
- 팔·다리는 관절 피벗의 **로컬 X축 회전**만으로 앞뒤로 흔든다.
- 비율 제안 (키 약 1.6, 머리가 큰 치비 비율):

| 부위 | 크기 (폭 × 높이 × 앞뒤, 유닛) | 위치 |
|---|---|---|
| 다리 | 0.17 × 0.40 × 0.20 | 골반 아래 |
| 운동화 | 0.18 × 0.06 × 0.26 | 다리 끝, 앞으로 살짝 |
| 몸통(블레이저) | 0.44 × 0.50 × 0.28 | y 0.46–0.96 |
| 셔츠 / 넥타이 | 0.16 × 0.30 × 0.02 / 0.06 × 0.26 × 0.03 | 몸통 앞면 |
| 팔 / 손 | 0.13 × 0.40 × 0.15 / 0.13 × 0.11 × 0.13 | 어깨 피벗 아래 |
| 머리 | 0.56 × 0.56 × 0.56 | y 0.98–1.54 |
| 머리카락 | 위 0.60 × 0.08 × 0.60, 뒤 0.60 × 0.36 × 0.08 | 머리 위·뒤 → 맨 위 약 1.62 |
| 눈 | 0.07 × 0.10 × 0.02 | 머리 앞면 |

- 크기·색은 생성된 **프리팹에서 직접** 고친다 (생성기는 프리팹이 없을 때만 만듦). 머테리얼은 단색 URP Lit 6개(살구·검정·남색·흰색·빨강·회색)를 `Materials/SideScroll/`에 만든다.

---

## 4. 레이어 배치표

| 레이어 | Z 위치 | 정렬 레이어 / 순서 | 패럴랙스 계수 | 이미지 | 머테리얼 |
|---|---|---|---|---|---|
| Backdrop | +5 | Default / -10 | **0.3** | `bg_day_loop` × 3장 (BackdropSet) | 조명 없는 스프라이트 (`Sprite-Unlit-Default`) |
| Corridor | +1 | Default / 0 | 1 (월드에 고정, 컴포넌트 없음) | 타일 7칸 (CorridorLayout) | 조명 없는 스프라이트 |
| Character | 0 | — (3D 메시) | — | 기본 도형 | URP Lit (조명 받음) |

- **패럴랙스 계수**: 화면에서 보이는 이동량 = 카메라 이동량 × 계수. 1이면 복도와 같이 지나가고, 0이면 화면에 붙어 안 움직이고, 0.3이면 복도보다 훨씬 느리게 지나가 멀리 있는 산처럼 보인다.
  계산: `레이어 x = 카메라 x × (1 − 계수) + 기준 x`.
- **무한 반복**: 배경 한 장 폭 W(2688px → 8.96유닛)로, 카메라 기준 "지금 화면 왼쪽 끝을 덮는 장의 번호"를 계산해 3장을 `k·W` 간격으로 다시 놓는다. 화면 폭(16:9에서 5.12)보다 W가 넓어 2장이면 충분하지만, 넓은 화면을 위해 장 수는 설정값(기본 3).
  계산식 하나로 매번 위치를 정하므로 "밀려서 틈이 생기는" 누적 오차가 없다.
- **세로 위치**: 배경 아래쪽을 바닥(y=0)에 맞추면, 유리창(y 1.14–2.55)으로 배경의 위쪽 100–523px 부분(하늘 조금 + 숲)이 보인다. `BackdropOffsetY`·`BackdropScale`로 조절한다.
- **그리는 순서**: 캐릭터(불투명 3D)가 먼저 그려지고, 스프라이트(반투명)는 정렬 순서대로 배경 → 복도 순으로 그려진다. 복도가 캐릭터보다 뒤(z=+1)라 깊이 검사에서 캐릭터 부분은 가려지지 않는다.
- **유리 비침**: 유리는 이미지 자체 알파(약 35%)로 반투명 → 스프라이트 기본 알파 블렌딩만으로 뒤의 배경이 비친다. 별도 셰이더 없음.

### 임포트 설정 (제작 시 메뉴로 일괄 적용)
Sprite (2D and UI), Single, Full Rect, **PPU 300**(설정 에셋), **Pivot: Bottom Center**, Filter **Point**, 압축 None, Mip Map 끔, Alpha Is Transparency 켬, Wrap Clamp, **최대 크기 4096** (배경 폭 2688이 줄지 않게).

---

## 5. 카메라

- **직교 크기(Orthographic Size, 화면 세로의 절반 길이) = 복도 높이 / 2 = 1.44**, 카메라 y = 1.44 고정. 직교 크기는 세로 기준이라 **화면 비율이 바뀌어도 위아래는 항상 꼭 맞고**, 가로로 보이는 폭만 달라진다 (16:9에서 5.12 = 타일 한 칸).
- x는 캐릭터를 부드럽게 따라간다 (`SmoothDamp`, 따라가기 시간 설정값).
- x 범위: `[복도 왼쪽 + 화면 반폭, 복도 오른쪽 − 화면 반폭]`. 화면 반폭은 매 프레임 `직교 크기 × 화면 비율`로 다시 계산해 창 크기를 바꿔도 맞다. 화면이 복도보다 넓으면 복도 가운데에 고정.

---

## 6. 애니메이션

- **"지금 무슨 상태인가"는 Core 상태 머신만 정한다.** `CharacterAnimatorView`는 `StateChanged`를 받아 `Animator.CrossFadeInFixedTime(상태 이름, 섞기 시간)`만 부른다. Animator Controller에는 **전이 화살표·파라미터가 없다** (Animator가 따로 판단하면 상태가 둘이 돼서 어긋남).
- 클립은 `CharacterAnimationBuilder`가 `CharacterAnimationConfig` 값으로 만든다. 값을 바꾸면 메뉴로 다시 만든다.

| 상태 | 움직임 (기본값) | 한 주기 |
|---|---|---|
| Idle | Body y ±0.012 숨쉬기, 팔 ±2° | 2.4초 |
| Walk | 팔·다리 ±25° (팔과 다리는 반대 방향, 왼쪽·오른쪽도 반대), Body y ±0.02 | 0.8초 |
| Run | 팔·다리 ±45°, Body 앞으로 10° 기울기, Body y ±0.04 | 0.5초 |

- 상태 전환 섞기 0.12초 (설정값).

---

## 7. 설계에서 고민한 선택지

| 주제 | 고른 것 | 다른 방법 | 고른 이유 |
|---|---|---|---|
| 카메라 | **직교 + 스크립트 패럴랙스** | 원근 + 레이어를 실제 깊이에 배치 | 원근에서는 깊이마다 크기가 달라져 복도 타일 이음매와 픽셀 크기가 정확히 맞지 않는다. 직교는 계수 하나로 속도를 바로 조절할 수 있다 |
| 이미지 표시 | **SpriteRenderer** | 텍스처 입힌 Quad | 알파, 정렬 순서, 좌우 반전(FlipX)이 기본 기능. Quad는 머테리얼·UV를 직접 다뤄야 한다 |
| 캐릭터 애니메이션 | **Animator + 생성 도구로 만든 클립** | 코드로 팔다리 각도를 계산 (절차적) | 애니메이션 창에서 눈으로 확인·수정할 수 있고, 복셀 모델로 바꿀 때 클립만 교체하면 된다. 단점: 값을 바꾸면 메뉴로 클립을 다시 만들어야 한다 (절차적 방식은 플레이 중 바로 반영) |
| 이동 처리 | **Transform 직접 이동 + 범위 제한** | Rigidbody (Z 이동·회전 고정) | 부딪힐 것이 복도 양 끝뿐이라 물리가 필요 없다. 물리 엔진은 미세한 밀림·회전이 생길 수 있어 "Y·Z 절대 불변" 보장이 오히려 어렵다. 충돌체는 나중 상호작용 감지용으로 루트에 하나만 둔다 |
| 입력 맵 위치 (승인) | **기존 `GameControls`에 `SideScroll` 맵 추가** | 새 `.inputactions` 에셋 | 키 설정이 한 곳에 모여 나중에 키 변경 화면을 만들기 쉽다. 기존 `Gameplay` 맵은 건드리지 않아 프로토타입 1 동작은 그대로 |
| 조작법 안내 (승인) | **새 작은 컴포넌트 `SideScrollHintView`** (UI/SideScroll) | `ControlsHintView`에 "문구 종류" 필드를 추가해 재사용 | 기존 컴포넌트는 `GameTexts.ControlsHint` 하나만 읽게 고정돼 있어 재사용하려면 프로토타입 1 스크립트를 고쳐야 한다. 코드가 짧아 고쳐서 얻는 이득이 작다. 겉모양(위치·크기·폰트)은 같게 만든다 |
| 복도 생성 시점 | **플레이 시작 때 생성 + 에디터 미리보기** | 에디터 메뉴로 씬에 구워 두기 | 데이터(CorridorLayout)가 유일한 기준이 된다. 미리보기 오브젝트는 씬 파일에 저장하지 않아(HideFlags) 데이터와 씬이 어긋날 일이 없다 |
| 이미지 기준점(Pivot) (승인) | **Bottom Center** (스펙 기본안 Bottom Left 대신) | Bottom Left | SpriteRenderer의 FlipX는 기준점을 축으로 뒤집는다. 왼쪽 아래 기준이면 반전된 마지막 문이 한 칸 왼쪽으로 튀어 위치 보정 코드가 필요하다. 가운데 아래 기준이면 `x = (i + 0.5) × 폭`으로 반전 여부와 관계없이 같은 계산 |
| 정렬 레이어 (승인) | **Default + 정렬 순서 숫자** | Backdrop/Corridor 정렬 레이어 추가 | 정렬 레이어 추가는 프로젝트 설정 변경이다. 레이어가 둘뿐이라 숫자로 충분하다 |
| 끝에서 바깥으로 누를 때 | **Idle (제자리 걸음 안 함)** | Walk 유지 | 벽에 대고 걷는 동작이 어색하다. 방향 전환은 그대로 되므로 반대쪽으로 바로 돌아설 수 있다 |
| 필터 모드 (승인) | **Point로 시작** | Bilinear | 픽셀이 또렷하다. 단, 864px 이미지를 1080px 화면에 그리면 1.25배라 픽셀 크기가 고르지 않고, 느리게 움직이는 배경에서 미세한 깜빡임이 보일 수 있다. 보이면 ① 배경만 Bilinear ② 복도까지 Bilinear를 같은 장면 캡처로 비교해 보고한다 |

---

## 8. 디자인 패턴

| 패턴 | 적용한 클래스 | 왜 썼는지 | 안 쓰면 생기는 문제 |
|---|---|---|---|
| **상태 패턴** (State, 상태마다 클래스를 두고 같은 인터페이스로 다룸) | `ICharacterState`, `IdleState`, `WalkState`, `RunState`, `CharacterStateMachine` | 상태별 동작과 전이 조건이 각 클래스에 모인다. 새 상태는 클래스 추가로 끝난다 | `switch`와 `if`가 한 메서드에 쌓여, Jump·CastSpell을 넣을 때마다 모든 분기를 다시 확인해야 한다 |
| **템플릿 메서드** (공통 흐름은 부모, 다른 부분만 자식) | `MoveStateBase` → `WalkState`/`RunState` (목표 속도만 다름) | 가속·방향 처리 코드를 한 번만 쓴다 | 걷기·달리기에 같은 코드가 복사돼 한쪽만 고치는 실수가 생긴다 |
| **옵저버 / 이벤트** (상태가 바뀌면 알리고, 관심 있는 쪽이 구독) | `StateChanged`, `FacingChanged` → `CharacterAnimatorView`, `StateDebugView`, `SideScrollCharacter` | 상태 결정(Core)과 표현(애니메이션·UI)을 분리. Core는 누가 듣는지 모른다 | 상태 머신이 Animator·UI를 직접 불러 Core가 Unity 화면 코드에 묶이고 단위 테스트가 불가능해진다 |
| **어댑터** (다른 형태의 값을 우리 약속 형태로 바꿈) | `SideScrollInputReader` → `MoveIntent` | 입력 장치(키보드·패드)를 바꿔도 Core는 그대로. 테스트에서 가짜 입력을 바로 넣을 수 있다 | 상태 머신이 `Keyboard.current`를 직접 읽어 테스트할 수 없고, 키 변경이 로직 수정이 된다 |
| **데이터 주도 설계** (동작을 코드가 아닌 데이터 에셋으로 정함) | `CorridorLayout`, `BackdropSet`, 설정 SO 3종 | 타일 순서, 밤 배경, 속도·계수를 코드 수정 없이 인스펙터에서 바꾼다 | 숫자와 순서가 코드에 박혀 바꿀 때마다 컴파일·코드 리뷰가 필요하다 |
| **컴포지션 루트 + 의존성 주입** (객체 조립을 한 곳에서 하고 필요한 쪽에 넣어 줌) | `CorridorBootstrap`, `SideScrollContext`, `ISideScrollBinder` | 누가 무엇을 쓰는지 한 파일에서 보인다. `FindObjectOfType`·싱글턴이 없다 | 컴포넌트마다 서로를 찾아다녀 실행 순서 문제와 숨은 의존이 생긴다 |
| **팩토리 / 빌더** (데이터를 보고 오브젝트를 만들어 냄) | `CorridorBuilder`, 에디터 생성 도구들 | 같은 데이터로 언제든 같은 결과를 다시 만든다 | 씬에 손으로 배치한 것과 데이터가 어긋난다 |
| **재활용** (만든 오브젝트를 버리지 않고 위치만 옮겨 다시 씀 — 오브젝트 풀과 같은 발상) | `ParallaxBackdrop`의 배경 복사본 3장 | 무한히 이어지는 배경을 고정 개수로 처리. 생성·삭제 없음 | 걸어갈수록 배경 오브젝트를 새로 만들고 지워 끊김·메모리 낭비가 생긴다 |

---

## 9. 확장 지점

| 나중에 할 일 | 고칠 곳 |
|---|---|
| **밤 배경으로 교체** | `BackdropSet` 에셋을 하나 더 만든다 (밤 스프라이트, 하늘 대체 색). `ParallaxBackdrop.SetBackdrop(set)`을 부르거나 부트스트랩에 연결된 세트를 바꾼다. 코드 수정 없음 |
| **복도 끝 문 → 다른 맵** | `CorridorLayout` 항목에 "출구 Id" 필드 추가 → `CorridorBuilder`가 그 타일 위치에 문 상호작용 오브젝트(프로토타입 1의 `IInteractable` 재사용)를 놓는다. 이동 범위의 끝 여유를 문 앞으로 맞추고, 씬 이동 서비스 하나를 Core에 추가 |
| **조작법 안내를 프로토타입 1과 합치기** | 문구를 "조작법 묶음" 데이터(SO, 줄 목록)로 옮기고, `ControlsHintView`와 `SideScrollHintView`를 하나의 `ControlsHintView(묶음 참조)`로 합친다. 씬마다 다른 묶음을 연결하면 된다. 합칠 때 프로토타입 1 씬의 안내 오브젝트에 묶음을 연결하는 작업이 필요 (프로토타입 1 수정이라 그때 승인) |
| **복도 NPC·상호작용** | 프로토타입 1의 `IInteractable`/`InteractableBase`를 그대로 쓴다. 거리 계산이 이미 위치 기반이라 x만 쓰는 복도에서도 동작. 입력 맵에 `Interact` 추가 |
| **Jump 상태 추가** | ① `CharacterStateId.Jump` ② `JumpState : CharacterStateBase` (Enter에서 위 속도, Tick에서 중력, 착지하면 Idle/Walk) ③ `MoveIntent`에 `JumpPressed` ④ Idle/Walk/Run의 `Evaluate`에 "점프 누름 → Jump" 한 줄씩 ⑤ 상태 머신 생성 시 등록 ⑥ `SideScrollMotor`에 y 속도 추가 (지금은 일부러 없음) ⑦ 애니메이션 클립 추가. 기존 상태 클래스 내부 로직은 그대로 |
| **CastSpell·Interact 상태** | Jump와 같은 방식. Enter에서 이동 잠금, 끝나면 Idle로. 마법 연출은 `StateChanged` 구독자로 추가 |
| **복셀 캐릭터로 교체** | 프리팹의 `Model` 자식만 바꾼다. 관절 이름(`ArmL_Pivot` 등)을 같게 하면 기존 클립 그대로, 다르면 새 Animator Controller만 연결. 루트의 로직 컴포넌트는 그대로 |
| **다른 복도(층·건물)** | `CorridorLayout` 에셋을 새로 만들어 연결 |

---

## 10. 폴더

```
Assets/_Project/
├─ Art/Corridor/, Art/Backdrop/      (사용자가 넣는 이미지 — 아직 없음)
├─ Scripts/Core/SideScroll/           상태 머신·상태·이동 계산·계산식·데이터 SO
├─ Scripts/Gameplay/SideScroll/       부트스트랩, 입력, 캐릭터, 애니메이션 표현, 카메라, 패럴랙스, 복도 빌더
├─ Scripts/UI/SideScroll/             조작법 안내, 상태 표시
├─ Scripts/Editor/SideScroll/         임포트 설정, 캐릭터 프리팹, 애니메이션, 씬 생성 도구
├─ Tests/SideScroll/                  상태 전이·이동·계산 단위 테스트
├─ Data/SideScroll/                   CorridorLayout, BackdropSet_Day, 설정 SO 3종
├─ Animations/SideScroll/             클립 3개 + Animator Controller (생성 도구가 만듦)
├─ Materials/SideScroll/              캐릭터 단색 머테리얼 6개
├─ Prefabs/SideScroll/                캐릭터 프리팹
└─ Scenes/Corridor_SideScroll.unity
```

- 프로토타입 1의 씬·스크립트는 수정하지 않는다. 예외는 승인받은 `GameControls.inputactions`의 `SideScroll` 맵 추가 하나.
- `Animations/SideScroll/` 추가 승인됨.

## 11. 제작 순서 (승인 후)
1. 이미지 확인 → 임포트 설정 → 복도 빌더·레이어 → 카메라 → 패럴랙스 → 플레이 확인·보고
2. 캐릭터 모델·프리팹 → 상태 머신·단위 테스트 → 이동·방향 전환 → 애니메이션 → 단계마다 보고
3. 완료 기준 1~9 확인, 직접 플레이 안내

---

## 12. 아바타 적용 — Student_Male + Mixamo (2026-11-03 승인)

큐브 캐릭터의 **로직 루트는 그대로 두고 `Model` 자식만** 리깅된 Student_Male로 바꾼다. 상태 머신·이동·입력·카메라는 수정하지 않는다.

### 12-1. 파일 확인 결과 (읽기만 함)

| 파일 | 확인 내용 |
|---|---|
| `Art/Characters/Student_Male/Student_Male_v02.fbx` | 스킨 메시 1개(정점 12,986), 뼈 23개 + Root. 이름이 Mixamo식(`Hips`, `Spine`~`Spine2`, `Neck`, `Head`, `Left/RightShoulder·Arm·ForeArm·Hand`, `Left/RightUpLeg·Leg·Foot·ToeBase`, 접두사 없음). 손가락 뼈 없음. **정면 = +Z** (발끝 방향, 큐브 캐릭터와 같음). T자세. 실제 높이 **1.012** (발바닥 −0.012 ~ 머리 1.000), 팔 벌린 폭 1.0. 현재 Rig = Generic, 머테리얼은 파일 안에 포함(`voxel_palette_mat`, URP Lit, 텍스처 연결됨) |
| `Student_Male_v02_tex.png` | 256×256 팔레트 텍스처. 현재 Bilinear + 압축 → 색 경계가 번질 수 있음 |
| `Animations/Mixamo/Y Bot@Idle.fbx` | 클립 `Idle` 8.33초, 30fps |
| `Y Bot@Walking.fbx` | 클립 `Walking` 1.03초 |
| `Y Bot@Standard Run.fbx` | 클립 `Standard Run` 0.73초 |
| 공통 (Mixamo 3개) | Y Bot 뼈대(`mixamorig:` 접두사, 손가락 포함 66개), cm 단위(파일 배율 0.01), Rig = Generic, 반복 재생 꺼짐. **Hips가 앞뒤로 이동하지 않음 → In Place 확인** |

→ 뼈 이름은 다르지만(`Hips` vs `mixamorig:Hips`), 둘 다 **Humanoid**(Unity가 사람 뼈대를 표준 부위로 매핑해 다른 뼈대끼리 애니메이션을 옮겨 입히는 방식)로 바꾸면 그대로 옮겨 입힐 수 있다. Humanoid 필수 뼈(골반, 척추, 머리, 팔 3마디, 다리 3마디)가 모두 있다.

### 12-2. 임포트 설정 (메뉴 도구로 적용, 값은 설정 에셋)

| 대상 | 설정 | 이유 |
|---|---|---|
| Student_Male FBX | Rig **Humanoid**, Avatar = 이 모델로 생성. **크기 배율 = 목표 키 1.6 ÷ 실제 높이 1.012 ≈ 1.58** (도구가 메시 높이를 재서 계산) | 키를 큐브 캐릭터와 같게. 모델 자체 배율로 맞춰 아바타·충돌체·애니메이션이 같은 크기 기준을 씀 |
| 팔레트 텍스처 | Filter **Point**, 압축 없음, Mip Map 끔 | 복셀 팔레트 색이 섞이지 않게 (복도 이미지와 같은 원칙) |
| Mixamo 3개 | Rig **Humanoid**, Avatar = 각 파일(Y Bot)로 생성. 클립 이름 `Idle`/`Walk`/`Run`, **반복 재생 켬**, 루트 회전·높이·앞뒤 이동 **포즈에 굽기(Bake Into Pose)** | 이동은 코드가 하므로 애니메이션이 캐릭터를 옮기거나 돌리지 못하게 |

### 12-3. 아바타 파이프라인 — "틀만 지키면 그대로 적용"

**폴더 규칙 (틀)**
```
Art/Characters/<캐릭터>/Model/   모델 .fbx + 텍스처 .png (Humanoid로 쓸 수 있는 사람 뼈대)
Art/Characters/<캐릭터>/Sprites/ 2D 스프라이트
Animations/<아무 하위 폴더>/     애니메이션 .fbx (Mixamo Without Skin 등) / .anim
```
**메뉴 (VoxelMagic/아바타/)** — 모델·애니메이션을 Project 창에서 선택하고 실행
| 순서 | 메뉴 | 하는 일 |
|---|---|---|
| 1 | 선택한 캐릭터 모델 준비 | Rig Humanoid, 목표 키(기본 1.6)에 맞춰 배율 자동 계산, 같은 폴더 텍스처 Point·압축 없음, `Data/Avatars/<이름>.asset`(AvatarSetup) 생성. 이름에 idle/walk/run이 들어간 Humanoid 클립을 찾아 Idle/Walk/Run에 자동 연결 |
| 2 | 선택한 애니메이션 Humanoid로 임포트 | Rig Humanoid, 클립 이름 = 파일 이름의 `@` 뒤, 반복 재생 켬, 루트 회전·높이·앞뒤 이동 포즈에 굽기 (폴더를 선택하면 안의 파일 모두) |
| 3 | 선택한 아바타로 복도 캐릭터 만들기 | Animator Controller(`Animations/Avatars/<이름>`, 상태 Idle/Walk/Run + 상태별 재생 속도 파라미터)와 프리팹(`Prefabs/SideScroll/SideScrollCharacter_<이름>`) 생성 → 복도 캐릭터로 지정 → 복도 씬 다시 만들기 |

| 이름 | 종류 | 어셈블리 | 한 줄 역할 |
|---|---|---|---|
| `AvatarSetup` | ScriptableObject | Core | 아바타 한 벌: 모델, 목표 키, 충돌체 반지름, Idle/Walk/Run 클립, 상태별 재생 속도, 만든 Controller·프리팹 |
| `AvatarAnimationSpeed` | MonoBehaviour | Gameplay | 아바타 Animator에 상태별 재생 속도를 매 프레임 넣음 → **플레이 중 AvatarSetup 값을 바꾸면 바로 반영** (복도에서 맞추기용) |
| `AvatarPipeline` | 에디터 도구 | Editor | 위 메뉴 1~3 |
| `CorridorConfig.CharacterPrefab` | 설정 값 | Core | 복도에 놓을 캐릭터. 비어 있으면 큐브 캐릭터 |

### 12-4. 복도 캐릭터 프리팹 (Model만 교체)

```
SideScrollCharacter_<이름> (루트)  [SideScrollCharacter] [CharacterAnimatorView] [CapsuleCollider]  ← 큐브와 같음
└─ Model (FBX 인스턴스)            [Animator: Avatar, Controller, Apply Root Motion 끔] [AvatarAnimationSpeed]
```
- Humanoid Avatar는 뼈 경로를 FBX 루트 기준으로 찾으므로, Animator는 FBX 인스턴스 자신(=Model)에 둔다.
- Animator 상태 이름이 `CharacterStateId`와 같은 `Idle`/`Walk`/`Run`이라 **`CharacterAnimatorView`·상태 머신 수정 없음**.
- 큐브 프리팹은 비교·대체용으로 남긴다.

### 12-5. 재생 속도·이동 속도 맞추기 — 복도에서
- 복도 씬을 플레이하면서 `Data/Avatars/<이름>`의 Idle/Walk/Run 재생 속도(또는 `CharacterMoveConfig`의 이동 속도)를 인스펙터에서 바꾸면 바로 보인다. ScriptableObject라 플레이를 멈춰도 값이 남는다.
- 애니메이션 랩의 "디딤발 속도"가 시작값을 알려 준다: 재생 속도 ≈ 게임 이동 속도 ÷ 디딤발 속도.

### 12-6. 애니메이션 랩 (테스트 씬, 툴 형태)
`Scenes/Sandbox/AnimationLab.unity` — 앞으로 가져올 모든 아바타·애니메이션을 확인하는 도구. 이식 뒤에도 남긴다 (빌드 목록에 넣지 않음).

| 영역 | 기능 |
|---|---|
| 아바타 (왼쪽 위) | 아바타 선택(`Data/Avatars`의 AvatarSetup), Humanoid 여부·키·뼈 수 표시, 키 비교 큐브(1.6) 켜기/끄기, 골반 이동량(제자리 여부) |
| 상태 버튼 (위 가운데) | Idle / Walk / Run — AvatarSetup에 연결된 클립으로 섞어서 전환 (복도와 같은 전환 확인) |
| 애니메이션 목록 (오른쪽) | `Assets/_Project/Animations` 아래 모든 클립. **새로고침**으로 새로 넣은 파일 바로 반영. Humanoid가 아닌 파일은 "Humanoid로 임포트" 버튼 (메뉴 2와 같은 처리) |
| 재생 (아래) | 재생/일시정지, 한 프레임 앞/뒤, 시간 슬라이더(끌어서 이동), 시간·프레임 표시, **반복 재생 On/Off**, 재생 속도(0~2배), 섞기 시간(0~1초) |
| 보간 테스트 | **A↔B 반복 전환**: 마지막에 재생한 두 클립을 정한 간격으로 계속 섞어 바꿈 → 보간 튐·발 꼬임 확인 |
| 측정 (왼쪽 아래) | 디딤발 속도(m/s, 재생 속도 1 기준) → 걷기·달리기 이동 속도에 맞는 재생 속도 제안 |
| 카메라 | 우클릭 드래그 회전, 휠 확대, 가운데 드래그 이동, 정면·옆·뒤·3/4 버튼 |
| 단축키 | Space 재생/일시정지, ←/→ 한 프레임, 1/2/3 Idle/Walk/Run, L 반복 |

- 클립 재생은 Animator Controller 없이 **Playable**(코드로 클립을 직접 섞어 재생하는 Unity 기능)로 한다 → 어떤 클립이든 상태를 만들지 않고 바로 재생·섞기·멈춤·시간 이동.
- 반복 재생 On/Off는 임포트 설정을 바꾸지 않고 랩에서만 적용.

**어셈블리**: 랩 코드는 `Scripts/Sandbox/`에 **새 asmdef `VoxelMagic.Sandbox`**(Core 참조)와 `Scripts/Sandbox/Editor/`의 `VoxelMagic.Sandbox.Editor`(에디터 전용)로 분리한다. 이유: 게임 코드가 랩을 참조할 수 없게 막고, 폴더째 지워도 다른 코드가 깨지지 않게. 랩만 키보드·마우스를 직접 읽는다. 파일 검색·임포트(에디터 기능)는 에디터 어셈블리가 연결해 주는 통로(`AnimationLabBridge`)로만 쓴다.

### 12-7. 고민한 선택지

| 주제 | 고른 것 | 다른 방법 | 이유 |
|---|---|---|---|
| 애니메이션 옮겨 입히기 | **Humanoid** | Generic + 뼈 이름 맞추기 | 뼈 이름(`mixamorig:` 접두사)·개수(66 vs 23)·비율이 달라 Generic으로는 바로 안 맞는다. Humanoid는 다른 아바타에도 같은 클립을 쓸 수 있다 |
| 키 맞추기 | **FBX 임포트 배율 자동 계산** | 프리팹에서 Model 배율 조절 | 배율이 걸린 Transform 아래 Animator는 충돌체·뼈 위치 계산이 헷갈린다 |
| 랩 재생 방식 | **Playable** | 클립마다 Animator 상태 만들기 | 새 클립을 넣을 때마다 Controller를 고칠 필요가 없다 |
| 재생 속도 조절 위치 | **AvatarSetup + Animator 파라미터** | Animator 상태 속도 직접 수정 | 플레이 중 바로 바꿔 볼 수 있고, 아바타마다 따로 저장된다 |
| 머테리얼 | **FBX에 포함된 것 그대로** | 파일로 꺼내기 | 이미 URP Lit + 텍스처 연결. 고칠 일이 생기면 그때 |

### 12-8. 제작 순서
1. 파이프라인(AvatarSetup, 메뉴 1~3) → Student_Male·Mixamo 3개 적용 → 크기·Humanoid 확인
2. 애니메이션 랩 (런타임 + 씬·UI 생성) → 재생·반복·섞기·측정·카메라 확인 → 캡처로 보고
3. 복도 캐릭터 교체 → 걷기·달리기·방향 전환·양 끝 확인 → 보고
4. 문서·커밋 확인

### 12-9. 애니메이션 랩: 도트 렌더링 On/Off (2026-11-03 승인, A 방식으로 완료)
3D 아바타를 낮은 해상도로 그린 뒤 확대해, 도트(픽셀 아트)처럼 보이게 하는 확인용 토글. **랩에서만** 쓴다.

| 방법 | 내용 | 장단점 |
|---|---|---|
| **A. 저해상도 렌더 텍스처 (추천)** | 카메라를 화면의 1/N 크기 텍스처에 그리고, Point 필터로 화면 가득 확대해 보여 준다. 끄면 원래대로 화면에 직접 그림 | 프로젝트 설정·URP 렌더러를 건드리지 않음. 랩 씬에만 영향. UI(Screen Space Overlay)는 선명하게 유지 |
| B. URP 렌더러 기능(전체 화면 픽셀화 패스) | `PC_Renderer`에 후처리 패스 추가 | 셰이더로 더 다양한 효과 가능하지만 **프로젝트 설정 변경**이고 모든 씬에 영향 → 이번에는 쓰지 않음 |
| C. Pixel Perfect Camera | URP 2D 렌더러 전용 | 3D 렌더러에서는 못 씀 |

**A 구성**
- `LabPixelView` (Sandbox, MonoBehaviour, 카메라에 붙임): 켜면 `RenderTexture`(화면 크기 ÷ 픽셀 크기, Point 필터, 깊이 버퍼 포함)를 만들어 카메라 출력으로 지정하고, 화면 맨 뒤 `RawImage`에 띄움. 창 크기가 바뀌면 다시 만듦. 끄면 해제.
- UI(아래 재생 패널 옆): **도트 렌더링** 토글(단축키 P) + **픽셀 크기** 슬라이더(2~8, 기본 4) + 현재 렌더 해상도 표시 (예: 480×270).
- 설정값(`AnimationLabConfig`): 픽셀 크기 범위·기본값.
- 마우스 카메라 조작·측정·재생 기능은 그대로.

**확장 지점**: 나중에 게임 본편(복도·바깥 맵)에도 도트 느낌을 쓰기로 하면, 이 방식을 카메라 공통 컴포넌트로 옮기거나 B(렌더러 기능)로 바꾼다 — 그때 다시 설계.

### 12-10. 복도 맵 도트 렌더링 On/Off (2026-11-03 승인: B 화면 전체 + 토글·슬라이더, 완료)
애니메이션 랩의 도트 렌더링(12-9, 저해상도 렌더 텍스처 방식)을 복도에도 쓴다. 복도는 랩과 달리 **이미 도트로 그린 2D 배경 + 3D 캐릭터**라서, 어디까지 낮은 해상도로 그릴지 고를 수 있다.

| 방식 | 내용 | 장단점 |
|---|---|---|
| **A. 캐릭터만 도트 (추천)** | 배경 카메라는 지금처럼 원래 해상도로 복도·창밖을 그리고, 캐릭터 전용 카메라가 캐릭터만 낮은 해상도(배경이 투명한 텍스처)로 그려 그 위에 겹친다 | 배경 이미지의 도트·하이라이트는 그대로 선명하고, 3D 캐릭터만 배경과 같은 도트 느낌이 됨. **캐릭터 전용 레이어(`Character`) 추가 필요 → 프로젝트 설정 변경이라 승인 필요** |
| B. 화면 전체 도트 | 랩과 같이 카메라 화면 전체를 낮은 해상도로 | 레이어 추가 없음. 하지만 배경 타일(세로 864px)을 화면 세로 약 270px(4배 기준)로 줄여 그리므로 창틀·하이라이트 같은 배경 디테일이 뭉개짐 |

**공통 구성**
- 랩의 `LabPixelView`를 게임 쪽 공용 컴포넌트 `PixelRenderView`(Gameplay)로 옮기고, 랩도 같은 컴포넌트를 쓴다 (코드 한 벌). Gameplay·Sandbox asmdef에 `UnityEngine.UI`·Gameplay 참조 추가
- 켜기/끄기: **P 키** (`GameControls`의 `SideScroll` 맵에 `TogglePixel` 액션 추가 — 기존 `Gameplay` 맵은 그대로)
- 화면 오른쪽 위 상태 표시 아래 패널: **도트 렌더링 (P) 토글 + 도트 크기 슬라이더(2~8) + `도트 4배 · 480×222` 표시** (사용자 요청으로 슬라이더 추가)
- 상태는 Core의 `PixelRenderState`(켜짐·크기, 바뀌면 이벤트) 하나로, P 키와 패널이 같은 상태를 바꾼다. 공용 컴포넌트는 UI 어셈블리의 `PixelRenderView` (화면 출력이라 UI에 둠)
- 설정(`CorridorConfig`): 시작할 때 켜기 여부, 도트 크기(기본 4)
- 캐릭터 이동·카메라·패럴랙스·상태 머신은 그대로
