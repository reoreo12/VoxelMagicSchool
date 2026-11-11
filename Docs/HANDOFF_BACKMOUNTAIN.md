# 인수인계 — 뒷산(BackMountain) 채집·포획 프로토타입 (2026-11-11)

> 새 대화는 `CLAUDE.md` → 이 문서 → `Docs/BACKMOUNTAIN_PROTOTYPE_GUIDE.md`(지침, 맨 아래 "추가 항목") 순서로 읽는다.
> 날짜별 변경은 `Docs/CHANGELOG.md` 2026-11-11 항목들. 브랜치 `Back-mountain-map` (커밋 `6d7703e`까지 사용자가 직접 커밋).

## 0. 사용자·작업 방식
- 답변은 한국어. 자체 엔진 경험 있음, Unity는 배우는 중 → Unity 방식을 비교해 설명
- 설계 먼저 → 사용자 "제작 시작" 뒤에만 스크립트·씬·에셋. 기능 하나마다 플레이 확인 → 확인한 것/못 한 것을 나눠 보고
- **지침과 사용자 말이 다르면 사용자 말이 우선.** 질문은 한 번에 여러 개 해도 된다
- 커밋·푸시는 사용자가 한다(요청할 때만). 커밋 전 브랜치 확인. 패키지·프로젝트 설정 변경, `Assets/_Project/`·`Docs/` 밖 수정 전에는 묻는다
- 기존 씬 오브젝트를 지우기보다 끄는 쪽을 사용자가 선호 (예: C3·D 청크는 지우지 않고 꺼 둠)
- 그 맵에서 할 수 있는 동작의 UI·시스템만 가져온다 (뒷산에는 낚시·농사 UI 없음)

## 1. 씬 구성 (`Assets/_Project/Scenes/BackMountain.unity`)
```
Directional Light
Main Camera (Map Preview, off)        ← 원래 맵 미리보기 카메라, 꺼 둠
MountainFloor                          ← 흙 바닥 청크 T·A1·A2·A3·B·C1·C2 (FBX 인스턴스, MeshCollider = 렌더 메시)
Mountain_Floor_C3 (off), Mountain_Floor_D (off)   ← 맵 축소로 꺼 둠 (사용자가 루트로 옮김)
MountainBackdrop
 ├ Backdrop_front / Backdrop_back      ← 바위산 봉우리 20개 (Spire_A~D × x/z = FBX 8종, 그림자 Off)
 └ Backdrop_apron                      ← 이끼 바닥 청크 AP1~AP3 (MeshCollider 있음, 그림자 Off)
[Prototype]                            ← 테스트 오브젝트 전부 (지우면 한 번에 정리)
 ├ Gathering   서식지 12개 (Habitat_Clearing/Bush/TreeShade/RockCrevice 각 3)
 ├ _Mountain   MountainBootstrap + MountainDebugInput + MagicCircleStub
 ├ Requests    RequestBoard (11, 2) · Creature (4.6, 0.7)  ← FarmPond에서 복사
 ├ Capture     Floor(16×16, 윗면 4.2) · Zone(트리거+CaptureZone, 풀숲 12개 자식) · NearPath(트리거) · Sign · Walls(투명)
 └ PathEndWall (invisible)   ← C2 동쪽 끝 x = 48 낙하 방지 (C3·D를 다시 켜면 지울 것)
_Bootstrap (GameBootstrap) · Player · Main Camera(QuarterViewCamera) · UI · SceneTransition  ← FarmPond에서 Ctrl+C/V 방식 복사
```
- 박스 콜라이더 크기·센터는 사용자가 조정함: `Zone`(z 약 59.9~79.7), `NearPath`(스케일 1.5×1×2). **다시 만들기 메뉴를 돌리면 이 조정이 사라진다** (아래 3장)
- 플레이어 시작점 T (8, 0.1, −4.5). 이동 설정은 `Data/Config/MoveConfig_BackMountain`(걷기 4·달리기 5.5). FarmPond는 `MoveConfig`(3·4.5) — 맵별 속도는 씬의 `PlayerMover`·`AvatarAnimationSpeed`에 연결한 에셋으로 나눈다
- 해(`Directional Light`)는 FarmPond와 같은 (50, 330), Use Pipeline Settings 끔 + Depth Bias 0.1·Normal Bias 0 (복셀 그림자 구멍 방지, `Editor/VoxelShadowSettings`). 새 씬·새 조명도 같은 설정을 쓴다
- 조작법 문구는 `ControlsHintView._sceneText`(씬별). 디버그 키 안내는 `UI/HUDCanvas/DebugKeyHint (Prototype)`, 중앙 문구는 `CenterBanner (Prototype)`

## 2. 코드 구조 (모두 `Scripts/*/Mountain/`)
| 층 | 파일 | 역할 |
|---|---|---|
| Core | `MountainContext` | 뒷산 전용 서비스 묶음 + `IMountainContextBinder`·`DebugCommandList`. **`GameContext`는 건드리지 않음** |
| Core | `GatherMaterialDefinition`·`GatheringConfig`·`GatherSpawner`·`GatheringService` | 채집: 자기 서식지·시간대만, 새 하루에 직전 빈 슬롯 우선, 의뢰 재료 최소 1개 보장(수락 때도) |
| Core | `AnimalDefinition`·`CaptureConfig`·`CaptureService`·`IMagicCircleDrawer`·`MountainMessages` | 포획: 구역 안 5초마다 입질 1~2개(3초), 근처 길에서 맨 앞 풀숲 신호, 포획 성공/실패·강제 |
| Core | `MountainTexts` | 뒷산 문구 |
| Gameplay | `MountainBootstrap`(실행 순서 −900) | `GameBootstrap.Context`를 받아 위 서비스 조립, `_bindRoots`([Prototype], UI) 아래 바인더 연결 |
| Gameplay | `GatherHabitat`·`GatherPickup` | 서식지·슬롯(줍기 대상, `InteractableBase`) |
| Gameplay | `CaptureZone`·`GrassTuft`·`MagicCircleStub` | 구역 안/밖 판정(박스 bounds), 근처 길 신호, 풀숲 흔들림(잦아들며 멈춤), 마법진 대역(성공 확률 0.85) |
| Gameplay | `MountainDebugInput` | F1 새 하루 · F2 의뢰 교체 · F3 시간대 · F4 입질 즉시 · F5/F6 다음 마법진 성공/실패 (Inspector에서 키 변경) |
| UI | `DebugKeyHintView`·`CenterBannerView` | 디버그 안내·중앙 문구 |
| Editor | `BackMountainGatheringBuilder`·`BackMountainCaptureBuilder` | 데이터·프리팹 생성(있으면 유지) + 씬 배치 |
- 공용 코드에 넣은 것: `Inventory.DiscardSlot`, `RequestTracker.Abandon`, `RequestBoardWindow._boardRequests`(게시판별 의뢰), `RequestEntryView`·`QuestTrackerPanel` 서식지 힌트, `InventoryBar._debugRightClickDiscard`(뒷산만 켬), `ControlsHintView._sceneText`, `GameTexts.DiscardedFormat`
- 테스트: `Tests/GatheringTests.cs`·`CaptureTests.cs` (EditMode 전체 134개 통과)

## 3. 데이터·메뉴
- 재료 12종 `Data/Mountain/Gather/` (서식지마다 방과후 2 + 밤 1), 의뢰 4종 `Data/Mountain/Requests/`, 동물 3종 `Data/Mountain/Animals/`(산토끼 0.5·다람쥐 0.6·여우 0.8)
- 설정: `Data/Config/GatheringConfig`(채움 0.5), `CaptureConfig`(간격 5·흔들림 3·동시 1~2·근처 길 0.6초/쿨다운 8/세기 0.7·잦아듦 0.35), `MoveConfig_BackMountain`. **모든 숫자는 "가정" 기본값**
- 메뉴 `VoxelMagic/뒷산/채집 서식지 만들기·배치`, `…/의뢰 데이터 만들기`, `…/포획 구역 만들기·배치` — 배치 메뉴는 `[Prototype]/Gathering`·`Capture`를 **지우고 다시 만든다**(사용자 조정 사라짐). 새 아이템·의뢰를 만들면 `VoxelMagic/저장/정의 목록 채우기`

## 4. 알려진 문제·결정 대기 (기능)
- 내리막을 달릴 때(A3, 5.5 m/s) 최대 0.15초 살짝 뜸 → 지면 붙이기(접지 스냅) 여부 결정
- 의뢰 요구량 3개(산딸기 3)인데 보장은 1개 → 요구량만큼 보장할지
- "서서 기다리면 입질"인데 지금은 구역 안이면 걸어도 타이머가 흐름
- 마법진 스텁 대기 중 이동 가능 → 실제 마법진은 화면을 막는 창(ScreenState)으로
- `RequestTracker.Catalog`는 처음 시작한 씬의 부트스트랩 목록 → 게임 전체 의뢰 목록 정리 필요
- 지침의 디버그 패널(시간대·구역 안/밖·다음 입질까지·흔들리는 풀숲 수)과 "인벤토리 비우기" 키는 아직 없음 (우클릭 버리기만)
- 시간대는 뒷산 안에서만 쓰는 `TimeOfDayState` (게임 전체 시간 시스템 없음). 새 하루/시간대 전환 자동 재스폰은 디버그 키로만

## 5. Claude가 확인할 때의 요령
- MCP `execute_code`는 C# 6(CodeDom): `Object`는 `UnityEngine.Object`로, `in` 매개변수는 `ref`로. 같은 명령이 두 번 실행될 수 있어 상태를 바꾸는 코드는 `SessionState` 표시로 막는다
- 에디터가 뒤에 있으면 플레이 모드 프레임이 거의 안 돈다 → 컴포넌트 `Update`를 리플렉션으로 직접 부르고 서비스 `Tick`을 수동으로 진행해 확인. `EditorApplication.Step()`으로 한 프레임씩 진행 가능. 실제 키 조작은 사용자에게 부탁
- **시험 전 `GameSaveSystem.AutoSaveEnabled = false`** (사용자 저장 파일 보호). 사용자 저장에는 실제 진행(가방·의뢰·필기 점수)이 들어 있다
- 스크린샷은 `output_folder: "Temp/Captures"` (프로젝트 밖 경로는 거부됨)
- 씬 저장 전 `isDirty` 확인, 저장 뒤 `git diff --stat`으로 의도한 변경만 있는지 확인
