# 변경 기록

## 2026-11-25 — 균형 결정: 작물 성장 3·5시간
- 사용자 결정: 흔함 2시간이면 밤 20:00에 받은 의뢰를 그날 밤 바로 끝낼 수 있어 계획하는 맛이 약함 → **흔함 3시간 · 보통 5시간**, 희귀(괴물 고구마)는 다음 날 아침 그대로. 검은 무·꼿꼿한 벼 씨앗 값과 설명, 씨앗 기본값(3시간) 변경
- 문서 정리(새 대화 인수인계): `DESIGN_QUEST_TIMER.md` 6·7·10장을 구현에 맞추고 12장 "제작 현황과 다음 대화 인수인계" 추가, `CLAUDE.md` 작업표에 의뢰·타이머 행 추가(사용자 승인, 현재 작업 표시 이동), `PROJECT_CONTEXT.md`·Notion 세계관 페이지(타이머·작물·재료 표·의뢰·인터뷰 기록) 갱신
- 다음 날 자라는 작물 의뢰의 게시 마감은 5일까지(지금 값) 유지. 옛 재료·의뢰 에셋(무·대파·호박·씨앗, 번개 붕어, 이끼방울, 옛 의뢰 7개)은 남겨 둠(FBX가 들어와 새 재료로 완전히 바꾼 뒤 정리)

## 2026-11-24 — 작물 시간 성장, 새 재료 종, 의뢰 목록 개편
- 사용자 요청: 8일에 비해 작물 성장이 김 → **게임 시간 기준 성장**. 씨앗 `GrowHours`(0 = 다음 날 하루 시작). 흔함 2시간 · 보통 4시간 · 희귀 다음 날 아침. `TimedGrowthRule`(옛 `DailyGrowthRule` 대체), `FarmPlot.WateredAt`(누적 분), `GameClock.AbsoluteMinute`·`MinuteChanged`, `FarmGrid.SetClock`·`UpdateGrowth`. 시계가 멈추면 성장도 멈춤. 저장 기록 `WateredAt`
- 균형 보완: 다음 날 자라는 작물이 필요한 의뢰는 마지막 전달 날(6일)에는 게시하지 않음(`QuestConfig.Next Day Crop Lead Days` 1, `RequestDefinition.NeedsNextDayCrop`)
- 새 재료(사용자 구상, FBX 제작 중 → 모양은 기존 임시 프리팹): 농작 검은 무(흔함, 2시간)·꼿꼿한 벼(보통, 4시간)·괴물 고구마(희귀, 다음 날) + 씨앗, 낚시 초롱초롱 블롭피쉬(희귀, 빠름), 채집 가시잣(그늘·보통). 그늘버섯 보통 → 흔함. 채집 12종 설명을 사용자 문구로, 빈 크리처 설명 채움
- 설정에서 뺀 것(에셋 파일은 남김): 무·대파·호박(+씨앗), 번개 붕어(→ 블롭피쉬), 이끼방울(→ 가시잣). 붕어·메기는 낚시 흔함·보통 자리로 유지(구상에 낚시 종이 하나뿐이라)
- **의뢰 19개 새로**(`Data/Quest/Requests/`): 튜토리얼 1(검은 무·붕어·민들레), 쉬움 6(흔함 1~2개), 보통 6(보통 1 + 흔함 1~2), 어려움 6(희귀 1 + 다른 것 1~2), 모두 합계 3개 이하, 서식지 힌트·크리처 대사·전달 대사, 작물 재료만큼 씨앗 지급. `RequestPool`·`QuestConfig.Tutorial Request`·네 씬 부트스트랩 의뢰 목록·저장 정의 목록 갱신. 옛 의뢰 7개는 목록에서 뺌(파일은 남김)
- 테스트: 작물 시간 성장 4개·게시 마감 1개 → EditMode 184개 통과. 플레이: 16:00 물 → 18:00 검은 무, 20:00 벼, 다음 날 고구마
- 남은 것: 옛 에셋(무·대파·호박·씨앗, 번개 붕어, 이끼방울, 옛 의뢰 7개) 지울지 사용자 결정. 뒷산 `MountainDebugInput`의 의뢰 교체 목록은 옛 의뢰(3단계에서 정리)

## 2026-11-24 — 의뢰·타이머 2단계: 하루 시작 처리 (작물 · 채집 · 게시판) + 희귀도
- **아이템 희귀도·출처**(`Core/Data/ItemTypes`): `ItemDefinition`에 `Rarity`(흔함/보통/희귀)·`Source`(농작/낚시/채집/포획-보류). 배정: 무·대파 흔함, 호박 보통 / 붕어 흔함, 메기 보통, 번개 붕어 희귀 / 채집은 서식지마다 낮 재료 둘(흔함·보통) + 밤 재료 하나(희귀): 평지 민들레·쑥·이슬꽃, 덤불 산딸기·가시풀잎·반딧불열매, 그늘 이끼방울·그늘버섯·달빛버섯, 바위틈 돌이끼·반짝돌가루·별조각돌 / 동물(포획) 산토끼·다람쥐·여우
- `RarityTable`(Core/Spawning): 희귀도 가중치 뽑기(기본 60/30/10), 그 희귀도 후보가 없으면 낮은 희귀도로
- **작물**: 씨앗 `GrowDays`(무·대파 1일, 호박 2일). `DailyGrowthRule`(기본) = 물 준 뒤 하루 시작을 그 날 수만큼 지나면 다 자람, `InstantGrowthRule`은 튜토리얼용으로만 남김. `FarmGrid.AdvanceDay`·`GrowAll`(치트)·`TryRestore`. **밭 격자를 `PlayerProgress`로 옮김**(씬을 넘어 하나, 어느 씬에서든 하루 시작에 자람). 저장 기록에 `DaysWatered`·`Grown` 추가. `FarmFieldView`·`FarmSoilView`는 연결 때 이미 있는 작물을 그림. `GameBootstrap`의 밭 보관·복원 코드 삭제
- **채집**: `GatheringConfig`를 새 규칙으로(서식지 종류마다 자리 8 · 켜짐 4 · 희귀도 60/30/10 · 재료 12종 목록), 옛 최소·최대·추가 확률 삭제. `GatherDayRoller`가 하루 시작에 뽑고 `GatherDayState`(진행 상태)가 하루 동안 유지(씬을 오가도 같은 자리, 주운 자리는 그날 안 생김). `GatheringService`는 씬 슬롯 → 자리 번호만 이음(같은 종류 서식지들에 번갈아 매김). `GatherSpawner`·시간대 재배치·의뢰 재료 보장 삭제. 뒷산 서식지 슬롯 2 → 3(그늘 18·바위틈 12·평지 9·덤불 9칸, 앞 8자리 사용). `MountainBootstrap`의 시간대는 게임 시계를 따라감, F1 = 채집 자리 다시 뽑기, F3 = 방과후↔밤 시각 이동. 채집 밸런스 창 갱신
- 버그 수정: 평지 서식지 `HideOnPlay`가 부트스트랩 뒤에 돌아 그 아래 슬롯 재료 메시까지 꺼서 처음에 재료 4개가 안 보였음 → 채집 슬롯 메시는 건너뜀
- **낚시**: `FishingConfig` 희귀도 60/30/10 → 그 희귀도 물고기 중 가중치. `FishingSession.ForcedNextRarity`(치트)·`ForcedFish`(튜토리얼)·`SpeedModifier`(`IFishSpeedModifier`, 주문 "느려져라" 자리)
- **게시판**: `RequestDefinition`에 난이도·서식지 힌트·합계 3개 경고. `QuestConfig`(`Data/Quest/`: 최대 수락 3, 쉬움·보통 1일·어려움 3일부터, 튜토리얼 1일, 마지막 전달 6일, 7일부터 닫힘, 반복 방지 2일, 점수 2/4/8, 밤에만), `RequestPool`(지금 의뢰 7개, 난이도는 재료 희귀도로 임시 배정: 쉬움 3·보통 3·어려움 1). `RequestBoardState`: 하루 시작에 난이도별 하나씩, 남은 의뢰는 만료, 받아 둔·전달한·최근 2일 게시 의뢰 제외 → 모자라면 가장 오래전 게시된 것부터 풀기, 덜 게시된 의뢰 가중치
- **하루 시작 단계**(`Core/Time/DayStartSteps`): 작물(1) → 채집(2) → 게시판(3). `DayStartReason`(새 게임 첫날엔 작물 안 자람). `PlayerProgress.EnsureWorld(WorldSetup)`이 시계·밭·채집·게시판을 한 번 만들고 단계를 건다. `GameBootstrap`에 `Gathering`·`Quest`·`Request Pool` 필드(네 씬 연결)
- 치트 패널 "재료·게시판" 탭: 채집 자리 다시 뽑기·켜진 자리 표시, 작물 모두 다 자라게, 게시판 새로 뽑기, 다음 낚시 희귀도 고정. 디버그 표시에 게시판·채집 자리·작물 줄
- 테스트: `GatheringTests` 새 규칙으로 다시 씀(비율 60/30/10 통과), `DayStartTests` 추가(게시판·낚시 비율·단계 순서), `FarmTests` 하루 성장 → EditMode 182개 통과
- 확인(플레이, 자동 저장 끔): 텃밭 2일 → 무·호박 심고 물 → 3일 무 자람·호박 1/2일 → 4일 호박 자람(작물 모양 바뀜), 게시판 2일 쉬움·보통 / 3일부터 어려움 포함 3개, 뒷산에서 같은 날 채집 자리 16곳(종류마다 4) 표시·줍기. 콘솔 에러 없음
- 아직: 의뢰 수락·전달은 옛 `RequestTracker`(3단계에서 교체). 게시판 창 UI는 아직 옛 목록. 지금 의뢰 7개는 합계 3개 규칙·난이도 구성과 다른 것이 있음 → 3단계에서 의뢰 데이터 정리

## 2026-11-24 — 구운 화살표 되돌림
- 사용자 판단: 비스듬한 각도에서 구운 그림이 더 뭉개져 화살표처럼 안 보임 → **되돌림**. 원인은 원본 화살표가 "둥근 갈색 판 안에 1px 크림색 화살표 무늬"라 어떤 회전 방식이든 비스듬한 각도에서 무늬가 깨짐(바늘 모양 B안·부드러운 회전 C안 비교 그림을 보여 드렸고, 지금은 원래 방식 선택)
- `TimeArrowBaker`·구운 시트·임포트 규칙·테마 `Arrow Frames`·`TimeHudView`의 구운 그림 분기를 지움. 원본 한 장을 분 단위로 부드럽게 회전, 회전 중심 (33, 16.5)은 유지
- 그림을 바꾸고 싶으면 나중에 바늘 모양처럼 실루엣이 얇은 화살표를 그리는 것을 검토

## 2026-11-24 — 시간대 화살표를 각도별로 구운 그림으로
- 사용자 피드백: 도는 동안 중심이 흔들리고 도트 모양이 바뀜. 원인 ① 회전 중심이 화면 픽셀 사이(1.5배라 (49.5, 24.75)px) ② 도트 한 장을 매 프레임 소수 각도로 돌리면 최근접 샘플링이라 각도마다 모양이 바뀜
- 해결(사용자 선택): **각도별로 미리 구운 화살표를 바꿔 끼움**(UI_SPEC 7-3 방식 2). 새 에디터 도구 `Tools/Editor/UI/TimeArrowBaker`, 메뉴 **`VoxelMagic/UI/시간대 화살표 굽기`**: 테마 `Time › Arrow`(원본 10×8)를 RotSprite(Scale2x 3번 = 8배 → 회전 → 8×8 점 중 가장 많은 색)로 75° → −75°, **3° 간격 51장** 굽기 → `Art/UI/Sprout Lands (수정본)/weather/time_arrow_baked.png`(24×24 칸 8열, 칸 가운데 = 회전 중심) → 임포트 규칙(Grid 24×24, `UiImportRules`에 규칙 추가)으로 자르기 → 테마 `Time › Arrow Frames`에 넣음
- 테마 `Time`: `Arrow Frames`·`Arrow Frame Step`(3°) 추가, `ArrowFrame(angle)`. `TimeHudView`: 구운 그림이 있으면 돌리지 않고 가장 가까운 각도 그림으로 바꿈(회전 0, 그림 가운데를 `Arrow Pivot`에), 없으면 예전처럼 한 장 회전
- 확인(플레이): 16:00 0번 → 17:20 8번 → 20:00 25번 → 23:59 50번, 화살표 위치(회전 중심)가 모든 시각에서 같은 화면 좌표, 회전 0. 콘솔 에러 없음
- 주의: 시트는 Sprout 수정본 폴더라 git에 안 들어간다(라이선스). 다른 PC에서는 메뉴로 다시 굽는다. 원본 화살표가 10×8로 작아 비스듬한 각도에서는 안쪽 밝은 화살표 무늬가 흐려진다(필요하면 칸을 손으로 다듬거나 원본을 키운다)

## 2026-11-24 — 임박 경고 20분 전으로, 룰렛 화살표 회전 중심 맞춤
- 사용자 요청: 1시간 전부터 화살표가 빨개지는 게 정신없음 → 경고 구간 **19:40~20:00(밤 임박), 23:40~24:00(하루 끝 임박)**. `Data/Time/TimeConfig` 값과 코드 기본값 함께 변경. 테스트 `임박_경고는_20분_전부터` 추가(EditMode 173개 통과)
- 화살표가 중심보다 아래에서 도는 문제: 테마 `Time › Arrow Pivot`이 (35.08, 18.10)으로 룰렛 반원 중심보다 1.6px 아래였음. 룰렛 그림(13×17)을 확인해 반원 중심 = 룰렛 왼쪽 변 가운데(틀 기준 **(35, 16.5)**)로 맞춤. 화살표 그림 안 회전 중심 (0,4)(꼬리 가운데)는 맞았음. 플레이에서 화살표 회전 중심과 룰렛 반원 중심의 화면 좌표가 같음(차이 0px) 확인. 콘솔 에러 없음
- 이어서 사용자 피드백(세로·각도는 좋음, 화살표가 룰렛판에 너무 가까워 방과후·밤 그림을 가림): 회전 중심 가로만 35 → **33**(2px 왼쪽). 테마 값과 코드 기본값 함께

## 2026-11-24 — 일정표 이름, 치트를 버튼 UI로, 룰렛 화살표 보간
- 사용자 일정표 반영: `CalendarConfig` 날 이름 1 크리처와 만남·튜토리얼(D-7) / 2 자유 플레이 / 3 지각 레이스 / 4 마법 대결 / 5 석고상 보스전 / 6 마지막 의뢰 전달 / 7 불꽃 축제(일과 없음, 스토리 날 = 시계 멈춤) / 8 급수 시험 → 엔딩(D-Day). 날 종류는 그대로
- 치트: 키보드에 Insert가 없고 Home·PgUp·PgDn·End가 Fn 조합이라 불편 → **모든 동작을 패널 버튼으로**. 화면 왼쪽 가장자리 `치트` 버튼으로 여닫음(` 키도 됨). 맨 위 줄 = 일시 정지·디버그 표시·배속 ×1~×16·"월드도 같은 배속", 시간·날짜 탭 = 시각 이동, **1~8일 날짜 버튼**(D-n 표시), 다음 날·스토리 날 끝내기·스스로 끝내기·다음 날 보정. 나머지 단축키는 기본 없음(`[Cheat] Hub` Inspector에서 지정 가능)
- 룰렛 화살표(사용자 결정, UI_SPEC 13장의 "정시마다 스텝"을 바꿈): 게임 시계의 분(소수)으로 매 프레임 각도를 구해 **부드럽게 돈다**. 시계가 멈추면 같이 멈춤. `IGameTime.Minute` 추가(분을 글자로 보여 주지는 않음), 테마 `ArrowAngle(float hour, …)` 추가. 시계 없는 미리보기는 정시 각도
- 확인: EditMode 172개 통과, 플레이(FarmPond)에서 16:00 75° → 16:30 65.6° → 20:00 0° → 23:59 −74.7°로 연속, 패널 캡처. 콘솔 에러 없음
- 주의: 플레이 중에 스크립트를 다시 컴파일하면 진행 상태(정적)가 사라진다(이번에 한 번 겪음) → 플레이를 끄고 다시 켤 것

## 2026-11-24 — 의뢰·타이머 1단계: 게임 시계 · 원인별 멈춤 · 하루 흐름 · 시간 치트
- 설계 `Docs/DESIGN_QUEST_TIMER.md` 승인("제작 시작", 10장 질문은 [제안]대로). 브랜치 `feature/quest-timer`
- Core `Time/`: `TimeConfig`(구간 방과후 16~20시 210초·밤 20~24시 150초, 경고 19·23시, 하루 시작 16:00, 경비원 +60분, 보정 범위 −90~+120분), `CalendarConfig`(1·7·8일 스토리, 3·4·5일 이벤트(건너뜀), 2·6일 자유, 시험 8일), `GameClock`(`IGameTime` 구현, 경계를 순서대로 알림, 배속), `DayFlow`(하루 시작 단계 `IDayStartStep`, 경비원·스스로·스토리 끝, 마지막 날 정지), `StartTimeAdjustments`(늦추기 최대 + 앞당기기 최대)
- `TimePause`: 참조 카운트 → **원인별 등록·해제**(`Acquire(level, cause)`, `Release(cause)`, `Causes`). `ScreenStack`은 `화면:<이름>` 원인. `PauseCauses`(스토리·이벤트·튜토리얼·맵 이동·치트·마지막 날)
- `IGameTime`에 `DaysUntilExam`·`Warning`·`IsHeld`·`PeriodStartHour`·`DayEndHour`와 이벤트 추가. `PlayerProgress.Clock/Days`(씬을 넘어 유지), `GameContext.Clock/Days`
- Gameplay: `GameClockDriver`(unscaledDeltaTime, 화면 멈춤·시계 잡기 없을 때만, 맵 이동 중 잡기), `GameBootstrap`에 `Time`·`Calendar` 필드(네 씬 연결), `TimeScaleDriver.DebugWorldSpeed`
- UI `TimeHudView`: D-n을 Core 값으로(1일째 D-7, 시험 날 `D-Day` 문구 키 `ui.time.examDay`), 룰렛 각도를 시계 구간 시각으로, 경고 중 화살표 색·깜빡임(테마 `Time › Warning Arrow Color`·`Warning Blink Seconds`), "멈춰 있음"에 시계 잡기도 포함. 테마 미리보기 `Night Start Hour` 기본 19 → 20
- 새 어셈블리 `VoxelMagic.DebugTools`(`Scripts/Debug/`): `CheatHub`(스스로 생김), `CheatPanel`(` 키, 시간 탭), `DebugOverlay`(Insert, 오른쪽), `TimeCheats`. 단축키 Home 일시 정지 · PageUp/PageDown 배속 ×1~×16 · End +1시간. 패널: 시각 이동(16·19·20·23·23:59), 다음 날·그날로 이동, 스토리 날 끝내기, 스스로 하루 끝내기, 다음 날 보정 ±, 월드도 같은 배속(촬영용)
- 임시: `CalendarConfig.New Game Start Day = 2`(튜토리얼 전)
- 테스트: `GameClockTests` 13개 추가, `ScreenStackTests` 원인별 멈춤으로 수정 → EditMode 172개 통과
- 확인(FarmPond 플레이, 자동 저장 끔): 2일 16:00 시작 → 19:00 경고(화살표 색) → 20:00 밤 → 23:00 경고 → 24:00 경비원 → 3일 17:00 시작(D-5), 책 = 전체 정지·대화 = 시계만 정지·겹쳐 열고 닫아도 다시 흐름, 치트 패널·디버그 표시 캡처. 콘솔 에러 없음

## 2026-11-24 — 초상화 가운데 정렬, 표정 넘기기 치트 키
- `Creature_Portrait_Atlas_72.png`: 원본 칸 안에서 크리처가 오른쪽으로 치우쳐 있어(왼쪽 여백 3, 오른쪽 1) 표정 6칸을 1px 왼쪽으로 옮겨 좌우 2/2로 맞춤. 세로는 위 4·아래 3(높이 65라 더 맞출 수 없음). 어둠 칸은 꽉 찬 그림이라 그대로
- 치트 키(에디터·개발 빌드): 대사창이 떠 있을 때 `]` = 다음 표정, `[` = 이전 표정. 화자의 기본 → 표정 목록 순서로 돌고, 다음 대사로 넘어가면 그 줄의 표정으로 돌아간다. 콘솔에 지금 표정 키 표시. `Gameplay/Debug/DialogueCheatKeys`(스스로 생김, "[Cheat] Dialogue") → Core `DialogueChannel.RequestPreviewExpression(step)` → UI `DialogueWindow`(의존 방향 유지). `SpeakerProfile`에 `ExpressionCount`·`ExpressionKey(i)` 추가
- 확인(플레이 모드, 자동 저장 끔): 키 입력으로 7개 순환·되돌리기, 다음 줄에서 그 줄 표정(happy)으로 복귀, 초상화 좌우 여백 같음

## 2026-11-24 — 크리처 초상화 적용 ('그것' 표정 7종)
- 사용자가 넣은 `Art/UI/Portrait/Creature/Creature_Portrait_Atlas_336.png`(1344×672)·`_64.png`(256×128), 둘 다 4×2칸에 표정 7개. 64 버전은 336을 면적 평균으로 줄인 것과 픽셀까지 같음 → 같은 방식으로 336을 **72×72로 줄여 `Creature_Portrait_Atlas_72.png`(288×144)** 를 만듦(초상화 창 72에 늘림 없이 1:1). 반투명 가장자리는 알파 128 기준으로 불투명/투명
- 임포트 규칙에 이름 붙인 자르기 추가: `it_normal`·`it_surprised`·`it_closed`·`it_squint`·`it_happy`·`it_shy`·`it_dark`. 규칙 적용으로 원본 336·64 시트도 Point·PPU 16·압축 없음이 됨(쓰지는 않음)
- 화자 `Speaker_It`: 기본 초상화 `it_normal`, 표정 키 `surprised`·`closed`·`squint`·`happy`·`shy`·`dark`(대사 줄의 expression에 이 키를 넣으면 바뀜, 지금 대사는 표정 키를 안 씀)
- 초상화가 배경이 투명해 게임 화면이 비치므로 초상화 아래에 단색 배경 `PortraitBacking` 추가. 테마 `Dialogue › Portrait Backing Color`(임시 #E9D6AD, 크림보다 조금 진함). 빌더가 새 화자를 만들 때 기본 초상화로 `it_normal`을 씀
- 확인(플레이 모드, 자동 저장 끔): 표정 키 7개 모두 해당 그림으로 바뀜, 캡처 확인

## 2026-11-24 — 대사창 배율 1.25, ▼ 여백 맞춤
- 사용자 결정: 1.5배는 너무 커서 **1.25배**로 줄임(글씨도 함께 작아짐). 테마 `Dialogue › Scale`은 0.05 단위로 받게 바꿈. 1080p에서 도트 하나가 2.5px라 폭이 조금 고르지 않음(1·1.5·2배만 고름)
- 화면(960×540)에서 495×120, x 232~727, 아래 8~128
- ▼ 위치 (290,82) → **(301,80)**: 글자판(크림색) 오른쪽 끝·아래 끝에서 같은 여백 8(전에는 오른쪽 19, 아래 6)
- 확인(플레이 모드): 실제 납품 대사로 열어 위치·여백 확인
- 이름 세로 가운데: 전에는 줄 높이(textBounds) 기준이라 12px 굵게에서 위 여백 0·아래 2로 위에 붙어 보였음. 실제 글자 모양(글리프 위·아래) 기준으로 바꾸고, 이름표 칸을 위 베벨·아래 그늘을 뺀 크림색 줄 (4,79) 72×13으로 고침 → 12px 위아래 1, 10px 2, 8px 3으로 같음

## 2026-11-24 — 대사창 초상화·이름표 확대 (A안)
- (아래 배율 1.5·▼ (290,82)는 같은 날 1.25·(301,80)으로 바뀜)
- 그림: `dialogue_left_column.png` 64×80 → **80×96**(초상화 창 (4,4) 72×72, 이름표 (4,78) 72×14), `dialogue_panel.png` 320×80 → **320×96**(글자판 안쪽 (5,5)~(315,91)). 균일한 안쪽 줄을 복제해 늘려서 테두리·베벨 모양은 같음. 파일 이름·GUID 유지. `_preview` 합성 그림 갱신
- 높이는 패널도 96으로 맞춤(사용자 결정, A안). 겹침 4 그대로, 전체 396×96 → 1.5배 594×144(960×540에서 x 183~777, 아래 8~152). 퀵슬롯은 덮고 마력·기력 바(x 약 896~)와는 떨어짐
- 테마 `Dialogue`: `Portrait Size`(72×72) 추가, 그림이 없을 때 쓸 `Left Column Size`(80×96)·`Panel Size`(320×96)·`Arrow Size`(7×4) 추가, 이름표 (4,78,72×14), 본문 (14,20,292×48), ▼ (290,82). `DialogueWindow`의 숫자(56×56, 64×80, 320×80, 7×4)를 테마 값으로 바꿈
- 테스트용 초상화는 새로 만들지 않고 `portrait_placeholder_56.png`를 72×72로 늘려 씀(사용자 결정, 정수 배가 아니라 도트 폭이 조금 고르지 않음). `SpeakerProfile` 설명 문구 72×72
- 이름 맞춤(이름표 폭 72, 여백 0): 12px 굵게 6자, 10px 7자, 8px 9자까지. 6자는 이름표를 꽉 채움
- 확인(플레이 모드, 자동 저장 끔): 실제 납품 대사로 열림, 이름 4~10자 글씨 변화 확인, EditMode 157개 통과

## 2026-11-23 — 초상화 대사창 (UI_SPEC 6장)
- 설계 대화에서 온 `Art/UI/UI_SPEC.md` 사본의 6장(대사)을 `Docs/UI_SPEC.md`에 합치고 사본은 지움(사용자 승인)
- Core `DialogueChannel`(`context.Dialogue`: Play·Finish·Abort·HasPresenter), `DialogueLine`, `SpeakerProfile`(SO: 이름·기본 초상화·표정별 초상화). `GameScreen.CreatureDialog` → `Dialogue`로 이름 바꿈. 정책: 게임 시계만 정지(사용자 결정: 캐릭터 애니메이션 계속, 문제 생기면 전체 정지로), ESC·바깥 클릭으로 닫히지 않음
- 대사 결과는 마지막 대사 뒤에 한 번에: `CreatureNpc.TryDeliver(context, speaker)`가 완료 대사 + 재료 설명을 먼저 재생하고, 끝나면 `Requests.Deliver`. 도중 강제 종료면 아무것도 안 바뀜. 화자·대사창이 없으면 바로 납품. F11 치트도 씬 크리처 화자로 대사창을 거침
- UI `DialogueWindow`(Popup 층 `UI_Dialogue`): 패널(320×80) 위에 왼쪽 열(64×80)을 4px 겹쳐 380×80, 1.5배(그림이 640×360 기준이라 960×540에서 의도 크기), 화면 아래 가운데. 초상화(4,4)는 왼쪽 열 아래, 이름은 이름표(4,62,56×14) 가운데(글자 모양 기준 정수 px), 본문(14,14,292×48) 갈무리11 12px·낱말 줄바꿈·3줄 넘으면 경고, ▼(290,66) 1px 끊어 오르내림. 타이핑 `maxVisibleCharacters` 초당 30자(unscaled) → 전체 표시 → 다음. 같은 프레임 중복 입력 무시, 대사를 연 입력(E)은 진행으로 안 이어짐. 열 때 6px 올라오며 0.12초 페이드
- 이름 맞춤(제안 채택): 갈무리11 Bold 12px → 갈무리9 10px → 갈무리7 8px, 작은 글씨는 1px 겹쳐 굵게, 그래도 넘치면 `…`. 이름표 폭 56 기준 4자 / 5자 / 7자까지
- 입력: `GameControls` `Gameplay/Submit`(Space) 추가 → 열린 화면이 있으면 `Screens.Submit()`(E와 같음). SETUP.md 기록
- 대사 그림: 임포트(Point·압축 없음·PPU 16·Single) 확인, `dialogue_ui` 폴더를 UiAtlas에 등록(`_preview`는 텍스처라 안 들어감). 메뉴 `VoxelMagic/UI/화면 만들기/대사창`, 화자 `Data/Speakers/Speaker_It`(세 씬 크리처에 연결)
- 옛 `CreatureDialogView`는 아무것도 하지 않게 비움(새 대사 화면을 닫아 버리지 않도록)
- 확인(플레이 모드): 재료를 다 모으고 납품 → 대사창 열림·의뢰 그대로·점수 그대로·게임 시계만 정지, ESC로 안 닫힘, 같은 프레임 두 번 입력 = 한 번, 마지막 대사 뒤 닫힘 + 의뢰 완료 + 필기 15. 테스트 4개 추가(EditMode 157개 통과)

## 2026-11-23 — 책 메뉴 탭을 장 표지 + 오른쪽 버튼으로, 지침 갱신
- 사용자 결정: 책의 모든 탭은 왼쪽 = 고르는 곳 또는 장 표지, 오른쪽 = 내용. 고를 것이 없는 탭은 왼쪽에 장 표지
- UI `BookCoverView`(장 표지): 탭 아이콘 4배(48) + 이름 굵게 + 구분선 80 + 한 줄 문구(Small, #967854), 세로 가운데. 테마 `Book › 장 표지`
- `MenuBookPage`: 왼쪽 장 표지(`메뉴` / `잠시 책을 덮고 쉬어 가요.`), 오른쪽 `나가기` 제목·구분선·`메인 화면으로`·`게임 종료`, 맨 아래 `책 닫기`. 문구 `ui.book.menu.caption`·`header`(임시)
- 확인(플레이 모드): ESC → 메뉴 탭, 버튼은 오른쪽 페이지, 표지 아이콘 48×48이 왼쪽 내용 칸 가로 가운데. Unity 창 포커스가 없어 캡처는 못 함
- `UI_INTEGRATION_GUIDE.md`: 페이지 규칙·장 표지·메뉴 탭 배치, 꺼진 채 내려간 뷰의 구독 해제(`if (this == null) { OnDestroy(); return; }`, 사용자 커밋 22c9c88에서 들어온 규칙), 새 탭 페이지는 `OnOpened`에서 다시 배치

## 2026-11-23 — 씬 이동 뒤 E 줍기 오류(MissingReferenceException) 수정
- 원인: `GameBootstrap`은 꺼진 UI에도 `Bind`를 부르고, Unity는 한 번도 켜진 적 없는 오브젝트에 `OnDestroy`를 부르지 않는다. Creature_Cave의 꺼진 `InventoryBar`가 씬을 넘어 유지되는 `Inventory` 구독을 풀지 못하고 남아, 뒷산에서 줍기 → `SlotChanged`에서 파괴된 Image에 접근해 예외 → `GatheringService.TryPick`이 중간에 멈춤(가방에는 들어가고 서식지에는 남음)
- 수정: 씬을 넘어 유지되는 Core(`Inventory`·`RequestTracker`·`ScoreBook`) 이벤트를 받는 처리기 맨 앞에 `if (this == null) { OnDestroy(); return; }` — 파괴된 구독자는 첫 이벤트 때 스스로 구독을 푼다. 대상: `InventoryBar`, `QuickSlotBarView`, `ScoreHud`, `ScoreHudView`, `QuestBarView`, `QuestListView`, `QuestBookPage`, `QuestToastView`, `QuestTrackerPanel`, `RequestBoardWindow`, `InventoryEquipmentLink`
- 확인(플레이 모드): Creature_Cave에서 시작 → 뒷산으로 이동 → 줍기 성공(가방 0→1, 슬롯 비움), 콘솔 에러 없음, `Inventory.SlotChanged` 구독자에 파괴된 객체 없음. EditMode 테스트 153개 통과
- 새 UI가 씬을 넘는 Core 이벤트를 구독하면 같은 확인을 넣을 것

## 2026-11-23 — 책 메뉴 탭(배치만) + UI 연결·제작 지침
- UI `MenuBookPage`: 왼쪽 페이지에 `메뉴` 제목·구분선, `메인 화면으로`·`게임 종료` 버튼, 맨 아래 `책 닫기`(사용자 요청). 책 닫기만 동작(화면 스택에서 책 닫기 → 시간 다시 흐름). 메인 화면·게임 종료는 기능이 없어 로그만, 기능이 생기면 `MainMenuRequested`·`QuitRequested`에 연결
- UI `BookMenuButton`: 가운데 글자, 마우스를 올리면 굵게
- 사용자 피드백 반영: 찢김 없는 책 페이지 버튼 그림을 새로 그림(`Art/UI/book_ui/buttons/btn_page_{normal,hover,pressed}.png`, 12×12, 9-slice 4, 넘김 종이 외곽선색 #6B4B2A·모서리 1px 둥글게, 위 밝은 줄·아래 그늘 줄, 누름은 반대). 버튼 높이 24 → 28. 글자는 실제 글자 모양(textBounds) 기준으로 왼쪽=오른쪽, 위=아래 여백이 같게 정수 px로 맞추고, 남는 여백이 홀수면 버튼 폭·높이를 1px 줄여 맞춤(지금 세 버튼 187×28, 위아래 7)
- 테마 `Book › 메뉴 탭`(버튼 높이 24, 간격 8), 문구 `ui.book.menu.*`. `UiBookBuilder`가 메뉴 탭에 이 페이지를 만듦
- 확인(플레이 모드): ESC → 메뉴 탭, 메인 화면으로 → 로그만(책 그대로), 책 닫기 → 닫힘·timeScale 1
- 문서 `Docs/UI_INTEGRATION_GUIDE.md` 새로: 기능별 연결 지점(마력·기력 `IPlayerVitals`/`SetVitals`, 시간 `IGameTime`/`SetTime`, 점수 `ScoreBook`, 의뢰 `IAcceptedRequestList`, 퀵슬롯 `Inventory`, 책 `BookState`, 화면 스택·시간 정지·입력), 바인딩 시점 주의, 새 UI 제작 틀(파일 위치·ThemedWidget·테마·문구·픽셀 규칙·층·빌더·리스트), 확인 방법. `HANDOFF_UI.md` 맨 위에서 이 문서를 가리킴

## 2026-11-23 — 책 페이지 넘김 연출(5-3) + 탭 휠 전환
- UI `BookPageTurner`: README 규칙 그대로 9프레임 × 45ms(unscaled), 폭 = round(W × |cos(πp)|)(W = 책등 가운데 ~ 반대편 표지 안쪽), 들어 올림 round(4 × sin(πp)), 폭 비율 0.8↑ leaf_0 / 0.45↑ leaf_1 / 그 아래 leaf_2, k=4 leaf_edge, 반대편은 Scale X −1. 그림자(shadow_strip)는 종이 바깥 가장자리에 붙이고 종이 안쪽(RectMask2D)에서 자름, k=4는 양쪽
- `BookView`: 탭을 바꾸면 한 장씩 넘김. 앞으로(아래 탭)는 k=0에 오른쪽 내용·탭 선택 그림, k=8에 왼쪽 내용을 바꾸고 뒤로는 반대. 멀리 있는 탭은 이어서 넘기고 넘기는 중 입력은 마지막 탭만 목표로 남김. 책을 열 때는 넘기지 않고 바로 보임. `BookPage.SetHalves`로 왼쪽·오른쪽 내용을 따로 켜고 끔
- 탭 위 휠(사용자 요청): 위 = 이전 탭, 아래 = 다음 탭, 양 끝에서 멈춤, 마지막 탭으로 기억(`BookTabView.Scrolled`)
- 테마 `Book › 페이지 넘김`(프레임 시간·수, 표지 두께 4, 들어 올림 4, leaf 비율 경계, leaf·단면·그림자 스프라이트)
- 확인(플레이 모드): 프레임별 종이·그림자 위치를 뽑아 실제 스프라이트로 그려 README 프리뷰와 같은 모양인지 확인, 휠 아래 1칸·끝에서 멈춤·위 1칸, 의뢰 → 설정까지 이어 넘긴 뒤 설정 페이지만 남음. Unity 창이 포커스가 없을 때는 게임 화면이 다시 그려지지 않아 움직임은 캡처로 못 봄 → 사용자 확인 필요

## 2026-11-20 — (b) 책 화면 틀 + 의뢰 목록 탭 (자세히 패널 대신)
- 사용자 결정: 자세히 패널(10-5) 대신 책의 맨 앞 탭 "의뢰 목록"에서 목록과 자세한 내용을 본다. 탭 순서 의뢰 목록 → 마법 스펠 → 마법진 → 환경 설정 → 나가기/종료 메뉴. 책은 프리뷰 비율로 크게(페이지 220×300 × 2 + 책등 8 = 448×300, 임시)
- Core `BookState`(지금 탭·직접 고른 마지막 탭·먼저 보여 줄 의뢰)와 `GameContext.Book`. 책 아이콘 = 마지막으로 직접 고른 탭(처음 의뢰 목록), ESC(열린 화면 없음) = 메뉴 탭(마지막 탭으로 기억 안 함, 5-2 제안), HUD 의뢰 항목 = 의뢰 탭 + 그 의뢰
- Core `HudState.IsQuestBarVisible`: 의뢰 바 X가 바·목록을 끄고, 책 의뢰 탭의 `화면에 의뢰 목록 표시` 체크 상자로 다시 켬(저장은 환경 설정 저장을 만들 때)
- Core `IAcceptedRequestList.ExpectedReward`: 처음 납품하는 재료 보너스까지 포함한 필기 점수 예상값(규칙은 `ScoreCalculator`)
- UI `BookScreen`·`BookView`(책 그림·사이드 탭 5개, 탭 아이콘 가운데 정렬)·`BookPage`(빈 페이지)·`QuestBookPage`(왼쪽: `수락한 의뢰(n)`, 구분선, 의뢰 버튼 목록 — 고른 칸 진한 종이 + 굵게, 완료 파란 글자, 넘치면 휠·스크롤바, 맨 아래 체크 상자 / 오른쪽: 제목·대사·필요한 재료·`보상 필기 점수 +n`)·`QuestBookRowView`·`ThemedCheckbox`
- `UiTextWrap.ByWords`: 한글 대사를 낱말 단위로 줄바꿈(TMP 기본은 글자마다 끊어 "상식/이야"가 됨, 프로젝트 TMP 설정은 건드리지 않음)
- 팝업 바깥(입력 막기 판)을 누르면 ESC처럼 맨 위 화면을 닫음(`UiRaycastBlocker.Clicked`)
- 테마 `Book`(페이지 크기·여백·탭 위치·탭 색·아이콘·체크 상자·의뢰 탭 간격), 문구 `ui.book.quest.*`. 메뉴 `VoxelMagic/UI/화면 만들기/책`(UI_Root의 Popup 층)
- 확인(Creature_Cave 1080p, 자동 저장 끔): 의뢰 항목 → 의뢰 탭 + 그 의뢰(timeScale 0), ESC → 메뉴 탭, 바깥 클릭 → 닫힘, 책 아이콘 → 처음 의뢰 탭·직접 고른 탭 기억, 체크 상자 ↔ 의뢰 바 숨김. EditMode 153개 통과
- 사용자 피드백 반영: 사이드 탭을 9-slice로 키움(평상시 37×32, 선택 41×32, 외곽선 1px 그대로, 간격 34), 탭 아이콘 2배(24)로 표지 밖 안쪽 칸 가운데에. `화면에 의뢰 목록 표시` 글씨를 체크 상자 크기에 맞춰 `Small`(갈무리9 10px)로
- 딤 켬: 팝업 뒤 입력 막기 판 색 = 검정 알파 0.6(테마 `Modal › Blocker Color`). 리니어 색 공간이라 0.45로는 약하게 보여 0.6으로(화면 밝기 약 65%)
- 남은 것: 탭 전환 페이지 넘김 연출(5-3), 다른 탭 내용, 체크 상자 상태 저장

## 2026-11-20 — 날짜 판 D-n, 완료 알림 X 수정
- 버그: 완료 알림의 X가 눌리지 않았음. 알림이 있는 `HudAnimated` 층에 GraphicRaycaster가 없었음(연출 전용 층으로 만들었기 때문) → UI_Root의 `HudAnimated`에 추가, `UiRootPrefabBuilder`도 같게. 플레이 중 X 위치 레이캐스트가 `Close`에 맞고 다음 알림으로 넘어가는 것 확인
- 사용자 결정: 8일 플레이, 날짜 판은 `D-{n}`(문구 `ui.time.day`). n = 플레이 날 수 − 며칠째 + 1 → 1일째 D-8 … 마지막 날 D-1. 날 수는 테마 `Time › Play Days`(8), 게임 시계가 생기면 규칙 쪽 값으로 옮길 것

## 2026-11-20 — 외곽선 v2 비교 (책·가방 버튼, 점수 숫자·밑줄)
- 사용자 외곽선 그림: `menu_icons/buttons/btn_{book,bag}_{normal,hover,pressed}_v2.png`(20×20), `score_ui/v2/digits/score_digit_N_v2.png`(13×14), `score_ui/underline/score_underline_N_v2.png`·`score_ui/v2/underline/score_underline_N_v2.png`(가로세로 +2)
- 메뉴 `VoxelMagic/UI/외곽선 버전/외곽선 v2 쓰기`·`원본 쓰기`: 테마의 버튼·숫자·밑줄 스프라이트와 `Scores › Outline Px`(0/1)만 바꿈
- `ScoreNumberView`: 숫자를 왼쪽 자리부터 채우고 그리는 순서도 왼쪽 → 오른쪽(이웃 외곽선 겹침). 피치 9 그대로. 밑줄은 `숫자 칸 높이 − 2 × 외곽선`에 둬서 원래 밑줄 자리와 같게. 테마가 바뀌면 숫자를 다시 그림
- 사용자 결정: 외곽선 v2 채택
- 외곽선 버튼은 20px × 2배 = 40이라 아이콘 줄이 8px 커지고, 그 아래 의뢰 바·목록도 8px 내려감

## 2026-11-20 — (f) 의뢰 드롭다운 목록·완료 알림 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 10-2~10-4·10-6·13장)
- Core `IAcceptedRequestList` 확장: `Accepted`(항목 = 의뢰·제목·수락 순서·요구 아이템별 진행·진행률·완료), `RequestBecameComplete`(조건을 다 채워 납품 가능해진 순간). `RequestTracker`가 진행이 바뀔 때마다 항목을 다시 만들고 `AcceptedListChanged`를 냄. 저장에서 불러온 의뢰가 이미 납품 가능하면 알림을 다시 내지 않음. 보상까지 받은(납품한) 의뢰는 목록에서 빠짐
- UI `QuestListView`(HudDynamic `HUD_QuestList`): 의뢰 바 바로 아래, 폭 = 바 폭, 높이 = 항목에 맞춤(위아래 여백 같음). 정렬은 완료 → 진행률 높은 순 → 먼저 수락한 순(사용자 결정). 제목 + 아이템마다 한 줄 `무 0/3`(사용자 결정). 완료 항목 글자 `#4A90C8`. 제목+1줄 항목 4개 높이를 넘으면 RectMask2D로 자르고 휠·스크롤바(끌기 가능)로 스크롤, 항목은 보이는 만큼만 풀에서 재사용(가상 스크롤). 목록 위 휠은 카메라 줌 대신 스크롤. 항목이 없으면 패널을 숨김. 항목 클릭은 `ItemClicked`(자세히 패널 연결 자리)
- UI `QuestToastView`(HudAnimated `HUD_QuestToast`): 화면 왼쪽 아래 `의뢰 완료: 제목`, 3초 + 페이드 0.4초(unscaled), 여러 건은 차례로, X로 지금 알림 닫기(사용자 요청). 의뢰 바를 꺼도 뜸
- `ThemedText.SetColorOverride`(완료 글자색), 테마 `Quest List`·`Quest Toast`, 문구 `ui.quest.toast`·`ui.quest.requirement`, 메뉴 `VoxelMagic/UI/HUD 만들기/의뢰 목록·완료 알림`, 미리보기 메뉴 `VoxelMagic/UI/미리보기/`(가짜 의뢰 6건, 알림 2건 — 지금 규칙은 한 번에 하나만 수락해서)
- 세 게임 씬의 옛 프로토타입 `QuestTracker`(오른쪽 위)를 끔
- 사용자 피드백 반영: 목록 글자를 갈무리9 10px(`Small`)로 줄임. 갈무리9에는 굵은체가 없어 의뢰 제목은 1px 오른쪽에 한 번 더 그려 굵게(`Quest List › Title Pixel Bold`). 제목 앞 항목 기호 `· {0}`(문구 `ui.quest.itemTitle`), 재료 줄 들여쓰기 8. 여백 왼쪽·오른쪽 6 → 9, 위·아래 5 → 10(10-2: 위아래는 항상 같게)
- 치트(`RequestCheatKeys`, 에디터·개발 빌드): F7 = 진행 중 의뢰의 모자란 재료 1개 넣기, F12 = `[Cheat] Requests` Inspector의 `Item To Add`를 `Amount To Add`만큼 넣기(F1~F6·F8~F11은 다른 디버그 키가 씀). 인벤토리에 실제로 들어가므로 자동 저장이 켜져 있으면 저장 파일에 남음
- 확인(Creature_Cave 1080p, 자동 저장 끔): 의뢰 수락 → `무 0/3`, 무 3개 → 파란 글자 + 왼쪽 아래 알림, 가짜 6건 정렬·스크롤·스크롤바. EditMode 153개 통과. 왼쪽 아래 옛 조작 안내(`ControlsHintView`)와 알림이 겹침

## 2026-11-20 — 시간대 UI 배치 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 8장·13장)
- Core `IGameTime`(날짜·정시·시간대 `TimeOfDay` Day=방과후/Night=밤, `TimeChanged`)과 `GameContext.Time`·`SetTime`. 게임 시계는 아직 없음 → 연결 전에는 테마의 미리보기 값(1일째, 16시)을 보임
- 임포트 규칙: 수정본 날씨 시트에서 `time_frame`(6,7 65×54, 틀 + 날짜 패널 한 조각)·`time_roulette`(17,71 13×17)·`time_arrow`(4,85 10×8, 가로), 원본 작은 날씨 아이콘에서 `time_icon_after_school`(노을, 7,71)·`time_icon_night`(달, 7,39) 18×17
- UI `TimeHudView`(오른쪽 위, 가장자리 8, 2배): 그리는 순서 틀 → 룰렛 → 시간대 그림(틀보다 앞, 사용자 결정으로 왼쪽 1·아래 1 보정한 9,8) → 화살표 → 글자. 화살표는 정시마다 정수 각도로 회전(스텝, 13장): 방과후 16~19시 = 75°→0°, 밤 19~24시 = 0°→−75°(구간별로 나눠 색 경계에서 바뀜, 임시값). 날짜 패널 `{0}일째`(임시). 개발 빌드에서 게임 시계가 멈추면 "멈춰 있음"(8-2). 글자는 묶음 배수를 되돌려 다른 HUD 글자와 같은 크기
- 공용 툴팁 `UiTooltipView`(Overlay 층 `UI_Tooltip`, 종이 바탕 `Tooltip` 스킨 = 의뢰 바 종이 임시): 마우스 옆 정수 px, 화면 밖이면 반대쪽. 시간대 HUD에 올리면 `방과후 16:00`(분 없음)
- 테마 `Time`(위치·각도·구간 시각·미리보기·툴팁 거리·스프라이트), `Tooltip`(여백·글자 스타일). 문구 `ui.time.afterSchool`·`night`·`tooltip`·`day`·`paused`
- 프리팹 `HUD_Time`(HudDynamic 층), 메뉴 `VoxelMagic/UI/HUD 만들기/시간대·툴팁`. `UiHudBuilder.Replace`가 넣은 인스턴스를 돌려줌(툴팁 연결용)
- 사용자 피드백 반영: 틀을 사용자 v2 시트(`Sprout Lands (수정본)/weather/Weather_UI_Free_v2.png`, 날짜 판 가운데·잎 없음, 원본 폴더에 있던 것을 수정본 폴더로 옮김)의 `time_frame`(7,7 56×54)으로 교체. v2 시트의 룰렛은 원본 색이라 룰렛·화살표는 계속 Noon 수정본에서 자름. 조각 위치는 v2 기준(그림 8,8 / 룰렛 35,8 / 날짜 3,36)
- 화살표 회전 중심을 룰렛 반원 중심(34, 16.5)으로(0.5px 아래). `ArrowPivot`은 0.5 단위 값
- 날짜 글자를 수락한 의뢰 제목과 같은 `HudTitle`(굵게)로, 테마 `Time › Date Style`
- 시간대 묶음 배수 2 → 1.2(사용자 요청 0.6배, 아래에서 1.5로 다시 바꿈)
- 퀵슬롯 0.8배: 칸 40×44 → 32×36, 아이콘 32 → 24(16px 도트 1.5배라 선명)
- 시간대 배수 1.2 → 1.5(사용자 요청 '1.2와 2의 중간', 0.5 단위로 맞춤). 1.2에서는 조각마다 화면 px 반올림이 달라 룰렛·그림 옆에 틈이 비쳤음 → 1.5(1080p에서 도트 하나 = 3px)로 게임 화면 틈 없음. `Scale`은 0.5 단위로 반올림
- 틀 뒤 받침(`Time › Backing` 8,8 41×18, 틀 안쪽 테두리 색)을 깔아 씬 화면처럼 정수 px가 아닌 곳에서도 틀 구멍 뒤 틈이 비치지 않게 함
- 날짜 글자: 1.5배에서는 굵은 12px가 날짜 판 안쪽(밝은 테두리 안)에 들어감 — 크기는 그대로
- 확인(Creature_Cave 1080p, 자동 저장 끔): 16시 방과후·21시 밤 아이콘·화살표 −30°, 툴팁 `밤 21:00`, 시계 정지 시 "멈춰 있음". 런타임 회전한 화살표는 도트 격자가 어긋나 보임 → 사용자 판단 대기. 임박 효과(23시~)는 아직 없음

## 2026-11-20 — (e) 점수 HUD (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 9장·13장)
- UI `ScoreNumberView`: v2 숫자를 피치 9로 이어 붙이고(폭 9n+2), 자릿수별 밑줄(1~2자리 → 2번, 3 → 3번, 4 → 4번)을 가로 가운데 내림으로 바로 아래에. 숫자 4개·밑줄 1개를 미리 만들어 켜고 끔, 값이 같으면 갱신 안 함, 문자열 없이 자릿수 계산
- UI `ScoreHudView`: 화면 위 가운데에 "필기 예상 점수" + 숫자(v1 밑줄), 여백, "실기 예상 점수" + 숫자(v2 밑줄). `ScoreBook.ScoreChanged`로만 갱신, 숫자 폭이 바뀌면 다시 가운데 정렬(왼쪽 끝 기준 정수 px)
- 테마 `Scores`(위 여백 8, 묶음 사이 24, 글자–숫자 4, 숫자 배수 2, 피치 9, 최대 4자리, 숫자·밑줄 스프라이트) — 임시값. 새 글자 스타일 `HudLabel`(갈무리11 Bold, 임시 밝은 종이색 `#F4E8CC` — 게임 화면 위에 바로 놓여서). 문구 `ui.score.written`·`ui.score.practical`
- 프리팹 `HUD_Score`(HudDynamic 층), 메뉴 `VoxelMagic/UI/HUD 만들기/점수`. 세 게임 씬의 기존 프로토타입 `ScoreHud`(실기·필기 채움 바)를 끔
- 기력 아이콘: 사용자가 옮긴 `bar_ui/icons/icon_stamina.png`를 쓰도록 테마·빌더·아틀라스 갱신(옛 `bar_ui/icon/`에 같은 파일이 남아 있음)
- 테마 `Scores › Written Offsets` · `Practical Offsets` 추가(각각 `Group`·`Label`·`Number`, x +오른쪽 / y +아래, px): 필기·실기 묶음 전체, 이름 글자만, 숫자만 따로 손으로 옮긴다. 자동 배치(가운데 정렬·간격) 위에 더해지고 줄 가운데 계산에는 들어가지 않는다
- `CLAUDE.md` 작업 표에 UI 작업 줄 추가(`Docs/HANDOFF_UI.md` → `Docs/UI_SPEC.md`), 사용자 승인
- 확인(Creature_Cave 1080p): 필기 85·실기 9, 필기 100·실기 100(3자리 밑줄). 지금 점수 규칙(`ScoreBook`)은 100 상한이라 4자리는 게임에서 볼 수 없음. 규칙 검사 0개, EditMode 153개 통과

## 2026-11-20 — (d) 퀵슬롯 바 + 마력·기력 상태 바 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 7-1·7-2)
- 임포트 규칙에 이름 붙인 조각 자르기(`UiSliceRect`, 그림 왼쪽 위 기준 좌표) 추가. 기존 규칙 에셋에는 새 기본 규칙만 덧붙임. Sprout 인벤토리 시트에서 7조각: `inv_bar_dark`(329,121 30×32)·`inv_slot_light`(233,121)·`inv_slot_mid`(281,121) 경계 4/5/4/7, 선택 프레임 중간 색 모서리 `inv_select_tl/tr/bl/br`(273·308, 0·36, 11×12). 아틀라스에는 아직 넣지 않음(Sprout은 git 제외라 아틀라스 항목이 팀원 쪽에서 비게 됨 — 결정 필요)
- Core: `ItemDefinition`에 `Icon`(Sprite) 추가, `IPlayerVitals`(마력·기력 0~1 + 바뀜 알림) 추가, `GameContext.Vitals`/`SetVitals`(아직 규칙 모델이 없어 null → 바는 가득 찬 상태로 보임)
- UI: `QuickSlotBarView`(화면 아래 가운데, 칸 5개 미리 생성, 인벤토리 이벤트로만 갱신, 칸 클릭 = 선택 요청), `QuickSlotView`(아이콘 16×16 가운데, 아이콘 없는 아이템은 단색 칸 + 첫 글자, 개수 2개 이상이면 작은 글씨), `SelectionFrameView`(모서리 4조각, 0.08초 보간 이동·정수 px 반올림·unscaled time, 자체 캔버스), `StatusBarView`·`StatusBarsView`(오른쪽 아래, 마력 → 기력, 정수 px 채움 + 윗면 하이라이트, 아이콘 위 2px)
- 가운데 배치는 0.5px 어긋남을 피하려고 왼쪽 끝 기준 정수 px로 계산(`PixelLayout.PlaceBottomCenter`), 창 폭이 바뀌면 다시 계산
- 테마 `QuickSlots`(칸 30×32, 간격 2, 안쪽 여백 2, 아래 여백 8, 선택 프레임 바깥 2·이동 0.08초)·`StatusBars`(여백 8, 바 높이 80, 바 사이 10, 아이콘 간격 2) — 모두 임시값. 패널 모양 `QuickSlotBar`·`QuickSlot`·`QuickSlotAlt`·`StatusBarFrame`
- 프리팹 `HUD_QuickSlot`(HudStatic 층)·`HUD_QuickSlotCell`·`HUD_StatusBars`(HudDynamic 층), 메뉴 `VoxelMagic/UI/HUD 만들기/퀵슬롯·상태 바`. 지팡이 아이템에 임시 아이콘 `item_staff.png`(7-3 방식 1)
- 세 게임 씬의 기존 프로토타입 `InventoryBar`(아래 가운데 줄)를 끔(삭제하지 않음). 기존 "선택한 아이템 이름" 글자는 문서에 없어 새 바에 넣지 않음
- 검사기: 부모가 클릭을 받는 그래픽(퀵슬롯 칸 배경)은 Raycast 경고에서 뺌
- 사용자 피드백 반영: 칸 30×32 → **40×44**, 아이콘 16 → **32**(16px 도트 정수 2배), 바 220×58 (테마 값만 바꿈)
- 마우스 휠: 퀵슬롯 바 위에서 돌리면 칸 변경(아래로 = 다음 칸, 끝에서 처음으로 돎), 게임 화면 위에서는 기존처럼 카메라 줌. `HudState.IsPointerOverScrollHud`(바가 마우스 들어옴/나감으로 알림)를 `GameInputReader`가 보고 줌을 쉰다. 낚시 미니게임 중 휠은 그대로 낚시 바가 먼저. 바 배경도 Raycast를 켜서 칸 사이에서도 휠이 먹음
- 사용자 피드백 반영: 마력·기력 바를 아이콘과 함께 **정수 2배**로 키움 — 테마 `Status Bars › Scale`(2, 묶음 전체에 정수 배 Scale, 오른쪽 아래 기준이라 여백 유지)와 바 높이 80 → **48**(2배 후 화면에서 96). 검사기는 이제 같은 정수 배 Scale을 허용(소수·가로세로 다른 Scale만 경고)
- 사용자 피드백 재반영: 바는 원래 비율(16×80) 그대로 **2배**(화면 가상 32×160)로 바 높이 80 복귀. 아이콘은 바와 따로 `Status Bars › Icon Scale`(0.5 단위, 기본 **1.5** → 24px 아이콘이 36). 1080p·4K(캔버스 짝수 배)에서는 선명하고, 1440p(3배)·720p(1배)에서는 도트 폭이 조금 고르지 않을 수 있음
- 사용자 피드백 3차: 순서 **기력(왼쪽) → 마력(오른쪽)**. 바 묶음 배수 2 → **1.5**(16×80 → 화면 가상 24×120, `Scale`은 0.5 단위로 바뀜). 여백은 이제 **바 프레임 기준**(아이콘은 프레임 밖으로 삐져나와도 됨): 화면 오른쪽·아래 끝 ↔ 프레임 **16**, 기력 ↔ 마력 프레임 사이 9(확대 후 13.5) < 16. 아이콘 1.5배(36) 유지. 검사기는 가로세로 같은 0.5 단위 배수를 허용
- 사용자 피드백 4차: 가장자리 여백을 다시 **8**(바 프레임 기준), 기력 ↔ 마력 프레임 사이 **4**(확대 후 6) < 8. 아이콘은 바보다 아주 조금 넓게: `Icon Scale` **1.125**(24 → 27, 바 24보다 양옆 1.5씩 튀어나옴). 아이콘 배수는 이제 자유 값(정수·0.5 단위가 아니면 도트 폭이 조금 고르지 않음)
- 퀵슬롯 칸 프리팹이 빈 칸 상태(아이콘·단색 칸·글자 꺼짐)로 시작하도록 수정 — UI 에디터 씬에서 흰 사각형이 보이던 문제
- 확인(Creature_Cave 1920×1080 = 2배): 바 170×46 가운데, 지팡이 아이콘, 3번 선택 시 프레임 이동, 상태 바 가득 참. 규칙 검사 0개, EditMode 153개 통과

## 2026-11-20 — UI 배율 2배 + 왼쪽 위 HUD 피드백
- 테마 기준 높이 360 → **480** (사용자 결정): 1080p 2배(가상 960×540), 1440p 3배, 4K 4배, 720p·768p 1배. UI_SPEC 13장 갱신
- 책·가방 아이콘 정수 배 확대: 테마 `TopLeft › Icon Scale`(기본 2 → 32×32). `ThemedButton`에 크기 배수(`SetSizeScale`)를 두어 버튼이 다시 그려져도 크기가 유지됨
- 의뢰 바: `Bar Size` 대신 `Bar Width`(144) + `Bar Padding Y`(3). 바 높이 = 제목 글자 줄 높이(16) + 여백 × 2 = 22. 제목은 바 세로 가운데 줄 높이 칸에 둠
- 글자 스타일에 `띄어쓰기 더(px)` 추가, 새 스타일 `HudTitle`(갈무리11 Bold, 띄어쓰기 +2px) → 바 제목에 적용("의뢰"와 "(0)" 사이가 붙어 보이던 문제). 스타일에 `LineHeightFrom` 추가
- 확인: Creature_Cave 1920×1080(2배) 캡처, UI_Root 규칙 검사 0개
- 바 제목 문구를 `수락한 의뢰({0})`로 ("의뢰"와 개수 사이 띄어쓰기 없음, 사용자 결정). 크기 값은 지금 상태로 확정(아이콘 2배, 바 22, 여백 3)

## 2026-11-20 — Silver · 네오둥근모 Pro 비교
- 사용자가 넣은 `Silver.ttf`, `NeoDunggeunmoPro-Regular.ttf` 설계 크기 측정: Silver 19px(한글 높이 11, 줄 높이 21), 네오둥근모 Pro 16px(한글 높이 13, 줄 높이 16). 둘 다 Bold 없음. `Fonts/Pixel/Silver`·`NeoDunggeunmo` 고정 + 예비 에셋. 네오둥근모에 없는 기호 19개(※ ℃ Ⅰ~Ⅴ 화살표 ★☆♡♥♪ 등)
- 의뢰 바 6개(갈무리11 Bold·갈무리11·갈무리9·갈무리7·Silver·네오둥근모)를 1080p에서 나란히 캡처(플레이 중 임시, 저장 안 함)
- 버그 수정: 테마를 인스펙터에서 바꿀 때 "SendMessage cannot be called during OnValidate" 경고 → 위젯이 편집 모드에서는 한 박자 뒤(delayCall)에 적용. 폰트 재굽기 때 "Main Object Name does not match filename" 경고 → 같은 파일 이름으로 `_Baking` 임시 폴더에 저장 후 복사

## 2026-11-20 — 작은 갈무리 폰트 시험 + 값 바로 고쳐 보기 (브랜치 `feature/tool-UI-editor`)
- 갈무리9·갈무리7(Regular만 있음, Bold 없음)을 받아 `Art/Font/`에 넣음(SETUP.md). 설계 크기 측정: 갈무리9 10px(줄 높이 13), 갈무리7 8px(줄 높이 11). `Fonts/Pixel/Galmuri9`·`Galmuri7` 고정 에셋 + 예비 에셋
- 글자 스타일에 `폰트 지정`(비우면 테마 기본) 추가. 글자 크기는 이제 쓰는 폰트 에셋의 설계 크기 × 배수(테마의 공용 설계 크기 값은 없앰). 새 스타일 `Small`(갈무리9 10px)·`Tiny`(갈무리7 8px)
- 문구 표(`Texts_ko`)를 고치면 테마 변경으로 넘겨 화면 글자가 바로 바뀜
- 버그 수정: 폰트를 다시 구우면 에셋 이름이 "… (Baking)"으로 남던 문제(`FontAssetFileWriter`)
- 의뢰 바 제목에 Small·Tiny를 넣어 1080p 캡처 비교. 프리팹은 아직 Bold(갈무리11 Bold 12px) 그대로, 사용자 선택 대기

## 2026-11-20 — (a) 왼쪽 위 책·가방 버튼 + 의뢰 드롭다운 바 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 5-4·10-1)
- Core `IAcceptedRequestList`(수락한 의뢰 수 + 바뀜 알림, UI 연결 창구). `RequestTracker`가 구현(지금 규칙은 한 번에 하나라 0 또는 1)
- UI `TopLeftMenuView`(책 → 책 화면, 가방 → 확장 인벤토리를 화면 스택에 연다. 아직 없는 화면은 열지 않고 로그만), `QuestBarView`(화살표로 목록 펼침/접힘 즉시 전환, 펼침 = 위 화살표·접힘 = 아래 화살표, "수락한 의뢰 (n)"은 개수가 바뀔 때만 다시 씀, X는 아이콘만), `PixelLayout`(정수 px 배치), `UiTextKeys`, `UiScreenRouter.HasScreen`
- 테마 `TopLeft` 값(임시): 모서리 여백 8, 아이콘 간격 4, 아이콘 줄과 바 사이 4, 바 144×22, 바 버튼 안쪽 5, 버튼–제목 4. 문구 표에 `ui.quest.bar.title` = "수락한 의뢰 ({0})"
- 프리팹 `Prefabs/UI/HUD/HUD_TopLeftMenu`(HudStatic 층), `HUD_QuestBar`(HudDynamic 층) — 위젯 프리셋을 조합, UI_Root에 넣음. 메뉴 `VoxelMagic/UI/HUD 만들기/왼쪽 위 메뉴·의뢰 바`
- 게임 씬 `FarmPond`·`BackMountain`·`Creature_Cave`에 UI_Root 배치, `_Bootstrap`(GameBootstrap) 연결 목록에 추가. 기존 프로토타입 UI(오른쪽 위 의뢰 토글, 점수, 인벤토리 줄)는 그대로 두고 해당 단계에서 교체. 화살표와 Q가 같은 펼침 상태를 쓴다
- 사용자 피드백 반영: 의뢰 바 높이 22 → **16**(테마 값, 갈무리11 줄 높이 16과 같음), 바 제목을 **갈무리11 Bold**로 (`ThemedText.SetStyle`). 글자 크기는 갈무리11의 최소 정수 배(12px)라 더 줄이려면 더 작은 폰트가 필요
- 확인(Creature_Cave, 1920×1080 = 3배): 책·가방(16px) 줄 아래 의뢰 바, 화살표 전환, 의뢰 수락 시 (0) → (1), 책 버튼은 책 화면이 없어 열리지 않고 시간도 그대로. UI_Root 규칙 검사 0개, EditMode 153개 통과

## 2026-11-20 — UI 에디터 씬 · 픽셀 폰트 · 위젯 프리셋 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 1-1·3장)
- 폰트 설계 크기 측정: 갈무리11은 12px에서 글자 테두리가 격자에 완전히 맞음(부드러운 렌더 중간 알파 0%). 스타더스트 3.0은 정수 격자에 맞는 크기가 없고, 13px에서 획이 고르게 1px로 나옴(14px은 윗줄이 잘림). Regular·Bold 같은 결과
- 픽셀 폰트 TMP 에셋 `Fonts/Pixel/`: `Galmuri11`·`Galmuri11 Bold`(12px)·`Stardust3`·`Stardust3 Bold`(13px). RASTER_HINTED, Point 필터, 고정(Static) 아틀라스 1024 한 장(KS X 1001 2,350자 + 기호 + ASCII + Core·UI·Data에 쓰인 글자, 약 2,560자). 각각 예비 Dynamic 에셋(`… Fallback`, 256, 빌드 때 글자 비움)을 fallback으로 연결. 폰트에 없는 글자: `−`(스타더스트 둘, 갈무리 Bold), `↔`(갈무리 Bold)
- 설정 `Data/UI/PixelFontSet.asset`, 메뉴 `VoxelMagic/UI/픽셀 폰트 다시 굽기`(`Editor/Fonts/PixelFontAssetBuilder`). 글자 모으기·GUID 유지 저장을 `FontCharsetCollector`·`FontAssetFileWriter`로 빼고 Pretendard 빌더(`KoreanFontAssetBuilder`)도 같이 쓰게 정리(동작 같음, ASCII는 항상 포함)
- 위젯: `ThemedWidget`(기반) + `ThemedText`(글자 스타일·문구 키, Raycast 끔) · `ThemedButton`(3상태 Sprite Swap, 스프라이트 크기 그대로) · `ThemedPanel`(Sliced/Tiled). 테마에 글자 스타일·버튼 모양(책·가방·화살표 위/아래·닫기)·패널 모양(의뢰 종이 4종·스크롤 트랙/핸들)·문구 표 연결 추가(`UiThemeSkins`). 임시 글자색 `#5C4128`, 임시 폰트 갈무리11
- 위젯 프리셋 `Prefabs/UI/Widgets/`: `W_Text`, `W_Button`, `W_PaperPanel`, `W_Scrollbar`(트랙 8 + 핸들 6) — 메뉴 `VoxelMagic/UI/테마 기본값·위젯 프리셋 만들기`
- UI 작업대 창(메뉴 `VoxelMagic/UI/UI 작업대`, `Data/Tools/UI/UiWorkbenchConfig.asset`): ① 해상도 — 16:9·16:10·21:9·4:3 10개 버튼으로 Game View 해상도 전환(리플렉션, 실패하면 경고만), 배율·가상 크기 표시 ② 검사 · 굽기 — 선택한 UI의 규칙 위반 목록(정수 위치·크기, 홀수 + 가운데 기준점, 불필요한 Raycast, LayoutGroup·ContentSizeFitter·Mask, Point 아닌 스프라이트, 설계 크기의 정수 배가 아닌 글자, Scale) → 클릭하면 그 오브젝트 선택, `Prefabs/UI/`로 프리팹 굽기 ③ 테마 · 도구 — 임포트 규칙·폰트 굽기·프리셋·씬 열기 버튼 + 테마 인스펙터
- `UiRaycastBlocker`(일부러 클릭을 받는 판 표시, 검사 예외) → UI_Root의 InputBlocker에 붙임
- UI 에디터 씬 `Scenes/Tools/UIEditor.unity`(빌드 목록에 넣지 않음, 메뉴 `VoxelMagic/UI/UI 에디터 씬 열기`): 카메라·EventSystem(Input System)·UI_Root + 폰트 비교판 `FontCompareBoard`(Tools 어셈블리). 크림 종이(Tiled)·갈색·어두운 배경 × 폰트 4종, 인스펙터에서 문장 3개 중 선택·크기 배수 1~3·배경 하나만 보기
- 확인: 1920×1080(3배)에서 스크린샷 — 도트가 정수 배로 선명, 종이 Tiled 찢김 반복 정상. UI_Root 전체 규칙 검사 위반 0개. EditMode 153개 통과

## 2026-11-19 — UI 공통 기반 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md` 4-A·13장)
- Core `Screens/`: `ScreenStack`(팝업 화면 스택, ESC는 맨 위부터 닫고 비어 있으면 `MenuRequested`, `Submitted`), `ScreenPolicy`(화면별 시간 정지 단계·ESC 닫힘·뒤쪽 입력 막기·조작 막기, 기본값은 13장 결정), `TimePause`(전체 정지 / 게임 시계만 정지 참조 카운트), `HudState`(의뢰 목록 펼침/접힘)
- Core `Common/TextTable`(키 → 문장 ScriptableObject, `ITextSource`). 없는 키는 `#키`로 보임
- 기존 `ScreenState` 삭제 → `GameContext.Screens`·`Pause`·`Hud`. 사용처 교체: `GameInputReader`(ESC: 화면 닫기 → 낚시 취소 → 메뉴 요청, Q: 의뢰 목록 토글), `PlayerMover`·`PlayerInteractor`(`BlocksGameplay`), `CreatureNpc`·`RequestBoard`·`RequestCheatKeys`, UI `CreatureDialogView`·`RequestBoardWindow`·`QuestTrackerPanel`. 동작은 그대로이고, 의뢰 게시판이 열리면 이제 전체 정지(timeScale 0), 크리처 대화는 게임 시계만 정지
- Gameplay `TimeScaleDriver`: `TimePause` 전체 정지를 `Time.timeScale`에 반영, `GameBootstrap`이 만들고 씬이 내려갈 때 원래 값으로 되돌림
- UI `Common/`: `UiTheme`(캔버스 기준 높이 360·PPU 16·최소 배율, 층별 정렬 순서, 입력 막기 판 색(지금 투명 = 딤 없음), 폰트), `PixelCanvasScaler`(정수 배율, Pixel Perfect, 에디터에서도 동작), `UiLayerCanvas`(층 캔버스 정렬 순서를 테마에서), `UiScreen`·`IUiScreen`(팝업 기반, 숨기면 비활성), `UiScreenRouter`(스택 → 화면 보이기/숨기기, 입력 막기 판 위치)
- 에디터 `Tools/Editor/UI/UiRootPrefabBuilder`(메뉴 `VoxelMagic/UI/UI 루트 프리팹 만들기`): `Prefabs/UI/UI_Root.prefab`(HudStatic 0 / HudDynamic 10 / HudAnimated 20 / Popup 100 / Overlay 200), `Data/UI/UiTheme.asset`, `Data/UI/Texts_ko.asset`. UI_Root는 아직 어느 씬에도 넣지 않음(첫 화면 제작 때 배치)
- 테스트 `ScreenStackTests` 17개(스택·시간 정지·ESC·메뉴 요청·정수 배율·문구 표). EditMode 전체 통과

## 2026-11-19 — UI 준비 단계 (브랜치 `feature/tool-UI-editor`, `UI_SPEC.md`)
- Sprout Lands를 `.gitignore`에 추가. 사용자가 색을 수정한 `Weather_UI_Free_Noon_ver.png`는 원본과 분리해 `Art/UI/Sprout Lands (수정본)/weather/`로 이동(GUID 유지)
- 패키지 `com.unity.2d.sprite` 1.0.0 설치, Sprite Packer Mode를 Sprite Atlas V2로 (SETUP.md)
- UI 임포트 규칙: `Tools/Editor/UI/UiImportRules`(규칙 데이터) + `UiImportRuleApplier`(메뉴 `VoxelMagic/UI/임포트 규칙 적용`). 규칙 에셋 `Data/Tools/UI/UiImportRules.asset`. `Art/UI` PNG 96장에 적용, 9-slice 경계(책·탭·leaf·바 프레임·종이 패널·스크롤바), `score_ui/v2/score_digits_sheet` 11×12 칸 10장으로 자름. Sprout Lands 시트 자르기는 퀵슬롯·시간대 UI 단계에서
- Sprite Atlas `Art/UI/UiAtlas.spriteatlasv2`: 지금 쓰는 스프라이트 71개 (회전 끔, Tight Packing 끔, 패딩 4, Point, 압축 없음). 미리보기·ver1 숫자·초기 탭 아이콘·별 v3·아이콘 받침·가로 지팡이 원본은 넣지 않음
- 종이 패널 Tiled 확인: Unity Tiled가 Border(가장자리)도 길이 방향으로 반복한다. 76·140·144·300 폭과 22~200 높이에서 직접 이어 붙인 예상 이미지와 픽셀 단위로 일치 (Canvas Reference PPU 16, Pixel Perfect). 조각 분리 스크립트는 필요 없음

## 2026-11-17 — 크리처 굴 정식 씬 + 뒷산·복도 포탈 (브랜치 `Creature-cave-map`)
- 새 씬 `Scenes/Creature_Cave.unity`: 타일 맵 에디터의 `Map_Creature_Cave`(구운 바닥·소품·횃불·조명 스위치·바닥 빛)를 옮겨 옴 + BackMountain의 `_Bootstrap`·`Player`·`Main Camera`·`Directional Light`·`SceneTransition`·`UI`(공통 UI만: 뒷산 전용 `DebugKeyHint`·`CenterBanner` 제외)
  - 콜라이더(씬 인스턴스에만, 메시 크기): `Low_Table`·`Candle_Stand`·`Bookshelf_Filled`·`Book_Stack`·`Cardboard_Box`. `Cat_Bed`는 사용자가 직접
  - 바닥 둘레 보이지 않는 벽 `Map_Creature_Cave/Bounds (invisible)` (바닥 칸 테두리마다 높이 3 m 상자, 바닥 밖으로 떨어지지 않게)
  - `Requests`: 기본 도형 의뢰 게시판(뒷산에서 가져옴, 입구 왼쪽 (97, 0, 0.35)) + `Cave_Creature_Cat`(`CreatureNpc`·`CreatureReactions`·BoxCollider 0.80×0.77×0.67·머리 위 안내 위치)
  - 조명 스위치의 태양을 이 씬의 `Directional Light`로, 원래 값은 이 씬 기준으로 다시 저장
  - 포탈 `Portal_BackMountain`: 통로 끝 트리거 → 뒷산 `FromCave`, 도착 지점 `FromBackMountain`(통로 안 (99, 0.1, −3.5)). 플레이어 기본 위치도 같은 곳
- 크리처 애니메이션: `Cat_Attack_L`·`Cat_Attack_R`·`Cat_Jump` 클립 반복 끔, 컨트롤러에서 끝나면 `Cat_Idle`로 (`Cat_Get_Hit`과 같은 전이). `Cat_Sleep_Start` → `Cat_Sleep`(반복) 상태 추가
- `CreatureReactions`(Gameplay, 새 스크립트): 말 걸면 반응 하나를 무작위로(직전과 다르게) 재생. 반복 동작은 머무는 시간 뒤 Idle로 크로스페이드. 목록·시간은 Inspector
- `CreatureNpc`: 반응이 연결돼 있으면 전달할 수 없을 때 "말 걸기"(E → 반응). 연결 안 된 기존 기본 도형 크리처는 동작 그대로. `GameTexts.Talk` 추가
- BackMountain 씬: `Portal_Creature_Cave`(동굴 입구 안쪽 트리거, 도착 지점 `FromCave` 입구 앞), `Portal_Corridor`(시작 T 남쪽 끝 트리거, 도착 지점 `FromCorridor` (8, 0.1, −3.5))
- Corridor_SideScroll 씬: 왼쪽 문 앞 `Portal_BackMountain`(오른쪽 포탈과 좌우 대칭, 도착 지점 `FromBackMountain`), 부트스트랩 연결 목록에 추가. `CorridorConfig`에 왼쪽 포탈 설정 추가, `CorridorSceneBuilder`가 왼쪽 포탈도 만듦
- Build Settings에 `BackMountain`·`Creature_Cave` 추가 (SETUP.md)
- BackMountain·FarmPond의 기본 도형 크리처 → 고양이 크리처: `Creature` 루트(위치·`CreatureNpc`)는 그대로, 상자·눈을 지우고 `Cave_Creature_Cat`을 `Visual`로 넣음(옛 눈 방향을 보게), BoxCollider 0.80×0.77×0.67, 머리 위 `PromptAnchor`, `CreatureReactions` 연결 → 전달할 수 없을 때 "말 걸기"로 무작위 반응. `Prototype_FarmFishing`은 그대로
- BackMountain·FarmPond의 기본 도형 의뢰 게시판(`RequestBoard`) 삭제 (참조하던 곳 없음). Creature_Cave 게시판은 사용자가 새 메시로 교체
- 개발용 임시 치트 `RequestCheatKeys`(Gameplay/Debug, 에디터·개발 빌드만, 플레이 시작 때 "[Cheat] Requests" 자동 생성, 모든 씬): F10 = 의뢰 게시판 창 열기(같은 `RequestBoardWindow`), F11 = 크리처에게 의뢰 전달·완료(못 하면 부족 재료/의뢰 없음 토스트). GameBootstrap이 없는 복도에서는 동작 안 함. `CreatureNpc.TryDeliver`(정적)로 전달 로직을 크리처 E와 공유
- 개발용 `CreatureReactionClickDebug`(Gameplay/Debug, 에디터·개발 빌드만): F8 프리 카메라가 켜져 있을 때 마우스 왼쪽 클릭 → 연결한 크리처의 무작위 반응(E 말 걸기와 같음). Creature_Cave의 `Cave_Creature_Cat`에만 붙임. `FreeCameraToggle.AnyOn`(정적, 켜짐 여부) 추가
- 개발용 프리 카메라(`FreeCameraToggle`, F8 그대로: 다른 디버그 키는 뒷산 F1~F6, 이동 치트 F9, 의뢰 치트 F10·F11이라 겹치지 않음): 켜 있는 동안 플레이어 입력 리더(`GameInputReader`·`SideScrollInputReader`)를 꺼서 WASD가 카메라만 움직이게 함, 돌아가면 다시 켬. 켠 채로 씬이 바뀌면 프리 카메라를 끔. 원래 플레이 시작 때 스스로 생겨 모든 씬에서 동작 (씬 수정 없음)
- 버그 수정: BackMountain(과 거기서 가져온 Creature_Cave) 플레이어의 `PlayerMover._moveConfig`가 비어 이동이 안 되던 문제 → 두 씬에 `VoxelMagic/캐릭터/텃밭 플레이어 이동·애니 통일`과 같은 연결 적용 (FarmPond와 같은 `MoveConfig`, `CharacterAnimatorView`, 공용 Controller, `CharacterActionView`, 옛 `PlayerAvatarAnimator` 끔)

## 2026-11-16 — 동굴 바닥 타일을 타일 맵 에디터 팔레트에 추가 (브랜치 `feature/tool-tile-editor`)
- 처음 넣은 `Tile/cave/cave_cobblestone.png`(와 타일 에셋)는 사용자 요청으로 삭제. 대신 `Tile/cave_floor_tiles_v3/` 26장(`cave_stone_01~06`, `cave_stone_moss_<방향>_NN`)을 씀
- 임포트 설정 적용(Sprite·Point·압축 없음·32 PPU) → 타일 `Art/Map/TileAssets/cave_floor_tiles_v3/floors/*.asset` 26개
- `FarmTileSetBuilder`: `ExtraFloorPacks`(`cave_floor_tiles_v3`)의 타일을 Farm 팔레트 경계 세트 아래(7줄째부터)에 이끼 방향대로 3×3 블록으로 배치. 변형 번호 01·02·03마다 블록 하나(가운데 = `cave_stone_01~03`, 03 블록은 모서리 그림이 없어 빈칸), 남은 `cave_stone_04~06`은 블록 아래 한 줄. 바닥 줄(맨 윗줄)의 farm 타일 위치는 그대로
- `GroundSurfaceBuilder`: 이름이 `cave_stone`으로 시작하는 타일 = 돌 종류 (앞부분 규칙 `PrefixDefaults`)
- 참고: 그 전에 구운 `Baked/Cave`의 칸 데이터는 지운 `cave_cobblestone`을 가리키므로, 다시 불러오면 그 칸이 비어 있음 (구운 프리팹 모양은 그대로)
- 타일 맵 에디터 창: 새 맵 "바탕 바닥 고르기" 드롭다운 (`TileAssets/<팩>/floors` 바닥 타일 목록, cave 포함)
- 타일 맵 에디터 창: 맵을 고르거나 새로 만들면 카메라·치비·1 m 상자(Compare/Center)만 따라가고, 연못·밭(Compare/East)은 붙어 있던 맵에 그대로 (`TileMapCompareRig`에 `_eastTarget`·`FollowCenter` 추가)

## 2026-11-12 — 채집 슬롯·드랍 밸런스 (서식지 종류별, 슬롯 2칸·0~1개) (브랜치 `Back-mountain-map`)
- 사용자 요청: 슬롯 2칸, 서식지마다 0~1개 무작위 (너무 얻기 쉽지 않게, 밸런스는 추후 조절)
- `GatheringConfig`: `_fillRatio`(슬롯 절반·최소 1개) 대신 서식지 종류별 `HabitatBalance` 목록 — 슬롯 수, 최소·최대 개수, 추가분 확률, 슬롯 배치 모양. 기본값 슬롯 2·0~1개·확률 0.5(가정) → 서식지가 비어 있을 확률 50%
- `GatherSpawner`: 개수 = 최소 + (최대 − 최소)개를 각각 확률로 굴림, 슬롯 수 이하. 0개 가능. 의뢰 재료 보장(맵 전체 최소 1개)은 그대로
- 에디터 창 `VoxelMagic/뒷산/채집 밸런스` (`Editor/Mountain/GatheringBalanceTool`): 종류별 값 편집, 평균 개수·빈 확률 표시, 씬 서식지 슬롯 수 표시(안 맞으면 경고), "슬롯 수 적용" = 기본 도형 프리팹 4종 + 씬의 서식지(기능 붙이기로 만든 것 포함) 슬롯을 늘리거나(배치 모양대로, 서식지 크기 비율 유지) 줄이고(뒤쪽부터) 모든 슬롯을 지면에 맞춤
- 기능 붙이기 도구의 기본 슬롯도 같은 데이터(슬롯 수·배치 모양)를 씀. 채집 배치 메뉴는 데이터가 비면 기본값을 채움
- 적용: 프리팹 4종(4·3·3·4칸 → 2칸), 씬 서식지 13개 모두 2칸 (사용자가 만든 `Mountain_Pine_v5_Droop (2)` 포함)
- 확인: 테스트 136개 통과(0~1개·최대 제한 테스트 추가) / 플레이 새 하루 400번: 서식지당 평균 0.49개, 빈 서식지 51%, 2개 이상 0번, 다른 서식지 재료 0, 맵 전체 평균 6.4개 / 산딸기 의뢰 중에는 100번 모두 1개 이상 / 콘솔 에러 0

## 2026-11-12 — FBX에 기능 붙이기 도구 + 초입 나무 FBX 교체 + 이동 속도 치트 (브랜치 `Back-mountain-map`)
- 메뉴 `VoxelMagic/뒷산/선택한 오브젝트에 기능 붙이기/` (`Editor/Mountain/BackMountainAttachTool`): 서식지 4종·포획 풀숲·의뢰 게시판·크리처. 선택한 오브젝트 위에 기능 루트(지면 높이)를 씌우고 FBX를 그 아래 `Visual`로 옮김 (위치·회전·크기·FBX 콜라이더 유지)
  - 서식지 슬롯: FBX 아래 "Slot…" 빈 오브젝트가 있으면 그 자리(쓰고 나면 지움), 없으면 기본 도형 프리팹 슬롯 배치를 FBX 가로 반지름 × 0.8에 맞춰 늘림. 슬롯은 지면 높이로, 서식지는 슬롯 쪽이 길을 보게 돌림. `[Prototype]/Gathering` 아래로
  - 풀숲: `GrassTuft` + 흔들 대상 `Visual`(밑동 축), `Capture/Zone` 아래로. 게시판·크리처: 상호작용 영역 BoxCollider(트리거, FBX 렌더 범위)·안내 위치(맨 위 + 0.4 m), `Requests` 아래로
- 초입 기본 도형 나무 3그루(`Habitat_TreeShade_1~3`)를 끄고 사용자 나무 FBX(Oak_v5·Oak_v4·Pine_v5, 사용자가 붙인 줄기 BoxCollider 복사)를 같은 자리에 놓은 뒤 도구로 그늘 서식지 기능을 붙임
- 버그 수정: 꺼 둔 서식지·풀숲도 연결 단계에서 이벤트를 구독해 재료 재배치 때 IndexOutOfRange → `MountainBootstrap`은 켜진 서식지·풀숲만 번호를 주고, 번호가 없는 것은 `Bind`에서 건너뜀
- 개발용 이동 속도 치트 (모든 맵, 에디터·개발 빌드만): F9로 걷기·달리기 ×3 켜고 끄기, 왼쪽 위에 "[치트] 이동 속도 ×3" 표시. `Core/Characters/Locomotion/MoveSpeedCheat`(배율) + `Gameplay/Debug/MoveSpeedCheatKey`(플레이 시작 때 스스로 생김, 씬 수정 없음). 걷기·달리기 애니 재생도 같은 배율. 플레이를 끝내면 꺼짐
- 플레이어를 초입 T (8, 0.1, −4.5)로 되돌림
- 확인: 테스트 135개 통과(치트 테스트 1개 추가, 이동 테스트는 치트 초기화) / 플레이: 켜진 서식지 12개(꺼진 3개는 재료 0), 새 나무 3그루에 그늘 재료 배치, 참나무 슬롯에서 "E: 줍기" → 그늘버섯 가방, 줄기로 걸어가면 0.7 m에서 막힘, 치트 끔 4·5.5 → 켬 12·16.5 m/s, 한글 표시 정상 / 풀숲·게시판·크리처 변환은 임시 상자로 구조만 확인하고 지움 / 콘솔 에러 0. FarmPond·복도에서 치트는 같은 이동 코드라 동작할 것으로 보지만 그 씬에서 직접 확인은 안 함

## 2026-11-11 — 뒷산 플레이어 그림자 구멍 수정 + 인수인계 문서 (브랜치 `Back-mountain-map`)
- 원인: 뒷산 해(`Directional Light`)에 Depth Bias 0.1·Normal Bias 0 값은 있었지만 **Use Pipeline Settings가 켜져 있어** URP 기본값(Normal Bias 0.5)을 씀 → 복셀 아바타 그림자에 구멍 (2026-11-03 FarmPond와 같은 문제)
- 수정: `VoxelShadowSettings.Apply`로 씬 조명만 파이프라인 설정 끔 + Depth 0.1·Normal 0 (FarmPond와 같은 값). 프로젝트 URP 설정은 그대로
- 확인: 같은 카메라 위치 전/후 캡처 비교 — 바뀐 화소 306개(0.13%), 모두 어두워지는 쪽(그림자 구멍이 메워짐), 밝아진 곳 0
- 문서: `Docs/HANDOFF_BACKMOUNTAIN.md` 새로 작성, `BACKMOUNTAIN_PROTOTYPE_GUIDE.md` 끝에 "추가 항목"(남은 기능·FBX 교체·최적화 표), `CLAUDE.md` 작업 표에 뒷산 줄 추가(사용자 승인)

## 2026-11-11 — 뒷산 배경 바위산(봉우리 청크) 제작 (브랜치 `Back-mountain-map`)
- 새 FBX 11개 `Art/Map/Mountain/Backdrop/`: 봉우리 `Spire_A·B·C·D` × 길이축 x·z (8개, 각 1.4천~2.8천 삼각형) + 이끼 둔덕 `AP1~AP3` (3개, 합 약 2.4만). 메시는 보이는 면(윗면·−X·−Z)만, 콜라이더 없음
- 팔레트 `Mountain_palette.png` 12~15번 칸에 바위 4색 추가(#B5AA9A·#9A8F82·#7C7268·#5E564E). 1~11번 색은 그대로라 기존 바닥 FBX 영향 없음. 머티리얼은 기존 `Mountain_Voxel` 재사용
- 씬 `BackMountain`에 `MountainBackdrop`(앞줄·뒷줄·둔덕, 총 23개, 약 6만 삼각형) 배치. 그림자 끔. 씬 저장은 사용자 확인 후
- 봉우리 FBX를 8개 → 4개(`Spire_A~D`)로 줄임: 보이지 않던 +X/+Z 면까지 채워(삼각형 약 +60%, 씬 합계 약 8.3만) X·Z 방향 변형을 FBX가 아닌 인스턴스의 Y축 90° 회전으로 처리. 씬의 고유 메시는 23개 → 7개. 옛 `Spire_?x/?z.fbx` 8개는 `Backdrop/_Unused/`로 옮겨 둠(삭제는 사용자)
- 제작 스크립트·배치표: `Source/Backdrop/` (`gen_spires.py`, `gen_apron.py`, `layout.py`, `export_fbx.py`, `placement.json`)

## 2026-11-11 — 뒷산 맵 줄이기 + 맵별 이동 속도 (브랜치 `Back-mountain-map`)
- 맵별 이동 속도: `Data/Config/MoveConfig_BackMountain.asset`(걷기 4·달리기 5.5, 사용자가 올린 값)을 만들어 뒷산 `PlayerMover`·`AvatarAnimationSpeed`에 연결. FarmPond가 쓰는 `MoveConfig`는 걷기 3·달리기 4.5로 되돌림. 코드 변경 없음 (이동 설정은 씬의 플레이어마다 연결하는 에셋)
- 뒷산 씬에서 `Mountain_Floor_C3`·`Mountain_Floor_D` 청크 삭제 (사용자 요청, FBX 파일은 남김). 길 끝(C2 동쪽 변 x = 48)에 투명 벽 `[Prototype]/PathEndWall (invisible)` (높이 3 m)
- 확인: 플레이에서 뒷산 플레이어가 `MoveConfig_BackMountain` 사용, 길 끝에서 동쪽으로 달려도 x 47.57에서 멈춤(낙하 없음), 남은 청크 T·A1~A3·B·C1·C2 / 콘솔 에러 0

## 2026-11-11 — 뒷산 포획 다듬기: 근처 길 신호 흔들림 + 놓침 비율 (브랜치 `Back-mountain-map`)
- 사용자 의도: 근처 산길을 지나갈 때 맨 앞 풀 하나만 짧게 흔들려 "여기에 뭔가 온다"를 알리기. 다가가는 풀마다 흔들리던 것과, 멀어지면 흔들림이 뚝 끊기던 것을 고침
- `CaptureZone`: 구역 안 '앞쪽 풀숲' 흔들기를 없애고, 근처 길 영역(`NearPath`, x 8.5~20 · z 53.5~60)에 걸어 들어오면 `_frontTuft` 하나만 한 번 흔듦. 길을 벗어났다 다시 와야 하고 쿨다운 8초(가정). 영역은 기본 카메라(Pitch 55·거리 12)에서 맨 앞 풀숲이 화면 안에 보이는 구간으로 잡음
- `CaptureService.PassBy`: 구역 밖에서도 됨 (입질 중인 풀숲은 그대로). 흔들림은 상태가 끝나도 `GrassTuft`가 잦아들게 멈춤 (`CaptureConfig` 지나갈 때 0.6초·세기 0.7·잦아듦 0.35초, 가정)
- 맨 앞 풀숲 `GrassTuft_02`를 (12.5, 66) → (12.5, 65.3)으로 길 쪽에 붙임
- `MagicCircleStub`: 무작위 정확도 대신 성공 확률(가정 0.85)로 요구 정확도 이상/미만을 돌려줌 → 놓침 약 15%
- 확인: 테스트 134개 통과 / 플레이: 산길(z 44 → 코너 → x 24)을 걸을 때 맨 앞 풀숲만 1번 (8.5, 53.9)에서 흔들림, 곧바로 되돌아 걸어도 0번(쿨다운), 구역 안에서 풀숲 사이를 걸어도 0번 / 흔들림 세기 0.6초 유지 후 0.84초에 0으로 잦아듦 / 스텁 2000번: 요구 0.5·0.6·0.8 모두 성공 84~87% / 콘솔 에러 0 / 자동 저장 끔

## 2026-11-11 — 뒷산 낚시 UI 정리 + 포획 (브랜치 `Back-mountain-map`)
- 뒷산 씬에서 뺌 (사용자: 그 맵에서 하는 동작의 UI만): `HUDCanvas/FishingMinigame`, 플레이어 `BiteIndicator`, `CharacterActionView`(낚시·물 주기 동작). Core의 낚시·농사 서비스는 공용 `GameContext`가 만들어 그대로 있으나 뒷산에서 쓰는 곳 없음
- `ControlsHintView._sceneText`: 씬별 조작법 문구 (비우면 기본). 뒷산: 이동·Shift 달리기·E·1~5·스크롤 카메라 줌·Q·Esc
- 포획 Core: `AnimalDefinition`(요구 정확도), `CaptureConfig`(입질 간격 5초·흔들림 3초·동시 1~2개·지나갈 때 0.4초/3 m/±60°, 모두 가정), `CaptureService`(구역 안에서만 입질, 구역 밖에서는 흔들리던 것만 끝까지, 포획 중 풀숲은 멈춰 기다림, 성공/실패, 다음 결과 강제), `IMagicCircleDrawer`(실제 마법진으로 바꿔 끼울 자리), `MountainMessages`(중앙 문구)
- Gameplay: `GrassTuft`(흔들림·"E: 포획"), `CaptureZone`(영역 안/밖, 앞쪽 가장 가까운 풀숲 지나갈 때 흔들기), `MagicCircleStub`(1초 뒤 0.3~1 무작위 정확도, 가정). `MountainBootstrap`이 포획도 조립. 디버그 키 F4 입질 즉시·F5/F6 다음 마법진 성공/실패
- UI: `CenterBannerView`("OOO 포획!"·"놓쳤다", 1.5초), 디버그 안내에 마법진 강제 상태
- 데이터·씬: 동물 3종 `Data/Mountain/Animals/`(산토끼 0.5·다람쥐 0.6·여우 0.8), `Data/Config/CaptureConfig`, 프리팹 `Prefabs/Mountain/GrassTuft`. 메뉴 `VoxelMagic/뒷산/포획 구역 만들기·배치` → `[Prototype]/Capture`: 바닥 16×16 m(윗면 4.2), 트리거(0.5 m 안쪽·높이 3 m), 풀숲 12개, "산 동물 조심" 팻말, 투명 벽(서·북·동 1.2 m). 정의 목록 다시 채움
- 확인: 테스트 133개 통과(새 5개) / 플레이: 길(10, 57)은 구역 밖·입질 없음, 걸어서 z 64.56에서 구역 진입(단차 없음), 앞 풀숲 2개 지나갈 때 흔들림, F4 입질 → "E: 포획" → 강제 성공 "여우 포획!"·가방·획득 정보 / 강제 실패 "놓쳤다"·흔들림 멈춤 / 콘솔 에러 0 / 자동 저장 끔. 실시간 5초 간격·3초 흔들림은 에디터가 백그라운드라 프레임이 안 돌아 단위 테스트로만 확인

## 2026-11-11 — 뒷산 프로토타입: 등반 확인 + 채집 (브랜치 `Back-mountain-map`)
- 지침: `Docs/BACKMOUNTAIN_PROTOTYPE_GUIDE.md`. 씬 `BackMountain`
- `feature/character-system`을 `Back-mountain-map`에 병합 (사용자 선택, 충돌 없음) → Shift 달리기·이동/동작 애니·저장이 들어옴
- 씬: 청크 9개에 Mesh Collider (FBX는 그대로). FarmPond의 `_Bootstrap`·Player·Main Camera·UI·SceneTransition을 복사(참조 유지), 플레이어 T 시작점 (8, 0.1, −4.5). 원래 카메라는 `Main Camera (Map Preview, off)`로 꺼 둠
- 등반: 길 중심선을 따라 4 m/s로 오르내림 → 막힘·낙하 없음 (오름 34초, 내림 33초). B 북쪽 변은 포획 구역 바닥 전까지 열려 있음
- 채집 Core: `GatherMaterialDefinition`(서식지·방과후/밤·힌트·모양), `GatheringConfig`(채움 비율, 가정 0.5), `GatherSpawner`(자기 서식지·시간대만, 직전 빈 슬롯 우선, 의뢰 재료 보장), `GatheringService`, `MountainContext`(GameContext는 그대로)
- Gameplay: `MountainBootstrap`(실행 순서 −900), `GatherHabitat`, `GatherPickup`(InteractableBase, "E: 줍기"), `MountainDebugInput`(F1 새 하루, F3 시간대 전환, Inspector에서 키 변경). UI: `DebugKeyHintView`(조작법 위)
- 데이터·에셋: 재료 12종 `Data/Mountain/Gather/` (서식지마다 방과후 2 + 밤 1), 서식지 프리팹 4종 `Prefabs/Mountain/`, 머티리얼 `Materials/Mountain/`. 메뉴 `VoxelMagic/뒷산/채집 서식지 만들기·배치`로 `[Prototype]/Gathering`에 12개 배치(지면 맞춤). 정의 목록 다시 채움
- 확인: 테스트 125개 통과(새 5개) / 플레이: 줍기·누적·가방 가득 참 안내, 새 하루 재배치, 밤엔 밤 재료만 / 콘솔 에러 0 / 시험 중 자동 저장 끔(저장 파일 안 바뀜)

## 2026-11-11 — 뒷산 의뢰 연동 + 인벤토리 우클릭 버리기 (브랜치 `Back-mountain-map`)
- 의뢰 4종 `Data/Mountain/Requests/` (그늘버섯 1 / 산딸기 3 / 쑥 1 + 돌이끼 1 / 달빛버섯 1(밤)), 재료 5종에 크리처 설명 추가. 메뉴 `VoxelMagic/뒷산/의뢰 데이터 만들기`
- 씬: FarmPond의 `RequestBoard`·`Creature`를 `[Prototype]/Requests`로 복사 (게시판 (11, 2), 크리처 (4.6, 0.7)). 게시판 창 920×820·목록 높이 650 (의뢰 4개가 들어가게, 이 씬만)
- `RequestBoardWindow._boardRequests`: 채우면 그 게시판에는 그 의뢰만 (비우면 예전처럼 전체 목록 → FarmPond 그대로)
- `RequestEntryView`·`QuestTrackerPanel`: 채집 재료면 서식지 힌트를 같이 표시. 의뢰를 내려놓으면 추적 패널 숨김
- Core: `RequestTracker.Abandon()`, `Inventory.DiscardSlot()`, `GatherSpawner.EnsureRequired()`, `GatheringService`가 의뢰를 받으면 필요한 재료를 1개 이상 보장 (다른 재료는 그대로)
- 디버그: F2 의뢰 교체(`MountainDebugInput._swapRequests`, 완료한 의뢰는 건너뜀). `InventoryBar._debugRightClickDiscard`를 켜면 칸 우클릭 시 그 칸을 통째로 버림 (뒷산 씬만 켬)
- 확인: 테스트 128개 통과 / 플레이: 게시판에 뒷산 의뢰 4개·힌트, F2 교체·다시 처음으로, 의뢰를 받자 그늘버섯 1개 보장, 우클릭으로 산딸기 2개 버림(왼쪽 클릭은 선택만), 줍기 → 크리처 전달 → 필기 +15·대사·추적 "완료!" / 콘솔 에러 0 / 자동 저장 끔

## 2026-11-11 — 동작 애니메이션 다듬기 + 옛 Controller 정리
- 옛 Controller 삭제 (사용자 승인, GUID로 사용처 0 확인): `Animations/Avatars/Student_Male.controller`, `Student_Male_v05.controller`. `Student_Chibi_v2.controller`는 Prototype·TileMapEditor 씬이 써서 남김
- 물 주기 휘적임 한 번 줄임: `ClipImportPlan`의 `Watering` 구간 0~168 → **84~168** (두 번째 왕복만, 끝 자세 = 기본 자세), 길이 5.60 → 2.80초, 물 들어가는 이벤트 0.3 → **0.4**(1.12초). Override는 같은 클립 이름이라 그대로 연결됨
- 재생 속도 (`Data/Config/CharacterActionConfig`): 던지기 1 → **2**(4초 → 2초, 찌 놓는 순간 1.04초), 낚아채기 1 → **2.5**(4초 → 1.6초) — 사용자: 던지기·성공 동작이 너무 길어 획득 정보 UI가 훨씬 먼저 뜸
- `CharacterActionView`: 이미 그 이동 상태(예: 낚아채기가 끝나 Idle)면 다시 섞지 않음 (Idle→Idle 다시 섞기 없앰)
- 확인 (텃밭 플레이, 0.05초씩 진행): 던지기 1.05초에 놓음·1.65초 기다리기, 낚아채기 1.6초 뒤 Idle, 물 주기 1.20초에 물·2.90초 끝 / 테스트 120개 통과 / 콘솔 에러 0 / 시험 저장 파일 지움

## 2026-11-10 — 캐릭터 시스템 단계 2-④-2: 공용 Controller + 낚시·물 주기 애니메이션 연결
- 설계: `DESIGN_CHARACTER.md` 9-4 (사용자 답: 입질 1초는 기다리기 자세, 미니게임부터 올리기 왕복, 잡으면 낚아채기, 놓치거나 거두면 Idle, 물 주기 중 이동 막고 물은 클립 중간에)
- **공용 Controller**: `Animations/Avatars/Base/CharacterBase.controller` (상태 9개: Idle/Walk/Run + FishCast·FishWait·FishReel·FishReelBack·FishHook·Water, 파라미터 IdleSpeed·WalkSpeed·RunSpeed·ActionSpeed, 이어짐: FishCast→FishWait, FishReel↔FishReelBack, FishHook·Water→Idle) + 빈 열쇠 클립 `Base/Placeholders/*.anim`. 아바타 6개 모두 `Animations/Avatars/<아바타>.overrideController`로 바꾸고 `AvatarSetup._controller` 연결, 복도 프리팹 3개(Chibi_v2·Male·Male_v05) Animator도 Override로
- 메뉴 `VoxelMagic/아바타/공용 Controller 다시 만들기 (모든 아바타)`(`CharacterControllerBuilder`), 메뉴 3(복도 캐릭터 만들기)도 이제 Override를 만듦
- Core: `CharacterActionId`(동작 이름표 = Controller 상태 이름), `CharacterActionConfig`(동작별 재생 속도·섞기·물 주기 안전 시간, `Data/Config/CharacterActionConfig`), `AvatarSetup` 동작 클립 칸 6개(모든 아바타에 같은 Humanoid 클립 자동 연결)
- Core 낚시: `FishingSession.WaitForCastRelease`·`ReleaseCast`·`CastReleased`·`IsCastReleased` — 던지기가 "놓는 순간"까지 기다렸다가 `CastSeconds` 동안 날아감, 신호가 없으면 `FishingConfig._castReleaseTimeoutSeconds`(6초) 뒤 스스로 놓음
- Core 물 주기: `FarmService.UseWateringAction`·`IsWatering`·`WateringStarted`·`ApplyPendingWater`·`FinishWatering`·`WateringFinished` — 물 주기 동작 중에는 이동·상호작용 막고, 물은 이벤트 순간(이벤트 없으면 끝날 때) 한 번
- Gameplay: `CharacterAnimationEvents`(모델, 클립 이벤트 `OnActionEvent(문자열)` → C# 이벤트), `CharacterActionView`(플레이어, 낚시 상태·물 주기 → Animator 상태, 끝나면 이동 상태로), `FishingDriver`(찌·줄은 놓는 순간부터), `PlayerMover`·`PlayerInteractor`(물 주는 중 멈춤·E 막음), 애니메이션 랩 인스턴스에도 이벤트 수신 부품
- 클립 이벤트 (`ClipImportPlan` 새 칸 `Events`): `Fishing Cast_Throw` CastRelease 0.52(2.08초, 손이 앞으로 가장 빠른 순간), `Watering` WaterApply 0.3(1.68초, 지팡이를 처음 다 뻗은 순간) — 표에서 바꾸고 '동작 클립 준비' 다시 실행
- 텃밭 씬: 메뉴 '텃밭 플레이어 이동·애니 통일'이 Override Controller·이벤트 수신·동작 뷰까지 연결, 씬 저장
- 확인: 테스트 120개 통과(새 7개) / 텃밭 플레이(Animator·세션을 0.05초씩 직접 진행): 던지기 시작 → 찌 숨김 → 클립 이벤트로 2.10초에 놓음·찌 보임 → 0.6초 뒤 기다리기, Animator FishCast→FishWait 자동 → 입질 중 FishWait 유지 → E로 미니게임 FishReel↔FishReelBack 왕복(4.85초마다) → 잡음 FishHook → 끝나 Idle / 놓침 → Idle / 물 주기 Water 재생, 1.75초에 물, 5.65초 끝날 때까지 IsWatering, 끝나면 Idle / 기다리기 화면에서 지팡이 끝→연못 찌 줄 / 복도: Override로 Idle/Walk/Run 클립 그대로 / 콘솔 에러 0 / 시험 저장 파일 지움
- 확인 못 함: 실제 키보드(E·WASD)로 처음부터 끝까지, 실제 프레임 속도에서의 모습 — 사용자 플레이로 확인 필요
- 옛 Controller: `Student_Chibi_v2.controller`는 Prototype·TileMapEditor 씬이 아직 씀, `Student_Male.controller`·`Student_Male_v05.controller`는 이제 아무도 안 씀 (지우지 않음)

## 2026-11-10 — 캐릭터 시스템 단계 2-④-1: 낚시·물 주기 클립 준비 + 이동 설정 이름 변경
- 이동 설정 `Data/Config/FarmMoveConfig` → **`MoveConfig`** (복도를 뺀 3D 맵 플레이어 공용, 사용자 결정. GUID 그대로라 씬 연결 유지). 값은 사용자가 맞춘 걷기 3·달리기 4.5, 재생 배율 1.6·1.6. 코드 경로·툴팁 갱신
- Editor: `ClipImportPlan`(에디터 전용 SO, `Data/Avatars/ClipImportPlan` — 파일·프레임 구간·반복·뒤집기 표), `ActionClipTools` + 메뉴 `VoxelMagic/아바타/동작 클립 준비 (ClipImportPlan)`: 표대로 Humanoid 재임포트(메뉴 2와 같은 루트 고정), 구간별 클립, 뒤집은 `.anim` 생성
- 클립 (사용자 지정 구간): `Fishing Cast` → `Fishing Cast_Throw`(0~120, 4.00초)·`Fishing Cast_Reel`(120~끝, 4.77초), `Fishing Idle`(반복, 3.53초), `Watering`(5.60초). 뒤집은 클립 `Animations/Mixamo/Reversed/Fishing Cast_Throw_Reverse.anim`(낚아채기)·`Fishing Cast_Reel_Reverse.anim`(올리기 왕복의 돌아오는 쪽)
- `AvatarPipeline` 자동 클립 연결: 키워드가 든 클립 중 이름이 가장 짧은 것 ("Fishing Idle"이 Idle로 잘못 연결되지 않게)
- 확인 (랩, Student_Chibi_v2): 6개 클립 모두 Humanoid로 목록에 보임·재생 / 뒤집은 클립 = 원본 t ↔ 길이−t 손 위치 차이 0.00cm / 이음새 던지기 끝↔올리기 시작, 올리기 끝↔뒤집은 올리기 시작, 뒤집은 올리기 끝↔올리기 시작 모두 0cm / 던지기 1.2초 3/4 화면에서 손잡이를 쥔 지팡이 / 콘솔 에러 0
- 알아 둘 점: `Fishing Idle` 첫 자세는 던지기 끝과 손 위치 27.6cm 차이 → 4단계 Animator에서 섞어서 넘김. 랩 목록에서 뒤집은 클립처럼 이름이 길면 글자가 두 줄로 겹쳐 보임(표시만)

## 2026-11-10 — 캐릭터 시스템 단계 2-②③: 텃밭·복도 이동 통일 + 텃밭 달리기 + 걷기 재생 속도
- 설계: `DESIGN_CHARACTER.md` 9-3 (사용자: 텃밭 이동 속도 4는 그대로, 걷기 애니 재생이 너무 빠름 → 낮춤, 달리기는 이동·재생 모두 2.25배)
- Core `Characters/Locomotion/`: `ICharacterMotor`(이동기 계약)·`ILocomotionSource`(이동 상태 알림), `PlanarMotor`(바닥 평면 가속·감속, 대각선 정규화), `LocomotionStateMachine`(Idle/Walk/Run 공용 상태 머신)
- Core 변경: `MoveIntent`에 `Planar`(바닥 평면 방향)와 평면용 생성자, `CharacterBody.Motor`를 `ICharacterMotor`로, Idle/Walk/Run `Tick`이 `Motor.Drive` 호출, `SideScrollMotor`가 `ICharacterMotor` 구현, 복도 `CharacterStateMachine`은 방향 전환·끝 막힘만 하고 나머지는 `LocomotionStateMachine`에 맡김 (공개 API 그대로), `CharacterMoveConfig`에 상태별 애니메이션 재생 배율(`AnimationScaleFor`)
- Gameplay: 텃밭 `PlayerMover`가 공용 상태 머신 + `PlanarMotor` + Shift 달리기, 이동 방향은 지금처럼 `PlayerConfig` 카메라 Yaw 기준. `CharacterAnimatorView`를 `ILocomotionSource` 공용으로(복도는 그대로 Bind, 텃밭은 `PlayerMover`가 연결). `AvatarAnimationSpeed`가 장면 이동 설정의 배율을 곱함. `GameInputReader.Run`
- 입력: `GameControls` `Gameplay` 맵에 `Run`(왼쪽 Shift) — SETUP.md 입력 액션
- 데이터: `Data/Config/FarmMoveConfig`(→ 같은 날 `MoveConfig`로 이름 변경, 복도를 뺀 3D 맵 공용. 처음 값: 걷기 4·달리기 9, 가속 40·감속 50, 재생 배율 걷기 1.6·달리기 3.6 — 예전 걷기 재생은 이동 속도 ÷ 1 = 4배였음)
- Editor: 메뉴 `VoxelMagic/캐릭터/텃밭 플레이어 이동·애니 통일`(`PlayerLocomotionSetup`), `PrototypeSceneBuilder`도 같은 처리 (실행 안 함)
- 텃밭 씬(FarmPond): Model에 `CharacterAnimatorView`·`AvatarAnimationSpeed`, `PlayerMover`에 이동 설정·애니 뷰 연결, 옛 `PlayerAvatarAnimator`는 **끄기만**(삭제 안 함, Prototype 씬은 아직 사용)
- 확인: 테스트 113개 통과(새 6개, 복도 상태 머신 기존 테스트 그대로 통과) / 텃밭 플레이: 상태 머신에 입력을 넣어 Idle → D 걷기(속력 4, 방향 = 카메라 오른쪽) → Shift 달리기(9) → W+D 달리기(대각선도 9) → 놓기 Idle, Animator Idle/Walk/Run 따라감, 재생 값 Walk 1.6·Run 3.6 / 복도 플레이: 걷기 1.6·달리기 3.6·좌우 방향·Animator 그대로 / 콘솔 에러 0
- 확인 못 함: 실제 키보드 입력(에디터 포커스가 없어 가상 키가 액션에 안 들어감 — 사용자 플레이로 확인 필요)
- 이동 방향 질문(D가 화면 대각선으로 간다는 보고): 코드는 카메라 기준이라 D = 화면 오른쪽으로 측정됨. 사용자가 카메라 Yaw를 바꿔 직접 시험하기로 함 (이동은 Yaw를 따라감)

## 2026-11-10 — 캐릭터 시스템 단계 2-①: 플레이어 데이터 유지 + 파일 저장
- 설계: `DESIGN_CHARACTER.md` 9번 (사용자 "제작 시작" 승인, 저장은 파일까지)
- Core `Session/`: `PlayerProgress`(인벤토리·점수·도감·의뢰 + 밭 보관본, 나중에 스탯 자리), `GameSession`(정적 보관소, 저장 직전 `Capturing`), `SaveData`·`SlotRecord`·`FarmPlotRecord`(JSON용, 버전 1), `ISaveStore`, `DefinitionCatalog`(ID → 에셋), `SaveMapper`(진행 상태 ↔ 저장 데이터, 모르는 ID 건너뜀)
- Core 변경: `GameContext.Create`가 `PlayerProgress`를 받아 씀(플레이어 데이터는 씬을 넘어 유지, 밭 격자·낚시·메시지·화면 상태는 씬마다 새로), `Inventory`·`ScoreBook`·`MaterialJournal`·`RequestTracker`에 불러오기용 `Restore`(internal), `MaterialJournal.DeliveredIds`, `RequestTracker.CompletedRequests`
- Gameplay: `Save/JsonFileSaveStore`(임시 파일에 쓴 뒤 바꿔치기), `Save/GameSaveSystem`(처음 한 번 불러오기/새 게임, 저장, 종료 때 저장), `GameBootstrap`(세션에서 진행 상태 받기, 연결 뒤 밭 다시 심기, 씬을 떠날 때 밭 보관), `SceneTransitionDirector`(페이드 아웃 뒤 로드 전에 저장)
- UI: 씬을 넘는 데이터 구독을 OnDestroy에서 풀도록 수정 — `QuestTrackerPanel`(해제 없던 것 + 씬에 들어올 때 진행 중 의뢰 바로 표시), `RequestBoardWindow`(람다 구독 → 메서드)
- Editor: `Save/SaveTools` 메뉴 `VoxelMagic/저장/`, `PrototypeSceneBuilder`가 정의 목록 연결 (실행 안 함)
- 데이터·씬: `Data/Save/DefinitionCatalog`(아이템 10·의뢰 3), `FarmPond` 씬 `_Bootstrap`에 연결·`_autoSave` 켬 (씬 diff: 이 두 줄 + 기존 컴포넌트 필드 `_probeHeight` 기본값 기록 한 줄)
- 확인: 테스트 107개 통과(새 6개: JSON 왕복, 모르는 ID, 인벤토리 이벤트, 진행 중 의뢰 재지급 없음, 점수 범위, 세션 보관) / 플레이: 텃밭에서 아이템·점수·의뢰·밭(물 준 칸 포함) 바꿈 → 복도(진행 상태 그대로, 저장 파일 생성) → 텃밭(인벤토리 같은 객체, 밭 2칸·물 상태·뷰, 의뢰 추적 패널 표시) → 인벤토리 이벤트 구독자에 사라진 씬 UI 없음 → 플레이 끄기(저장) → 다시 켜기(파일에서 그대로 불러옴) / 콘솔 에러 0 / 시험 저장 파일은 지움
- 덤: Unity가 `Animations/Mixamo/Y Bot@Watering.fbx.meta`를 만듦(사용자가 넣은 FBX의 임포트)

## 2026-11-10 — 뒷산 바닥 복셀 FBX 9개 (브랜치 `Farm-pond-map`)
- 설계: `Docs/DESIGN_MOUNTAIN_FLOOR.md`. 복셀 1칸 0.0667 m, 청크 16 × 16 m(입장 꼬리 T만 6 × 5 m), 윗면 y = 0 기준 절대 높이로 구움(길 높이 0 → 약 11 m)
- 만든 것 (`Art/Map/Mountain/`): `Mountain_Floor_T/A1/A2/A3/B/C1/C2/C3/D.fbx`, `Mountain_palette.png`(256 × 1, Point·Mip 끔·무압축), `Mountain_Voxel.mat`(`M_Voxel` 복사 + 팔레트), `MountainFloor_All.prefab`(청크 9개를 (x, 0, z)에 조립한 확인용), `Source/`(청크별 메시 데이터 `.npz`, 생성 스크립트 `hf.py`·`gen_mesh.py` — 파이썬, 유니티는 무시). 확인 씬 `Scenes/Tools/MountainFloorPreview.unity`
- 색: FarmPond 구운 아틀라스에서 읽은 길 `#8A5A36` + 외곽선 1복셀 `#4C321E`, 흙 `#6A4A33` 한 색, 연못 이끼색 `#507848` 덩어리. 연갈색 흙 없음. 바깥 둘레에 1.5 m(22복셀) 흙 단면 (윗 0.4 m 짙은 흙 + 그 아래 더 어두운 흙). 아랫면·안쪽 없음, 포획 구역이 붙을 B 북쪽 변은 단면 없이 열어 둠
- 임포트: 배율 1·회전 0(축·단위 구움 처리), Read/Write 끔, 메시 압축 끔, 콜라이더 없음, 머티리얼은 `Mountain_Voxel` 하나를 모든 청크가 공유
- 확인: 청크 경계 상자 (0~16, y, 0~16) 정확, 윗면 법선 전부 위, 윗면 면적 = 칸 수, 이웃 칸 단차 2복셀 이하, 삼각형 청크당 약 4만~5만 (총 약 38만). 이음새 틈 없음(화면 확인)
- 도구: 높이장 → 청크 greedy 메시(`.npz`) → Blender에서 FBX. MagicaVoxel로 고치고 싶으면 `gen_mesh.py`가 청크별 `.vox`도 만든다 (청크당 약 5 MB라 프로젝트에는 넣지 않음)
- FarmPond 씬·기존 에셋은 건드리지 않음 (`MountainFloorPreview`는 새 씬)

## 2026-11-09 — 씬 이동: 복도 ↔ FarmPond, 보이지 않는 트리거 + 픽셀 계단 원형 페이드
- Core: `SceneFlow/SceneTravel`(이동 중 여부·도착 지점 ID·이벤트, 정적), `IScreenFade` / `Data/Config/SceneTransitionConfig`(페이드 아웃·멈춤·인 0.4·0.05·0.4초, 블록 12 px@1080p, 디더 3블록, 색, 원 중심 = 플레이어, 정렬 순서 500) → `Data/Config/SceneTransitionConfig.asset`
- Gameplay: `SceneFlow/SceneTransitionDirector`(씬마다 하나: 입력 막기 → 페이드 아웃 → 비동기 로드 → 새 씬 진행자가 가린 채 시작 → 페이드 인 → 입력 풀기), `SceneTransitionTrigger`(보이지 않는 BoxCollider 범위에 플레이어 발 + 0.2 m 점이 들어오는 순간 이동. 물리 이벤트가 아니라 쿼터뷰·횡스크롤 공용), `SceneSpawnPoint`(도착 ID가 맞으면 부트스트랩보다 먼저 플레이어를 옮김, 횡스크롤은 이동 계산기 x도). `SideScrollMotor.Teleport` 추가
- 입력 막기: `GameInputReader`·`SideScrollInputReader`가 이동 중에는 게임플레이 액션 맵을 끔. 페이드 판이 클릭도 막음. 환경설정 UI는 다른 액션 맵 + 더 높은 정렬 순서로 만들면 영향 없음
- UI: `SceneFlow/PixelCircleFadeView` + 셰이더 `Art/Shaders/PixelCircleFade.shader`(화면을 블록으로 잘라 원 밖을 칠하고 테두리는 4×4 Bayer 디더) + `Art/Materials/UI_PixelCircleFade.mat`. 이미지 시퀀스 대신 셰이더로 (해상도 무관, 숫자로 조절)
- Editor: `SceneFlow/SceneTransitionRigBuilder`(진행자+페이드 캔버스, 트리거, 도착 지점 만들기). `CorridorSceneBuilder`는 원기둥 포털 대신 트리거 + 돌아올 도착 지점(`FromFarm`, 포털 왼쪽 1.2 m)을 만들고 부트스트랩 연결 맨 앞에 넣음
- `CorridorConfig` 포털: 이동 씬 `FarmPond`, 도착 ID `FromCorridor`, 트리거 폭 0.4 m 추가 / 지름·트리거 반지름·색 삭제. `ScenePortal.cs` 삭제 (`Art/Materials/CorridorPortal.mat`은 안 쓰지만 남겨 둠)
- 씬: 복도 — 원기둥 포털 지우고 `Portal_FarmPond`(트리거 x 33.28, 도착 `FromFarm` x 32.08) + `SceneTransition`. FarmPond — `World/Portal_Corridor`(꼬리 길 끝 트리거 (80, 1, −7.5) 4×2×1 m, 도착 `FromCorridor` (80, 0.1, −4.5)) + `SceneTransition`
- Build Settings: `FarmPond` 추가 (`SETUP.md`)
- 확인 (플레이 모드, 프레임 한 칸씩 진행): FarmPond 꼬리 끝 → 원형 픽셀 페이드 → 복도 x 32.08 도착 → 입력 다시 켜짐 / 복도 포털 → FarmPond (80, −4.5) 도착, 게임플레이 키 켜짐 / 화면: 블록 계단 + 디더 테두리 원이 플레이어 중심으로 좁아짐 / 콘솔 에러 0
- 처음엔 복도 캐릭터 발 높이가 트리거 바닥과 똑같아 잡히지 않아서, 발 + 0.2 m 점으로 검사하게 고침

## 2026-11-09 — 타일 맵 굽기: 충돌체를 칠한 칸 모양대로 BoxCollider 여러 개로
- 원인: 칠한 범위 전체를 감싸는 상자 하나라, 포탈 길(꼬리)을 빼면 옆의 빈 허공까지 덮어 플레이어가 허공을 걸었음
- Editor: `TileFloorBaker.AddFloorCollider` — 칠한 칸을 가로로 묶고 같은 폭의 다음 줄과 합쳐 직사각형마다 BoxCollider 하나 (빈 칸은 덮지 않음). 결과 표시는 "충돌체 N개 (칸 모양)"
- 다시 굽기: `FarmFishing` 380칸 → BoxCollider 7개. `FarmPond` 씬의 바닥은 프리팹이라 자동 갱신. `TileMapEditor` 씬은 저장하지 않음 (비교 장비가 자동으로 옮긴 위치가 저장되는 것을 막으려고 되돌림)
- 확인: 아래로 레이 → 꼬리(80,-5)는 바닥, 꼬리 옆 빈 곳(72,-5)·(88,-5)·모서리(70.5,0.5)는 바닥 없음 / 콘솔 에러 0. 플레이 중 실제로 떨어지는지는 에디터 시간이 멈춰 있어 확인 못 함
- 남은 점: 이제 맵 가장자리에서 떨어진다 → 가장자리 벽은 다음 작업

## 2026-11-09 — 정식 텃밭·연못 씬 `Scenes/FarmPond.unity` 추가 (브랜치 `FarmPond-Map`)
- `Prototype_FarmFishing`에서 복사 (한 번에 같이 복제해 서로 참조 유지): `_Bootstrap`, Main Camera, Directional Light, Player, UI(EventSystem·HUDCanvas 전체), `World/` RequestBoard·FarmField·Pond·Creature. Ground·School·GinkgoTrees는 안 가져옴
- `TileMapEditor`에서 복사: 구운 바닥 `World/FarmFishing_Floor`(프리팹 연결 유지, 위치 70,0,0), `SM_*` 25개 → 최상위 `Decor` 아래 (위치 그대로, `Decor`로 한 번에 켜고 끄기)
- 위치: Pond·FarmField는 에디터의 `Compare/East` 배치와 같은 월드 위치·회전 (Pond 74.61,0,10.59 / 회전 321, FarmField 82.6,0,4.94). Player (80,0.1,5), RequestBoard (80,0,9), Creature (80,0,-2)는 임시 위치
- 참조 정리: `GameBootstrap._bindRoots[0]`이 원본 씬 World를 가리켜 새 World로 고침. 그 밖의 빈 칸(_promptAnchor·_area)은 원본도 비어 있음
- 원본 두 씬은 저장하지 않고 닫음 (git 변경 없음). Build Settings·`CLAUDE.md` 표는 아직 안 바꿈
- 확인 (플레이 모드): 콘솔 에러 0, 플레이어 발밑 = 구운 바닥 충돌체, 카메라가 플레이어 따라감, 밭 36칸, HUD 표시. 심기·낚시·의뢰·대화 직접 조작은 아직 확인 안 함
- 남은 점: FarmField(82.6, 4.94)는 1 m 칸 격자에 딱 맞지 않음 (에디터 배치 그대로 둠)

## 2026-11-09 — 타일 맵 에디터: farm_grass 바탕 경계 세트 2개 추가 (사용자 제작 그림)
- 사용자가 `Art/Map/Tile/farm/transitions/`에 `farm_grass_to_dark_garden_soil`, `farm_grass_to_dry_dirt_path` (각 12장, 32 px, 기존과 같은 방향 이름 규칙 — 그림으로 확인)를 넣음
- Editor: `FarmTileSetBuilder`가 `meadow_grass_to_` 세트만이 아니라 **`<바탕 풀>_to_<바닥>` 세트를 모두** 처리. Rule Tile 이름: 기본 바탕(meadow_grass)은 `RT_<바닥>` 그대로, 다른 바탕은 `RT_<바탕>_to_<바닥>`. 팔레트에서 기본 바탕 세트가 먼저 오게 정렬(기존 배치 유지). `GroundSurfaceBuilder`가 새 Rule Tile 이름에서 바닥 종류를 읽음
- 실행: 임포트 설정(411장, 새 24장 포함) → farm 타일 72개(바닥 12 + 경계 60), Rule Tile 5개(새 `RT_farm_grass_to_dark_garden_soil`, `RT_farm_grass_to_dry_dirt_path`), 팔레트 Farm 82칸(Rule Tile 줄 5개, 경계 블록 5개) → 바닥 종류 표 77줄(새 26줄, 모두 "흙"). 기존 타일·Rule Tile·팔레트는 같은 자리에서 갱신 (GUID 그대로)
- 확인: 미리보기 장면(저장 안 됨)에서 farm_grass 바탕 위 새 Rule Tile 2개 → 바깥·안쪽 모서리·변 경계가 자동으로 붙음 / 콘솔 에러 0

## 2026-11-09 — 농장 맵: 밭 칸을 새 FBX(Farm_Field_Dry/Wet)로 교체, 물 주면 Wet으로
- 파일 (사용자): `Art/Map/Farm/Farm_Field_Dry.fbx`·`Farm_Field_Wet.fbx`(가로·세로 1.6 m, 높이 0.2 m, 바닥 한가운데 기준점, 메시 하나·카메라·조명 없음), 팔레트 `farm_field_dry_palette.png`·`farm_field_wet_palette.png`(64×8). UV가 팔레트 여러 칸을 씀 (Dry 6색, Wet 7색)
- 임포트 설정: 이전과 같게 — FBX 카메라·조명 끔, 애니메이션 없음 / 텍스처 Point·Mip Map 끔·압축 없음·Clamp
- 씬(`Prototype_FarmFishing`, 다시 만들지 않고 직접 수정·저장): `FarmField/Plots` 36칸을 `Farm_Field_Dry` 인스턴스로 교체 (이름·위치 그대로, 배율 1 ÷ 1.6 = 0.625, 윗면 0.125 m), 놓기 미리보기 높이 0.17 → 0.145. `Plots`에 `FarmSoilView` 추가
- Gameplay: 새 `Farming/FarmSoilView` — `FarmGrid` 이벤트를 듣고, 물 준 작물이 차지한 칸의 메시·머티리얼을 Wet 모델 것으로, 뽑으면 Dry로 되돌림 (칸은 위치로 격자에 짝지음, 모델은 Inspector에서 바꿀 수 있음). Core 변경 없음
- 씬 생성 도구: `FarmPlotModelPath` → `Farm_Field_Dry.fbx`, `FarmPlotWetModelPath` 추가, `Plots`에 `FarmSoilView` 연결
- 확인 (플레이 모드, 함수 직접 호출): 호박(2×2)·무 심기, 격자 끝은 "칸이 부족해요", 심은 뒤 Wet 0칸 → 호박 물 주기 → 다 자람 + 그 4칸만 Wet(머티리얼도 Wet) → 뽑기 → 호박 획득, Wet 0칸 → 무 물 주기 → 그 칸 Wet / 화면: 젖은 칸이 진하게 보이고 작물이 밭 위에 놓임 / 콘솔 에러 0
- 이전 `farm_grid.fbx`·`farm_grid.png`는 씬에서 쓰지 않지만 지우지 않음

## 2026-11-09 — 농장 맵: 밭 칸을 사용자 FBX(farm_grid)로 교체
- 파일 (사용자): `Art/Map/Farm/farm_grid.fbx`(가로·세로 2 m, 높이 0.3 m, 1복셀 0.1 m, 바닥 한가운데 기준점), `farm_grid.png`(256×1 팔레트)
- 임포트 설정: FBX **카메라·조명 가져오기 끔**(Blender의 카메라·조명이 같이 들어 있어 칸마다 매우 밝은 조명이 생겨 화면이 하얗게 번졌음), 애니메이션 없음 / 텍스처 Point·Mip Map 끔·압축 없음·Clamp
- 씬(`Prototype_FarmFishing`, 사용자가 고친 씬이라 다시 만들지 않고 직접 수정): `FarmField/Plots`에 6×6칸(`Plot_x_y`)을 칸 크기 1 m에 맞춰 배율 0.5로 배치(윗면 0.15 m), 예전 기본 도형 `Cells`는 꺼 둠(지우지 않음), 놓기 미리보기 높이 0.12 → 0.17(밭 윗면 위)
- 기능 코드 변경 없음 (밭 칸은 보이는 모양일 뿐이고 심기·물 주기·뽑기는 `FarmField` 격자 계산 그대로)
- 씬 생성 도구: 밭 모델이 있으면 같은 방식으로 배치(칸 크기 ÷ 모델 폭으로 배율, 미리보기 높이 = 윗면 + 0.02), 없으면 예전 기본 도형
- 확인 (상호작용 함수 직접 호출): 대파·호박·무 심기, 격자 끝은 "칸이 부족해요", 물 주기 → 다 자람, 뽑기 → 대파 획득, 작물이 밭 위에 보임 / 콘솔 에러 0
- 참고: 모델의 UV가 모두 팔레트 한 칸(68번, 갈색)을 가리킴 → 한 가지 색 + 음영으로 보임

## 2026-11-08 — 타일 맵 에디터: 충돌체 함께 굽기
- 사용자 요청 (설계 승인 절차 없이 바로 진행하라고 함, 브랜치 `feature/tool-tile-editor`): 구운 바닥이 충돌까지 맡아 게임 씬의 `Ground` 상자 없이 걸을 수 있게
- Gameplay: `TileMapDocument`에 맵별 "충돌체 함께 굽기"(기본 켬). Core: `TileMapConfig`에 충돌체 두께 0.5 m
- Editor: `TileFloorBaker`가 켜져 있으면 프리팹 루트에 **칠한 범위만 한 BoxCollider**(윗면 y = 0, 두께는 설정값)를 넣음, 끄면 안 넣음. 도구 창 굽기 영역에 체크박스, 결과 줄에 충돌체 크기
- 확인: 시험 맵 10×8칸 굽기 → BoxCollider 10×0.5×8 m, 윗면 0 / 끄고 다시 굽기 → 프리팹에 충돌체 없음 / 플레이 모드에서 CharacterController(중력 적용)를 2 m 위에서 떨어뜨림 → 바닥 위에 섬(y 0.08 = 기본 skin width), 발밑 바닥 "흙", 3 m 걸어도 그대로 섬, 칠한 범위 밖으로 나가면 떨어짐(예상대로) / 테스트 92개 통과, 콘솔 에러 0. 시험 맵·결과는 지움
- 이미 구운 맵(TestA, FarmSample)은 다시 굽기 전까지 충돌체 없음

## 2026-11-06 — 칩비 소켓을 위팔 본 기준으로 (Run에서 지팡이 틀어짐 수정)
- 사용자 보고: Run에서 지팡이 위치·각도가 프레임마다 틀어짐 (Idle·Walk는 괜찮아 보임)
- 원인 (측정): 칩비 팔 메시 `ArmR_Mesh`는 정점 전부가 `RightArm`(위팔) 본에만 붙어 있어 팔꿈치·손목이 안 보이는데, 소켓은 `RightHand` 본에 붙어 있어 애니메이션의 팔꿈치·손목 회전을 지팡이만 따라감. 보이는 팔 기준 소켓 어긋남(20프레임 최대): Idle 1.1°/0.1cm, Walk 28.6°/8.4cm, Run 124.5°/32.3cm
- 수정 (데이터만): `SocketProfile_Chibi` 기준 본 `RightHand` → `RightUpperArm`, 보정 위치 (0, 0.35, −0.015)·회전 (25.2, 59.8, 171.8) — Idle에서 예전 소켓과 같은 자리(차이 0.04cm, 0°). `CharacterSocketSetup` 기본값도 같게
- 확인: 랩 v2 Idle·Walk·Run 모두 보이는 팔 기준 어긋남 0.0°/0.0cm, Run 0.2초 옆 화면 / 농장: 소켓이 `RightArm` 아래, 낚싯줄 시작점 = Tip, 다른 칸 선택 시 숨김·다시 보임 / 콘솔 에러 0
- 원칙 (사용자 결정): 앞으로 사람형·크리처 아바타 모두 칩비처럼 팔·다리가 한 조각인 형태로 만듦 → 소켓은 "보이는 손이 실제로 따라가는 본"(칩비형은 위팔)에 붙인다. DESIGN_CHARACTER.md 8번
- 알아 둘 점: `Attachment_WandTemp` Hold Position이 지금 (0, −0.25, 0.181) — 사용자가 조절하던 값으로 그대로 둠. Y −0.25 때문에 지팡이가 주먹에서 약 31cm 떨어져 보임

## 2026-11-06 — 소켓·장착물 값 플레이 중 바로 반영 (에디터 전용)
- 사용자 요청: 플레이 중 `Attachment_WandTemp`의 Hold Position을 바꿔도 화면이 안 바뀜 (처음 붙일 때 한 번만 읽기 때문)
- Core: `AttachmentDefinition`·`SocketProfile`에 에디터 전용 `Edited` 이벤트 (`OnValidate`에서 알림, `#if UNITY_EDITOR`라 빌드에는 없음)
- Gameplay: `SocketHost`는 프로필 `Edited` 때 이미 만든 소켓 위치·회전을 다시 적용, `CharacterEquipment`는 장착물 정의 `Edited` 때 만든 인스턴스의 놓는 위치·회전을 다시 적용. 값이 바뀔 때만 실행(매 프레임 검사 없음), 오브젝트가 사라질 때 구독 해제. `SocketHost.Initialize`가 이전 프로필 구독을 먼저 풀도록 순서 수정
- 확인 (인스펙터와 같은 `SerializedObject` 적용으로): 농장 — Hold z +0.1 → 지팡이 바로 이동, 소켓 회전 X −20 → 소켓 바로 회전, 인벤토리로 내렸다 다시 들어도 반영 / 랩 — v1에서 같은 확인, v2로 바꾼 뒤에도 반영 / 시험 값은 원래대로 되돌림 / 테스트 101개 통과, 콘솔 에러 0
- 알아 둘 점: 플레이 중 프로필에 **새 소켓 항목을 추가**한 것은 다시 시작해야 생김 (값 변경만 바로 반영). 지금 `Attachment_WandTemp` Hold Position X가 −9.2 (사용자가 조절하던 값, 그대로 둠)

## 2026-11-06 — 지팡이 메시: Magic_Staff_v2.fbx 적용
- 에셋 (사용자 제공): `Art/Objects/Wand/Magic_Staff_v2.fbx`(복셀 32칸 길이 2.13, 메시 +Z 쪽이 굵은 연갈색 손잡이, −Z 쪽이 얇은 진갈색 자루), `staff_v2_palette.png`(32×8). 팔레트 임포트를 캐릭터 팔레트와 같게 Point·무압축·밉맵 끔으로 바꿈 (Bilinear면 이웃 색이 번짐)
- `Prefabs/Attachments/Wand_Temp`: 상자 `Mesh` → FBX 인스턴스 `Mesh`(배율 0.2719로 길이 0.58 = 임시 지팡이와 같게, 사용자 결정. Y 180°로 굵은 손잡이가 뒤(−Z)·얇은 끝이 앞(+Z), 콜라이더 없음). `Tip`은 얇은 끝. 처음엔 굵은 쪽을 머리 장식으로 잘못 보고 반대로 붙였다가 사용자 지적으로 뒤집음. 농장·애니메이션 랩이 같은 프리팹이라 둘 다 바뀜
- `Attachment_WandTemp` 놓는 위치 z 0.145 → 0.181: 손잡이 범위(프리팹 z −0.290 ~ −0.072)의 가운데를 쥠. 얇은 자루 전체가 손 위로 나옴 (사용자 요청: 손에 덜 잡히고 위로 더 올라오게)
- 확인 (농장 플레이): 손에 FBX 지팡이, 경계 크기 (0.12, 0.39, 0.56), 손잡이를 쥐고 얇은 자루가 앞쪽 위로, 낚싯줄 시작점 = 얇은 끝 Tip, 콘솔 에러 0. 농장 씬 파일은 바뀌지 않음 (프리팹만 바뀜)
- 이름: 프리팹·정의 이름은 아직 `Wand_Temp`·"임시 지팡이" 그대로

## 2026-11-06 — 캐릭터 공용 시스템 단계 1-b: 애니메이션 랩 장착물 패널 + 모든 패널 접이식
- 설정: `VoxelMagic.Tools.asmdef` 참조에 `VoxelMagic.Gameplay` 추가 (사용자 승인, SETUP.md 어셈블리 표). 랩은 농장과 같은 `HumanoidSocketSource`·`SocketHost`·`CharacterEquipment`를 그대로 씀 — 랩 전용 소켓 코드 없음
- Tools: `LabFoldout`(제목 줄을 누르면 내용 숨기고 높이를 제목 줄만큼, `-`/`+` 표시. ▼▶ 기호는 한글 폰트에 있는지 확인하려면 폰트 에셋이 바뀌어서 ASCII로), `AnimationLab` 장착물 처리(새 모델마다 부품을 붙이고 같은 장착물 다시 적용, 목록 "없음"+모든 `AttachmentDefinition`, 보이기 토글·단축키 E, 소켓 프로필 없음/소켓 없음 안내), `AnimationLabBridge.ScanAttachments`(에디터 검색, 빌드용 `_fallbackAttachments`), `AnimationLabConfig._equipFirstAttachmentOnStart`(기본 켬), `LabTexts` 문구
- 랩 씬 생성 도구: 패널 6개(아바타 / 장착물 / 측정·카메라 / 상태 / 애니메이션 목록 / 재생) 모두 접이식 제목 줄, 왼쪽 세로 묶음(아바타 → 장착물 → 측정·카메라, 위를 접으면 아래가 올라옴), 아래 재생 패널은 접으면 아래로 내려앉음. 측정 패널을 왼쪽 묶음으로 옮김(게임 뷰가 낮을 때 장착물 패널과 겹치던 것 해결). 예전 제목 글자 3개는 제목 줄로 대체
- 랩 씬 다시 만듦 (사용자 승인). 이전 씬과 오브젝트 이름 목록 비교: 사라진 것은 `Title` 3개뿐, 조명·카메라·배경 값 같음
- 확인 (플레이): Student_Chibi_v2 오른손에 임시 지팡이, 정보 "소켓: 오른손 · 장착: 임시 지팡이" / 아바타 패널 접기 → 장착물 패널 y −280 → −54로 올라옴, 제목 "+  아바타" / 보이기 끄기·켜기, "없음"·다시 지팡이 / v05(프로필 없음) 안내 → v2로 돌아오면 지팡이 다시 듦 / Walk 0.5초 옆 화면에서 손에 쥔 모습 / 콘솔 에러 0
- 추가 (사용자 보고: 랩 첫 아바타 `Student_Chibi`에서 지팡이가 안 보임): `Data/Avatars/Student_Chibi`에도 `SocketProfile_Chibi` 연결. 랩에서 v1·v2의 Idle 손 본 위치·회전을 재 보니 같음(손 회전 (354.8, 91.6, 172.1), 위치 차이 3mm) → 프로필 공유. 확인: 랩 시작 시 v1 오른손에 지팡이, 정보 "소켓: 오른손 · 장착: 임시 지팡이", 콘솔 에러 0
- 알아 둘 점: Student_Male(v02~v05)는 아직 소켓 프로필이 없어 안내만 나옴 — 손 크기·각도가 칩비와 달라 따로 맞춰야 함

## 2026-11-06 — 캐릭터 공용 시스템 단계 1-a: 농장 플레이어 오른손 소켓에 지팡이
- 설계: `DESIGN_CHARACTER.md` (사용자 "제작 시작" 승인). CLAUDE.md 작업 표에 이 작업 줄 추가(사용자 승인)
- Core `Characters/`: `SocketId`(소켓 이름표 SO), `SocketBinding`(본 + 위치·회전 보정), `SocketProfile`(아바타별 소켓 목록 SO), `AttachmentDefinition`(장착물 SO: 프리팹·소켓·놓는 위치·회전), `EquipmentState`(소켓별 장착, `Changed` 이벤트)
- Core 변경: `AvatarSetup._socketProfile`, `ToolDefinition._attachment` 필드 추가
- Gameplay `Characters/`: `ISocketSource`·`HumanoidSocketSource`·`TransformSocketSource`, `SocketHost`(처음 필요할 때 본 아래 소켓 한 번 생성·캐시), `AttachmentBase`(추상)·`WandAttachment`(Tip), `ITipProvider`, `ICastOrigin`, `CharacterEquipment`(장착물 인스턴스 캐시·켜고 끄기, 지팡이 끝을 낚싯줄 시작점으로 제공), `InventoryEquipmentLink`(지팡이 칸을 고를 때만 손에 듦)
- `FishingDriver`: `_castOriginSource`(ICastOrigin) 추가 — 손에 든 지팡이 끝, 없으면 기존 `WandTip`. 찌·줄 위치를 `Update` → `LateUpdate`로 옮김 (애니메이션이 손을 움직인 뒤에 그려야 줄이 지팡이 끝에서 한 프레임 밀리지 않음 — 옮기기 전 3.3cm 차이 확인)
- Editor: 메뉴 `VoxelMagic/캐릭터/기본 소켓·지팡이 데이터 만들기 (없는 것만)`, `VoxelMagic/캐릭터/농장 플레이어에 소켓·장착 붙이기`(지금 열린 씬에 부품 추가·낚시 연결·기존 Wand 끄기, 여러 번 실행해도 안전). `PrototypeSceneBuilder`도 같은 처리를 하도록 고침 (**실행하지 않음**)
- 데이터·에셋: `Data/Characters/Sockets/Socket_RightHand`(오른손), `Data/Characters/SocketProfiles/SocketProfile_Chibi`(RightHand 본, 위치 (−0.016, 0.05, −0.007), 회전 (22.7, 87.2, 174.6) — Idle 자세에서 예전 지팡이처럼 앞으로 31° 들리게 계산), `Data/Characters/Attachments/Attachment_WandTemp`(임시 지팡이, 뒤쪽 1/4을 쥠), `Prefabs/Attachments/Wand_Temp`(예전 Wand와 같은 0.05×0.05×0.58 상자·`M_Wand`, 콜라이더 없음, 끝에 `Tip`). `Student_Chibi_v2`·`Tool_Wand`에 연결
- 농장 씬: Player에 `SocketHost`·`CharacterEquipment`·`InventoryEquipmentLink`, Model에 `HumanoidSocketSource`, `FishingDriver._castOriginSource` 연결, 기존 `Wand` 비활성(삭제 안 함), `WandTip`은 대비용으로 둠. ⚠ 씬은 컴파일 새로 고침 중 에디터가 저장함 (diff는 위 변경뿐)
- 확인: 단위 테스트 101개 통과(새 9개) / 플레이: 지팡이가 `RightHand/Socket_RightHand/Wand_Temp`에 붙음, Walk 6프레임 동안 손 기준 끝 위치 그대로, 1번 칸 → 2번 칸 → 1번 칸 선택 시 숨김·다시 보임(인스턴스 1개 재사용), 지팡이 없을 때 줄 시작점 = `WandTip`, 던지기(Casting) 중 걸으며 줄 시작점 = 지팡이 끝(차이 0) / 콘솔 에러 0

## 2026-11-06 — 애니메이션 랩을 Sandbox → Tools 폴더로 옮김 (타일 맵 에디터와 같은 곳)
- 이동 (Unity 안에서 옮겨 GUID 유지): `Scenes/Sandbox/AnimationLab.unity` → `Scenes/Tools/`, `Scripts/Sandbox/` → `Scripts/Tools/`, `Data/Sandbox/` → `Data/Tools/`(AnimationLabConfig), `Materials/Sandbox/` → `Materials/Tools/`(M_Lab_*). 빈 `Scenes/Sandbox` 폴더 삭제
- 어셈블리 이름: `VoxelMagic.Sandbox` → `VoxelMagic.Tools`, `VoxelMagic.Sandbox.Editor` → `VoxelMagic.Tools.Editor` (asmdef 파일 이름·참조 함께). 네임스페이스 `VoxelMagic.Sandbox(.EditorTools)` → `VoxelMagic.Tools(.EditorTools)`. 다른 어셈블리·에셋에는 이 이름을 쓰는 곳이 없었음
- 코드 경로: 랩 씬 생성 도구의 씬·데이터·머테리얼 경로, 에셋 만들기 메뉴 `VoxelMagic/Sandbox/Animation Lab Config` → `VoxelMagic/Tools/...`
- 문서: AVATAR_GUIDE.md, SETUP.md(브랜치 표·그림자 표·어셈블리 표), DESIGN_CORRIDOR.md(랩 위치·어셈블리). 이전 기록(CHANGELOG 옛 항목, 인수인계 문서)은 그때 상태라 그대로 둠

## 2026-11-06 — 복도: 칩비 2D 아틀라스 연결 (V로 3D ↔ 2D)
- 파일 (사용자 제공 webp 1536×1024, Idle 4 · Walk 6 · Run 6, 오른쪽을 보는 그림): 압축으로 흐려진 알파를 정리(128 미만 → 투명, 이상 → 불투명)해 `Art/Characters/Student_Chibi/Sprites/Student_Chibi_sheet.png`로 저장, 칸 정보 `Student_Chibi_sprites.json`(칸 256×300, 줄마다 발바닥이 기준점 (140, 4)에 오게 잘라냄)
- 칩비 전용 폴더에 둔 이유: Student_Male 폴더에 두면 기존 `Student_Male_2D`를 덮어쓰고 v02~v05에도 연결됨
- `Data/Avatars/Student_Chibi_2D`: 목표 키 1.28(복도 칩비 0.8배와 같게) → PPU 210, Idle 3fps · Walk 8fps · Run 9fps(반복). 기존 2D 준비 도구(`SpriteCharacterImporter`)로 만들고, 같은 폴더에 모델이 없어 `Student_Chibi_v2` 아바타에 직접 연결
- 칩비 복도 프리팹 다시 만듦(메뉴 3과 같은 처리) → `Sprite2D` 자식 + 2D/3D 전환 포함, Model 배율 0.8 다시 적용, 복도 씬 다시 만듦 (밤 조명·사용자 값 그대로). 시작은 3D("2D로 시작" 꺼짐)
- 코드 변경 없음
- 확인 (한 프레임씩 진행): V → 2D(3D 렌더러 꺼짐)·V → 3D, 오른쪽 걷기 Walk 프레임 진행·반전 없음, 왼쪽 걷기 반전(flipX), 달리기 Run 프레임 진행, 발 y 0.10(바닥 띠 안), 높이 1.43(칸 포함) / 콘솔 에러 0
- 알아 둘 점: 2D 스프라이트는 조명 없는 머테리얼이라 **밤에도 밝게** 보임 (3D 칩비·복도는 어두워짐)

## 2026-11-06 — 타일 에디터: 구운 상태로 되돌리기 버튼
- 설계: `DESIGN_TILEMAP.md` 11-11 (사용자 요청·승인)
- Editor: `TileMapLibrary.RevertToBaked`/`BakedCellsOf` — 칠한 칸을 마지막 굽기 칸 데이터로 그 자리에서 바꿈 (Ctrl+Z 가능, 굽기 결과 파일은 읽기만). 도구 창 굽기 영역 "구운 상태로 되돌리기" (확인 창, 칸 데이터 없으면 꺼짐 + 이유 표시)
- 확인: 시험 맵 굽기 → 짚·바깥 칸으로 잘못 칠하기 → 되돌리기 → 구웠을 때와 모든 칸·결과 그림 같음, 맵 위치·구운 바닥 연결 그대로, 칸 데이터 파일 안 바뀜 / Ctrl+Z → 잘못 칠한 상태로 돌아감 / FarmSample은 칸 데이터가 없어 버튼 꺼짐 / 테스트 92개 통과, 콘솔 에러 0. 시험 맵·결과는 지움
- 정리: 이전 시험 맵 `_SurfaceTest`가 에디터 다시 실행(Redo)으로 씬에 되살아나 있던 것을 지움 (굽기 결과는 이미 없어 프리팹이 빠진 상태였음)
- 문서: 조작법 3장에 G 단축키 충돌(Shift+G 권장)·채우기 동작(실제 확인), 9장에 되돌리기

## 2026-11-06 — 타일 에디터 여러 맵 4·5단계: 바닥 종류 정보 + 단계별 조작법
- 설계: `DESIGN_TILEMAP.md` 11-10. 사용자 요청으로 4단계 뒤 확인 없이 조작법까지 한 번에 진행
- Core: `Tiles/GroundSurface`(바닥 종류, 표시 이름), `GroundSurfaceTable`(타일 → 종류, 빠진 줄만 더하기), `GroundSurfaceMap`(칸마다 종류 1바이트 + 종류 목록, 칸·로컬 위치로 찾기) + 테스트 4개 (전체 92개 통과). `TileMapConfig`에 바닥 종류 표 참조
- Gameplay: `GroundSurfaceSource`(구운 프리팹 루트, 월드 위치 → 종류), `GroundSurfaceQuery`(씬 전체 찾기 `TryGet`), `GroundSurfaceProbe`(확인용: 플레이 중 발밑 종류를 왼쪽 위에 표시, 에디터 씬 치비에 붙임)
- Editor: `GroundSurfaceBuilder` — 메뉴 `VoxelMagic/타일 맵/바닥 종류 표 만들기·채우기 (farm)`: `Data/Tiles/Surfaces/Surface_{Grass,Soil,Mud,Straw,Stone,Wood}`(풀·흙·진흙·짚·돌·나무판) + `Data/Tiles/GroundSurfaceTable`(51줄: 바닥 12 + Rule Tile 3 + 경계 36, Rule Tile·경계는 재료 이름으로), 설정에 연결. `TileFloorBaker`가 `<맵>_Surfaces.asset` 저장 + 프리팹 루트에 `GroundSurfaceSource`, 표에 없는 타일은 경고. 도구 창에 바닥 종류 수·표 열기/만들기. 씬 생성 도구가 치비에 확인 표시를 붙임 (지금 씬에도 추가)
- 확인: 표 다시 실행 → 새 줄 0 (고친 줄 보존) / 시험 맵(8×6 풀 + 흙·짚) 굽기 → 바닥 종류 3종, 표에 없는 칸 0, 795바이트 / 플레이 모드 찾기: 풀·흙 가운데·흙 경계 칸(흙)·짚 맞음, 맵 밖·옛 굽기(FarmSample) 위는 없음 / 10만 번 찾기 14 ms / 치비를 흙 위로 옮기면 "발밑 바닥: 흙" 표시(한글 정상) / 콘솔 에러 0. 시험 맵·결과·임시 캡처는 지움. `FarmSample`은 사용자 요청으로 다시 굽지 않음 (칸 데이터·바닥 종류 없음)
- 문서: `TILEMAP_EDITOR_GUIDE.md`를 단계별 조작법으로 다시 씀 (처음 준비 · 씬 열기 · 새 맵 · 칠하기 · 여러 맵 · 칸 크기 · 굽기 · 바닥 종류 · 게임 씬 적용 · 다시 불러오기 · 확인 도구 · 문제 해결 · 메뉴)

## 2026-11-06 — 타일 에디터 여러 맵 3단계: 칸 데이터 저장 · 구운 맵 다시 불러오기
- 설계: `DESIGN_TILEMAP.md` 11-5·11-6
- Core: `Tiles/TileMapCells` (칸 위치 → 칠한 타일, 같은 타일은 한 번만 두고 번호로) + 테스트 4개 (전체 88개 통과)
- Editor: `TileFloorBaker`가 `<맵>_Cells.asset`도 저장 (Rule Tile 칸은 Rule Tile 자체로, 덮어쓰기 확인 문구에 칸 데이터 추가). `TileMapLibrary` — `LoadBakedMap`(칸 데이터대로 새 맵을 칠하고 구운 프리팹 연결, 칠하기 보기로 시작, 같은 이름이 씬에 있으면 그 맵을 돌려줌, 음수 칸이 있어도 다른 맵과 안 겹치게 배치, Ctrl+Z 한 번), `BakedMaps`, `FindCells`(프리팹·칸 데이터·폴더 어느 것을 골라도), `FindMap`. 도구 창 "구운 맵 불러오기" — 구운 맵 목록(칸 수 / 씬에 있음 → [선택] / 칸 데이터 없음), Project에서 고른 파일로 불러오기, 다른 맵 숨기기
- 확인: 시험 맵(8×6 풀 + 흙·짚 Rule Tile + 음수 칸 + 경계 낱장, 50칸) 굽기 → 칸 데이터 50칸·4종 → 씬에서 지우고 불러오기 → **50칸 모두 타일·결과 그림까지 같음**, 구운 프리팹 연결, 칠하기 보기 / 같은 맵 다시 불러오기 → 새로 안 만듦 / 고친 뒤 다시 굽기 → 칸 데이터 갱신(흙 9 → 12칸), 구운 바닥 인스턴스 그대로 / 플레이 모드 정상, 콘솔 에러 0. 시험 맵과 결과는 지움
- 남은 것: `FarmSample`은 칸 데이터가 없어 아직 불러오기 목록에서 "칸 데이터 없음" → 한 번 다시 구우면 생김 (기존 결과 덮어쓰기라 사용자 확인 후)

## 2026-11-06 — 타일 에디터 여러 맵 2단계: 맵 목록 · 새 맵
- 설계: `DESIGN_TILEMAP.md` 11-2~11-4
- Core: `Tiles/TileMapPlacement` (새 맵 자리 = 기존 맵들 동쪽 끝 + 간격) + 테스트 4개 (전체 84개 통과). `TileMapConfig`에 새 맵 기본 크기 40×40·간격 4칸 추가
- Editor: `TileMapLibrary` — `NewMap`(이름 검사, 겹치지 않게 배치, 바탕 타일 채우기, 다른 맵 숨기기, Ctrl+Z 한 번으로 취소), `SetVisible`, `AllMaps`, `SuggestName`. `TileMapEditorWindow` — 맵 목록(체크 = 보이기, ▶ 작업 대상, 칸 크기·구움 여부, [선택] = 보이기 + 선택 + 씬 뷰 맞춤), "작업 대상 말고 다른 맵 모두 숨기기", "새 맵"(이름·크기·바탕 타일·다른 맵 숨기기 → 만든 뒤 선택하고 칠하기 시작), 창 스크롤
- 확인: 새 맵 40×40(풀) → (14, 0, 0)에 1,600칸, FarmSample 숨겨짐 / 두 번째 새 맵(8×6, 빈 맵) → (58, 0, 0), 숨긴 맵도 자리 계산에 들어감 / 같은 이름 거부 / Ctrl+Z 두 번 → 두 맵 모두 사라지고 FarmSample 다시 보임 / 창 그리기 에러 없음, 맵 선택 → 비교 오브젝트가 그 맵 가운데로 / 플레이 모드 정상, 콘솔 에러 0. 시험 맵은 지움
- 참고: 컴파일 직후 콘솔에 Unity 내부 경고 `Assertion failed ... kDontSaveInEditor` 1번 (스택 없음). 이후 작업에서는 다시 안 나옴 — Tile Palette 창이 열린 채 다시 컴파일할 때 Unity가 내는 것으로 추정

## 2026-11-06 — 타일 에디터 여러 맵 1단계: 맵 하나 = 루트 하나
- 설계: `DESIGN_TILEMAP.md` 11-1·11-7 ("제작 시작" 승인)
- 구조: `Maps/Map_<이름>`(TileMapDocument + TileGridScaler) > `Grid`(X 90°, 배율 = 칸 크기) > `Paint`, 구운 바닥은 맵 루트 아래. 비교 오브젝트는 `Compare`(TileMapCompareRig) > `Center`(카메라·치비·상자)·`East`(연못)
- Gameplay: `TileMapDocument`(Grid·Paint 참조, 칠한 범위·칸 → 월드 위치), `TileGridScaler`(맵 루트로 옮김, 자기 Grid·구운 바닥 배율만, 위치는 안 건드림), 새 `TileMapCompareRig`(작업 대상 맵 가운데·동쪽 끝으로)
- Editor: `TileFloorBaker`(맵 하나만 굽고 그 루트 아래에 놓음), `TileMapEditorWindow`(선택한 오브젝트가 속한 맵 = 작업 대상, 비교 오브젝트·Tile Palette 대상도 따라감), 새 `TileMapLibrary`(맵 만들기·이름 검사/바꾸기·옛 구조 옮기기 메뉴 `VoxelMagic/타일 맵/옛 구조 씬을 여러 맵 구조로 옮기기`), 씬 생성 도구도 새 구조로
- 지금 씬: 다시 만들지 않고 구조만 옮김 (칠한 100칸·구운 바닥 그대로, Ctrl+Z 한 번으로 되돌릴 수 있게 묶음)
- 확인: 맵 루트를 (20, 0, 3)으로 옮기면 칸·구운 바닥·비교 오브젝트가 같이 옮겨짐 / 칸 크기 2 → Grid·구운 바닥 배율 2 / 루트를 끄면 칠한 칸·구운 바닥 함께 숨겨짐 / 시험 맵(6×6)을 만들어 그것만 구움 → FarmSample 결과 파일 안 바뀜, 구운 바닥은 시험 맵 루트 아래 / 선택 기준 대상: 시험 맵 Paint 선택 → 시험 맵, FarmSample 조각 선택 → FarmSample / 같은 이름 거부 / 플레이 모드: 두 맵 모두 칠하기 층 꺼지고 구운 바닥 보임, 콘솔 에러 0 / 테스트 80개 통과. 시험 맵과 그 굽기 결과는 지움

## 2026-11-05 — 타일 에디터 4단계: 굽기 방식 확정, 칠하기 편의, 사용법
- 사용자 결정: 본 방식 = **굽기** (플레이 전에 한 번 구워 파일로 저장, 게임은 파일만 씀). 땅은 런타임에 바꾸지 않고, 물 주기 등 변화는 텃밭 칸 오브젝트(나중에 MagicaVoxel FBX)에서만 → `DESIGN_TILEMAP.md` 10장
- 도구 창: "칠하기 시작"(Tile Palette 열기 + 팔레트 Farm + 대상 Paint 자동 선택 + 칠하기 보기), 에디터 씬이 아닐 때 "타일 맵 에디터 씬 열기" 버튼. 메뉴 `VoxelMagic/타일 맵/타일 맵 에디터 씬 열기` 추가
- 문서: `Docs/TILEMAP_EDITOR_GUIDE.md` (사용법)
- 확인: "칠하기 시작" → palette Farm · target Paint 선택됨, 기본 붓으로 칠하기 됨(시험 칸은 되돌림) / 구운 프리팹을 빈 씬에 놓으면 Transform·MeshFilter·MeshRenderer만으로 그대로 보임 (범위 (0,0,0)~(10,0,10)) / 콘솔 에러 0

## 2026-11-05 — 타일 에디터 3단계: 굽기 + 바닥 프리팹 + 도구 창, 확인 1~4
- 설계: `DESIGN_TILEMAP.md` 7·8장
- Core: `Tiles/TileAtlasLayout` (아틀라스 배치·UV, 여백 제외) + 테스트 4개 (`TileAtlasLayoutTests`, 전체 80개 통과)
- Gameplay: `TileMapDocument` (맵 이름·구운 칸 크기·구운 바닥, 플레이 중 구운 바닥 켬), `PaintOnlyTilemap` (플레이 중 칠하기 층 끔), `TileGridScaler`가 구운 바닥 배율도 맞춤
- Editor: `TileFloorBaker` (칠한 칸 → 아틀라스 PNG·URP Lit 머티리얼·16칸 조각 메시·`<맵>_Floor.prefab`, 이전 결과가 있으면 덮어쓰기 전에 확인 창, 이번에 없는 조각 메시는 지움), `TileMapEditorWindow` (메뉴 `VoxelMagic/타일 맵/타일 맵 에디터 창`: 맵 이름, 칸 크기·후보 버튼, 굽기, 보기 전환, 해 높이·방향). 씬 생성 도구가 두 컴포넌트도 붙이게 함 (지금 씬에는 다시 만들지 않고 컴포넌트만 추가)
- 결과: `Art/Map/Baked/FarmSample/` — 칸 100, 타일 24종, 조각 1, 아틀라스 180×180 (여백 2px), 프리팹 루트 배율 1 (= 칸 1 m)
- 확인 1 (방향): 칸 (0,0) = 월드 (0.5, 0, 0.5), 위쪽 = +Z
- 확인 2 (Tilemap + URP Lit): **예상과 달리 그림이 나오고 그림자도 받음**. 평균 색 차이는 작음 (구운 바닥 (115,129,72) / Lit Tilemap (117,131,78) / Unlit Tilemap (119,133,70)), 화소 28%가 조금씩 다름. 확인 후 원래 머티리얼(Sprite-Unlit-Default)로 되돌림
- 확인 3 (그림자 줄무늬): 해 높이 50°·20°·10°에서 줄무늬 안 보임 (그림자만 길어짐)
- 확인 4 (이음새): 여백 0px과 2px로 각각 구워 먼 시점·낮은 시점 비교 → 차이 화소 0~1개, 이음새 안 보임 (Point·밉 없음이라). 여백 2px는 안전 여유로 유지. 비교용 임시 결과 `_SeamTest_Pad0`은 지움
- 플레이 모드: '칠하기' 보기로 두고 들어가도 칠하기 층 꺼지고 구운 바닥만 보임, 조각 렌더러 1개, 콘솔 에러 0. 칸 크기 2.133으로 바꾸면 구운 바닥도 바로 2.133배 (되돌림)

## 2026-11-05 — 타일 에디터 2단계: 타일 맵 에디터 씬 + 10×10 샘플
- 사용자 정리: 테스트 씬이 아니라 **타일 맵 에디터 씬** (칠하고 구워서 다른 게임 플레이 씬에 땅으로 씀) → `DESIGN_TILEMAP.md` 0·2·3·7장 반영
- Gameplay: `Tiles/TileGridScaler` ([ExecuteAlways]) — `TileMapConfig.CellSize`를 FloorGrid 배율로 바로 반영, 샘플 가운데·동쪽 끝 기준점도 옮김
- Editor: `Tiles/TileMapEditorSceneBuilder` — 메뉴 `VoxelMagic/타일 맵/타일 맵 에디터 씬 만들기` (씬이 있으면 덮어쓰기 전에 확인 창)
- 씬: `Scenes/Tools/TileMapEditor.unity` — 해(농장과 같은 값), FloorGrid(X 90°) + Paint Tilemap 한 층, 10×10 샘플(흙 4×4·길 2칸 폭·짚 ㄱ자·1칸 폭 줄·대각선 칸), 치비(Student_Chibi_v2, 배율 1)·1 m 상자·연못(2, 2, 1.5)
- 고침: 씬 생성 도구가 새 씬을 열기 전에 설정을 읽어, 씬 전환 때 에셋이 내려가며 설정 참조가 비던 문제 → 새 씬을 연 뒤 읽음
- 확인: 칸 (0,0) 가운데 = 월드 (0.5, 0, 0.5), (9,9) = (9.5, 0, 9.5), 팔레트 위쪽 = +Z(북) / 칸 크기 1 → 2.133으로 바꾸면 Grid 배율·기준점이 바로 따라옴(씬 다시 만들기 불필요), 되돌림 / 플레이 모드: 칠한 칸 유지, 치비 Idle, 콘솔 에러 0 / Tilemap은 조명·그림자를 받지 않음(예상대로 → 굽기 단계에서 해결)

## 2026-11-05 — 타일 에디터 1단계: 타일 팩 임포트 설정 + farm 타일·Rule Tile·팔레트
- 브랜치: `feature/tool-tile-editor` · 설계: `DESIGN_TILEMAP.md` 5·6장 ("제작 시작" 승인)
- Core: `TileMapConfig` (칸 크기 1 m, 후보 2.133/1.067/1, 타일 32 px, 샘플 10, 조각 16, 여백 2 px, 바뀌면 `Changed`) → `Data/Tiles/TileMapConfig.asset`
- Editor: `Tiles/FarmTileSetBuilder` — 메뉴 `VoxelMagic/타일 맵/타일 팩 임포트 설정 적용 (전체)`, `VoxelMagic/타일 맵/farm 타일·Rule Tile·팔레트 만들기` (이미 있으면 덮어쓰기 전에 확인 창). `VoxelMagic.Editor.asmdef`에 `Unity.2D.Tilemap.Extras`·`Unity.2D.Tilemap.Editor` 참조 추가
- 에셋: 타일 팩 387장 임포트 설정 변경(.meta 수정, GUID 유지), `Art/Map/TileAssets/farm/` (Tile 48 = 바닥 12 + 경계 36, Rule Tile 3), `Art/Map/TilePalettes/Farm.prefab` (54칸)
- 확인: 387장 모두 설정 일치. 미리보기 장면(저장 안 됨)에서 Rule Tile이 바깥·안쪽 모서리·변을 맞게 고름, 경계는 칸 가운데라 칠한 영역이 둘레로 반 칸씩 줄어 보임, 1칸 폭 줄은 맞는 그림이 없어 경계 없이 끊김(4-3 예상대로) / 콘솔 에러 0. 씬이 아직 없어 플레이 모드 확인은 다음 단계에서

## 2026-11-05 — 타일 에디터: 타일맵 패키지 2개 설치
- 브랜치: `feature/tool-tile-editor`
- 설계: `DESIGN_TILEMAP.md` (설계안, "제작 시작" 승인 전)
- 패키지: 2D Tilemap Editor 1.0.0, 2D Tilemap Extras 4.1.2 설치 (`Packages/manifest.json`, `packages-lock.json`), 사용자 승인
- 확인: 메뉴 `Window > 2D > Tile Palette` 열림, `RuleTile` 형식 있음, 콘솔 에러 0

## 2026-11-04 — 복도 밤 조명 나누기: 바닥 빛 그림 + 천장 조명(칩비)
- 설계: DESIGN_CORRIDOR.md 17번 (사용자 결정: 미리보기는 빼고 조명만 분리)
- 이유: 사용자가 점 조명을 바닥으로 내려 쓰니(간격 5.7, 높이 −0.5, 세기 1.3) 바닥 빛 모양은 맞지만 칩비를 아래에서 올려 비추고, 쿠키 진하기를 따로 조절할 수 없었음
- `Lamp_i` 아래 `FloorGlow`(조명이 아닌 도트 원 스프라이트, 더하기 합성) + `CeilingLight`(천장 스포트, 아래로 보다가 벽 쪽으로 25° 기울여 칩비 앞면까지)
- `CorridorConfig`: 점 조명 항목 → 바닥 빛(가운데 높이 0.2·벽 앞 0.05·가로 2.4·세로 0.4·색·진하기 0.35·도트 32픽셀·4단계) + 천장 조명(높이 2.7·z −0.8·기울기 25·각도 60·거리 4·세기 4·색) + 벽이 천장 조명 받는 배율(기본 0). 간격·끝 여유는 그대로(사용자 값 5.7 → 7개), 색·쿠키 크기·단계는 이름만 바뀌어 값 이어짐
- Core: `SideScrollMath.DotCircleLevel`(계단식 도트 원 밝기, 테스트 1개 → 전체 76개 통과)
- 셰이더: 새 `Shaders/SpriteAdditive.shader`(조명 없는 더하기 합성, 머테리얼 `Art/Materials/CorridorFloorGlow.mat`), `SpriteLit3D`에 `_CorridorLampOnWall` 배율
- 고침: 두 셰이더가 Unity 6 스프라이트 색·좌우 반전(`unity_SpriteColor`·`UnityFlipSprite`)을 반영하지 않아 바닥 빛이 흰색으로 진하게 나옴 → URP `Core2D.hlsl` 사용
- `CorridorLighting`: 밤에 바닥 빛·천장 조명 켜고, 벽 배율 전역 값 설정. 쿠키 그림 `lamp_dot_cookie.png` 삭제 → `Art/Lights/lamp_floor_glow.png`
- 사용자가 플레이하며 정한 최종 값 (`CorridorConfig`): 끝 여유 2.5, 바닥 빛 높이 0.1·세로 0.2·진하기 0.15·도트 80픽셀·6단계, 천장 조명 세기 6 (나머지는 위 기본값)
- 확인 (한 프레임씩 진행): 밤 — 칩비 발밑에 따뜻한 계단식 도트 타원, 칩비가 천장 조명으로 밝아짐, 벽에는 추가 빛 없음, 창밖 밤 배경 그대로 / 낮 — 바닥 빛·천장 조명 꺼짐, 오른쪽 문 정상 / 콘솔 에러 0

## 2026-11-04 — 복도: 낮/밤 전환(N) + 밤 배경 + 도트 점 조명 (3D 조명)
- 설계: DESIGN_CORRIDOR.md 16번. 처음 설계(2D Renderer + Sprite Light)는 사용자 결정으로 **3D 조명**으로 바꿈 → URP·프로젝트 설정 변경 없음
- 셰이더: `Shaders/SpriteLit3D.shader` (`VoxelMagic/Sprite Lit 3D`) — 스프라이트 그림 × (전역 `_CorridorAmbient` + 3D 추가 조명, Forward+·쿠키 지원). 해는 쓰지 않아 낮 화면은 이전과 같음. 머테리얼 `Art/Materials/CorridorSpriteLit.mat`을 복도 타일에 사용, 창밖 배경은 계속 조명 없는 스프라이트
- Core: `TimeOfDayState`(낮/밤 + 이벤트), `SideScrollContext.TimeOfDay`, `SideScrollMath.LampPositions`(간격 이하로 고르게, 양 끝 여유), `CorridorConfig` 낮/밤(밤으로 시작 끔, 낮/밤 배경 세트, 복도 밝기 색 낮 흰색·밤 (0.12, 0.14, 0.26), 밤 해 ×0.15·주변광 ×0.3)·점 조명(간격 4, 끝 여유 1.5, 높이 1.9, z −2, 각도 34, 거리 6, 세기 5, 따뜻한 색, 쿠키 32픽셀·4단계), 문구 "밤 (N)"·조작법에 N
- 데이터: `BackdropSet_Night`(bg_night_loop, 하늘 대체 색 남색). 이미지 임포트는 기존 배경 설정 그대로
- 입력: `SideScroll` 맵에 `ToggleNight`(N) 추가
- Gameplay: `CorridorLighting` — 배경 세트 교체, 전역 복도 밝기, 해 세기·주변광(3D 캐릭터), 점 조명 켜고 끄기. 플레이를 멈추면 낮 값으로 되돌림
- UI: `NightTogglePanel` — "카메라 고정 (C)" 아래 "밤 (N)" 토글
- Editor: 복도 씬 생성 도구가 타일 조명 머테리얼, 점 조명(스포트, 그림자 없음) 10개, 도트 원 쿠키 `Art/Lights/lamp_dot_cookie.png`(Cookie·Point·압축 없음)·밤 배경 세트, HUD 토글을 만듦. 조작법 글상자 높이 230
- Tests: `TimeOfDayTests` 4개 (전체 75개 통과)
- 확인 (한 프레임씩 진행): 낮 화면이 이전과 같음 → 밤: 창밖 밤 배경, 복도 어두워짐, 벽에 빛 원 10개(첫 값 각도 60은 원이 겹쳐 34로 줄이고 세기 3 → 5), 해 0.15, 토글 표시 일치, 도트 렌더링(P)과 함께 빛 원 가장자리도 도트 / 콘솔 에러 0. 밤으로 처음 바꿀 때 에디터가 셰이더 변형을 컴파일하는 몇 프레임 동안 화면이 하늘색으로 보임 (에디터 전용, 빌드에는 없음)

## 2026-11-04 — 농장 맵: 플레이어 아바타를 칩비(Student_Chibi_v2)로 교체
- 브랜치: `feature/prototype-farm-fishing`에 `feature/prototype-corridor` 병합 (빨리 감기, 충돌 없음)
- `PlayerConfig` 아바타 `Student_Male_v05` → `Student_Chibi_v2` (복도와 같은 Controller, Idle/Walk만 사용), 메뉴 `VoxelMagic/프로토타입 씬 다시 만들기`로 씬 다시 생성
- 확인: 플레이어 Model = Student_Chibi_v2.fbx, Humanoid, 키 1.63(렌더러 범위), 시작 Idle, 움직임 감지 시 Walk 재생 속도 파라미터 갱신 / 콘솔 에러 0
- 문서: DESIGN.md 12번 (칩비 교체 + 지금 기능에 필요한 애니메이션 목록, 연결에 필요한 작업)

## 2026-11-04 — 복도: 오른쪽 문 포털 (E → 농장·낚시 맵)
- 설계: DESIGN_CORRIDOR.md 15번 승인 후 제작
- 설정 변경 (사용자 승인): Build Settings 씬 목록에 `Corridor_SideScroll`, `Prototype_FarmFishing` 추가 → SETUP.md
- Core: `InteractionFocusState`(지금 대상의 안내 문구 + 변경 이벤트), `SideScrollContext.Interaction`, `CorridorConfig` 포털 설정(켜기·씬 이름·오른쪽 끝에서 거리 2.56·키 1.0·지름 0.6·트리거 반지름 0.2·색·검사 높이 0.5), 문구 "농장으로 가기"·조작법에 E
- 입력: `SideScroll` 맵에 `Interact`(E) 추가 (프로토타입 1 `Gameplay` 맵은 그대로)
- Gameplay: `ScenePortal`(`IInteractable`, 트리거 CapsuleCollider, 목록에 없는 씬이면 에러 로그), `SideScrollInteractor`(발 + 0.5 높이 점의 트리거 겹침 검사 → 대상·안내, E로 실행), `SideScrollInputReader.InteractPressed`
- UI: `SideScrollPromptView` — 화면 아래 가운데 "E  농장으로 가기" (대상이 있을 때만)
- Editor: 복도 씬 생성 도구가 포털(루트: 포털 + 트리거, 자식 Visual: 반투명 원기둥)·머테리얼 `Art/Materials/CorridorPortal.mat`(URP Unlit 반투명)·안내 UI 생성, 조작법 글상자 높이 200. `SetString` 확장 메서드 추가
- Tests: `InteractionFocusTests` 1개 (전체 71개 통과)
- 확인 (한 프레임씩 진행): x 32.987(범위 밖) 안내 없음 → x 33.28(포털 중심) "E  농장으로 가기" 표시 → E(상호작용 실행) → `Prototype_FarmFishing` 열림(플레이어·UI 정상) / 콘솔 에러 0

## 2026-11-04 — 복도: 카메라 따라가기 토글 (고정 ↔ 부드럽게, C)
- 설계: DESIGN_CORRIDOR.md 14번 승인 후 제작
- Core: `CameraFollowState`(고정/부드럽게 + 변경 이벤트), `SideScrollContext.CameraFollow`, `CorridorConfig` "카메라 고정으로 시작"(기본 켬), 조작법에 C 추가
- 입력: `SideScroll` 맵에 `ToggleCameraFollow`(C) 추가
- Gameplay: `SideScrollCamera` — 고정이면 매 프레임 플레이어 x로 바로(복도 끝 제한은 그대로), 부드럽게면 기존 SmoothDamp
- UI: `CameraFollowPanel` — "2D 캐릭터 (V)" 아래 "카메라 고정 (C)" 토글. 씬 생성 도구는 토글 패널을 같은 함수로 쌓도록 정리, 조작법 글상자 높이 140 → 170
- 칩비 크기: 사용자가 씬에서 Model 배율 0.8로 줄인 것을 프리팹(`SideScrollCharacter_Student_Chibi_v2`)으로 옮김 (씬을 다시 만들어도 유지. 메뉴 3을 다시 실행하면 1로 돌아감)
- Tests: `CameraFollowTests` 1개 (전체 70개 통과)
- 확인 (한 프레임씩 진행): 부드럽게 → 캐릭터 7.08 / 카메라 6.03(늦게 따라감), 고정 → 캐릭터 8.563 = 카메라 8.563, 오른쪽 끝 → 카메라 32.481(허용 최대값)에서 멈춤, 토글 표시 일치 / 콘솔 에러 0

## 2026-11-04 — 복도: 플레이어 3D 모델을 칩비(Student_Chibi_v2)로 교체, 2D 끔
- 메뉴 1·2는 건너뜀 (v2는 랩에서 이미 Humanoid로 준비됨, 직접 고친 Hips 매핑 유지), 메뉴 3 기능(`BuildCharacter` + `UseInCorridor`) 실행
- 생성: `Animations/Avatars/Student_Chibi_v2.controller`, `Prefabs/SideScroll/SideScrollCharacter_Student_Chibi_v2.prefab` (2D 버전 없음 → `Sprite2D` 없이 3D만)
- `CorridorConfig`: 캐릭터 프리팹 v05 → Student_Chibi_v2, 2D로 시작 켬 → 끔. 복도 씬 다시 생성
- 확인 (상태 머신에 입력 값 직접): 키 약 1.63(렌더러 범위), Idle → Walk(오른쪽 1.52 m/s) → Run(왼쪽) → 왼쪽 끝 2.56에서 멈춤·Idle, 2D 상태 꺼짐 / 콘솔 에러 0
- 되돌리기: `CorridorConfig` 캐릭터 프리팹을 `SideScrollCharacter_Student_Male_v05`로, 2D로 시작 켜고 복도 씬 다시 만들기

## 2026-11-03 — 복도: 2D 스프라이트 캐릭터 + 2D/3D 전환 (V)
- 설계: DESIGN_CORRIDOR.md 13번 승인 후 제작
- Core: `SpriteCharacterSet`(상태별 프레임·fps·반복·목표 키) + `SpriteClipMath`(시간 → 프레임), `AvatarSetup._sprites`(2D 버전 연결), `CharacterViewState`(2D/3D + 변경 이벤트), `SideScrollContext.CharacterView`, `CorridorConfig`에 2D로 시작(기본 켬)·스프라이트 정렬 순서(10), 조작법에 V 추가
- 입력: `SideScroll` 맵에 `ToggleCharacterView`(V) 추가
- Gameplay: `SpriteFrameAnimator`(상태 이벤트 → 프레임 재생, 회전 정면 고정, 왼쪽이면 좌우 반전), `CharacterViewSwitcher`(2D면 3D 렌더러 끄고 스프라이트 켬, Animator는 계속)
- UI: `CharacterViewPanel` — 복도 HUD 도트 렌더링 패널 아래 "2D 캐릭터 (V)" 토글
- Editor: `SpriteCharacterImporter`(메뉴 `VoxelMagic/아바타/선택한 2D 스프라이트 준비`), 복도 캐릭터 만들기(메뉴 3)가 2D 버전이 있으면 `Sprite2D` 자식과 전환 컴포넌트를 함께 넣음, 복도 HUD에 토글 패널
- 생성: `Data/Avatars/Student_Male_2D`(Idle 8프레임 6fps · Walk 8프레임 10fps · Run 8프레임 12fps, PPU 51.25 = Idle 82px → 키 1.6), Student_Male v02~v05 아바타에 연결, v05 복도 프리팹·복도 씬 다시 생성. 스프라이트 PNG 임포트: Sprite·Point·압축 없음·Mip Map 끔
- Tests: `CharacterViewTests` 9개 (전체 69개 통과)
- 확인 (상태 머신에 입력 값 직접): 2D로 시작(3D 렌더러 꺼짐), 걷기 → Walk 프레임, 왼쪽 달리기 → Run 프레임·좌우 반전, 루트가 90° 돌아도 스프라이트 회전 (0,0,0), 발 y 0.08(바닥 띠 안), 정렬 순서 10 / V → 3D(애니메이션 Run 이어짐) → V → 2D / 콘솔 에러 0

## 2026-11-03 — 폴더 구조 정리: 캐릭터별로 묶기
- 사용자 요청: 3D 모델(`Art/Characters/`)과 2D 스프라이트(`Art/Sprites/Characters/`)가 따로 흩어져 복잡했음
- 새 구조: `Art/Characters/<캐릭터>/Model/`(3D 모델·텍스처), `Art/Characters/<캐릭터>/Sprites/`(2D 아틀라스·json). `Animations/`는 모든 캐릭터가 같이 쓰는 클립이라 그대로 따로 둠
- 이동 (Unity 안에서 옮겨 GUID 유지): Student_Male v02~v05 `.fbx`·`_tex.png` → `Model/`, 사용자가 넣은 `Student_Male_Idle/Walk/Run.png`·`Student_Male_sprites.json` → `Sprites/`. 빈 `Art/Sprites/` 갈래 삭제
- `AvatarPipeline` 아바타 이름 규칙: 모델 폴더 이름 → **캐릭터 폴더 이름**(Art/Characters 바로 아래), 모델이 여러 개면 파일 이름 (기존과 같음)
- 확인: 아바타 데이터 4개 모델 경로 새 위치, 복도 프리팹(v02·v05) 메시·아바타 연결 정상, PlayerConfig 아바타 정상, 새 모델 자동 준비 0개 / 콘솔 에러 0
- 문서: AVATAR_GUIDE.md, DESIGN_CORRIDOR.md, SETUP.md 경로 갱신

## 2026-11-03 — 복셀 아바타 그림자 구멍 수정 (씬 조명 Normal Bias 0)
- 원인: 복셀 메시는 면마다 정점이 따로라, URP 그림자 Normal Bias가 면을 법선 방향 안쪽으로 밀 때 면 사이가 벌어져 그림자에 구멍이 생김
- 비교 (같은 위치·같은 카메라 7곳: 아바타·게시판·학교 벽·은행나무·연못 돌·텃밭·크리처, 현재 0.5 vs Normal 0): 바뀐 픽셀 0.03~0.46%, **모두 어두워지는 쪽**(밝아진 곳 0) — 아바타 그림자 구멍이 메워지고 다른 물체 그림자 가장자리가 한 줄 정도 넓어짐. 빛 받는 바닥·벽·돌 표면에 새 얼룩(Shadow Acne) 없음 → 롤백하지 않음
- 적용: `Editor/VoxelShadowSettings`(씬 조명만 파이프라인 설정 대신 Depth Bias 0.1·Normal Bias 0), 프로토타입 씬·애니메이션 랩 씬 생성 도구에서 사용, 두 씬 다시 생성. 프로젝트 URP 설정은 그대로
- Editor asmdef에 `Unity.RenderPipelines.Universal.Runtime` 참조 추가
- 확인: 근접 캡처에서 아바타 그림자 꽉 참 / 콘솔 에러 0
- 참고: 노말 맵은 화면에 그릴 때의 음영에만 쓰이고 그림자 모양에는 쓰이지 않아 이 문제와 관계없음. 근본 해결은 모델에서 정점 합치기(Weld)지만, 복셀의 각진 음영이 바뀔 수 있어 지금은 하지 않음

## 2026-11-03 — 프로토타입 1: 마우스 스크롤 카메라 줌, 기본 거리 가깝게
- `GameInputReader`: 스크롤은 낚시 미니게임 중이면 잡기 바, 창이 열려 있지 않은 평소에는 `ZoomRequested` 이벤트(카메라 줌)
- `QuarterViewCamera`: 스크롤 위 = 가까이, 아래 = 멀리. 거리를 줌 범위로 제한하고 따라가기와 같은 부드러움으로 다가감
- `PlayerConfig`: 카메라 기본 거리 14 → **8**, 줌 범위 4~16, 스크롤 한 칸 1 (에셋 값도 8로 변경)
- 씬 다시 생성 (카메라에 입력 연결)
- 확인: 시작 거리 8, 위로 10칸 → 4에서 멈춤, 아래로 20칸 → 16에서 멈춤, 부드럽게 4까지 다가감 / 콘솔 에러 0
- 그림자 구멍 원인 확인: 아바타 메시(정점 20,122 · 삼각형 11,902)는 복셀 면마다 정점이 따로 있는 구조라, URP 그림자 Normal Bias(조명 0.4)가 면을 법선 방향으로 안쪽으로 밀 때 면 사이가 벌어져 구멍이 생긴다. 플레이 중 조명만 Normal Bias 0으로 바꾸자 구멍이 사라짐 (저장하지 않은 실험)

## 2026-11-03 — 프로토타입 1: 플레이어를 아바타(Student_Male_v05)로 임시 교체
- 설계: DESIGN.md 11번, 사용자가 이동 애니메이션 Walk 선택
- Core: `PlayerConfig`에 아바타(AvatarSetup, 비우면 기본 도형)·Walk 기준 속도(1 m/s)·Walk 시작 속도(0.1)·섞기 시간(0.15초) 추가, 아바타 = `Student_Male_v05`
- Gameplay: `PlayerAvatarAnimator`(움직이면 Walk, 멈추면 Idle — 농사·낚시·대화는 Idle. Walk 재생 속도 = 실제 이동 속도 ÷ 기준 속도)
- Editor: `PrototypeSceneBuilder.BuildPlayer`가 아바타가 있으면 몸통·머리·넥타이 도형 대신 아바타 모델(`Model`, Animator = v05 Controller, 루트 이동 끔)을 넣음. 지팡이·지팡이 끝·입질 표시·CharacterController는 그대로. 프로토타입 씬 다시 생성
- 확인: 시작 Idle → 4 m/s 이동 시 Walk(재생 속도 약 3.3~4배) → 멈추면 Idle, 낚시 상태 Idle, Model 위치 (0,0,0) 유지 / 콘솔 에러 0 / 단위 테스트 60개 통과
- 참고: 아바타 키 1.6이라 기존 도형(머리까지 약 2.1)보다 작아 보임, 지팡이는 몸 앞 고정 위치. 되돌리려면 `PlayerConfig` 아바타를 비우고 `VoxelMagic/프로토타입 씬 다시 만들기`

## 2026-11-03 — 복도 맵 도트 렌더링 On/Off (화면 전체)
- 설계: DESIGN_CORRIDOR.md 12-10, 사용자가 B(화면 전체) + 토글·슬라이더 선택
- 공용화: 랩의 `LabPixelView` → `UI/Rendering/PixelRenderView` (같은 .meta로 옮겨 기존 씬 참조 유지), 랩도 같은 컴포넌트 사용. `VoxelMagic.Sandbox`·`Sandbox.Editor` asmdef에 `VoxelMagic.UI` 참조 추가
- Core: `PixelRenderState`(켜짐·도트 크기·범위 제한·변경 이벤트), `SideScrollContext.PixelRender`, `CorridorConfig`에 시작 시 켜기·도트 크기(4)·범위(2~8), 조작법 문구에 P 추가
- 입력: `GameControls`의 `SideScroll` 맵에 `TogglePixel`(P) 추가 — 기존 `Gameplay` 맵은 그대로. `SideScrollInputReader`가 P로 상태 토글
- UI: `PixelRenderPanel`(토글·슬라이더·표시 ↔ 상태 ↔ 뷰), 복도 HUD에 패널·EventSystem·GraphicRaycaster·전체 화면 출력 RawImage 추가
- Tests: `PixelRenderStateTests` 4개 (전체 60개 통과)
- 확인: P(상태 토글) → 켜짐·UI 토글 체크·480×222 (4배), 슬라이더 6 → 320×148, 글자 표시 갱신 / 화면 도트 렌더링 캡처 확인 / P 바인딩 `<Keyboard>/p`, 기존 Gameplay 맵 액션 10개 그대로 / 콘솔 에러 0 / TMP 폴백 폰트 변경 없음
- 참고: 도트 모드에서 창틀·난간 일부에 분홍 선이 보임 — 원본 타일 이미지의 유리 경계 색이 낮은 해상도에서 드러나는 것으로 보임 (이미지 확인 필요)

## 2026-11-03 — 복도 캐릭터를 Student_Male_v05로 교체
- 애니메이션 랩에서 v05 + Idle/Walking/Standard Run 확인 후 (사용자), 메뉴 3과 같은 처리로 복도에 적용
- 생성: `Animations/Avatars/Student_Male_v05.controller`(Idle/Walk/Run + 속도 파라미터), `Prefabs/SideScroll/SideScrollCharacter_Student_Male_v05.prefab`
- `CorridorConfig` 캐릭터 프리팹 → `SideScrollCharacter_Student_Male_v05`, 복도 씬 다시 생성 (v02 프리팹·데이터는 그대로 남김)
- 확인 (상태 머신에 입력 값 직접): Walk·Run 애니메이션 상태 일치, A 한 프레임에 Y 270°, 양 끝 2.56·33.28에서 Idle, Y 0.12·Z 0 고정, Model 위치 (0,0,0) 유지 / 콘솔 에러 0
- 재생 속도는 아직 기본값 1 (`Data/Avatars/Student_Male_v05`에서 플레이 중 조절)

## 2026-11-03 — 애니메이션 랩: 도트 렌더링 On/Off
- 설계: DESIGN_CORRIDOR.md 12-9 (A. 저해상도 렌더 텍스처, 프로젝트 설정·URP 렌더러 변경 없음) 승인 후 제작
- Sandbox: `LabPixelView`(카메라를 화면 ÷ 도트 크기 해상도의 Point 필터 텍스처에 그려 화면 맨 뒤 RawImage로 확대, 창 크기 바뀌면 다시 생성), 랩 UI에 도트 렌더링 토글(P)·도트 크기 슬라이더(2~8, 기본 4)·해상도 표시, `AnimationLabConfig`에 도트 크기 범위·기본값, 재생 패널 폭 1320 → 1500
- 확인: 켜면 1920×888 화면에서 480×222(4배)·240×111(8배)로 그려지고 UI는 선명 / 끄면 카메라가 화면에 직접 그림, RawImage 꺼짐 / 마우스 입력을 막지 않음(Raycast Target 끔) / 콘솔 에러 0
- 참고: 바닥 격자 같은 가는 선은 도트가 클수록 끊겨 보인다 (확인용 화면 특성)
- 아바타 자동 준비 보완: 아바타 데이터가 있어도 모델이 Humanoid가 아니면 다시 준비 (`Student_Male_v05.fbx`의 .meta가 기본값으로 다시 만들어져 Generic이 된 것을 발견 → 다시 준비해 Humanoid·배율 1.6 복구)

## 2026-11-03 — 애니메이션 랩: 새 캐릭터 모델 자동 준비
- 랩 시작·새로고침 때 `Art/Characters/` 아래에서 AvatarSetup이 없는 .fbx를 찾아 자동으로 준비(Humanoid·키 1.6 배율·텍스처·AvatarSetup·클립 자동 연결)하고 아바타 목록에 추가, 아래쪽에 "새 모델 N개를 준비해 목록에 추가했습니다" 표시
- `AvatarPipeline.PrepareNewModels()`, `AnimationLabBridge.PrepareNewModels` 추가
- 확인: 사용자가 넣은 `Student_Male_v04.fbx` → 플레이 시작 때 `Data/Avatars/Student_Male_v04` 생성, 목록 Student_Male / v03 / v04, v04 키 1.60·Humanoid 뼈 22개 / 콘솔 에러 0
- 주의: `Art/Characters/` 안의 .fbx는 모두 캐릭터(Humanoid)로 처리된다. 소품 모델은 다른 폴더에 둔다

## 2026-11-03 — 아바타 버전 비교 (Student_Male_v03)
- 파일 (사용자): `Art/Characters/Student_Male/Student_Male_v03.fbx`, `Student_Male_v03_tex.png` (v02와 같은 폴더)
- `AvatarPipeline` 이름 규칙: 이미 이 모델을 쓰는 AvatarSetup이 있으면 그것을 갱신, 없으면 폴더에 모델이 하나일 때 폴더 이름·여러 개일 때 파일 이름 → v03 준비가 기존 `Student_Male`(v02)을 덮어쓰지 않음
- v03 준비: `Data/Avatars/Student_Male_v03` 생성, Humanoid, 배율 1.6 (키 1.6), 텍스처 Point, Idle/Walk/Run 자동 연결
- 애니메이션 랩: 아바타를 바꿔도 클립·재생 시간·일시정지·카메라 유지 (확인: Run 0.40초 일시정지 상태로 v02 ↔ v03 전환, 카메라 이동 0)
- 복도 캐릭터는 그대로 v02 (`SideScrollCharacter_Student_Male`)

## 2026-11-03 — 프로토타입 2: 아바타 파이프라인 · 애니메이션 랩 · Student_Male 복도 적용
- 파일 (사용자): `Art/Characters/Student_Male/`(Student_Male_v02.fbx, 텍스처 png), `Animations/Mixamo/`(Y Bot@Idle·Walking·Standard Run, Without Skin, In Place)
- 임포트 설정: Mixamo 3개 Humanoid·반복 재생·루트 굽기, 클립 이름 Idle/Walking/Standard Run / Student_Male Humanoid, 배율 1.58 (키 1.012 → 1.6), 텍스처 Point·압축 없음·Mip Map 끔
- Core: `AvatarSetup`(SO), `CorridorConfig`에 캐릭터 프리팹 필드
- Gameplay: `AvatarAnimationSpeed`(상태별 재생 속도를 Animator 파라미터로, 플레이 중 반영)
- Editor: `AvatarPipeline` 메뉴 `VoxelMagic/아바타/` 1~3 (모델 준비 / 애니메이션 Humanoid 임포트 / 복도 캐릭터 만들기). 복도 씬 생성기는 설정의 캐릭터 프리팹을 우선 사용
- Sandbox(새 asmdef `VoxelMagic.Sandbox`, `VoxelMagic.Sandbox.Editor`): 애니메이션 랩 — `AnimationLab`, `LabClipPlayer`(Playable 재생·섞기·반복·시간 이동), `FootSlideMeter`, `LabCamera`, `AnimationLabBridge`, `AnimationLabConfig`, 씬 생성 메뉴 `VoxelMagic/아바타/애니메이션 랩 씬 다시 만들기`
- 생성: `Data/Avatars/Student_Male`, `Animations/Avatars/Student_Male.controller`(Idle/Walk/Run + 속도 파라미터), `Prefabs/SideScroll/SideScrollCharacter_Student_Male`, `Scenes/Sandbox/AnimationLab.unity`, `Data/Sandbox/AnimationLabConfig`, `Materials/Sandbox/M_Lab_*`
- 문서: `Docs/AVATAR_GUIDE.md`(새 아바타 넣는 순서)
- 랩 확인: 키 1.60(큐브와 같음), Humanoid 뼈 22개 매핑 / Idle→Walk 섞기 진행률 표시 / 반복 끄면 끝(0.733초)에서 멈춤, 켜면 다시 반복 / 시간 이동 0.3초 + 1프레임 → 0.333초 / A↔B: Idle ↔ Walking / 디딤발 속도 걷기 약 1.0 m/s·달리기 약 2.15 m/s, 골반 이동 폭 0.03~0.06 m (제자리) / 한글 글자 빠짐 없음
- 복도 확인 (상태 머신에 입력 값 직접): Idle→Walk→Run 애니메이션 상태 일치, A 한 프레임에 Y 270°, 양 끝 2.56·33.28에서 Idle, Y 0.12·Z 0 고정, Model 위치 (0,0,0) 유지 (루트 이동 없음) / 콘솔 에러·경고 0 / 단위 테스트 56개 통과
- 문제와 처리: 랩 UI 생성 중 기본 폰트로 한글을 먼저 그려 `Assets/TextMesh Pro/.../LiberationSans SDF - Fallback.asset`이 바뀜 → git으로 되돌리고, 폰트를 먼저 지정하도록 수정
- 미확인 (직접 확인 필요): 실제 키·마우스 조작, 재생 속도·이동 속도 자연스러움, 달리기 자세에서 바지·몸통 메시가 꺾여 보이는 정도 (모델 가중치 문제일 수 있음)

## 2026-11-02 — 프로토타입 2 제작 2단계: 캐릭터 (모델 · 상태 머신 · 이동 · 애니메이션 · HUD)
- 입력: `GameControls.inputactions`에 `SideScroll` 맵 추가 (Move = 1D Axis A/D·←/→, 둘 다 누르면 0 / Run = Left Shift). 기존 `Gameplay` 맵은 그대로 (추가된 줄만 있음)
- Core: `MoveIntent`·`Facing`·`CharacterStateId`, `ICharacterState`·`CharacterStateBase`·`IdleState`·`MoveStateBase`·`WalkState`·`RunState`, `CharacterStateMachine`(StateChanged·FacingChanged 이벤트), `SideScrollMotor`(x만), `CharacterMoveConfig`, `CharacterAnimationConfig`, `SideScrollTexts`. `SideScrollContext`에 이동 설정·상태 머신 추가
- Gameplay: `SideScrollInputReader`, `SideScrollCharacter`(위치 Y·Z와 회전 Y ±90°를 매 프레임 덮어씀), `CharacterAnimatorView`(상태 이벤트 → CrossFade)
- UI: `SideScrollHintView`(왼쪽 아래 조작법), `StateDebugView`(오른쪽 위 현재 상태, 설정으로 끔)
- Editor: `SideScrollCharacterBuilder`(메뉴 `VoxelMagic/복도/캐릭터 프리팹 다시 만들기`), `CharacterAnimationBuilder`(메뉴 `VoxelMagic/복도/캐릭터 애니메이션 다시 만들기`), 씬 생성기에 캐릭터·입력·HUD 추가
- 생성: `Prefabs/SideScroll/SideScrollCharacter.prefab`(키 1.6, 루트 = 로직 + 캡슐 충돌체 하나, `Model` 자식 = 큐브 모델 + Animator), `Animations/SideScroll/`(Idle·Walk·Run 클립, Controller — 전이 없음), `Materials/SideScroll/M_SS_*` 6개, `Data/SideScroll/CharacterMoveConfig`·`CharacterAnimationConfig`
- Tests: `CharacterStateMachineTests` 12개, `SideScrollMotorTests` 6개 추가 (전체 56개 통과)
- 플레이 확인 (상태 머신에 입력 값을 직접 넣어 진행): 오른쪽 걷기 Idle→Walk, 속도 1.6, 카메라 x 따라감 / Shift → Run 속도 3.6, 앞으로 기울기·팔다리 ±45° / A 한 프레임에 Y 회전 90 → 270(-90) 즉시 / 양 끝 x 2.56·33.28에서 Idle로 멈춤, 카메라 3.36·32.48로 제한 / 26초 이동 내내 캐릭터 Y 0.12·Z 0 고정, X·Z축 회전 0 / 상태 표시 문구 "상태: 걷기" 등으로 바뀜 / 콘솔 에러 0
- 확인 방법 한계: 에디터가 포커스를 받지 않아 가상 키 입력이 입력 액션에 들어가지 않았다. 입력 바인딩(A/D·←/→·Shift만 있고 W/S 없음)은 설정을 읽어 확인했고, 실제 키 조작과 Point 필터 깜빡임은 직접 플레이로 확인 필요
- 참고: 일시정지 상태로 캡처해 화면의 상태 글자가 갱신되지 않은 채 찍혔다 (값은 바뀐 것을 확인)

## 2026-11-02 — 프로토타입 2 제작 1단계: 복도 맵 (임포트 · 복도 · 카메라 · 패럴랙스)
- 설계: `Docs/DESIGN_CORRIDOR.md` (사용자 결정 반영 후 "제작 시작" 승인)
- 이미지 임포트 설정 (`Art/Corridor` 4장, `Art/Backdrop` 1장): Sprite·Single·Full Rect, PPU 300, 기준점 Bottom Center, Point 필터, 압축 없음, Mip Map 끔, Alpha Is Transparency, Clamp, 최대 크기 4096 (배경 폭 2688이 줄지 않게)
- Core: `CorridorLayout`, `BackdropSet`, `CorridorConfig`(SO), `CorridorBounds`, `SideScrollMath`(패럴랙스·반복 배치·카메라 제한·직교 크기), `SideScrollContext`/`ISideScrollBinder`
- Gameplay: `CorridorBootstrap`(조립), `CorridorBuilder`(타일 배치, 에디터 미리보기는 씬에 저장 안 함), `ParallaxBackdrop`(계수 0.3, 3장 반복), `SideScrollCamera`(직교 크기 1.44, x만 따라가기, 복도 범위 제한)
- Editor: `CorridorSpriteImportTool`(메뉴 `VoxelMagic/복도/이미지 임포트 설정 적용`), `CorridorSceneBuilder`(메뉴 `VoxelMagic/복도/복도 씬 다시 만들기`)
- 데이터: `Data/SideScroll/CorridorConfig`, `CorridorLayout`(문→창문→민벽→창문+사물함→창문→민벽→문(반전)), `BackdropSet_Day`
- 씬: `Scenes/Corridor_SideScroll.unity`
- Tests: `SideScrollMathTests` 10개 추가 (전체 38개 통과)
- 플레이 확인: 복도 7칸 x 2.56~33.28 간격 5.12로 이음매 없음, 마지막 문 반전 / 화면 세로에 복도 높이 꼭 맞음(직교 크기 1.44) / 유리 뒤로 숲·하늘 비침, 하이라이트 점 보임 / 카메라 대상 x 15로 이동 시 배경 화면 이동량 = 카메라 이동량 × 0.3 (계산값과 일치), 빈틈 없음 / 양 끝에서 카메라 x 3.36·32.48로 제한, 복도 밖 안 보임 / 콘솔 에러·경고 0
- 문제와 처리: 씬 생성 중 열려 있던 `Prototype_FarmFishing.unity`가 TMP 내부 값(m_TextStyleHashCode 등)만 바뀐 채 저장됨 → 의도한 변경이 아니라 git 원래 상태로 되돌림
- 미확인: Point 필터 깜빡임은 캐릭터가 걸어 움직일 때 확인 예정 (지금은 에디터 비활성 상태라 연속 움직임을 볼 수 없음)

## 2026-10-30 — 한글 폰트를 Pretendard 고정 아틀라스로 교체
- 이유: 맑은 고딕 동적(DynamicOS) 에셋은 플레이할 때마다 글자가 추가돼 Git 변경으로 잡히고, 맑은 고딕은 게임에 넣어 배포할 수 없는 폰트
- 추가: `Fonts/Pretendard/Pretendard-Regular.otf`(v1.3.9, SIL OFL 1.1, 라이선스 `OFL.txt`), 문자 목록 `KoreanCharset.txt`(영문·숫자·기호, 한글 자모, KS X 1001 한글 2,350자)
- 생성: `Fonts/Pretendard SDF.asset` — 고정(Static) 모드, 2048×2048 아틀라스 1장, 샘플링 32pt, 여백 4, 글자 2,558개, 약 9.7MB. 문자 목록 + Core 스크립트·데이터 에셋에 쓰인 글자를 함께 굽는다
- `KoreanFontAssetBuilder`: OS 폰트 대신 폰트 파일로 고정 에셋을 굽도록 변경, 메뉴 `VoxelMagic/한글 폰트 다시 굽기`. 다시 구워도 GUID를 유지한다. 문구를 새로 쓸 때 문자 목록에 없는 글자(예: 똠)가 생기면 다시 굽는다
- 씬·프리팹(`UI_InventorySlot`, `UI_RequestEntry`) 다시 생성해 새 폰트 연결, `MalgunGothic SDF.asset` 삭제
- 확인: 플레이 모드에서 TMP 텍스트 51개 모두 Pretendard, 빠진 글자 없음, 한글 정상 표시 / 플레이 후 폰트 에셋 파일 해시 그대로 / 콘솔 에러·경고 0 / 단위 테스트 28개 통과

## 2026-10-30 — 낚시: 입질 전 E로 낚싯대 거두기
- `FishingSession.PressInteract()`: 던지는 중·기다리는 중(Casting·Waiting)에 E를 누르면 `Cancel()` → 대기 상태로, 찌·낚싯줄 숨김 (사용자 직접 구현)
- 입질 중 E(미니게임 시작), 결과 중 E(다시 던지기), 이동 키·Esc 취소는 그대로
- 문서: DESIGN.md(구현하며 바뀐 점, 7-6 상태 전이), PLAYTEST.md(조작·시나리오), Notion 낚시 항목 갱신

## 2026-10-30 — 제작 8단계 완료 기준 시나리오 확인
- 한 번의 플레이에서 1~9번을 차례로 확인, 모두 통과
  1. 시작 화면: 실기 0/100·필기 0/100, 조작법 6줄, 1번 칸 지팡이
  2. 게시판 `E / 의뢰 보기` → 창 열림 → "국물 재료가 필요해" 수락
  3. 대파 씨앗 2 지급, 진행 토글 `□ 대파 0/2`, `□ 붕어 0/1`
  4. 1×2로 두 번 심기, 미리보기 초록 / 심어진 칸·격자 끝 빨강
  5. 물 주기 → 즉시 성장, 뽑기 → 획득 정보(대파 58.6cm), 닫힘 요청 시 닫힘, `■ 대파 2/2`
  6. 낚싯줄 던지기 → 4.1초 뒤 입질("!") → 미니게임 → 붕어(16.5cm), 정보 중 E → 바로 다시 던지기
  7. 크리처 `E / 전달하기` → 대사 3줄 → 필기 25, 점수 바 목표 25, 토글 "완료!"
  8. 진행 중 다른 의뢰 수락 불가(이유 표시), 완료 후 다른 의뢰 수락 가능(무 씨앗 3 지급)
  9. 콘솔 에러·경고 0, Core 단위 테스트 28개 통과
- 확인 방법 한계: 에디터가 포커스를 받지 않아 입력은 상호작용 함수를 직접 호출해 진행했다. 이동으로 정보 닫기·낚시 취소, 3초 뒤 정보 자동 닫힘, 실제 키·스크롤 감각은 직접 플레이로 확인 필요
- `Docs/PLAYTEST.md`: 직접 플레이 안내와 조절 값 위치 정리

## 2026-10-30 — 제작 7단계 ⑥ 점수 UI (7단계 완료)
- UI: `ScoreHud`(화면 상단 `실기 0/100`, `필기 0/100` + 채움 바. ScoreBook 이벤트로 목표값을 받고 `GameBalanceConfig.ScoreFillSpeed`(기본 40점/초)로 숫자·바가 부드럽게 차오름)
- 씬 생성기: 점수 HUD 추가
- 플레이 확인: 시작 시 실기 0/100·필기 0/100, 조작법, 1번 칸 지팡이 표시 / 전달 후 필기 바가 0 → 25로 차오름(바 25%), 실기는 0 유지 / 단위 테스트 28개 통과

## 2026-10-30 — 제작 7단계 ⑤ 크리처 전달
- Gameplay: `CreatureNpc`(의뢰 조건 확인 → 안내, E로 전달 → 완료 대사 + 재료별 설명 대사 요청, 대화 창 열기)
- UI: `CreatureDialogView`(대사 한 줄씩, E·클릭으로 넘김, 마지막에 닫힘)
- 수정: 상호작용 안내 상자가 문구 길이에 맞춰 늘어나도록 변경(긴 불가 이유가 두 줄로 접혀 "E"와 겹치던 문제)
- 플레이 확인: 의뢰 없을 때 "게시판에서 의뢰를 받아 오자" / 부족할 때 "의뢰 물품이 부족해요: 대파 0/2, 붕어 0/1" / 충족 시 "전달하기" / E → 의뢰 아이템만 빠짐(남는 대파 유지), 필기 0 → 25(기본 15 + 새 종류 2×5), 진행 토글 "완료!", 대사: 완료 대사 → [대파] 설명 → [붕어] 설명 → 닫힘 / 게시판에서 완료 표시, 다른 의뢰 수락 가능 / 단위 테스트 28개 통과

## 2026-10-30 — 제작 7단계 ④ 낚시
- Gameplay: `PondArea`(지팡이로 E → 던지기, 물가에서 안쪽으로 찌 착수 지점 계산), `FishingDriver`(FishingSession 진행 + 찌 포물선·출렁임·입질 가라앉음, 낚싯줄 LineRenderer), `BiteIndicator`(머리 위 "!" 기본 도형)
- UI: `FishingMinigameView`(세로 바·물고기·잡기 바·잡기 게이지, 안내 문구)
- 씬 생성기: 연못 상호작용·찌·낚싯줄, 손에 든 지팡이와 끝점, "!" 표시, 미니게임 패널 추가
- 플레이 확인: 지팡이 없으면 "지팡이를 들고 낚시하자" / 지팡이 E → 던지기 → 기다림(2~6초) → 입질("!" 표시, 찌 가라앉음) / 입질 1초 놓치면 "입질을 놓쳤다…" 후 다시 기다림 / E → 미니게임 표시 / 바를 물고기에 맞추면 게이지 상승 → 붕어 잡음: 인벤토리 추가, 획득 정보(크기 cm), 진행 토글 `■ 붕어 1/1`, 미니게임 닫힘 / 결과 중 E → 바로 다시 던지기 + 정보 닫힘 / Esc → 낚시 취소(찌·줄 숨김) / 단위 테스트 28개 통과
- 미확인: 이동 키로 낚시 취소(같은 `Fishing.Cancel()` 경로, 가상 키 입력이 들어가지 않아 직접 확인 필요)
- 참고: 한글 폰트 에셋(DynamicOS)은 플레이할 때 글자가 추가되며 파일이 바뀌므로 커밋에서 제외하고 있다 (→ 같은 날 Pretendard 고정 폰트로 교체해 해결)

## 2026-10-30 — 제작 7단계 ③ 텃밭
- Gameplay: `FarmField`(텃밭 전체 = 상호작용 대상 1개, 플레이어 앞 칸 기준), `FarmFieldView`(FarmGrid 이벤트 → 작물 모양), `CropView`(새싹↔다 자람 교체, 커지는 연출), `PlacementPreview`(칸 크기 미리보기, 초록/빨강)
- UI: `AcquisitionPopup`(이름·설명·크기, `GameBalanceConfig.AcquiredInfoSeconds` 뒤 사라짐, 이동 시 즉시 닫힘)
- 데이터 생성기: 작물 모양 프리팹(`Prefabs/Crops/Crop_Sprout·Radish·GreenOnion·Pumpkin`)을 기본 도형으로 만들어 씨앗에 연결. 그레이박스에서 잘 보이도록 크기를 키움
- 플레이 확인: 대파 씨앗으로 1×2 심기 2번(씨앗 2 → 0) / 미리보기: 빈 칸 초록, 심어진 칸 빨강 "이미 심어져 있어요", 격자 끝 빨강 "칸이 부족해요" / 새싹에 지팡이 없으면 "지팡이로 물을 주자" / 지팡이 E → 즉시 다 자람 / E → 뽑기: 인벤토리 추가, 칸 비움, 획득 정보(예: "대파 획득! / 크기 51.9cm"), 진행 토글 `대파 1/2` / 이동하면 획득 정보 즉시 닫힘 / 무(1×1)·호박(2×2) 모양 확인 / 단위 테스트 28개 통과

## 2026-10-30 — 제작 7단계 ② 인벤토리
- UI: `InventoryBar`, `InventorySlotView`(프리팹 `Prefabs/UI_InventorySlot.prefab`), `ToastView`(짧은 알림, 시간은 `GameBalanceConfig.ToastSeconds`)
- 씬 생성기: 하단 중앙 인벤토리 줄(칸 수는 `GameBalanceConfig.InventorySlotCount`), 선택한 아이템 이름, 알림 상자 추가
- 플레이 확인: 5칸 표시(색 사각형 + 첫 글자, 개수, 번호) / 숫자 키 2 → 대파 씨앗 선택·노란 테두리 강조 / 칸 클릭 → 선택 / 같은 아이템은 한 칸에 겹침 / 가득 찬 상태에서 새 아이템 추가 실패 + "가방이 가득 찼어요" 알림 / 의뢰 진행 토글이 인벤토리 변화에 따라 갱신(`■ 붕어 1/1`)

## 2026-10-30 — 제작 7단계 ① 의뢰 게시판
- Editor: `PrototypeDataBuilder`(메뉴: VoxelMagic/예시 데이터 만들기) — 지팡이, 작물 3, 씨앗 3, 물고기 3, 의뢰 3 SO 생성, 연못 물고기 표(붕어 60·메기 30·번개 붕어 10) 채움. 기존 에셋은 덮어쓰지 않음
- UI: `UIPanel`(추상, 열기/닫기), `RequestBoardWindow`, `RequestEntryView`(프리팹 `Prefabs/UI_RequestEntry.prefab`), `QuestTrackerPanel`
- Core: `GameBalanceConfig.QuestCompleteDisplaySeconds`(완료 표시 시간, 기본 2초), 게시판·토글 문구를 `GameTexts`에 추가
- 씬 생성기: 데이터 생성 연결, 시작 아이템(지팡이 1번 칸), 게시판 창·진행 토글 UI 추가
- 플레이 확인: 시작 시 지팡이 1번 칸 / 게시판 E → 의뢰 3개 표시(한글 정상) / "국물 재료가 필요해" 수락 → 대파 씨앗 2 지급, 창 닫힘, 진행 토글 `□ 대파 0/2`, `□ 붕어 0/1` / 진행 중 다른 의뢰 수락 불가(이유 표시) / Q 접기·펼치기 / 대파 2개 → `■ 대파 2/2`(초록) / 완료 후 게시판 "완료", 토글 "완료!" / 단위 테스트 28개 통과
- 체크 표시: 맑은 고딕에 ✓ 글자가 없어 ■(완료)·□(미완료)로 표시

## 2026-10-30 — 제작 6단계 완료
- 수정: 씬 생성기에서 일부 도형이 원점에 놓이던 문제(학교 건물·나무 기둥이 플레이어를 감쌈) → 도형 위치를 부모 기준으로 통일
- 수정: 바닥을 두께 있는 박스로 바꾸고 플레이어를 y=0.1에서 시작 (캐릭터가 바닥을 뚫고 떨어지던 문제)
- 플레이 확인: 쿼터뷰 화면, W 입력 시 화면 위쪽으로 이동·회전, 게시판 근처 `E / 의뢰 보기` 표시(한글 정상), E로 창 열림 → 이동 잠금·안내 숨김, Esc로 닫힘, 멀어지면 안내 사라짐. 콘솔 에러 없음

## 2026-10-30 — 제작 6단계 (첫 시도)
- 입력 액션 에셋 `_Project/Input/GameControls.inputactions` (Move, Interact, Slot1~5, Scroll, ToggleTracker, Cancel)
- Gameplay: GameBootstrap, GameInputReader, PlayerMover, PlayerInteractor, IInteractable/InteractableBase/InteractableRegistry, QuarterViewCamera, RequestBoard
- UI: InteractionPromptView, ControlsHintView
- Editor: KoreanFontAssetBuilder(맑은 고딕 DynamicOS 폰트 에셋), PrototypeSceneBuilder(그레이박스 씬 생성)
- 생성: `Prototype_FarmFishing.unity`, 설정 SO 4종(`Data/Config`), 머테리얼(`Materials/M_*.mat`), 한글 폰트 에셋
- 확인: 컴파일 에러 없음, 플레이 시 Core 조립·카메라 위치 정상, 조작법 한글 표시 정상. 이동·상호작용 플레이 확인은 에디터 종료로 미완료
- 참고: 새 씬 생성 시 Unity가 `ProjectSettings/SceneTemplateSettings.json`(기본값)을 자동으로 만들었다

## 2026-10-30 — 제작 4~5단계
- CLAUDE.md: "모든 스크립트 맨 위에 용도·의도 주석(최대 3줄)" 규칙 추가
- 폴더 구조와 asmdef 5개 생성 (Core, Gameplay, UI, Editor, Tests)
- TMP Essential Resources 가져오기 (`Assets/TextMesh Pro/`)
- Core 로직 작성
  - 데이터: ItemDefinition(추상) / Tool·Seed·Produce·Fish, RequestDefinition, 설정 SO 4종
  - 인벤토리(Inventory), 텃밭(FarmGrid, FarmService, 성장 규칙), 의뢰(RequestTracker), 점수(ScoreBook, ScoreCalculator, MaterialJournal)
  - 낚시 상태 머신(FishingSession, FishingMinigame, FishMotion)
  - 공용: GameContext, PromptChannel, ScreenState, GameMessages, GameTexts, IRandom
- EditMode 단위 테스트 28개 작성, 전부 통과
- DESIGN.md: 구현하며 바뀐 점(0-1절) 기록

## 2026-10-30
- 패키지 추가: MCP for Unity v10.3.0 (`Packages/manifest.json`). 자세한 내용은 SETUP.md
- DESIGN.md: 한글 폰트를 TMP + 맑은 고딕 동적 폰트로 결정

## 2026-10-29
- `Docs/DESIGN.md` 설계 문서 작성 (제작 전 검토용)
