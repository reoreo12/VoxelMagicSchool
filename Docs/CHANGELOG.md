# 변경 기록

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
- 참고: 한글 폰트 에셋(DynamicOS)은 플레이할 때 글자가 추가되며 파일이 바뀌므로 커밋에서 제외하고 있다

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
