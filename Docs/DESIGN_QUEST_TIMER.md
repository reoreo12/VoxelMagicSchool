# 설계 문서 — 의뢰 시스템 정식 개편 + 게임 타이머 + 치트

> 지침: `Docs/QUEST_TIMER_SPEC.md` · 브랜치 `feature/quest-timer` · 작성 2026-11-24
> 상태: **승인됨(2026-11-24 "제작 시작")**, 10장 질문은 답이 없어 [제안]대로 진행. 진행: 1단계(시계·멈춤·하루 흐름·시간 치트) 완료, 2단계(하루 시작 처리·희귀도) 완료, 3단계(의뢰 개편·세이브 v2) 완료(2026-11-25) → 4단계(튜토리얼 모드) 대기
> 사용자 결정(2026-11-24): 작물은 게임 시간 기준(2026-11-25 균형 결정: 흔함 3시간·보통 5시간·희귀 다음 날 아침, 지침 3-1의 "1~2일"을 대체), 재료 종은 사용자 구상 목록(농작 검은 무·꼿꼿한 벼·괴물 고구마, 낚시 블롭피쉬 + 붕어·메기 유지, 채집 12종), 의뢰 19개 새로.
> 제작 중 바뀐 점(2단계): 채집 자리 번호는 같은 종류 서식지들에 번갈아 매김(서식지가 많아도 고루 퍼지게), 게시판 반복 방지는 제한을 풀 때 가장 오래전 게시된 의뢰부터, 하루 시작 단계에 `DayStartReason`(새 게임 첫날엔 작물 안 자람), 진행 상태 세계는 `PlayerProgress.EnsureWorld(WorldSetup)`.
> 제작 중 바뀐 점: `DebugToolsConfig` SO는 만들지 않음. **치트는 키 대신 패널 버튼**(화면 왼쪽 `치트` 버튼 또는 ` 키, 2026-11-24 사용자 요청: Insert 없음·Fn 조합 불편), 나머지 단축키는 기본 없음. 기존 치트 키(F7·F10·F11·F12·`[` `]`)는 의뢰 단계에서 패널로 옮김. 룰렛 화살표는 보간 회전(사용자 결정). 일정표 이름은 사용자 표대로(7일 불꽃 축제 = 일과 없는 스토리 날). 새 게임 시작 날은 튜토리얼 전까지 2일(임시)
> 표기: [제안] = Claude가 고른 기본 동작(바꿀 수 있음), [질문] = 정해 주면 좋은 것

---

## 0. 지금 구조 요약 (프로토타입 1 + 뒷산 + UI 작업 이후)

| 영역 | 지금 클래스 | 지금 동작 | 지침과 다른 점 |
|---|---|---|---|
| 의뢰 데이터 | `RequestDefinition` (SO) | 제목, 크리처 대사, 필요 아이템, 지급 아이템(씨앗), 기본 보상, 완료 대사 | 난이도·서식지 힌트 없음, 보상이 고정 값 |
| 의뢰 규칙 | `RequestTracker` | **한 번에 하나만** 수락, 인벤토리 변화마다 진행 재계산, 전달 시 `기본 보상 + 처음 전달한 재료 보너스` | 최대 3개, 게시판 매일 교체, 포기, 고르는 전달, 희귀도 점수 |
| UI 창구 | `IAcceptedRequestList`, `AcceptedRequestEntry` | 드롭다운·책 의뢰 탭·완료 알림이 이 인터페이스만 봄 (여러 건 가정으로 이미 만듦) | 그대로 쓸 수 있음 |
| 게시판 | `RequestBoard`(Gameplay), `RequestBoardWindow`(UI) | 전체 의뢰 목록(또는 씬별 목록)을 늘어놓고 수락 | 오늘의 3개만 보여야 함 |
| 전달 | `CreatureNpc` | 진행 중 의뢰 하나를 대사 후 전달 | 완료 가능 목록에서 골라 반복 |
| 점수 | `ScoreBook`(상한 100), `ScoreCalculator`, `MaterialJournal` | 필기/실기, 처음 전달 재료 보너스 | 상한 없음, 희귀도 점수 합 |
| 인벤토리 | `Inventory` | 고정 칸, 전부 되거나 전부 안 됨, `SlotChanged` | 그대로 |
| 텃밭 | `FarmGrid`·`FarmPlot`·`FarmService`, `IGrowthRule`=`InstantGrowthRule` | 물 주면 바로 자람. 격자는 **씬마다 새로** 만들고 떠날 때 `PlayerProgress`에 보관본 저장 | 1~2일 뒤 자람, 하루 시작에 성장 (씬 밖에서도) |
| 낚시 | `FishingSession`, `FishingConfig.PickFish`(물고기별 가중치) | 입질 때 가중치로 물고기 선택 | 희귀도 60/30/10 → 그 희귀도의 물고기 |
| 채집 | `GatherSpawner`·`GatheringService`·`GatheringConfig` | 서식지 오브젝트 12개(종류마다 3개 × 슬롯 2), 시간대 바뀔 때·디버그 키로 다시 배치, 진행 중 의뢰 재료 1개 보장. **씬마다 새로 뽑음** | 종류마다 자리 8곳 중 4곳, 희귀도 확률, 보장 없음, 하루 단위로 고정 |
| 시간 | `IGameTime`(UI 창구만), `TimePause`(참조 카운트 2단계), `ScreenStack`, `TimeScaleDriver`, 뒷산·복도의 `TimeOfDayState` | **게임 시계 없음.** HUD는 테마 미리보기 값. 멈춤은 원인 구분 없는 카운트 | 시계, 원인별 멈춤, 하루 흐름 |
| 세이브 | `PlayerProgress`·`GameSession`·`SaveData v1`·`SaveMapper` | 인벤토리·점수·도감·완료/진행 의뢰 1개·밭 | 새 상태(날짜·시각·게시판·수락 목록)를 담을 자리 필요 |
| 치트 | `RequestCheatKeys`(F7·F10·F11·F12), `MountainDebugInput`(F1~F6·F8·F9 등), 대사 `[` `]` | Gameplay 어셈블리 안, 키 하나씩 | 패널·배속·날짜 이동 등 |
| 씬 | `FarmPond`(텃밭+연못), `BackMountain`(채집), `Creature_Cave`(게시판+크리처) | 씬마다 `GameBootstrap`이 `GameContext`를 새로 만듦 | **시계·게시판·수락 목록은 씬을 넘어 살아야 함** |

핵심 관찰: 씬마다 `GameContext`를 새로 만들기 때문에, 시계·하루 상태·게시판·수락 목록·밭·오늘의 채집 자리는 **씬을 넘어 유지되는 `PlayerProgress` 쪽**에 있어야 한다. 하루 시작은 플레이어가 어느 씬에 있든 일어나므로, 텃밭 성장과 채집 자리 뽑기는 **씬 오브젝트 없이 데이터만으로** 처리할 수 있어야 한다.

---

## 1. 프로토타입 1에서 바뀌는 점

| 기존 클래스 | 처리 | 이유 |
|---|---|---|
| `RequestDefinition` | **수정** | 난이도, 서식지 힌트 한 줄 추가. `_givenItems` → 의미를 "수락 시 지급 씨앗"으로 명확히(필드 이름은 에셋 호환 위해 유지). `_baseReward` 삭제(점수는 재료 희귀도 합). 필요 재료 합계 3개 초과 시 경고(OnValidate) |
| `ItemDefinition` | **수정** | 희귀도(흔함/보통/희귀), 출처(농작/낚시/채집/포획-보류) 필드 추가 |
| `SeedDefinition` | **수정** | 자라는 데 걸리는 날 수(1~2) 추가 |
| `RequestTracker` | **교체** → `RequestLog` | 한 건 규칙이 구조 전체에 박혀 있음(Active 하나, 진행 리스트 하나). 수락 목록·상태 머신·포기·고르는 전달로 새로 쓰는 게 고치는 것보다 작고 안전 |
| `RequestTypes` | **수정** | `RequestStatus`를 새 상태(게시판/진행 중/완료 가능/전달됨/포기/만료)로, `AcceptResult`에 `SlotsFull`·`NotOnBoard`·`BoardClosed` 추가, `AlreadyActive` 삭제 |
| `IAcceptedRequestList`·`AcceptedRequestEntry` | **그대로**(작게 수정) | UI가 이미 여러 건 기준. `ExpectedReward` 설명만 "희귀도 점수 합"으로, 항목에 `Id` 추가(포기 호출용) |
| `ScoreCalculator` | **교체** → `WrittenScoreRule` | 보너스 규칙 삭제, 희귀도 점수표 합 |
| `ScoreBook` | **수정** | 상한 삭제(지침 2-7). `MaxScore`를 쓰던 옛 `ScoreHud`는 씬에서 꺼져 있으므로 막대 계산만 임시 기준값으로 |
| `MaterialJournal` | **그대로** | 점수에는 안 쓰지만 "전달해 본 재료" 기록은 도감 자리로 유지 |
| `GameBalanceConfig` | **수정** | `MaxScore`·`NewItemKindBonus` 삭제 |
| `FarmGrid`·`FarmPlot` | **수정** | 물 준 뒤 지난 날 수 기록, `AdvanceDay()`. 격자를 **`PlayerProgress`로 옮겨** 씬을 넘어 하나만 둠(보관본 복사 삭제) |
| `IGrowthRule` / `InstantGrowthRule` | **수정** | `OnDayStart(plot)` 추가. 새 `DailyGrowthRule`(기본), `InstantGrowthRule`은 튜토리얼 모드에서만 |
| `PlayerProgress` | **수정** | 시계·하루 흐름·게시판·수락 목록·밭 격자·오늘의 채집 자리·튜토리얼 모드를 함께 가짐 |
| `FishingConfig.PickFish` | **수정** | 희귀도 표 → 그 희귀도 물고기 중 가중치. 다음 낚시 희귀도 고정(치트)·물고기 지정(튜토리얼) |
| `FishingSession` | **수정** | 물고기 속도 배율 자리(`IFishSpeedModifier`), 튜토리얼 즉시 입질 |
| `GatherSpawner` | **교체** → `GatherDayRoller` + `GatherDayState` | 하루 시작 때 한 번 뽑아 하루 동안 유지(씬을 오가도 같은 자리), 희귀도 확률, 보장 없음 |
| `GatheringService` | **수정** | 뽑기 대신 `GatherDayState`를 읽고 줍기만. 시간대 전환 재배치·의뢰 재료 보장 삭제 |
| `GatheringConfig` | **수정** | 서식지 **종류마다** 자리 8 / 켜지는 자리 4(최소·최대·확률 방식 대신), 희귀도 표 |
| `GatherMaterialDefinition` | **그대로** | 방과후/밤 출현 필드는 남기되 이번 규칙에서는 쓰지 않음([제안] 8-4) |
| `TimePause` | **수정** | 원인별 등록·해제(`Acquire(level, cause)`), 지금 멈춘 원인 목록 제공. 카운트 대신 원인 집합 |
| `ScreenStack` | **수정** | 화면 이름을 원인으로 넘김 (동작은 같음) |
| `IGameTime` | **수정** | `DaysUntilExam`, `Warning`(임박 경고 상태), `WarningChanged` 추가 |
| `TimeOfDayState`(뒷산) | **그대로** | 뒷산에서는 시계의 시간대를 받아 넣음(조명 등 기존 구독자 유지) |
| `GameContext` | **수정** | `SetTime` 자리에 시계 연결, `Requests` 타입 `RequestLog`, 게시판·하루 흐름·튜토리얼 노출 |
| `GameBootstrap` | **수정** | 시계 구동기 붙이기, 밭 복원 코드 삭제(격자가 진행 상태에 있음) |
| `CreatureNpc` | **수정** | "완료 가능 목록 → 고르기 → 대사 → 전달 → 남았으면 다시 고르기" |
| `RequestBoard`(Gameplay) | **수정** | 게시판 닫힌 날(7일~)·[제안] 밤에만 열림 안내 |
| `RequestBoardWindow`·`RequestEntryView`(UI) | **수정** | 오늘의 게시 의뢰 3개, 칸 꽉 참 이유, 씨앗 다시 받기 버튼 (임시 UI 그대로 재사용) |
| `QuestBookPage`(UI) | **수정(작게)** | 오른쪽 페이지에 임시 `포기` 버튼 → 확인 창 |
| `TimeHudView`(UI) | **수정(작게)** | D-n을 Core 값으로, 임박 경고 구독(화살표 색·깜빡임은 테마 값) |
| `QuestTrackerPanel`(UI, 옛 HUD) | **삭제** | 세 씬에서 이미 꺼져 있고 한 건 규칙에 묶여 있음 |
| `RequestCheatKeys`(Gameplay/Debug) | **교체** → 새 Debug 어셈블리의 치트 | 정식 빌드에서 빠지는 별도 어셈블리로 모음. 키 F7·F10·F11·F12 기능은 유지 |
| `MountainDebugInput` | **수정** | "새 하루"=하루 흐름 치트 호출, "의뢰 교체" 삭제(게시판 치트로) |
| `SaveData`·`SaveMapper` | **수정** | 버전 2: 날짜·시각·다음 날 시작 보정·게시판·수락 목록·오늘 채집 자리·밭 날 수. v1 파일은 새 게임으로 시작(경고 로그). 세이브 기능 자체(D-N 슬롯)는 범위 밖, 자동 저장이 깨지지 않게만 |
| 테스트 `RequestAndScoreTests`·`GatheringTests`·`SaveSystemTests`·`ScreenStackTests` | **수정** | 새 규칙에 맞춤 |

---

## 2. 클래스 목록

### Core (`VoxelMagic.Core`, 일반 C# + SO)

| 이름 | 종류 | 폴더 | 한 줄 역할 |
|---|---|---|---|
| `TimeConfig` | SO | Core/Time | 하루 구간(시작·끝·현실 초), 경고 시각, 하루 시작 기본 시각, 경비원 지연, 시작 보정 합치기 규칙·범위 |
| `CalendarConfig` | SO | Core/Time | 날짜별 날 종류·시계가 흐르는지·멈춘 날 표시 시각, 시험 날, 새 게임 시작 날 |
| `DaySegment` | 직렬화 구조체 | Core/Time | 구간 하나(시간대, 시작·끝 분, 현실 초) |
| `GameClock` | 클래스 (`IGameTime`) | Core/Time | 날짜·분 단위 시각·시간대. `Advance(실제초)`로 흐르고 정시·시간대·경고·자정 이벤트를 순서대로 냄 |
| `PauseCause` | 열거형 | Core/Time | 멈춤 원인: 화면(책·가방·게시판·자세히·대화·전달 고르기·확인 창), 스토리, 이벤트, 튜토리얼, 맵 이동, 치트 |
| `DayFlow` | 클래스 | Core/Time | 하루 시작·끝 진행자. 하루 시작 단계 실행, 자정(경비원)·스스로 끝내기·스토리 날 끝내기, 날 종류별 시계 정지 |
| `IDayStartStep` | 인터페이스 | Core/Time | 하루 시작에 할 일 하나 (`Order`, `Run(day)`) |
| `StartTimeAdjustments` | 클래스 | Core/Time | "다음 날 방과 후를 N분 늦게/일찍" 요청을 원인별로 모아 합친 값을 냄 |
| `ItemRarity`·`ItemSource`·`RequestDifficulty`·`DayKind`·`TimeWarning` | 열거형 | Core/Data | 희귀도, 재료 출처(포획 자리 포함), 난이도(쉬움·보통·어려움·튜토리얼), 날 종류, 경고 종류 |
| `QuestConfig` | SO | Core/Quest | 최대 수락 수, 난이도별 등장 시작 날, 마지막 전달 날, 게시판 닫히는 날, 반복 방지 일수, 희귀도별 점수, 튜토리얼 의뢰, [제안] 밤에만 열기 |
| `RequestPool` | SO | Core/Quest | 게시판에 올라올 수 있는 의뢰 전부 (씬 부트스트랩 목록 대신 하나로) |
| `RequestPicker` | 클래스 | Core/Quest | 그날 게시할 의뢰를 난이도별로 하나씩, 반복 방지 규칙으로 고름 |
| `RequestBoardState` | 클래스 | Core/Quest | 오늘 게시판(최대 3개), 하루 시작 때 교체(받지 않은 것은 만료), 게시 이력 |
| `RequestLog` | 클래스 (`IAcceptedRequestList`) | Core/Quest | 수락 목록. 수락·포기·전달·진행 재계산·완료 가능 목록·상태 변경 알림 |
| `AcceptedRequest` | 클래스 | Core/Quest | 수락한 의뢰 하나(고유 번호, 수락 순서, 상태, 재료별 진행) |
| `RequestState` | 열거형 | Core/Quest | 게시됨 / 진행 중 / 완료 가능 / 전달됨 / 포기 / 만료 |
| `DeliverySession` | 클래스 | Core/Quest | 크리처 전달 대화 한 번: 고를 목록 → 고름 → 대사 → 전달 → 다시 목록 (UI·Gameplay가 단계만 따라감) |
| `SeedRefillRule` | 정적 클래스 | Core/Quest | 진행 중 의뢰에 아직 필요한 씨앗 수 계산(심은 것·거둔 것·가진 씨앗 빼기) |
| `WrittenScoreRule` | 정적 클래스 | Core/Quest | 의뢰 점수 = 필요 재료(개수 포함) 희귀도 점수 합 |
| `RarityTable` | 직렬화 구조체 | Core/Spawning | 희귀도 확률(흔함·보통·희귀, 포획용 자리 포함). `Roll(random)` |
| `GatherDayRoller` | 클래스 | Core/Spawning | 서식지 종류마다 자리 N곳 중 M곳을 켜고 자리마다 희귀도 → 재료 |
| `GatherDayState` | 클래스 | Core/Spawning | 오늘 켜진 자리와 재료(씬을 넘어 유지), 줍기, 바뀌면 알림 |
| `DailyGrowthRule` | 클래스 (`IGrowthRule`) | Core/Farming | 물 준 작물이 하루 시작마다 하루씩 자라 정한 날 수가 되면 다 자람 |
| `FishPicker` | 클래스 | Core/Fishing | 희귀도 → 물고기. 다음 한 번 희귀도 고정, 물고기 지정(튜토리얼) |
| `IFishSpeedModifier` | 인터페이스 | Core/Fishing | 물고기 속도 배율 자리 (나중에 "느려져라" 주문) |
| `TutorialMode` | 클래스 | Core/Tutorial | 켜고 끔 + 세 기능(시간 정지, 순서 잠금 농작→낚시→채집, 결과 보장). 지금 허용된 활동 알려 줌 |
| `ActivityKind` | 열거형 | Core/Tutorial | 농작·낚시·채집 (잠금 대상) |
| `DaySteps` (`CropGrowthStep`, `GatherRespawnStep`, `BoardRefreshStep`) | 클래스 | Core/Time | 하루 시작 3단계(순서 1·2·3) |
| `WorldSnapshot` 류 (`ClockSnapshot`, `RequestSnapshot` …) | 직렬화 클래스 | Core/Session | 세이브용 순수 데이터. 각 상태에 `ToSnapshot()`/`Restore()` |

### Gameplay (`VoxelMagic.Gameplay`, Unity 연결)

| 이름 | 종류 | 역할 |
|---|---|---|
| `GameClockDriver` | MonoBehaviour | 매 프레임 `unscaledDeltaTime × 배속`으로 시계를 흐르게 함. 씬 멈춤(`TimePause`)과 진행 멈춤이 모두 비었을 때만 |
| `GameBootstrap` | 수정 | 시계 구동기 추가, 맵 이동 중 멈춤 원인 등록 |
| `CreatureNpc` | 수정 | `DeliverySession`을 열고 단계마다 대사창·고르기 창 |
| `RequestBoard` | 수정 | 열 수 없는 날·시간 안내 |
| `FarmField`·`FishingDriver`·`GatherPickup` | 수정 | 튜토리얼 순서 잠금 확인(막히면 안내 문구), 낚시 즉시 입질 |
| `MountainBootstrap` | 수정 | 뒷산 `TimeOfDayState`를 시계 시간대에 맞춤, `GatherDayState` 연결 |

### UI (`VoxelMagic.UI`, 임시 UI 포함)

| 이름 | 종류 | 역할 |
|---|---|---|
| `RequestBoardWindow`·`RequestEntryView` | 수정 | 오늘의 3개, 거절 이유, 씨앗 다시 받기 |
| `DeliveryChoiceWindow` | 새로(임시) → **삭제(2026-11-26)** | 완료 가능 의뢰 고르기 창. 대사창 안 선택지(`DialogueChannel.Ask`, UI_SPEC 6-4)로 바뀜 |
| `ConfirmDialog` | 새로(임시) | 예/아니요 확인 창 (`GameScreen.Confirm`). 포기에 처음 쓰고 책 메뉴 확인 창에도 재사용 |
| `QuestBookPage` | 수정 | 임시 `포기` 버튼 |
| `TimeHudView` | 수정 | Core 날짜·경고 구독 |

### Debug (새 어셈블리 `VoxelMagic.DebugTools`, 폴더 `Scripts/Debug/`) — 만듦 (2026-11-24)

| 이름 | 종류 | 역할 |
|---|---|---|
| `CheatHub` | MonoBehaviour | 플레이 시작 때 스스로 생기는 치트 루트(DontDestroyOnLoad), 지금 씬의 컨텍스트 찾기. 키는 ` 패널 여닫기만 기본(나머지 단축키는 Inspector에서 선택) |
| `CheatPanel` | IMGUI 창 | **키보드 없이 마우스로 쓰는 임시 UI**(사용자 요청: Insert 없음, Fn 조합 불편). 화면 왼쪽 가장자리 `치트` 버튼으로 여닫음. 맨 위 줄 = 일시 정지·디버그 표시·배속 ×1~×16·월드도 같은 배속, 탭 = 시간·날짜(시각 이동, 1~8일 버튼, 다음 날 보정) / 재료·게시판 |
| `DebugOverlay` | IMGUI 글자 | 화면 오른쪽. 날짜·분 단위 시각·시간대·경고·배속·멈춤 원인·다음 날 보정, 게시판·채집 자리·작물 남은 시간 |
| `TimeCheats`·`DayStartCheats` | 일반 C# | 패널 버튼이 부르는 동작 모음 (`RequestCheats`·`ItemCheats`는 3·5단계) |

- 모든 파일을 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`로 감싼다(asmdef의 define 제약은 "또는"을 못 써서 코드에서 처리). 어느 어셈블리도 이 어셈블리를 참조하지 않는다 → 정식 빌드에서 빈 어셈블리가 된다.
- IMGUI를 쓰는 이유: 개발 전용이라 프리팹·씬 연결 없이 어느 씬에서든 뜨고, 정식 UI(uGUI 테마)를 건드리지 않는다. 한글은 IMGUI 기본 글꼴로도 깨지지 않아 `DebugToolsConfig`(폰트·키 SO)는 만들지 않았다.

---

## 3. 상속·구현 구조

```
IGameTime (UI 창구)
 └─ GameClock

IAcceptedRequestList (UI 창구)
 └─ RequestLog

IDayStartStep
 ├─ CropGrowthStep      (Order 1)  FarmGrid.AdvanceDay
 ├─ GatherRespawnStep   (Order 2)  GatherDayRoller → GatherDayState
 └─ BoardRefreshStep    (Order 3)  RequestBoardState.Refresh

IGrowthRule
 ├─ DailyGrowthRule     (기본)
 └─ InstantGrowthRule   (튜토리얼 모드)

IFishSpeedModifier       (지금 구현 없음 = 배율 1)

ItemDefinition (abstract SO)          ← 희귀도·출처 필드 추가 (깊이 1 그대로)
 ├─ SeedDefinition / ProduceDefinition / FishDefinition / GatherMaterialDefinition / ToolDefinition

UIPanel (기존 추상)
 ├─ RequestBoardWindow, DeliveryChoiceWindow, ConfirmDialog

InteractableBase (기존 추상, IInteractable)
 ├─ RequestBoard, CreatureNpc, GatherPickup …
```
상속은 모두 기존 1단계 그대로, 새 기반 클래스는 만들지 않는다. 확장은 인터페이스로 끼운다.

---

## 4. 상태 다이어그램

### 4-1. 의뢰 하나

```
            하루 시작(뽑힘)
                 │
                 ▼
           ┌──────────┐  하루 시작(안 받음)   ┌──────┐
           │ 게시됨    │ ───────────────────▶ │ 만료  │ (게시판에서 사라짐, 이력에만)
           └──────────┘                       └──────┘
                 │ 수락 (칸 < 3, 게시판 열린 날) → 씨앗 지급
                 ▼
           ┌──────────┐  재료 다 모임          ┌──────────┐
           │ 진행 중   │ ─────────────────────▶ │ 완료 가능 │
           │          │ ◀───────────────────── │          │
           └──────────┘  재료가 줄어듦(버림·     └──────────┘
             │    ▲      다른 의뢰에 냄)            │ 크리처에게 골라서 전달
       포기   │    │                               │ (그 순간 다시 확인)
             ▼    │                               ▼
        ┌──────┐  │                          ┌──────────┐
        │ 포기  │  │                          │ 전달됨    │ → 재료 빠짐, 필기 점수
        └──────┘                             └──────────┘
   칸이 빔, 씨앗 회수 안 함                     칸이 빔
```
- 칸을 차지하는 상태 = 진행 중 + 완료 가능.
- "완료 가능"은 저장하지 않고 인벤토리로 매번 계산한다(재료가 줄면 자동으로 진행 중으로 돌아감).
- 7일 이후 남아 있는 의뢰는 전달할 수 없는 채 남는다(크리처가 "이제 늦었어" 안내).

### 4-2. 하루의 흐름

```
 [하루 시작]  day = N, 시각 = 16:00 + 보정(합친 값)
   │  1. 작물 하루 성장 → 2. 채집 자리 새로 뽑기 → 3. 게시판 교체(안 받은 것 만료)
   │  날 종류 확인
   ├─ 스토리 날(1·7·8): 시계 멈춤(원인 "스토리") ─ 스토리가 끝나면 FinishStoryDay() → 다음 날
   │     1일: 튜토리얼 의뢰만 게시, 튜토리얼 모드 켬, 표시 시각 = 밤 20:00 고정
   │          튜토리얼 의뢰 전달 → 1일 끝 → 2일 시작 (임시 흐름)
   ├─ 이벤트 날(3·4·5): 이벤트 단계 = 이번엔 "건너뜀" → 바로 방과 후
   └─ 자유 날(2·6) ↓
 [방과 후] 16:00 ─ 흐름 (구간 속도: 4시간/210초)
   │  19:40 경고 "곧 밤"
   ▼
 [밤] 20:00 ─ 흐름 (4시간/150초)   ← 게시판·크리처 전달 (밤에만 [제안])
   │  23:40 경고 "곧 하루 끝"
   ▼
 [하루 끝]
   ├─ 24:00 도달 → 경비원에게 걸림 → 다음 날 +60분 보정 요청
   └─ (선택) 스스로 끝내기 EndDayVoluntarily() → 보정 없음  [미정: 지금은 치트로만]
   ▼
 [다음 날 시작]  (8일 이후 없음: 시험 = 이번 범위 밖)

 어느 구간에서든: 멈춤 원인이 하나라도 있으면 시계 정지
   (책·가방·게시판·자세히 = 전체 정지 timeScale 0 / 대화·전달 고르기 = 시계만 / 스토리·이벤트·튜토리얼·맵 이동·치트 = 시계만)
```

---

## 5. 이벤트(알림) 목록

| 발행 | 이벤트 | 구독 |
|---|---|---|
| `GameClock` | `TimeChanged` (정시가 바뀔 때, `IGameTime`) | `TimeHudView` |
| `GameClock` | `HourChanged(day, hour)` | (디버그 표시, 나중에 시각 연동 연출) |
| `GameClock` | `PeriodChanged(TimeOfDay)` | 뒷산 `TimeOfDayState`(조명), `RequestBoard` 안내 갱신 |
| `GameClock` | `WarningChanged(TimeWarning)` (없음 / 밤 임박 / 하루 끝 임박) | `TimeHudView`(화살표 색·깜빡임), 토스트 |
| `GameClock` | `MidnightReached` | `DayFlow` |
| `DayFlow` | `DayEnded(day, reason: 경비원/스스로/스토리 끝)` | 경비원 안내 토스트, (나중에 세이브 D-N, 방으로 이동) |
| `DayFlow` | `DayStarted(day, DayKind)` | `DebugOverlay`, (나중에 이벤트·스토리 연결) |
| `TimePause` | `Changed` (+ `Causes` 목록) | `TimeScaleDriver`, `GameClockDriver`, `TimeHudView`("멈춰 있음"), `DebugOverlay` |
| `RequestBoardState` | `OffersChanged` | `RequestBoardWindow`, `DebugOverlay` |
| `RequestLog` | `RequestStateChanged(AcceptedRequest, 이전, 이후)` | 토스트, `DebugOverlay` |
| `RequestLog` | `AcceptedListChanged` (기존) | 드롭다운 `QuestListView`·`QuestBarView`, 책 `QuestBookPage`, `RequestBoardWindow` |
| `RequestLog` | `RequestBecameComplete` (기존) | 완료 알림 `QuestToastView` |
| `RequestLog` | `Delivered(DeliveryReport)` | 점수 연출, `DayFlow`(1일 튜토리얼 끝 판단) |
| `RequestLog` | `Abandoned(AcceptedRequest)` | 토스트 |
| `DeliverySession` | `StepChanged` (고르기 / 대사 / 끝) | `CreatureNpc`(고르기도 대사창 선택지로) |
| `GatherDayState` | `Changed` | `GatherHabitat`/`GatherPickup` 보이기, `DebugOverlay` |
| `FarmGrid` | `PlotChanged` (기존, 하루 성장 때도) | `FarmFieldView`·`CropView` |
| `TutorialMode` | `Changed`, `StepChanged(허용 활동)` | `DayFlow`(시계 정지 원인), `FarmField`·`FishingDriver`·`GatherPickup`(잠금 안내) |
| `ScoreBook` | `ScoreChanged` (기존) | 점수 HUD |
| `Inventory` | `SlotChanged` (기존) | `RequestLog`(진행 재계산), `TutorialMode`(단계 진행) |

씬을 넘어 사는 객체(시계·의뢰·게시판·밭)에 씬 오브젝트가 구독하므로, 기존 규칙대로 **OnDestroy에서 반드시 구독 해제**한다.

---

## 6. 데이터 (ScriptableObject와 예시 값)

| 에셋 | 위치 | 예시 값 |
|---|---|---|
| `TimeConfig` | `Data/Time/` | 구간: 방과후 16:00~20:00 / 210초, 밤 20:00~24:00 / 150초 · 경고: 19:40 밤 임박, 23:40 하루 끝 임박 (사용자 결정으로 20분 전) · 하루 시작 16:00 · 경비원 +60분 · 보정 범위 −90 ~ +120분 · 프레임당 최대 경과 0.25초 |
| `CalendarConfig` | `Data/Time/` | 1일 크리처와 만남·튜토리얼(스토리, 표시 20:00 밤) · 2일 자유 플레이 · 3일 지각 레이스 · 4일 마법 대결 · 5일 석고상 보스전(3~5일 이벤트, 지금은 건너뜀) · 6일 마지막 의뢰 전달 · 7일 불꽃 축제(일과 없는 스토리 날) · 8일 급수 시험 → 엔딩 · 시험 날 8 · **새 게임 시작 날 2(임시, 튜토리얼 단계에서 1로)** |
| `QuestConfig` | `Data/Quest/` | 최대 수락 3 · 쉬움·보통 1일부터, 어려움 2일부터(2026-11-25 사용자 결정, 원래 3일) · 튜토리얼 1일 · 마지막 전달 6일 · 게시판 닫힘 7일부터 · 같은 의뢰 다시 게시 금지 2일 · 다음 날 자라는 작물 의뢰 마감 여유 1일(→ 5일까지 게시) · 점수: 흔함 2 / 보통 4 / 희귀 8 · 튜토리얼 의뢰 `Request_Tutorial` · 밤에만 열기 켬 |
| `RequestPool` | `Data/Quest/` | 쉬움 6 · 보통 6 · 어려움 6 (`Data/Quest/Requests/`, 아래) |
| `GatheringConfig`(수정) | `Data/Config/` | 종류마다 자리 8 · 켜짐 4 · 희귀도 60/30/10 · 재료 12종 · 서식지 종류별 슬롯 수(씬 배치용, 지금 3) |
| `FishingConfig`(수정) | `Data/Config/` | 희귀도 60/30/10 · 붕어(흔함)·메기(보통)·초롱초롱 블롭피쉬(희귀) |

**재료 종(2026-11-24 사용자 구상, FBX 제작 중 → 모양은 임시 프리팹)**

| 출처 | 흔함 (2점) | 보통 (4점) | 희귀 (8점) |
|---|---|---|---|
| 농작 (성장, 2026-11-25 균형 결정) | 검은 무 (물 주고 3시간) | 꼿꼿한 벼 (5시간) | 괴물 고구마 (다음 날 아침) |
| 낚시 | 붕어 | 메기 | 초롱초롱 블롭피쉬 (빠름) |
| 채집: 큰 나무 그늘 | 그늘버섯 | 가시잣 | 달빛버섯 |
| 채집: 바위틈 | 돌이끼 | 반짝돌가루 | 별조각돌 |
| 채집: 덤불 | 산딸기 | 가시풀잎 | 반딧불열매 |
| 채집: 초입 평지 | 민들레 | 쑥 | 이슬꽃 |

- 설정에서 뺀 옛 에셋(파일은 남김): 무·대파·호박과 씨앗, 번개 붕어, 이끼방울, 옛 의뢰 7개. FBX가 들어와 새 재료로 바꾼 뒤 정리.

**의뢰 19개** (`Data/Quest/Requests/`, 대사는 임시)
- 튜토리얼: 검은 무 1 + 붕어 1 + 민들레 1 (6점)
- 쉬움 6 (4점): 검은 무 2 / 붕어+민들레 / 산딸기 2 / 그늘버섯+검은 무 / 돌이끼+붕어 / 민들레 2
- 보통 6 (6~8점): 꼿꼿한 벼+검은 무 / 메기+그늘버섯 / 쑥+민들레+돌이끼 / 가시풀잎+산딸기 2 / 가시잣+붕어 / 반짝돌가루+돌이끼+검은 무
- 어려움 6 (12~14점): 괴물 고구마+꼿꼿한 벼(다음 날 작물) / 블롭피쉬+메기 / 이슬꽃+쑥 / 반딧불열매+산딸기+가시풀잎 / 달빛버섯+가시잣 / 별조각돌+반짝돌가루
- 작물 재료는 개수만큼 씨앗을 준다. `_baseReward`는 옛 `RequestTracker`용으로 점수 합을 넣어 둠(3단계에서 `WrittenScoreRule`로 대체)

---

## 7. 세부 규칙 제안

### 7-1. 시계
- 시각은 **하루 0시부터의 분(float)**. 정시·경고·구간 경계를 지나칠 때는 한 프레임에 여러 개를 지나도 **순서대로 하나씩** 이벤트를 낸다(배속 ×16에서도 빠짐없이).
- 속도는 지금 시각이 속한 구간의 `(끝−시작)분 / 현실 초`. 방과 후가 15:30에 시작하면 첫 구간보다 이른 시각은 첫 구간 속도를 쓴다.
- 구동은 `Time.unscaledDeltaTime`(timeScale과 무관) × 치트 배속. 멈춤 = 씬의 `TimePause`(화면) + 진행 상태의 멈춤(스토리·튜토리얼·맵 이동·치트). 디버그 표시는 둘의 원인을 합쳐 보여 준다.
- 낚시·마법진 등 월드 동작은 지금처럼 `Time.deltaTime`. 치트 배속은 시계만 빠르게 한다. **[제안]** 촬영용으로 "월드도 같이 빠르게"(timeScale도 배속) 토글을 치트 패널에 둔다.

### 7-2. 멈춤 원인
- `TimePause.Acquire(level, cause)` / `Release(cause)`. 같은 원인을 두 번 등록해도 한 번으로 센다(꼬임 방지), 원인마다 단계(전체/시계만)를 기억. 모두 해제돼야 흐름.
- 원인은 열거형 `PauseCause` + 화면은 `GameScreen` 값으로 자동 생성. 맵 이동(페이드) 중에는 "맵 이동" 원인을 등록한다(지침의 "시간을 따로 쓰지 않는다"를 "그동안 흐르지 않는다"로 해석).

### 7-3. 다음 날 시작 시각 보정
- 요청: `RequestNextDayShift(source, minutes)` — 원인(경비원, 지각 레이스 등)마다 한 값만 기억(같은 원인이 다시 오면 덮어씀).
- **[제안] 합치기 = 늦추기 중 가장 큰 값 + 앞당기기 중 가장 큰 값**, 범위로 자름. 예) 레이스 승리 −30 + 경비원 +60 → +30(16:30 시작). 경비원 +60과 석고상 패배 +60 → +60(17:00, 벌을 두 번 겹치지 않음). 다 더하는 방식보다 결과가 예측 가능하다.
- 쓰고 나면 비운다.

### 7-4. 스스로 하루 끝내기 [미정]
- **[제안]** 방(기숙사 침대)에 "자기" 상호작용을 두고, 밤에만 가능, 확인 창 후 `DayFlow.EndDayVoluntarily()` → 경비원 보정 없이 다음 날. 이번에는 메서드와 치트만 만들고 기본은 24:00까지 흐름.

### 7-5. 게시판 뽑기 (반복 방지)
- 난이도마다 후보 = 풀 − 지금 수락 중인 의뢰 − 이미 전달한 의뢰 − 최근 2일 안에 게시된 의뢰.
- 후보가 비면 단계적으로 완화: ① 최근 게시 제한 풀기 → ② 전달한 의뢰 허용 → ③ 그 난이도 생략. **제한을 푼 단계에서는 가장 오래전 게시된 의뢰만 후보**로 남겨 어제 것이 바로 다시 나오지 않게 한다(구현 중 추가).
- **다음 날 아침에 자라는 작물이 필요한 의뢰**는 `게시 날 + 1 > 마지막 전달 날`이면 게시하지 않는다(6일 게시판 제외, 2026-11-25 결정).
- 후보 안에서는 **덜 게시된 의뢰일수록 가중치 높게**(1 / (1 + 게시 횟수)).
- 1일은 튜토리얼 의뢰 1개 고정. 7일 이후는 게시하지 않음.

### 7-6. 씨앗 지급·다시 받기
- 수락 시: 의뢰의 지급 씨앗을 전부 넣을 공간이 없으면 수락 거절("가방에 씨앗 넣을 자리가 없어요") — 지금 규칙 유지.
- 다시 받기: 작물별로 **더 줄 수 있는 씨앗 = (진행 중 의뢰들이 요구하는 그 작물 합) − (가진 작물) − (밭에 심긴 그 씨앗, 아직 안 거둔 것) − (가진 그 씨앗)**, 0보다 작으면 0. 게시판 창의 "씨앗 다시 받기" 버튼(크리처 대화에도 같은 메서드를 연결할 수 있음).
- 이 식이면 몇 번을 눌러도 "의뢰를 끝내는 데 필요한 만큼"을 넘을 수 없다. 이미 낸 의뢰는 진행 중이 아니므로 빠진다.
- **[결정 2026-11-25, 기능은 아직 없음] 덜 자란 작물 파내기**: 나중에 넣으면 파낸 칸의 **씨앗을 가방에 돌려준다**(밭 −1, 가방 씨앗 +1이라 위 식의 값이 그대로 → 다시 받기로 더 얻을 것 없음). 지금 밭에서는 다 자란 작물만 뽑을 수 있다.

### 7-7. 전달
- `DeliverySession.Begin()` → 완료 가능 목록이 비었으면 "아직 다 모으지 못했어" + 부족한 것 한 줄.
- 고르면 그 순간 다시 확인 → 크리처 대사(전달 대사 + 재료마다 설명) → **마지막 대사 뒤에** 재료 빼기·점수(대사 도중 끊기면 아무것도 안 바뀜, 지금 규칙 유지) → 남은 완료 가능 의뢰가 있으면 다시 고르기, 없으면 끝.
- 고르기 창·대화 중에는 게임 시계만 멈춤(대화와 같게).

- [제안, 3단계 승인 2026-11-25] 완료 가능한 의뢰가 **하나뿐이면 고르기 창 없이** 바로 대사로 간다. 둘 이상일 때만 `DeliveryChoiceWindow`.
- 크리처: 낮에는 "밤에 와" 안내, 7일부터(마지막 전달 날 뒤)는 "이제 늦었어" 안내.

### 7-8. 포기
- `RequestLog.Abandon(id)` — UI와 무관한 메서드. 확인 창은 부르는 쪽(UI) 책임. 칸이 비고, 포기한 의뢰는 게시판에 다시 올라오지 않음(그날 게시판에서도 사라진 상태). 그 밖의 손해 없음.
- **[결정 2026-11-25, 아직 구현 안 함 — 지금은 씨앗을 그대로 둠] 포기하면 지급한 씨앗 회수**: 그 의뢰가 준 씨앗 종류마다 **가방에 있는 씨앗만, 준 개수까지** 회수한다(`회수 = min(준 개수, 가방의 그 씨앗 수)`). **이미 밭에 심은 씨앗·자라는 작물·거둔 작물은 건드리지 않는다**(어느 칸이 어느 의뢰 몫인지 가릴 수 없고, 키운 작물이 사라지면 벌처럼 느껴짐). 다른 받은 의뢰도 같은 씨앗이 필요해 모자라게 되면 7-6 "씨앗 다시 받기"로 채운다.
- [제안, 3단계 승인] 포기한 의뢰는 게시판 뽑기에서 **전달한 의뢰와 같게** 다룬다: 평소엔 빠지고, 후보가 바닥나는 마지막 완화 단계에서만 허용.
- 포기 버튼은 책 의뢰 탭 오른쪽 페이지(`QuestBookPage`)에 둔다(자세히 패널은 책 의뢰 탭으로 대체됨, UI_SPEC 2026-11-20).

### 7-8-1. 게시판 창·치트 (3단계 승인)
- 수락한 의뢰는 게시판(`RequestBoardState.Offers`)에서 빠지고, 창에는 남은 의뢰만 보인다. 씬별 `_boardRequests` 목록은 없앤다.
- 옛 `RequestCheatKeys`(F7·F10·F11·F12)와 뒷산 F2(의뢰 교체)는 지우고, 치트 패널에 "의뢰" 탭을 최소로 만든다(받은 의뢰 재료 넣기, 받은 의뢰 모두 비우기, 게시판 열기). 나머지 의뢰 치트는 5단계.

### 7-9. 채집 자리
- 자리 번호 = (서식지 종류, 그 종류 안 순번 0~7). **같은 종류 서식지 오브젝트들에 번갈아 매긴다**(각 서식지의 0번 슬롯 → 1번 슬롯 …). 서식지 수가 종류마다 달라도(지금 그늘 6·바위틈 4·덤불 3·평지 3개) 자리가 고루 퍼진다. 서식지 슬롯은 3칸(합계 9~18칸, 앞 8자리 사용). 씬 슬롯 합계가 자리 수보다 적으면 경고 로그.
- 하루 시작에 뽑은 결과를 진행 상태에 두므로 씬을 나갔다 와도 같은 자리, 주운 자리는 그날 다시 안 생김.
- 재료의 방과후/밤 출현 구분은 쓰지 않음(하루 한 번 뽑기, 사용자 구상도 모두 방과후). 필드는 남김.
- 희귀도 표에는 "포획" 자리를 만들어 두되 0으로 둠(나중에 포획 표 65/30/5).

### 7-10. 튜토리얼 모드
- 켜면: 원인 "튜토리얼"로 시계 정지 / 성장 규칙을 `InstantGrowthRule`로 / 낚시 입질 대기 0 + 튜토리얼 의뢰의 낚시 재료 지정 / 채집 켜진 자리에 튜토리얼 의뢰의 채집 재료를 반드시 놓음 / 순서 잠금.
- 순서 잠금: 튜토리얼 의뢰의 농작 재료를 가방에 얻으면 낚시 허용, 낚시 재료를 얻으면 채집 허용. 잠긴 활동은 안내 문구("먼저 텃밭부터 해 보자")와 함께 막힘. 세 기능은 각각 켜고 끌 수 있음.
- 1일 시작 때 자동으로 켜고, 튜토리얼 의뢰를 전달하면 끔.

---

## 8. 확장 지점

| 확장 | 붙는 자리 |
|---|---|
| 세이브 (D-N 날짜별) | 모든 상태가 `ToSnapshot/Restore`를 가짐. `DayFlow.DayEnded`/`DayStarted`에서 "N일째 시작" 스냅숏을 날짜별 슬롯에 쓰면 됨. 지금은 자동 저장(슬롯 0)에 v2로만 |
| 포획 재료 | `ItemSource.Capture`, `RarityTable`의 포획 칸, `IDayStartStep` 하나 더(포획 동물 배치) |
| 이벤트 결과 → 다음 날 시작 | `DayFlow.RequestNextDayShift("race", −30)` 한 줄 |
| 장르 이벤트·스토리 날 | `CalendarConfig`의 날 종류 + `IDayContent`(나중에) — 이벤트가 끝나면 `DayFlow.FinishEvent(시작 시각)` / `FinishStoryDay()` |
| 밤 길이 조정 | `TimeConfig` 구간 현실 초. 날마다 다르게 하려면 `CalendarConfig` 날짜별 구간 덮어쓰기 칸 추가 |
| 스스로 하루 끝내기 | `DayFlow.EndDayVoluntarily()` + 침대 상호작용 |
| 하루 시작 위치(방으로 이동 등) | `DayStarted`를 구독하는 Gameplay 컴포넌트에서 `SceneTravel` |
| "느려져라" 주문 | `IFishSpeedModifier` 구현을 `FishingSession`에 넣기 |
| 책 의뢰 상세 페이지 | `RequestLog.Abandon`, `ExpectedReward`, `SeedRefillRule` 이미 있음 |

---

## 9. 고민한 선택지

| 고민 | 고른 것 | 다른 안과 이유 |
|---|---|---|
| 시계를 어디에 둘까 | `PlayerProgress`(씬을 넘어 유지) + 씬마다 얇은 구동기 | DontDestroyOnLoad 싱글톤 MonoBehaviour: 간단하지만 Core 테스트가 어렵고 세이브와 분리됨 |
| 시계 시간원 | `unscaledDeltaTime` + 멈춤 원인 | `Time.deltaTime`: 전체 정지(timeScale 0)에는 맞지만 "시계만 멈춤"·치트 배속과 섞이면 꼬임. 지침도 timeScale과 별개를 요구 |
| 멈춤 방식 | 원인 집합(같은 원인 중복 등록 무시) | 카운트: 지금 방식. 열고 닫기 짝이 어긋나면 영영 멈추고, 디버그 표시에 원인을 못 보여 줌 |
| 의뢰 규칙 | `RequestTracker`를 새 `RequestLog`로 교체 | 고치기: Active·_progress 하나 구조라 거의 모든 메서드가 바뀜. UI 인터페이스는 그대로라 UI 쪽 영향은 작음 |
| 완료 가능 상태 저장? | 저장 안 하고 매번 계산 | 저장: 같은 재료를 두 의뢰가 요구할 때 어긋남. 계산하면 지침 2-5 "고를 때마다 다시 계산"이 저절로 됨 |
| 밭 격자 위치 | `PlayerProgress`로 옮김 | 보관본 유지: 하루 시작이 텃밭 밖에서 일어나면 보관본과 격자 두 군데를 키워야 함 |
| 채집 뽑기 시점 | 하루 시작에 데이터로 뽑아 하루 유지 | 씬 들어올 때마다 뽑기(지금): 들락날락하면 다시 뽑혀 지침 "하루 시작 때 켜진다"와 어긋남 |
| 보정 합치기 | 방향별 최댓값 합 | 모두 더하기: 벌이 겹쳐 너무 늦어짐 / 최댓값 하나: 승리 보상과 경비원 벌이 서로 지움 |
| 치트 UI | 별도 어셈블리 + IMGUI | uGUI: 프리팹·테마 연결 비용, 정식 빌드에서 빼기 번거로움 |
| 게시판 시간 제한 | 밤에만(설정으로 끔) | 아무 때나: 지침 "밤에 굴에서" 흐름과 어긋남. 테스트 때는 치트로 20:00 이동 |

---

## 10. 질문 → 결과 (2026-11-24 "제작 시작" 때 답이 없어 [제안]대로, 이후 결정 반영)

1. 새 어셈블리 `VoxelMagic.DebugTools` → **만듦**. 기존 `Gameplay/Debug` 치트 키(F7·F10·F11·F12·`[` `]`)는 아직 그대로, 의뢰 치트는 3단계에서 패널로 옮김
2. 게시판·전달 밤에만 → **[제안]대로 밤에만**(`QuestConfig.Only At Night`, 3단계에서 적용)
3. 기존 의뢰 7개 → **새 의뢰 19개로 교체**(사용자 재료 구상 기준, 2026-11-24)
4. 희귀도 배정 → 6장 재료 표대로(사용자 구상 종 기준으로 다시 정함)
5. 날짜 표시 → **1일째 D-7 … 8일째 D-Day**(사용자 일정표와 같음)
6. 경비원 뒤 시작 위치 → [제안]대로 지금은 제자리
7. 세이브 v2 → 3단계에서 (기존 저장 파일은 새 게임으로 시작)
8. (2026-11-25 사용자 결정) 포기하면 가방에 남은 지급 씨앗만 준 개수까지 회수, 심은 것은 그대로(7-8, 구현 전). 덜 자란 작물 파내기는 넣게 되면 씨앗을 가방에 돌려줌(7-6). 어려움 의뢰 2일부터(6장)

---

## 11. 제작 순서 (승인 후, 지침 8장)

1. 시계·멈춤 원인·하루 흐름(Core) → 단위 테스트 → `GameClockDriver`·`TimeHudView` 연결 → 디버그 표시·시간 치트 → 플레이 확인·보고
2. 하루 시작 처리: 희귀도·출처 데이터, 작물 시간 성장, 채집 자리 뽑기, 게시판 교체 → 테스트 → 플레이 확인
3. 의뢰 개편: `RequestLog`, 수락 제한·씨앗 지급·다시 받기·포기(확인 창)·전달 고르기·점수, 세이브 v2 → 테스트 → 플레이 확인
4. 튜토리얼 모드 → 확인
5. 나머지 치트 → 확인
6. 완료 기준 1~10 처음부터 확인, 사용자 안내 → 피드백 → 문서(`DESIGN_QUEST_TIMER`·`SETUP`·`CHANGELOG`·`UI_INTEGRATION_GUIDE`) 정리

---

## 12. 제작 현황과 다음 대화 인수인계 (2026-11-25)

> 3단계부터는 새 대화에서 진행한다. 새 대화는 `CLAUDE.md` → 이 장 → `QUEST_TIMER_SPEC.md` → `CHANGELOG.md` 위쪽(2026-11-24·25 항목) 순서로 읽는다.

### 끝난 것
- **1단계 (시계)**: `GameClock`·`DayFlow`·`TimePause`(원인별)·`StartTimeAdjustments`, `GameClockDriver`, 시간대 HUD(D-n, 경고 19:40·23:40 화살표 색·깜빡임, 화살표는 분 단위로 부드럽게 회전 — 회전 중심 (33, 16.5)), 치트 패널(마우스 버튼) + 디버그 표시
- **2단계 (하루 시작)**: 희귀도·출처, 작물 시간 성장(`TimedGrowthRule`, 흔함 3·보통 5시간·희귀 다음 날), 밭 격자를 `PlayerProgress`로, 채집 자리 하루 뽑기(`GatherDayRoller`·`GatherDayState`), 낚시 희귀도, 게시판 교체(`RequestBoardState`), 하루 시작 단계(작물 → 채집 → 게시판), `PlayerProgress.EnsureWorld(WorldSetup)`
- 재료 종 교체와 의뢰 19개, 균형 결정(2026-11-25)
- 테스트: EditMode 184개 통과 (`GameClockTests`·`DayStartTests`·`GatheringTests`·`FarmTests` 등)

- **3단계 (의뢰 개편, 2026-11-25)**: `RequestLog`(최대 3개·씨앗 지급·포기·골라서 전달·밤에만·6일 마감), `AcceptedRequest`, `DeliverySession`(하나면 바로 대사), `SeedRefillRule`, `WrittenScoreRule`(희귀도 합), `ConfirmChannel`. 임시 UI `DeliveryChoiceWindow`·`ConfirmDialog`(코드로 모양), 게시판 창 새 규칙·씨앗 다시 받기, 책 의뢰 탭 `포기`. 점수 상한 삭제. **세이브 v2**(수락·전달·포기 목록, 날짜·시각·보정, 게시판·반복 기록, 오늘 채집 자리, v1은 새 게임). 치트 패널 "의뢰" 탭(최소). 옛 `RequestTracker`·`ScoreCalculator`·`QuestTrackerPanel`·`RequestCheatKeys`·뒷산 F2 삭제
- 테스트: EditMode 190개 통과
- **3단계 뒤 추가 (2026-11-25, 사용자 요청)**: 어려움 의뢰 2일부터, 퀵슬롯 우클릭 버리기 치트 재연결 + 조작법 안내 줄, 치트 "이동" 탭(맵 이동 5곳), 밭 앞 칸 기준·바라보는 방향으로 돌려 심기·물 주기 미리보기(이미 물 줌 막기). 포기 시 씨앗 회수·파내기는 규칙만 기록(7-6·7-8). EditMode 198개 통과

### 남은 옛 흔적
- `GameBalanceConfig`·의뢰 에셋 파일 안에 지운 필드(`_maxScore`·`_newItemKindBonus`·`_baseReward`) 값이 남아 있음(읽지 않음, 에셋을 다시 저장하면 사라짐)
- `GameBootstrap`의 씬별 의뢰 목록은 없앰(게시판은 `RequestPool`만 씀)

### 3단계 이후 할 일 (11장 순서)
3. ~~의뢰 개편·세이브 v2~~ 완료
4. 튜토리얼 모드 (새 게임 시작 날을 1로 되돌림)
5. 나머지 치트(의뢰·재료·점수 탭)
   - 포기 시 씨앗 회수(7-8 결정, 아직 구현 안 함): 시점은 사용자와 정하기
6. 완료 기준 1~10 확인

### 작업할 때 주의
- **플레이 중에 스크립트를 다시 컴파일하지 않는다.** Unity 설정이 "다시 컴파일하고 계속 플레이"라 정적 진행 상태(`GameSession`)가 사라진다. 컴파일 전에 `EditorApplication.isPlaying` 확인
- 씬을 연 뒤에 에셋을 다시 읽어 넣는다(씬 열기 전에 읽은 참조는 풀려 저장될 수 있음)
- 플레이 확인 때 자동 저장을 끈다(`GameSaveSystem.AutoSaveEnabled = false`)
- 에디터에 포커스가 없으면 플레이 프레임이 돌지 않는다 → 시계는 코드로 `Advance`해서 확인
- Creature_Cave·BackMountain 씬은 원래부터 저장 정의 목록이 연결돼 있지 않아 저장 경고가 뜬다(이번 작업과 무관)

