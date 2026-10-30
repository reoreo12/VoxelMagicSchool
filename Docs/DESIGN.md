# 설계 문서 — 게임플레이 프로토타입 1 (의뢰 · 농사 · 낚시 루프)

> 상태: **제작 중** (2026-10-30 "제작 시작" 승인. 4~8단계 완료: 기반 구조, Core·테스트, 그레이박스·상호작용, 게시판·인벤토리·텃밭·낚시·전달·점수 UI, 완료 기준 시나리오 통과. 사용자 피드백 대기)
> 기준 문서: `GAMEPLAY_PROTOTYPE_SPEC.md`
> 작성일: 2026-10-29

---

## 0. 프로젝트 환경 (파일로 확인, 읽기만 함)

| 항목 | 확인 결과 | 비고 |
|---|---|---|
| Unity 버전 | **6000.0.38f1** (Unity 6) | `ProjectSettings/ProjectVersion.txt` |
| 렌더 파이프라인 | **URP 17.0.3** | 품질 설정에 `PC_RPAsset` / `Mobile_RPAsset` 연결됨 |
| 입력 처리 | **Both** (구 Input Manager + 새 Input System) | `activeInputHandler: 2`. 새 Input System을 바로 쓸 수 있어 **설정 변경 불필요** |
| Input System | 1.13.0 설치됨 | 기본 `Assets/InputSystem_Actions.inputactions`는 건드리지 않고 우리 전용 액션 에셋을 따로 만든다 |
| UI | uGUI 2.0.0 (Unity 6에서는 TextMeshPro 포함) | `Assets/TextMesh Pro`(TMP 기본 리소스) **아직 없음** |
| 테스트 | Test Framework 1.4.6 설치됨 | Core 단위 테스트용 |
| 기타 | AI Navigation, Timeline, Visual Scripting, Collab Proxy, Rider/VS IDE | 이번 작업에 쓰지 않음 |
| **MCP for Unity** | **이 프로젝트의 `manifest.json`에 없음** | 아래 "확인 필요" 참고 |

### 확인 필요 (제작 전에 결정할 것)
1. **MCP 연결**: 첫 확인 때 응답한 에디터는 `Assets/VoiceSTT/Scenes/STTTest.unity`를 열고 있었다 — 이 프로젝트에 없는 경로라 **다른 프로젝트(STT 프로토타입)의 에디터**로 보인다. 그 뒤로는 연결된 에디터가 없다. 이 프로젝트에 MCP for Unity 패키지를 설치해야 한다 (패키지 설치라 승인 필요).
2. ~~한글 폰트 방식~~ → **결정됨 (2026-10-30): TMP 사용.** TMP Essential Resources를 `Assets/TextMesh Pro/`에 가져오는 것 승인됨. 맑은 고딕으로 TMP 동적 폰트 에셋을 만들어 `Assets/_Project/Fonts/`에 둔다. (가져오기는 제작 4단계에서 하고 SETUP.md·CHANGELOG.md에 기록)

---

## 0-1. 구현하며 바뀐 점 (2026-10-30)

| 변경 | 이유 |
|---|---|
| `FarmService` + `FarmAction` 추가 (Core) | "이 칸에서 지금 선택한 아이템으로 무엇을 할 수 있나"(심기/물 주기/뽑기, 불가 이유)를 Core에 두어 테스트할 수 있게 했다. `FarmField`(Gameplay)는 플레이어 앞 칸 계산과 표시만 한다 |
| `FishDefinition`의 "변덕" 값 → `목표 재설정 간격(초)` + `급이동 확률` + `급이동 배율` | 숫자 하나보다 직관적으로 조절할 수 있다. 간격이 짧을수록 변덕스럽다 |
| `FishWeight`(연못 출현 가중치), `GameTexts`(한국어 문구 모음), `RequestTypes`(결과 열거형·진행 상황) 추가 | 출현 확률을 데이터로 조절하고, 문구를 한 파일에서 고칠 수 있게 |
| `GameContext.Create(...)` 정적 생성 메서드 | 조립 순서를 Core 한 곳에 두어 부트스트랩을 얇게 유지 |
| `PrototypeSceneBuilder` 에디터 도구 추가 (메뉴: VoxelMagic/프로토타입 씬 다시 만들기) | 그레이박스 배치를 코드로 남겨 언제든 같은 씬을 다시 만들 수 있게. 설정 에셋은 없을 때만 만든다 |
| ~~한글 폰트 = OS 맑은 고딕 TMP DynamicOS 폰트 에셋~~ → **Pretendard(OFL) 고정(Static) 폰트 에셋** (`_Project/Fonts/Pretendard SDF.asset`, 원본 `Fonts/Pretendard/`) | 동적 에셋은 플레이할 때마다 글자가 추가돼 파일이 바뀌고, 맑은 고딕은 게임에 넣어 배포할 수 없는 폰트라서 교체. 한글 2,350자·영문·기호 + 게임 문구·데이터에 쓰인 글자를 미리 구워 두어 플레이해도 파일이 바뀌지 않음. 다른 PC에 폰트가 없어도 표시됨 |
| `RequestBoard` 상호작용(안내·창 열기 요청)을 6단계에서 먼저 구현 | 상호작용 시스템을 실제 대상으로 확인하기 위해. 게시판 창 UI는 7단계 |
| 입질 오기 전(던지는 중·기다리는 중) E → 낚싯대 거두기 (`FishingSession.PressInteract`에서 `Cancel()`) | 스펙에는 이동 키로만 취소했지만, 연못 앞에 선 채로 바로 거둘 수 있게 하려고 추가 (사용자 직접 구현). 입질 중 E(미니게임 시작)와 결과 중 E(다시 던지기)는 그대로 |
| 낚시 결과 유지 시간 = `GameBalanceConfig.AcquiredInfoSeconds` | 획득 정보 UI가 떠 있는 동안만 E로 다시 던지기가 되도록 두 값을 하나로 묶었다 |

## 1. 용어 풀이 (처음 나오는 전문 용어)

| 용어 | 한 줄 설명 |
|---|---|
| asmdef (어셈블리 정의) | 스크립트 폴더를 별도 묶음으로 컴파일하게 하는 파일. 묶음끼리 참조 방향을 강제할 수 있다 |
| ScriptableObject (SO) | 씬과 무관하게 에셋 파일로 저장되는 데이터 객체. 인스펙터에서 값을 바꿀 수 있다 |
| 캡슐화 | 내부 값은 숨기고, 정해진 메서드로만 바꾸게 하는 것 |
| 다형성 | 종류가 다른 객체를 같은 인터페이스로 똑같이 다루는 것 |
| 상태 머신 | "지금 어떤 단계인지"와 "어떤 조건에서 다음 단계로 넘어가는지"를 명확히 정해 두는 구조 |
| 부트스트랩 (Composition Root) | 씬 시작 시 필요한 객체를 한곳에서 만들고 서로 연결해 주는 컴포넌트 |
| 이벤트 구독 | "값이 바뀌면 나한테 알려 줘"라고 등록해 두는 것. UI는 이것으로만 갱신된다 |
| 풋프린트 (Footprint) | 작물이 텃밭에서 차지하는 칸 크기 (예: 1×2) |
| 프레젠터 / 뷰 | 게임 로직의 상태를 화면(3D 오브젝트나 UI)으로 보여 주기만 하는 쪽 |

---

## 2. 어셈블리 구성과 의존 방향

```
VoxelMagic.UI ───────┐
                     ├──► VoxelMagic.Core   (UnityEngine만 참조. UI·Input·Physics 코드 없음)
VoxelMagic.Gameplay ─┘
VoxelMagic.Tests ──────► VoxelMagic.Core
VoxelMagic.Editor ─────► Core (+ 필요 시 Gameplay, UI)   ※ 에디터 전용
```

- **Core**: 일반 C# 로직(인벤토리, 텃밭 격자, 의뢰, 점수, 낚시 상태 머신)과 ScriptableObject 데이터 정의. UnityEngine은 `ScriptableObject`, `Color`, `Vector2Int`, `Vector3` 같은 기본 타입 때문에만 참조한다. 씬 오브젝트·UI·입력은 모른다.
- **Gameplay**: MonoBehaviour. 플레이어, 상호작용 대상, 카메라, 3D 연출, 입력 읽기, 부트스트랩.
- **UI**: 화면 고정 UI. Core의 이벤트를 구독해 표시만 한다.
- **UI와 Gameplay는 서로 모른다.** 둘 사이의 소통은 모두 Core 객체(이벤트·명령)를 거친다.
  - 예: 게시판(Gameplay)이 창을 열라고 하면 `ScreenState.Open(Window.RequestBoard)`(Core) → 게시판 창(UI)이 그 이벤트를 받아 열린다.
- 부트스트랩(Gameplay)이 UI에 Core 객체를 넘겨줄 때는 Core의 `IGameContextBinder` 인터페이스를 통한다. 그래서 Gameplay가 UI 타입을 알 필요가 없다.

---

## 3. 클래스 목록

### 3-1. Core (`VoxelMagic.Core`) — 데이터(ScriptableObject)

| 이름 | 종류 | 한 줄 역할 |
|---|---|---|
| `ItemDefinition` | 추상 ScriptableObject | 모든 아이템의 공통 필드: id, 이름, 설명, 아이콘 색, 최대 겹침 수, 크리처의 재료 설명 |
| `ToolDefinition` | ScriptableObject | 지팡이 (물 주기·낚시 도구). `ToolKind` 열거형 포함 |
| `SeedDefinition` | ScriptableObject | 어떤 작물이 되는지, 칸 크기(풋프린트), 새싹/다 자란 모양 프리팹 |
| `ProduceDefinition` | ScriptableObject | 작물 결과물. 크기 범위(cm) |
| `FishDefinition` | ScriptableObject | 물고기. 크기 범위, 움직임 속도·변덕·급이동 확률 |
| `ISizedItem` | 인터페이스 | 크기 범위가 있는 아이템(작물·물고기) 공통 — 크기 굴림에 사용 |
| `RequestDefinition` | ScriptableObject | 의뢰 이름, 크리처의 한마디, 필요 아이템, 지급 씨앗, 기본 보상, 완료 대사 |
| `ItemAmount` | 직렬화 구조체 | (아이템, 개수) 쌍 |
| `FloatRange` | 직렬화 구조체 | 최소~최대 값과 무작위 굴림 |
| `GameBalanceConfig` | ScriptableObject | 숫자 설정 묶음: 인벤토리 칸 수, 점수 규칙, 정보 UI 시간 등 |
| `FarmConfig` | ScriptableObject | 격자 크기 N, 칸 크기(m), 미리보기 색 |
| `FishingConfig` | ScriptableObject | 입질 대기 범위, 입질 시간, 스크롤 감도, 게이지 증감 속도, 연못 물고기 표(가중치) |
| `PlayerConfig` | ScriptableObject | 이동 속도, 상호작용 반경, 카메라 거리·각도 |

### 3-2. Core — 로직 (일반 C# 클래스)

| 이름 | 종류 | 한 줄 역할 |
|---|---|---|
| `GameContext` | 클래스 | 아래 서비스들을 묶어 한 번에 전달하는 상자 |
| `IGameContextBinder` | 인터페이스 | `Bind(GameContext)` — 부트스트랩이 UI·월드 오브젝트에 Core를 연결할 때 사용 |
| `Inventory` | 클래스 | 5칸 고정, 겹쳐 쌓기, 추가/제거/개수 조회, 선택 칸. 이벤트: `SlotChanged`, `SelectionChanged` |
| `ItemStack` | 구조체 | (아이템, 개수) — 인벤토리 한 칸의 읽기 전용 상태 |
| `FarmGrid` | 클래스 | N×N 격자. 배치 가능 검사, 심기, 물 주기, 뽑기. 이벤트: `PlotAdded`, `PlotChanged`, `PlotRemoved` |
| `FarmPlot` | 클래스 | 심어진 작물 하나: 씨앗, 시작 칸, 풋프린트, 성장 단계(새싹/다 자람) |
| `PlacementResult` | 열거형 | 배치 결과: 가능 / 격자 밖 / 칸이 차 있음 |
| `IGrowthRule` | 인터페이스 | "물을 주면 어떻게 자라는가" 규칙. 교체 가능 |
| `InstantGrowthRule` | 클래스 | 물을 주면 즉시 다 자람 (이번 프로토타입 규칙) |
| `RequestTracker` | 클래스 | 의뢰 수락(동시 1개), 진행 계산, 전달 가능 여부, 전달 처리. 이벤트: `ActiveChanged`, `ProgressChanged`, `Completed` |
| `RequestProgress` | 클래스 | 필요 아이템별 현재/필요 개수, 전부 모았는지 |
| `AcceptResult` / `DeliverResult` | 열거형 | 수락·전달 결과와 실패 이유 |
| `ScoreBook` | 클래스 | 실기·필기 점수(최대 100). 이벤트: `ScoreChanged` |
| `ScoreCalculator` | 정적 클래스 | 기본 보상 + 처음 전달한 아이템 종류 수 × 보너스, 상한 적용 (순수 함수) |
| `MaterialJournal` | 클래스 | 이미 전달해 본 아이템 종류 기록 (필기 "공부한 재료" 목록) |
| `FishingSession` | 클래스 | 낚시 상태 머신. `Tick(dt)`, `Cast()`, `PressInteract()`, `Scroll(delta)`, `Cancel()` |
| `FishingState` | 열거형 | Idle → Casting → Waiting → Biting → Minigame → Result |
| `FishingMinigame` | 클래스 | 물고기 위치, 잡기 바 위치, 게이지 계산 |
| `FishMotion` | 클래스 | `FishDefinition` 값에 따라 물고기 목표 위치를 무작위로 바꾸는 움직임 |
| `IRandom` / `SystemRandom` | 인터페이스 / 클래스 | 무작위 값 공급. 테스트에서는 고정 값으로 교체 |
| `InteractionPrompt` | 구조체 | 안내 문구: 키, 행동 이름, 가능 여부, 불가 이유, 표시 위치 |
| `PromptChannel` | 클래스 | 현재 안내 문구를 보관하고 바뀌면 이벤트로 알림 (Gameplay가 쓰고 UI가 읽음) |
| `ScreenState` | 클래스 | 열린 창(게시판, 크리처 대화), 의뢰 토글 접힘 상태, "이동 잠금" 여부 |
| `GameMessages` | 클래스 | 획득 정보, 짧은 알림(토스트), 크리처 대사 요청을 UI로 보내는 이벤트 통로 |

### 3-3. Gameplay (`VoxelMagic.Gameplay`) — MonoBehaviour

| 이름 | 종류 | 한 줄 역할 |
|---|---|---|
| `GameBootstrap` | MonoBehaviour | 설정 SO를 받아 `GameContext`를 만들고, 월드·UI의 `IGameContextBinder`에 연결. 시작 시 지팡이 지급 |
| `GameInputReader` | MonoBehaviour | 새 Input System 액션을 읽어 C# 이벤트로 바꿈(이동, E, 1~5, 스크롤, Q, Esc) |
| `PlayerMover` | MonoBehaviour | CharacterController로 이동. 창이 열렸으면 멈춤, 이동 시 낚시 취소·획득 정보 닫기 |
| `PlayerInteractor` | MonoBehaviour | 가장 가까운 `IInteractable`을 골라 안내 문구를 `PromptChannel`에 쓰고, E를 누르면 실행 |
| `IInteractable` | 인터페이스 | `GetPrompt(ctx)`, `Interact(ctx)`, `ClosestPoint(from)` — 모든 상호작용 대상의 공통 계약 |
| `InteractionContext` | 구조체 | 상호작용 시 넘겨주는 정보: 선택한 아이템, 플레이어 위치·방향 |
| `InteractableBase` | 추상 MonoBehaviour | `IInteractable` 공통 구현: Core 연결 받기, 레지스트리 등록/해제, 거리 계산 |
| `InteractableRegistry` | 클래스 | 활성 상호작용 대상 목록 (레이어 추가 없이 가장 가까운 대상 찾기) |
| `RequestBoard` | MonoBehaviour | E: 의뢰 목록 창 열기 |
| `FarmField` | MonoBehaviour | 텃밭 전체를 하나의 대상으로 다룸. 플레이어 앞 칸을 계산해 심기/물 주기/뽑기 |
| `FarmFieldView` | MonoBehaviour | 흙 칸 그리기, `FarmGrid` 이벤트로 작물 모양 생성·교체·삭제 |
| `CropView` | MonoBehaviour | 작물 하나의 모양(새싹 ↔ 다 자람 전환, 짧은 성장 연출) |
| `PlacementPreview` | MonoBehaviour | 씨앗을 들었을 때 풋프린트 크기의 초록/빨강 미리보기 |
| `PondArea` | MonoBehaviour | E: 낚싯줄 던지기 (지팡이 선택 시) |
| `FishingDriver` | MonoBehaviour | 매 프레임 `FishingSession.Tick` 호출, 찌·낚싯줄(LineRenderer)·찌 흔들림 연출 |
| `BiteIndicator` | MonoBehaviour | 입질 때 주인공 머리 위 `!` 표시 (월드 공간 TMP) |
| `CreatureNpc` | MonoBehaviour | 의뢰 조건 확인 후 E: 전달하기 |
| `QuarterViewCamera` | MonoBehaviour | 고정 각도로 플레이어를 부드럽게 따라감 |

### 3-4. UI (`VoxelMagic.UI`)

| 이름 | 종류 | 한 줄 역할 |
|---|---|---|
| `UIPanel` | 추상 MonoBehaviour | 화면 고정 패널 공통: `Show()`/`Hide()`, CanvasGroup 페이드, `IsVisible` |
| `ScoreHud` | MonoBehaviour | 실기/필기 숫자와 채움 바. 점수가 오르면 바가 부드럽게 차오름 |
| `QuestTrackerPanel` | UIPanel | 진행 중 의뢰 이름과 `무 2/3` 같은 항목, 체크 표시, 접기/펼치기, "완료" 후 사라짐 |
| `InventoryBar` / `InventorySlotView` | MonoBehaviour | 5칸, 색 사각형 + 이름 첫 글자, 개수, 선택 강조, 클릭으로 선택 |
| `ControlsHintView` | MonoBehaviour | 조작법 안내 고정 표시 |
| `InteractionPromptView` | MonoBehaviour | `PromptChannel`을 구독해 대상 근처에 `E / 심기` 두 줄 표시 (월드→화면 좌표 변환) |
| `AcquisitionPopup` | UIPanel | 획득 아이템 이름·설명·크기, 3초 뒤 사라짐, 이동 시 즉시 닫힘 |
| `ToastView` | UIPanel | "가방이 가득 찼어요" 같은 짧은 알림 |
| `RequestBoardWindow` / `RequestEntryView` | UIPanel / MonoBehaviour | 의뢰 목록, 수락 버튼(비활성 이유 표시), 완료 표시 |
| `FishingMinigameView` | UIPanel | 세로 바, 물고기 아이콘, 잡기 바, 잡기 게이지 |
| `CreatureDialogView` | UIPanel | 전달 후 크리처의 한마디·재료 설명을 한 줄씩 보여 줌 (E로 넘김) |
| `KoreanFontProvider` | ScriptableObject | 한글 폰트 에셋 참조를 한 곳에 보관 |

### 3-5. Editor / Tests

| 이름 | 종류 | 어셈블리 | 한 줄 역할 |
|---|---|---|---|
| `KoreanFontAssetBuilder` | 에디터 클래스 | Editor | Pretendard로 TMP 고정 폰트 에셋을 굽는 메뉴(VoxelMagic/한글 폰트 다시 굽기). 다시 구워도 GUID 유지 |
| `InventoryTests` | 테스트 | Tests | 겹쳐 쌓기, 가득 참, 제거, 선택 |
| `FarmGridTests` | 테스트 | Tests | 1×1·1×2·2×2 배치, 격자 밖, 겹침, 뽑은 뒤 칸 비움 |
| `RequestTrackerTests` | 테스트 | Tests | 동시 1개 제한, 씨앗 지급, 진행 계산, 전달 조건 |
| `ScoreCalculatorTests` | 테스트 | Tests | 기본 보상, 새 종류 보너스, 100 상한 |
| `FishingSessionTests` | 테스트 | Tests | 상태 전이, 입질 놓침, 게이지 성공/실패 (고정 난수 사용) |

---

## 4. 상속 · 구현 구조도

```
IInteractable  «interface»  (Gameplay)
└─ InteractableBase  «abstract MonoBehaviour»     공통: 연결, 등록, 거리 계산
   ├─ RequestBoard        E: 의뢰 보기
   ├─ FarmField           E: 심기 / 물 주기 / 뽑기 (플레이어 앞 칸 기준)
   ├─ PondArea            E: 낚싯줄 던지기
   └─ CreatureNpc         E: 전달하기

ItemDefinition  «abstract ScriptableObject»  (Core)
├─ ToolDefinition         지팡이
├─ SeedDefinition         어떤 작물이 되는지, 풋프린트
├─ ProduceDefinition      작물 결과물 (크기 범위)      ── implements ISizedItem
└─ FishDefinition         움직임, 크기 범위            ── implements ISizedItem

UIPanel  «abstract MonoBehaviour»  (UI)       공통: Show / Hide / 페이드
├─ QuestTrackerPanel
├─ AcquisitionPopup
├─ ToastView
├─ RequestBoardWindow
├─ FishingMinigameView
└─ CreatureDialogView

IGrowthRule  «interface»  (Core)
└─ InstantGrowthRule      물 주면 즉시 성장   (나중에 DailyGrowthRule 추가)

IRandom  «interface»  (Core)
└─ SystemRandom           (테스트에서는 FixedRandom)

IGameContextBinder  «interface»  (Core)
├─ InteractableBase, FarmFieldView, FishingDriver, BiteIndicator ...   (Gameplay)
└─ ScoreHud, InventoryBar, UIPanel 하위 클래스들 ...                     (UI)
```

상속 깊이는 모두 2단계 이하 (`MonoBehaviour`/`ScriptableObject` 자체는 세지 않음).

---

## 5. 의존 관계도 (누가 만들고, 누가 구독하는가)

### 5-1. 생성 (GameBootstrap이 Awake에서 한 번)

```
GameBootstrap
 ├─ new SystemRandom()
 ├─ new Inventory(balance.SlotCount)                  → 지팡이 1개를 1번 칸에 추가
 ├─ new FarmGrid(farmConfig.Size, new InstantGrowthRule())
 ├─ new ScoreBook(balance.MaxScore)
 ├─ new MaterialJournal()
 ├─ new RequestTracker(inventory, scoreBook, journal, balance)
 ├─ new FishingSession(fishingConfig, inventory, random)
 ├─ new PromptChannel(), new ScreenState(), new GameMessages()
 └─ new GameContext(위 전부)
      └─ worldRoot·uiRoot 아래의 모든 IGameContextBinder.Bind(context)
```

### 5-2. 이벤트 구독 (→ 는 "구독한다")

| 발행 (Core) | 이벤트 | 구독자 |
|---|---|---|
| `Inventory` | `SlotChanged`, `SelectionChanged` | `InventoryBar`(UI), `RequestTracker`(진행 재계산), `PlacementPreview`(씨앗 들었는지) |
| `FarmGrid` | `PlotAdded/Changed/Removed` | `FarmFieldView`(작물 모양) |
| `RequestTracker` | `ActiveChanged`, `ProgressChanged`, `Completed` | `QuestTrackerPanel`, `RequestBoardWindow`(UI) |
| `ScoreBook` | `ScoreChanged` | `ScoreHud`(UI) |
| `FishingSession` | `StateChanged`, `MinigameUpdated`, `Caught`, `Escaped` | `FishingDriver`, `BiteIndicator`(Gameplay), `FishingMinigameView`(UI) |
| `PromptChannel` | `Changed` | `InteractionPromptView`(UI) |
| `ScreenState` | `WindowChanged`, `TrackerToggled` | `RequestBoardWindow`, `CreatureDialogView`, `QuestTrackerPanel`(UI), `PlayerMover`(이동 잠금) |
| `GameMessages` | `ItemAcquired`, `Toast`, `CreatureSpeech`, `AcquiredDismissed` | `AcquisitionPopup`, `ToastView`, `CreatureDialogView`(UI) |

| 발행 (Gameplay) | 이벤트 | 구독자 |
|---|---|---|
| `GameInputReader` | `Moved`, `InteractPressed`, `SlotKeyPressed`, `Scrolled`, `TogglePressed`, `CancelPressed` | `PlayerMover`, `PlayerInteractor` / 1~5 → `Inventory.Select` / 스크롤 → `FishingSession.Scroll` / Q → `ScreenState.ToggleTracker` / Esc → `ScreenState.CloseTop` |

> UI는 키보드를 직접 읽지 않는다. 입력은 Gameplay의 `GameInputReader` 하나에서만 읽고 Core 명령으로 바꾼다. (UI의 마우스 클릭만 예외 — 인벤토리 칸 클릭, 수락 버튼은 uGUI 버튼이 Core 메서드를 바로 호출)

### 5-3. E 키 처리 우선순위 (`PlayerInteractor`)

1. 창이 열려 있음(`ScreenState.IsModalOpen`) → `ScreenState.Submit()` (크리처 대사 넘기기 등). 게시판 창은 버튼으로만 조작.
2. 낚시가 진행 중(`FishingSession.State != Idle`) → `FishingSession.PressInteract()` (입질 잡기 / 결과 상태에서 다시 던지기)
3. 그 외 → 가장 가까운 `IInteractable.Interact(ctx)`

---

## 6. 씬 구성과 핵심 컴포넌트 — `Assets/_Project/Scenes/Prototype_FarmFishing.unity`

```
Prototype_FarmFishing
├─ _Bootstrap                [GameBootstrap]
│     설정 SO들(Balance/Farm/Fishing/Player)과 의뢰 목록, worldRoot·uiRoot를 인스펙터로 연결받아 Core를 조립한다.
├─ Main Camera               [Camera] [QuarterViewCamera]
│     Player를 대상으로 고정 각도(예: 기울기 45°, 방향 45°)로 따라간다. 회전 없음.
├─ Directional Light         [Light]
├─ Global Volume             [Volume]  (선택) 크리처 눈 Emission이 은은히 빛나게 Bloom
├─ World                      ← worldRoot
│  ├─ Ground                 [MeshCollider]  초록 평면. 이동 가능한 지면
│  ├─ School                 [BoxCollider]   크림색 박스 + 연녹색 띠 + 시계 원통. 통과 불가, 상호작용 없음
│  ├─ GinkgoTrees (×3)       [CapsuleCollider] 갈색 원통 + 노란 큐브
│  ├─ RequestBoard           [RequestBoard] [BoxCollider]
│  │     E → ScreenState.Open(RequestBoard). 판 + 기둥 2개
│  ├─ FarmField              [FarmField] [FarmFieldView]
│  │  ├─ Cells               (FarmFieldView가 실행 시 N×N 흙 칸 생성)
│  │  ├─ Crops               (CropView 인스턴스가 생기는 자리)
│  │  └─ PlacementPreview    [PlacementPreview]  반투명 초록/빨강 판
│  │     FarmField는 플레이어 앞 칸을 계산해 FarmGrid에 명령하고, View는 FarmGrid 이벤트로 그린다.
│  ├─ Pond                   [PondArea] [FishingDriver]
│  │  ├─ Water               [BoxCollider]  반투명 파란 평면 + 플레이어 진입을 막는 충돌체
│  │  ├─ Stones              돌 큐브들
│  │  └─ Bobber              [LineRenderer]  흰 구 찌. 지팡이 끝 ↔ 찌 사이 낚싯줄
│  └─ Creature               [CreatureNpc] [CapsuleCollider]
│        남색 큐브 + Emission 노란 눈 큐브 2개. RequestTracker로 조건 확인 → 전달
├─ Player                    [CharacterController] [GameInputReader] [PlayerMover] [PlayerInteractor]
│  ├─ Body / Head / Tie      남색 캡슐 + 살구색 머리 + 빨간 넥타이
│  ├─ WandTip                낚싯줄 시작점
│  └─ BiteIndicator          [BiteIndicator]  머리 위 "!" (월드 공간 TMP, 평소 숨김)
└─ UI                         ← uiRoot
   ├─ EventSystem            [EventSystem] [InputSystemUIInputModule]  새 Input System용 UI 입력
   └─ HUDCanvas              [Canvas(Screen Space Overlay)] [CanvasScaler(1920×1080 기준)]
      ├─ ScoreHud            상단: 실기 0/100, 필기 0/100 + 바
      ├─ QuestTrackerPanel   오른쪽 위: 의뢰 진행 (Q로 접기)
      ├─ InventoryBar        하단 중앙: 5칸
      ├─ ControlsHint        왼쪽 아래: 조작법
      ├─ InteractionPrompt   대상 근처: "E / 심기"
      ├─ AcquisitionPopup    중앙 위: 획득 정보
      ├─ Toast               중앙: 짧은 알림
      ├─ FishingMinigame     오른쪽: 세로 바 + 게이지
      ├─ CreatureDialog      하단: 크리처 대사창
      └─ RequestBoardWindow  중앙: 의뢰 목록 (열리면 이동 잠금)
```

- 입력 액션 에셋: `Assets/_Project/Input/GameControls.inputactions` (Move, Interact, Slot1~5, Scroll, ToggleTracker, Cancel). 기본 `InputSystem_Actions`는 그대로 둔다.
- 작물 프리팹: `Assets/_Project/Prefabs/Crops/` (무·대파·호박 각각 새싹/다 자람). 흙 칸 프리팹, 의뢰 행 UI 프리팹, 인벤토리 칸 UI 프리팹.
- 머테리얼: `Assets/_Project/Materials/` (URP Lit 단색. 물은 Transparent, 크리처 눈은 Emission).

---

## 7. 동작 흐름

### 7-1. 시작
1. `GameBootstrap`이 Core 객체를 만들고 지팡이를 1번 칸에 넣는다 → `Inventory.SlotChanged` → `InventoryBar` 갱신.
2. `ScoreHud`가 `실기 0/100`, `필기 0/100` 표시. `ControlsHint` 고정 표시.

### 7-2. 의뢰 수락
1. 플레이어가 게시판 근처 → `PlayerInteractor`가 `RequestBoard.GetPrompt()` = `E / 의뢰 보기` → `PromptChannel` → 화면 표시.
2. E → `ScreenState.Open(RequestBoard)` → 창 열림, `PlayerMover` 정지.
3. "국물 재료가 필요해" 수락 버튼 → `RequestTracker.Accept(def)`
   - 이미 진행 중이면 `AcceptResult.AlreadyActive` (버튼은 애초에 비활성 + "진행 중인 의뢰가 있어요")
   - 씨앗 넣을 칸이 없으면 `InventoryFull`
   - 성공: 대파 씨앗 2개 지급 → `ActiveChanged` → `QuestTrackerPanel` 등장 (`대파 0/2`, `붕어 0/1`)
4. Esc → 창 닫힘, 이동 재개.

### 7-3. 심기
1. 대파 씨앗 선택(2번 키) → `SelectionChanged` → `PlacementPreview` 활성.
2. 텃밭 위에서 `FarmField`가 플레이어 앞 칸 + 풋프린트(1×2)로 `FarmGrid.CanPlace()` 검사 → 미리보기 초록/빨강, 안내 `E / 심기` 또는 `칸이 부족해요`.
3. E → `FarmGrid.Plant()` + `Inventory.Remove(씨앗 1)` → `PlotAdded` → `CropView`(새싹) 생성.

### 7-4. 물 주기
1. 지팡이 선택 후 새싹 칸 앞 → `E / 물 주기`.
2. E → `FarmGrid.Water(plot)` → `IGrowthRule`(즉시 성장)이 단계를 "다 자람"으로 → `PlotChanged` → `CropView`가 짧게 커지는 연출 후 완성 모양.

### 7-5. 뽑기
1. 다 자란 작물 앞 → `E / 뽑기` (가방이 가득이면 `가방이 가득 찼어요`).
2. E → 크기 굴림(`ProduceDefinition.SizeRange`) → `Inventory.Add(대파)` → `FarmGrid.Remove(plot)`(칸 비움) → `GameMessages.ItemAcquired(대파, 54cm)` → `AcquisitionPopup` 3초 표시.
3. `Inventory.SlotChanged` → `RequestTracker` 재계산 → `ProgressChanged` → `대파 1/2`.
4. 이동 키 → `PlayerMover`가 `GameMessages.DismissAcquired()` → 팝업 즉시 닫힘.

### 7-6. 낚시 (상태 머신)

```
Idle ──E(지팡이, 연못 근처)──► Casting ──찌 착수(연출 시간)──► Waiting
Casting·Waiting ──E──► Idle (입질 오기 전 낚싯대 거두기, 찌 회수)
Waiting ──무작위 2~6초──► Biting  (머리 위 "!", 찌 가라앉음)
Biting ──입질 시간 안에 E──► Minigame
Biting ──시간 초과──► Waiting(재대기 설정 시) 또는 Idle + "물고기가 도망갔다"
Minigame ──게이지 100%──► Result(Caught): 인벤토리 추가 + 획득 정보
Minigame ──게이지 0%──► Result(Escaped): "놓쳤다…" 알림
Result ──E──► Casting (바로 다시 던짐, 정보 UI 닫힘)
Result ──정보 UI 시간(3초) 경과──► Idle
어느 상태든 ──이동 키 / Esc──► Idle (취소, 찌 회수)
```

- 미니게임 매 프레임: `FishMotion`이 물고기 위치를 움직이고, 스크롤로 잡기 바 이동, 겹치면 게이지 증가·아니면 감소.
- 잡은 물고기는 `FishingConfig`의 연못 물고기 표에서 가중치로 고른다.
- 인벤토리가 가득이면 잡기 성공 후 "가방이 가득 차서 놓아줬다" 알림.

### 7-7. 전달 → 점수 상승
1. 크리처 근처 → `RequestTracker.Evaluate()`:
   - 진행 중 의뢰 없음 → `게시판에서 의뢰를 받아 오자`
   - 부족 → `의뢰 물품이 부족해요: 대파 1/2`
   - 충족 → `E / 전달하기`
2. E → `RequestTracker.Deliver()`
   - 인벤토리에서 의뢰 아이템 제거
   - `ScoreCalculator`: 기본 15 + (처음 전달한 종류 2개 × 5) = 25 → `ScoreBook.AddWritten(25)` → `ScoreHud` 바가 부드럽게 차오름
   - `MaterialJournal`에 대파·붕어 기록
   - `GameMessages.CreatureSpeech(완료 대사 + 재료별 설명)` → `CreatureDialogView`가 한 줄씩 표시 (E로 넘김)
   - `Completed` → 진행 토글이 "완료!"로 바뀐 뒤 사라짐, 게시판에서 "완료" 표시, 새 의뢰 수락 가능

---

## 8. 데이터 목록 (ScriptableObject 에셋과 예시 값)

경로: `Assets/_Project/Data/{Items, Seeds, Fish, Requests, Config}/`

### 8-1. 아이템

| 에셋 | 타입 | 주요 값 | 크리처의 재료 설명 (필기 공부) |
|---|---|---|---|
| `Tool_Wand` | ToolDefinition | 이름 "지팡이", 겹침 1, 색 갈색 | "지팡이로 물도 주고 낚시도 한다고? …그게 원래 용도야. 주문은 덤이지." |
| `Seed_Radish` | SeedDefinition | 무 씨앗, 1×1 → `Produce_Radish`, 겹침 20 | — |
| `Seed_GreenOnion` | SeedDefinition | 대파 씨앗, 1×2 → `Produce_GreenOnion` | — |
| `Seed_Pumpkin` | SeedDefinition | 호박 씨앗, 2×2 → `Produce_Pumpkin` | — |
| `Produce_Radish` | ProduceDefinition | 무, 15~30cm, 흰색 | "무는 마력을 흡수해… 그래서 네 불 주문에서 무가 나온 거야." |
| `Produce_GreenOnion` | ProduceDefinition | 대파, 40~70cm, 초록 | "대파는 마력 전도율이 높아. 지팡이 대신 쓰는 놈도 있었지. 냄새 때문에 퇴학당했지만." |
| `Produce_Pumpkin` | ProduceDefinition | 호박, 20~45cm, 주황 | "호박은 변신 주문 기초 재료야. 마차가 되는 건 동화고, 보통은 더 큰 호박이 돼." |
| `Fish_Crucian` | FishDefinition | 붕어, 10~25cm, 속도 0.6, 변덕 0.1, 급이동 0 | "붕어는 기억력이 3초라서 저주가 안 걸려. 부러운 녀석." |
| `Fish_Catfish` | FishDefinition | 메기, 30~60cm, 속도 1.0, 변덕 0.3, 급이동 0.15 | "메기 수염은 마력을 감지해. 시험 감독관들이 몰래 기른다는 소문이 있지." |
| `Fish_ThunderCrucian` | FishDefinition | 번개 붕어, 8~15cm, 속도 1.8, 변덕 0.7, 급이동 0.3 | "번개 붕어를 잡았다고? …거짓말이지? 나도 못 잡았는데." |

### 8-2. 의뢰

| 에셋 | 이름 | 크리처의 한마디 | 필요 | 지급 씨앗 | 보상 | 완료 대사 |
|---|---|---|---|---|---|---|
| `Request_HungryIt` | 배고픈 '그것'의 부탁 | "배고파… 하얗고 아삭한 거. 아니, 너 말고." | 무 3 | 무 씨앗 3 | 필기 +10 | "아삭아삭… 좋아, 오늘 수업은 무다." |
| `Request_Broth` | 국물 재료가 필요해 | "국물엔 대파랑 붕어지. 이건 상식이야. 마법 상식." | 대파 2, 붕어 1 | 대파 씨앗 2 | 필기 +15 | "후루룩… 국물에서 시험 문제 맛이 나." |
| `Request_PumpkinPorridge` | 호박죽이 먹고 싶어 | "호박죽에 메기를 넣으면 맛있대. 누가? 내가." | 호박 1, 메기 1 | 호박 씨앗 1 | 필기 +20 | "…생각보다 별로네. 그래도 공부는 공부다." |

### 8-3. 설정

| 에셋 | 값 (예시) |
|---|---|
| `GameBalanceConfig` | 인벤토리 5칸 / 점수 최대 100 / 새 종류 보너스 +5 / 획득 정보 3초 / 토스트 2초 / 점수 바 차오름 속도 40점/초 |
| `FarmConfig` | 격자 6×6 / 칸 크기 1m / 앞 칸 거리 0.8m / 미리보기 초록·빨강 (알파 0.5) / 성장 연출 0.4초 |
| `FishingConfig` | 던지기 연출 0.6초 / 대기 2~6초 / 입질 시간 1초 / 놓치면 재대기(on) / 스크롤 감도 0.08 / 잡기 바 높이 0.25 / 게이지 시작 0.3 · 증가 0.35/초 · 감소 0.25/초 / 물고기 표: 붕어 60, 메기 30, 번개 붕어 10 |
| `PlayerConfig` | 이동 속도 4m/s / 상호작용 반경 1.6m / 카메라 거리 14m · 기울기 45° · 방향 45° · 따라가기 부드러움 8 |

---

## 9. 확장 지점

| 나중에 붙일 기능 | 어디를 고치면 되는가 |
|---|---|
| **크리처 굴 맵 분리 / 맵 이동** | Core 객체는 씬과 무관하므로 `GameContext`를 상주 씬(또는 DontDestroyOnLoad)의 부트스트랩이 들고, 새 씬은 로드될 때 `IGameContextBinder`로 연결만 받는다. `CreatureNpc`는 다른 씬에 옮겨도 코드 수정 없음. `ScreenState`에 맵 이동 중 잠금 추가 |
| **시간 타이머 / 날짜** | Core에 `IGameClock`(분·일 이벤트) 추가. `IGrowthRule`을 `DailyGrowthRule`로 바꾸면 "물 준 다음 날 성장"이 됨 (`FarmGrid` 수정 없음). 12일차 시험은 `IGameClock.DayChanged` 구독 |
| **과외 (실기 점수)** | `ScoreBook.AddPractical()`이 이미 있음. `TutoringService`(Core)만 추가. `ScoreHud`는 이미 두 점수를 구독 |
| **주문 효과가 낚시·채집에 영향** | `FishingSession`은 던질 때마다 `FishingTuning`(대기 시간, 게이지 속도, 물고기 가중치)을 **설정 + 수정자(modifier) 목록**으로 계산한다. 주문이 수정자를 추가하면 난이도가 바뀐다. 작물 크기 굴림도 같은 방식(`HarvestTuning`) |
| **음성 주문 · 마법진 입력** | 새 어셈블리 `VoxelMagic.Spells`(→ Core). `ISpellInputSource`(음성/마법진) → `SpellId` → `SpellEffect`가 위 수정자를 추가. `Assets/VoiceSTT/` 모듈은 `ISpellInputSource` 구현 어댑터 하나로 연결 |
| **새 작물·물고기·의뢰** | 코드 수정 없이 SO 에셋 추가. 의뢰 목록은 `GameBootstrap` 인스펙터의 배열(나중에 `RequestCatalog` SO로 분리 가능) |
| **세이브** | Core 클래스들이 상태를 private으로 갖고 있으므로 각 클래스에 `CaptureState()/RestoreState()`를 추가하는 방식 |

---

## 10. 설계에서 고민한 선택지

| 주제 | 다른 방법 | 고른 방법과 이유 |
|---|---|---|
| **SO 정의의 위치** | 별도 `Data` 어셈블리 / Core를 완전히 UnityEngine 없이 | **Core에 둠.** UI와 Gameplay 둘 다 아이템 정의가 필요하고 둘은 Core만 참조할 수 있다. 대신 Core 로직 클래스는 SO의 읽기 전용 값만 쓰고 Unity 화면 API는 쓰지 않아 테스트 가능 |
| **텃밭 상호작용 단위** | 칸마다 `FarmPlotCell` 대상 36개 | **텃밭 전체를 `FarmField` 하나로.** 2×2 작물은 여러 칸에 걸쳐 있어 "어느 칸 대상인가"가 애매하고 미리보기도 한 곳에서 계산해야 한다. 플레이어 앞 칸 기준이 더 자연스럽다 |
| **가장 가까운 대상 찾기** | Physics.OverlapSphere + "Interactable" 레이어 | **레지스트리 목록 + 거리 계산.** 레이어 추가는 프로젝트 설정 변경이고, 대상이 몇 개 안 되어 성능 차이 없음 |
| **Gameplay → UI 연결** | 조립 전용 어셈블리(App) 추가 / 싱글턴 | **Core의 `IGameContextBinder` 인터페이스.** 어셈블리를 늘리지 않고, Gameplay가 UI 타입을 몰라도 된다 |
| **낚시 로직 위치** | MonoBehaviour 코루틴 | **Core의 `FishingSession`(Tick 방식).** 상태 전이와 게이지 계산을 단위 테스트할 수 있다. 연출만 `FishingDriver`가 담당 |
| **상태 머신 형태** | 상태마다 클래스(State 패턴) | **열거형 + 전이 메서드.** 상태 6개, 각 상태 로직이 짧아 클래스로 나누면 파일만 늘어난다. 복잡해지면 State 패턴으로 바꾸기 쉬운 구조 |
| **크기 정보 보관** | 아이템마다 개별 크기 저장(겹침 불가) | **겹쳐 쌓고 크기는 획득 시에만 표시.** 스펙의 "같은 아이템은 한 칸에 겹침"을 따름. 나중에 "최대 크기 기록"이 필요하면 `MaterialJournal`에 추가 |
| **입력 읽기** | UI도 각자 키 입력 | **`GameInputReader` 한 곳.** 창이 열렸을 때의 E/Esc 우선순위를 한 곳에서 관리 |
| **한글 폰트 (결정: A)** | A) TMP + 맑은 고딕 동적 폰트 에셋 / B) 레거시 Text + `Font.CreateDynamicFontFromOSFont` | **A로 결정 (사용자 승인).** 글자 품질과 앞으로의 UI 확장에 유리. 단 TMP Essential Resources가 `Assets/TextMesh Pro/`(작업 폴더 밖)에 들어가므로 승인 필요. B는 폴더 밖 파일이 없지만 글자 품질이 낮고 나중에 TMP로 바꿀 때 UI를 다시 만들어야 함 |
