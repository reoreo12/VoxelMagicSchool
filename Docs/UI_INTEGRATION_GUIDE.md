# UI 연결·제작 지침 (2026-11-25 기준, 의뢰 개편 반영)

다른 사람이나 다른 대화(모델)가 UI 작업을 이어받거나, 게임 규칙 쪽에서 이미 만든 UI에 기능을 붙일 때 읽는 문서다.
브랜치 `feature/tool-UI-editor`. 함께 읽을 것: `Docs/HANDOFF_UI.md`(진행 상황·열린 질문) → `Docs/UI_SPEC.md` 13장(결정) → 이 문서.

- 프로젝트 규칙은 `CLAUDE.md`가 우선한다: 설계 먼저 보여 주고 "제작 시작" 승인 뒤 제작, 스크립트 맨 위 1~3줄 용도 주석, 숫자는 데이터(테마)로, 파일은 `Assets/_Project/`·`Docs/` 안에만, 패키지·설정 변경 전에 묻기, 기능마다 플레이 모드 확인 후 보고, 커밋은 사용자가 말할 때만.
- 의존 방향: `UI → Core`, `Gameplay → Core`. **Core는 UI를 모른다.** UI는 Core의 인터페이스·이벤트만 읽고, 게임 규칙(점수 계산, 시간 흐름 등)은 Core·Gameplay가 한다.

---

## 1. 연결 구조 한눈에

```
GameBootstrap (씬마다, Gameplay)
  └ GameContext.Create(...)          ← Core 서비스 묶음
  └ (여기서 SetVitals)               ← 규칙 쪽 모델을 넣는 자리. 아래 바인딩보다 먼저! (시계는 Create가 진행 상태에서 넣음)
  └ _bindRoots 아래 모든 IGameContextBinder.Bind(context)   ← UI_Root도 여기에 들어 있음
```

- UI 뷰는 `IGameContextBinder.Bind(GameContext)`에서 필요한 서비스를 받아 **이벤트를 구독**하고, `OnDestroy`에서 구독을 푼다(점수·의뢰·인벤토리는 씬을 넘어 유지되므로 꼭 풀 것).
- **꺼진 채 씬이 내려간 뷰**는 Unity가 `OnDestroy`를 부르지 않아 구독이 남는다(한 번도 켜지지 않은 오브젝트). 그래서 씬을 넘어 유지되는 서비스(점수·의뢰 등)의 이벤트 핸들러는 첫 줄에 `if (this == null) { OnDestroy(); return; }`를 둔다(`ScoreHudView`·`QuestListView`·`QuestToastView`·`QuestBookPage` 참고).
- 매 프레임 값을 읽지 않는다. 값이 바뀔 때 이벤트로만 다시 그린다.
- `UI_Root.prefab`이 들어간 씬: `FarmPond`, `BackMountain`, `Creature_Cave`. 각 씬의 `GameBootstrap._bindRoots`에 UI_Root가 연결돼 있다. 새 씬에 UI를 넣으려면 UI_Root 프리팹을 놓고 `_bindRoots`에 추가한다.
- **주의(바인딩 시점)**: `StatusBarsView`(마력·기력)와 `TimeHudView`(시간)는 `Bind` 때 한 번 `context.Vitals` / `context.Time`을 읽는다. 그래서 `SetVitals`·`SetTime`은 **`GameBootstrap.Awake`에서 `GameContext.Create` 직후, 바인딩 반복문 전에** 불러야 한다. 나중에 넣어야 한다면 GameContext에 "바뀜" 이벤트를 더하고 뷰가 다시 구독하도록 고친다.

---

## 2. 기능별 연결 지점

표의 "UI가 읽는 창구"만 맞춰 주면 UI는 고치지 않아도 된다.

### 2-1. 마력·기력 바 (화면 오른쪽 아래)
| 항목 | 내용 |
|---|---|
| 지금 | 값 모델 없음 → **가득 찬 바**로 보임 |
| UI가 읽는 창구 | `Core/Common/IPlayerVitals`: `float ManaRatio`(0~1), `float StaminaRatio`(0~1), `event Action VitalsChanged` |
| 연결 방법 | 규칙 쪽에서 `IPlayerVitals`를 구현한 클래스를 만들고, `GameBootstrap.Awake`에서 바인딩 전에 `Context.SetVitals(모델)` |
| UI 코드 | `UI/HUD/StatusBarsView.cs`, `StatusBarView.cs`. 테마 `Status Bars` |
| 남은 [미정] | 수치 표시(툴팁 등), 값이 낮을 때 효과, 바뀔 때 연출(UI_SPEC 7-2) |

### 2-2. 시간대 UI (화면 오른쪽 위)
| 항목 | 내용 |
|---|---|
| 지금 (2026-11-24) | **게임 시계 연결됨.** `PlayerProgress.Clock`(`Core/Time/GameClock`)이 `IGameTime`을 구현하고 `GameContext.Create`가 `Time`에 넣는다(바인딩 전). 시계가 없는 씬(복도)에서는 테마 미리보기 값 |
| UI가 읽는 창구 | `Core/Common/IGameTime`: `Day`, `Hour`(0~24 정시), `Minute`(하루 0시부터의 분, 화살표 보간용·글자로 표시 안 함), `Period`(Day = 방과후, Night = 밤), `DaysUntilExam`(1일째 7 … 시험 날 0), `Warning`(None / NightSoon / DayEndSoon), `IsHeld`(스토리·맵 이동·치트 등 화면 밖 이유로 시계가 잡힘), `PeriodStartHour(period)`·`DayEndHour`(룰렛 각도용 구간 시각), 이벤트 `TimeChanged`·`WarningChanged`·`HeldChanged`. 화면 멈춤은 `context.Pause` |
| 시간 규칙 | 구간·속도·경고 시각은 `Data/Time/TimeConfig`(방과후 16~20시 210초, 밤 20~24시 150초, 경고 19:40·23:40부터 20분, 2026-11-24 사용자 결정), 날 종류는 `CalendarConfig`. 시계는 `GameClockDriver`가 `unscaledDeltaTime`으로 돌리고, `context.Pause`(화면)나 `Clock.Holds`(스토리 등)에 원인이 하나라도 있으면 멈춘다 |
| UI 표시 규칙 | 화살표는 `IGameTime.Minute`(소수)로 매 프레임 각도를 구해 따라간다(2026-11-24 사용자 결정, 시계가 멈추면 같이 멈춤, 시계 없는 미리보기는 정시 각도).부드럽게 돈다(원본 한 장 회전. 각도별로 구운 그림도 시험했으나 원본이 작은 둥근 판 모양이라 더 뭉개져 되돌림). 각도 구간은 시계의 구간 시각(방과후 시작·밤 시작·하루 끝)을 쓴다. 툴팁 `방과후 16:00`. 날짜 판 `D-n`, 시험 날은 문구 키 `ui.time.examDay`(`D-Day`). 경고 중에는 화살표가 테마 `Time › Warning Arrow Color`로 바뀌고 `Warning Blink Seconds` 간격으로 깜빡인다(unscaled time, 0이면 색만). 개발 빌드에서 시계가 멈추면(화면 멈춤 또는 `IsHeld`) "멈춰 있음" |
| 테마에 남은 값 | `Play Days`·`After School Start Hour`·`Night Start Hour`(20)·`Night End Hour`·`Preview *`는 **시계가 없을 때 미리보기용**으로만 쓴다 |
| UI 코드 | `UI/HUD/TimeHudView.cs`. 테마 `Time` (화살표 회전 중심 `Arrow Pivot` (33, 16.5) = 세로는 룰렛 반원 중심, 가로는 반원 중심(35)보다 2px 왼쪽(룰렛 그림을 덜 가리게, 2026-11-24 사용자 결정), 그림 안 중심 `Arrow Pivot In Sprite` (0,4) = 꼬리 가운데) |
| 남은 [미정] | 화살표 그림(비스듬할 때 도트가 바뀜 → 실루엣이 얇은 바늘 모양으로 다시 그리는 안), 경고 효과 최종 아트 |

### 2-3. 점수 (화면 위 가운데)
| 항목 | 내용 |
|---|---|
| UI가 읽는 창구 | `Core/Score/ScoreBook`: `Get(ScoreKind.Written / Practical)`, `event ScoreChanged(kind, old, new)` |
| 지금 | `ScoreBook`이 100점 상한(규칙). UI는 4자리까지 준비됨. **사용자가 "추후 상한 변경" 예정** |
| UI 코드 | `UI/HUD/ScoreHudView.cs`, `ScoreNumberView.cs`(외곽선 v2 숫자, 왼쪽 자리부터 그림). 테마 `Scores` |

### 2-4. 수락한 의뢰 (왼쪽 위 바·목록, 완료 알림, 책의 의뢰 목록 탭)
| 항목 | 내용 |
|---|---|
| UI가 읽는 창구 | `Core/Requests/IAcceptedRequestList`: `AcceptedCount`, `Accepted`(항목 목록), `event AcceptedListChanged`(수락·납품·포기·진행 변화), `event RequestBecameComplete(entry)`(조건을 다 채운 순간 → 완료 알림), `ExpectedReward(entry)`(지금 납품하면 받을 필기 점수 = 필요 재료 희귀도 점수 합, 보너스 없음) |
| 항목(`AcceptedRequestEntry`) | `Request`(RequestDefinition), `Title`, `AcceptedOrder`(먼저 수락할수록 작음, = `Id` 포기 호출용), `Requirements`(아이템·현재·필요), `Progress`(0~1), `IsComplete`(납품 가능) |
| 지금 (2026-11-25) | 구현은 `Core/Quest/RequestLog`(`context.Requests`). **최대 3개**(`QuestConfig`), 수락하면 씨앗 지급·게시판에서 빠짐. 규칙 메서드: `CanAccept/Accept(r)`, `Abandon(id)`, `CanDeliver/Deliver(id)`, `ReadyRequests()`, `RefillableSeeds()/GiveRefillSeeds()`, `BoardAccess()/DeliveryAccess()`(밤에만·닫힌 날), `RewardOf(r)` |
| 포기 | `RequestLog.Abandon(id)`는 UI와 무관. 확인 창은 부르는 쪽이 `context.Confirm.Ask(문구, 예 콜백)`(Core `ConfirmChannel` → UI `ConfirmScreen`(`UI_Root` 팝업 층 `UI_Confirm`, `GameScreen.Confirm`). 예·아니요 글자는 Ask의 yesLabel·noLabel로 바꿀 수 있음. `UI_Root`가 없는 씬만 예비 `ConfirmDialog`). 지금은 책 의뢰 탭 오른쪽 아래 임시 `포기` 글자 버튼 |
| 전달 | `context.Delivery`(`DeliverySession`): `Begin()` → `Choosing`(둘 이상) / `Talking` → `Choose(id)` → `CompleteTalk()` / `Cancel()`. 크리처(`CreatureNpc`)가 단계에 따라 고르기 창(`GameScreen.DeliveryChoice`, 임시 `DeliveryChoiceWindow`, 게임 시계만 멈춤)과 대사창을 띄움 |
| 정렬(UI가 함) | 완료 → 진행률 높은 순 → 먼저 수락한 순. 보상까지 받은 의뢰는 목록에서 빠진다 |
| UI 코드 | `UI/Requests/QuestBarView`(바, X = 숨기기), `QuestListView`(드롭다운, 항목 클릭 = 책 의뢰 탭, 마우스 올림 표시는 `QuestListItemView`), `QuestToastView`(완료 알림), `UI/Book/QuestBookPage`(책 의뢰 탭) |
| 의뢰 게시판 | `UI/Requests/RequestBoardScreen`(`UI_Root` 팝업 층 `UI_RequestBoard`, `GameScreen.RequestBoard`) + 칸 `RequestBoardEntryView`: 오늘 게시판(`context.Board.Offers`), 칸별 짧은 이유, 아래 안내 한 줄(게시판 전체 이유·씨앗 결과, 없으면 받은 수 n/3), `씨앗 다시 받기`. 여는 쪽은 `Screens.Open(GameScreen.RequestBoard)`만. 값은 테마 `RequestBoard`. `UI_Root`가 없는 씬만 예비 `RequestBoardWindow` |

### 2-5. 퀵슬롯 (화면 아래 가운데)
| 항목 | 내용 |
|---|---|
| UI가 읽는 창구 | `Core/Inventory/Inventory`: `SlotCount`, `GetSlot(i)`, `SelectedIndex`, `Select(i)`, `event SlotChanged(i)`, `event SelectionChanged(i)`. 아이콘은 `ItemDefinition.Icon`(16×16 도트, 없으면 글자 대체) |
| 입력 | 1~5 키·클릭·퀵슬롯 위 휠 = 칸 선택(휠은 끝 칸에서 멈춤, 돌아 넘어가지 않음). 게임 화면 위 휠 = 카메라 줌(`HudState.IsPointerOverScrollHud`로 구분, `GameInputReader`) |
| 치트 (에디터·개발 빌드만, 2026-11-25) | 칸 **우클릭** = 개수와 상관없이 그 칸 통째로 버리기(`Inventory.DiscardSlot` + 토스트). `QuickSlotView.Setup(index, onClick, onRightClick)`, 정식 빌드에서는 우클릭을 연결하지 않음. 안내는 `ControlsHintView` 맨 아래 한 줄(`GameTexts.CheatDiscardHint`, 개발 빌드만). 정식 버리기 UI는 [미정] |
| UI 코드 | `UI/HUD/QuickSlotBarView.cs`, `QuickSlotView.cs`. 테마 `Quick Slots` |
| 남은 일 | 확장 인벤토리(가방, 7-4): 패널·칸 그림 없음, [미정] 많음 |

### 2-6. 책(메뉴) 화면
| 항목 | 내용 |
|---|---|
| 상태 | `Core/Screens/BookState`(`context.Book`): `Tab`, `LastChosenTab`, `FocusRequest`, `PrepareForIcon()`(책 아이콘 = 마지막으로 고른 탭), `PrepareForMenu()`(ESC = 메뉴 탭), `PrepareForRequest(r)`(의뢰 탭 + 그 의뢰), `ChooseTab(t)`. 여는 것은 `context.Screens.Open(GameScreen.Book)` |
| 탭 순서 | 의뢰 목록 → 마법 스펠 → 마법진 → 환경 설정 → 나가기/종료 메뉴 (`BookTab` enum 순서 = 위→아래) |
| 탭 전환 | 페이지 넘김 연출(9프레임, unscaled), 탭 위 휠 = 이전·다음 탭 |
| 페이지 규칙 | **모든 탭: 왼쪽 = 고르는 곳 또는 장 표지, 오른쪽 = 내용**(사용자 결정 2026-11-23). 고를 것이 있는 탭(의뢰 목록, 앞으로 마법 스펠·마법진·환경 설정 분류)은 왼쪽에 목록, 고를 것이 없는 탭(메뉴)은 왼쪽에 장 표지 |
| 장 표지 | `UI/Book/BookCoverView`: 탭 아이콘 정수 배(4배 = 48) + 탭 이름(굵게) + 구분선 + 한 줄 문구(Small, 옅은 색), 세로 가운데. 아이콘은 테마 탭 값에서, 문구는 Texts 키. 테마 `Book › 장 표지` |
| 메뉴 탭 | `UI/Book/MenuBookPage`: 왼쪽 장 표지(`메뉴` / `잠시 책을 덮고 쉬어 가요.` 임시), 오른쪽 `나가기`(임시) 제목 + 버튼. **배치만**. `책 닫기`만 동작. `메인 화면으로`·`게임 종료`는 `MainMenuRequested`·`QuitRequested` 이벤트에 기능을 붙인다(붙이기 전에는 로그만). 실수 방지 확인 창은 [잠정] |
| 환경 설정 탭 | 빈 페이지. 소리·화면 설정 위젯(그림 없음) + "의뢰 목록 표시" 토글(지금은 의뢰 탭에 있음). **켜짐 상태 저장**(`HudState.IsQuestBarVisible`)은 설정 저장을 만들 때 |
| 마법 스펠·마법진 탭 | 빈 페이지. 주문 아이콘 6종·마법진 도감 그림 없음 |
| 페이지 안 버튼 | `BookMenuButton`(그림 `book_ui/buttons/btn_page_*`, 테마 `Book › Page Button`): 글자 모양 기준으로 좌우·상하 여백을 같게 맞춘다. 새 페이지 버튼도 이것을 쓴다 |
| 새 탭 페이지 만들기 | `BookPage`를 상속(`Relayout()`에서 `Left`·`Right` 칸 안에 배치, `OnOpened()`는 펼쳐질 때 — 글자 크기를 재는 배치는 숨겨진 동안 정확하지 않으니 `OnOpened`에서 다시 배치), `UiBookBuilder`에 페이지 생성 추가. 왼쪽 페이지 규칙(목록 또는 `BookCoverView` 장 표지)을 따른다 |

### 2-7. 화면 스택·시간 정지·입력
- 팝업 종류: `Core/Screens/ScreenPolicy`의 `GameScreen`(Book, Inventory, RequestBoard, RequestDetail(지금 안 씀), Dialogue, DeliveryChoice(시계만 멈춤), Confirm). 임시 창(고르기·확인)은 `UI/Common/TempUiBuilder`로 코드에서 모양을 만든다(씬에는 컴포넌트만, HUDCanvas 아래).
- 정책(`ScreenPolicy.DefaultFor`): 책·가방·게시판 = 전체 정지(timeScale 0) + 뒤쪽 입력 막기 + 플레이어 조작 막기. 대사(Dialogue) = 게임 시계만 정지(캐릭터 애니메이션은 계속) + **ESC·바깥 클릭으로 닫히지 않음** + 딤 없음.
- 시간 정지는 `TimePause`가 **원인별로** 등록·해제한다(2026-11-24, 화면은 `화면:<GameScreen>` 원인, 같은 원인은 하나로 셈, `Causes`로 목록). 실제 timeScale은 `Gameplay/TimeScaleDriver`(개발용 월드 배속 `DebugWorldSpeed` 포함).
- 팝업 화면은 `UiScreen`을 상속하고 UI_Root의 `Popup` 층에 둔다. `UiScreenRouter`가 스택을 보고 켜고 끄며, 그 아래에 입력 막기 판(딤, 검정 알파 0.6)을 깐다. **판을 누르면 맨 위 화면이 닫힌다**(ESC와 같음).
- ESC(`GameInputReader`): 열린 화면을 위에서부터 닫음(닫히지 않는 화면이면 아무것도 안 함) → 없으면 낚시 취소 → 없으면 `Screens.RequestMenu()`(책 메뉴 탭). Q: 의뢰 목록 접기/펼치기(`HudState.ToggleQuestList`).
- 진행(Submit): 열린 화면이 있을 때 **Space**(`Gameplay/Submit` 액션)와 **E**(`PlayerInteractor`)가 모두 `Screens.Submit()` → `ScreenStack.Submitted`. 대사 넘기기 등에 쓴다.
- 연출(페이지 넘김, 완료 알림 페이드, 퀵슬롯 선택 이동)은 모두 `Time.unscaledDeltaTime`.

### 2-8. 초상화 대사창 (크리처 대화·수업)
| 항목 | 내용 |
|---|---|
| 재생 창구 | `Core/Dialogue/DialogueChannel`(`context.Dialogue`): `Play(lines, onFinished)`, `IsPlaying`, `HasPresenter`(이 씬에 대사창 UI가 있는지), `Finish()`(UI가 마지막 대사 뒤에 부름), `Abort()` |
| 대사 줄 | `DialogueLine(speaker, text, expression)`. 화자는 `Core/Data/SpeakerProfile`(SO: 이름표 이름, 기본 초상화 72×72, 표정 키별 초상화). 데이터 `Data/Speakers/`(지금 `Speaker_It` = '그것': 기본 `it_normal`, 표정 키 `surprised`·`closed`·`squint`·`happy`·`shy`·`dark`) |
| 초상화 그림 | `Art/UI/Portrait/Creature/Creature_Portrait_Atlas_72.png`(72×72 칸, 크리처를 칸 가운데로 맞춤, 임포트 규칙으로 이름 붙여 자름). 새 크리처는 원본을 72×72로 줄여 같은 폴더에 두고 `UiImportRuleApplier.DefaultRules`에 자르기 규칙 추가 → `임포트 규칙 적용` → 화자 SO에 연결. 배경이 투명하면 초상화 뒤 단색(테마 `Portrait Backing Color`)이 보인다 |
| **결과 반영 규칙** | 아이템 소모·의뢰 완료·보상은 **`onFinished` 안에서만** 한다. 대사 도중 강제 종료되면 아무것도 바뀌지 않아 "말 걸기 전" 상태로 남는다(UI_SPEC 6-1). 예: `CreatureNpc.TryDeliver(context, speaker)`는 대사를 먼저 재생하고 끝나면 `Requests.Deliver` |
| 대사창 UI가 없는 씬 | `HasPresenter`가 false면 대사 없이 결과를 바로 반영한다. 초상화 없는 대사는 이 창이 아니라 머리 위 말풍선(아직 없음) |
| UI 코드 | `UI/Dialog/DialogueWindow`(UiScreen, Popup 층 `UI_Dialogue`): 패널(320×96) → 초상화(72×72) → 왼쪽 열(80×96) → 이름 → 본문 → ▼. 4px 겹쳐 396×96, 1.25배(495×120, 사용자 결정), 화면 아래 가운데. ▼는 글자판 오른쪽·아래 여백 8 타이핑 `maxVisibleCharacters`(초당 30자, unscaled), 같은 프레임 중복 입력 무시, 클릭·Space·E로 진행. 본문 낱말 단위 줄바꿈, 3줄 넘으면 콘솔 경고 |
| 이름 맞춤 | 갈무리11 Bold 12px(4자) → 갈무리9 10px(5자) → 갈무리7 8px(7자) 순으로 이름표 폭 72에 맞추고(6자 / 7자 / 9자까지), 세로는 실제 글자 모양(글리프) 기준으로 크림색 줄(4,79,72×13) 가운데, 작은 글씨는 1px 겹쳐 굵게. 그래도 넘치면 `…`. 테마 `Dialogue › Name Styles` |
| 선택지(6-4) | `context.Dialogue.Ask(질문 줄, DialogueOption 목록, 고른 뒤(번호), ESC 번호)`. `DialogueOption(글자, 오른쪽 작은 글자, 흐리게)`. 고른 뒤 콜백·대사 끝 콜백 안에서 다음 `Play`/`Ask`를 부르면 대사창이 닫히지 않고 이어진다. 아무것도 열지 않으면 닫힌다. 입력: 화면이 열려 있으면 `GameInputReader`가 이동 키를 `Screens.Navigate(±1)`로, 닫히지 않는 화면의 ESC는 `Screens.CancelIgnored`로 보낸다 |
| 크리처 대화 | `CreatureNpc`(Gameplay): 메뉴 → 이야기(화자 `SmallTalk`)·과외(임시 대사, **과외 시스템이 생기면 `Conversation.OnMenu`의 MenuLesson에서 연다**)·의뢰 전달(`DeliverySession` 단계를 `Ask`/`Play`로)·그만두기. 대사창·화자가 없는 씬은 메뉴 없이 바로 전달. 치트 `CreatureNpc.StartDelivery`는 메뉴 없이 전달부터 |
| 테마 | `Dialogue`(배수, 아래 여백, 겹침 4, 초상화 위치·**크기**, 이름표·본문·▼ 위치, 그림이 없을 때 쓸 왼쪽 열·패널·▼ 크기, 타이핑 속도, 열기 페이드 0.12초·6px, 선택지 칸·줄 간격·들여쓰기·커서·색). 그림 크기를 바꾸면 이 값들을 함께 고치고 메뉴 `화면 만들기/대사창`을 다시 실행 |
| 옛 코드 | `CreatureDialogView`는 아무것도 하지 않게 비워 둠(씬에 남아 있음, 옛 UI 정리 때 삭제). `GameMessages.CreatureSpeechRequested`는 더 쓰지 않음 |

### 2-9. 그 밖의 HUD
- `HudState`(`context.Hud`): 의뢰 목록 펼침(`IsQuestListExpanded`), 의뢰 바 보이기(`IsQuestBarVisible`), 휠 쓰는 HUD 위 마우스(`IsPointerOverScrollHud`).
- 아이템 획득 창·토스트: 옛 `AcquisitionPopup`·`ToastView`(`GameMessages`). 새 지침으로 아직 안 바꿈.
- 옛 프로토타입(`InventoryBar`, `ScoreHud`, `QuestTrackerPanel`)은 세 씬에서 꺼 둠. `ControlsHintView`(왼쪽 아래 조작 안내)는 최종에 지움(사용자 결정). 지금은 개발 빌드에서 치트 안내 한 줄(우클릭 버리기)을 덧붙임.

---

## 3. UI 제작 틀 (새 UI를 만들 때)

### 3-1. 파일 위치
| 무엇 | 어디 |
|---|---|
| 런타임 UI 스크립트 | `Scripts/UI/<영역>/` (Common·Widgets·HUD·Requests·Book …), 네임스페이스 `VoxelMagic.UI` |
| Core 창구(인터페이스·상태) | `Scripts/Core/<영역>/`, 네임스페이스 `VoxelMagic.Core` (UnityEngine UI 참조 금지) |
| 에디터 빌더·도구 | `Scripts/Tools/Editor/UI/`, 메뉴 `VoxelMagic/UI/...` |
| 테마(크기·간격·색·스프라이트) | `Data/UI/UiTheme.asset`. 값 묶음 클래스는 `UI/Common/UiThemeHudValues.cs`, 테마에 필드·속성 추가는 `UiTheme.cs` |
| 문구 | `Data/UI/Texts_ko.asset`(TextTable), 키 이름은 `UI/Common/UiTextKeys.cs` |
| 프리팹 | `Prefabs/UI/HUD/*.prefab`(빌더가 저장), 공용 위젯 프리셋 `Prefabs/UI/Widgets/`(W_Text, W_Button, W_PaperPanel, W_Scrollbar) |
| 그림 | `Art/UI/<묶음>/`. 임포트 규칙 `Data/Tools/UI/UiImportRules.asset`(메뉴 `임포트 규칙 적용`) |

### 3-2. 위젯·뷰 패턴
- 테마를 따르는 것은 `ThemedWidget`을 상속하고 `Apply(UiTheme theme)`에서 **테마 값으로 크기·위치·스프라이트를 다시 입힌다**. 숫자를 코드에 박지 않는다. 편집 모드에서도 테마를 고치면 한 박자 뒤 적용된다.
- Core에 연결되는 뷰는 `IGameContextBinder`도 구현한다. `Bind`에서 구독, `OnDestroy`에서 해제, 씬을 넘어 유지되는 서비스의 핸들러는 첫 줄에 `if (this == null) { OnDestroy(); return; }`(1장 참고).
- `ThemedWidget.OnEnable/OnDisable/OnValidate`를 덮어쓸 때는 **`protected override` + `base.` 호출**(안 하면 테마 구독이 깨진다).
- 글자는 `ThemedText`(스타일: Body·BodyBold 12px 갈무리11, Small 10px 갈무리9, Tiny 8px 갈무리7, HudTitle·HudLabel 굵게). 직접 문자열 대신 `SetKey` 또는 `Theme.Texts.Get(키)`. 색만 바꿀 때 `SetColorOverride`.
- 한글 긴 문장 줄바꿈은 `UiTextWrap.ByWords`(TMP 기본은 낱말 중간을 끊음).
- 상속은 2단계까지(예: ThemedWidget → BookPage → QuestBookPage). 꼭 필요하면 3단계, 이유를 적는다.

### 3-3. 픽셀 규칙
- 캔버스: 기준 높이 480, 배율 = floor(화면 높이 ÷ 480)(1080p = 2배, 가상 960×540). PPU 16, Pixel Perfect.
- 배치는 `PixelLayout`(PlaceTopLeft·TopRight·BottomLeft·BottomRight·Stretch)로 **정수 px**. 가운데 정렬은 0.5 앵커 대신 왼쪽 끝에서 내림 계산.
- 확대는 정수 배가 원칙. 0.5 단위(1.5배)는 1080p에서만 고르게 보이고, 그 밖의 소수 배율은 도트 폭이 고르지 않고 조각 사이 틈이 생긴다(시간대 UI에서 확인됨).
- 9-slice가 필요한 그림은 임포트 규칙에 경계(왼·위·오·아래)를 넣고 `Image.Type.Sliced`(또는 반복 종이는 Tiled).
- 색은 리니어 공간이라 반투명(딤 등)은 생각보다 약하게 보인다.

### 3-4. 층(UI_Root 안 캔버스)
| 층 | 정렬 | 쓰임 |
|---|---|---|
| HudStatic | 0 | 거의 안 바뀌는 HUD(왼쪽 위 버튼, 퀵슬롯 바) |
| HudDynamic | 10 | 이벤트로 자주 바뀌는 HUD(의뢰 바·목록, 점수, 상태 바, 시간대) |
| HudAnimated | 20 | 움직이는 HUD(완료 알림) |
| Popup | 100 | 책 등 팝업 + 입력 막기 판 |
| Overlay | 200 | 툴팁 등 맨 위 |

- **클릭을 받는 위젯을 넣는 층에는 GraphicRaycaster가 있어야 한다**(HudAnimated에 없어서 완료 알림 X가 안 눌렸던 적이 있음). 지금은 다섯 층 모두 있음.
- 클릭이 필요 없는 그림은 `raycastTarget = false`.

### 3-5. 빌더(에디터 메뉴)로 만드는 방식
- UI는 손으로 씬에 만들지 않고 **빌더 메뉴**로 프리팹을 만들어 UI_Root 안의 같은 이름을 바꿔 끼운다. 테마 값은 덮어쓰지 않고 비어 있는 것만 채운다.
- 지금 있는 메뉴(`VoxelMagic/UI/`): `HUD 만들기/`(왼쪽 위 메뉴·의뢰 바, 퀵슬롯·상태 바, 점수, 시간대·툴팁, 의뢰 목록·완료 알림), `화면 만들기/책`·`화면 만들기/대사창`(열린 씬의 크리처에 화자 연결까지), `UI 루트 프리팹 만들기`, `테마 기본값·위젯 프리셋 만들기`, `임포트 규칙 적용`, `픽셀 폰트 다시 굽기`, `외곽선 버전/`, `미리보기/`, `UI 작업대`(해상도·검사·굽기·테마), `UI 에디터 씬 열기`.
- 새 HUD를 만들 때: (1) 테마 값 묶음 추가 → (2) 문구 키 추가 + 빌더에서 Seed → (3) 뷰 스크립트 → (4) 빌더(`Tools/Editor/UI/UiHud<이름>Builder.cs`)에서 프리팹 생성·층에 넣기·참조 연결 → (5) 메뉴 실행 → (6) 플레이 확인.

### 3-6. 리스트·성능
- 개수 제한이 없는 목록은 보이는 만큼만 항목을 만들어 재사용(가상 스크롤, `QuestListView`·`QuestBookPage` 참고). 정렬·높이 계산은 목록이 바뀔 때만.
- 픽셀 글꼴 글자를 칸·바탕 안 세로 가운데에 둘 때는 줄 높이가 아니라 `UiTextInk.CenterVertically(글자, 칸 위, 칸 높이)`를 쓴다(글자 모양이 줄 위쪽에 붙어 있어 줄 높이로 맞추면 위로 치우침). 책 안 글자 버튼은 `BookTextButton`.
- 스크롤바(`W_Scrollbar`)를 쓰는 화면은 연결할 때 `ThemedPanel.EnableScrollbarInput(scrollbar)`를 부른다. 테마 패널은 기본으로 마우스를 받지 않아, 이것이 없으면 손잡이를 끌 수 없다(휠만 됨).
- 같은 값이면 다시 그리지 않는다(`ThemedText.SetText`도 같은 글자면 무시).
- LayoutGroup·ContentSizeFitter 대신 코드로 정수 px 계산.

---

## 4. 확인·테스트 방법
1. 스크립트를 고치면 Unity 콘솔에 컴파일 오류가 없는지 본다.
2. 빌더 메뉴를 다시 실행해 프리팹을 갱신한다.
3. 플레이 모드 확인 전에 **자동 저장을 끈다**(`GameSaveSystem.AutoSaveEnabled = false`, 또는 GameBootstrap의 Auto Save 체크 해제). 안 끄면 테스트 아이템·점수가 저장 파일에 남는다.
4. 치트(에디터·개발 빌드): F7 = 진행 중 의뢰의 모자란 재료 1개, F12 = `[Cheat] Requests` Inspector에서 고른 아이템, F10 = 의뢰 게시판, F11 = 의뢰 전달(씬 크리처의 화자로 대사창을 거친 뒤 납품), 대사창이 떠 있을 때 `]`/`[` = 초상화 다음/이전 표정(그 줄에서만). (F1~F6은 뒷산 디버그, F8 자유 카메라, F9 이동 속도)
5. 여러 의뢰 미리보기: 메뉴 `VoxelMagic/UI/미리보기/`(플레이 중).
6. 캡처는 `Temp/UiCaptures`에. **Unity 창이 포커스가 없으면 게임 화면이 다시 그려지지 않아** 캡처가 이전 화면일 수 있다 → 같은 장면을 두 번 찍거나 값(위치·크기)을 뽑아 확인. 플레이 중 `Time.frameCount`가 늘지 않으면 `Application.runInBackground = true` + `EditorApplication.QueuePlayerLoopUpdate()`로 프레임을 돌린다(이 프로젝트는 PlayerSettings에서 이미 켜져 있음).
7. EditMode 테스트(지금 153개)가 통과하는지 본다.
8. `UI 작업대 › 검사`로 정수 위치·불필요한 Raycast 등 배치 규칙을 확인한다.

## 5. 기록
- 기능 하나가 끝날 때마다: `Docs/CHANGELOG.md`(맨 위), `Docs/HANDOFF_UI.md`(진행 표·열린 질문), 사용자 결정은 `Docs/UI_SPEC.md` 13장.
- 설정·패키지를 바꿨다면 `Docs/SETUP.md`도.
