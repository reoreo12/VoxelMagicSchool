# 변경 기록

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
