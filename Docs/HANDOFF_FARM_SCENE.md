# 인수인계 — 정식 텃밭(Farm) / 연못(Pond) 씬 만들기

> 작성: 2026-11-09 (타일 맵 에디터 대화에서). 새 대화는 이 문서와 `CLAUDE.md`부터 읽는다.
> 관련: `Docs/TILEMAP_EDITOR_GUIDE.md`(8장 게임 씬에 적용), `Docs/DESIGN.md`(프로토타입 1 설계), `GAMEPLAY_PROTOTYPE_SPEC.md`

## 0. 시작할 때 확인할 것 (사용자에게)
- [ ] **씬 구성**: 텃밭과 연못을 **씬 하나**(예: `Farm`)에 둘지, **씬 두 개**(`Farm`, `Pond`)로 나눌지. 지금 배치(아래 2장)는 텃밭·연못이 한 바닥(`FarmFishing`) 위에 함께 있다
- [ ] **씬 이름·위치** 제안: `Assets/_Project/Scenes/Farm.unity` (두 개면 `Pond.unity`도)
- [ ] **작업 브랜치**: 작업별 브랜치 규칙 (`SETUP.md`). 사용자가 만들었는지, 이름이 무엇인지 확인. 커밋 전 브랜치 확인
- [ ] `CLAUDE.md` 작업 표에 이 작업 줄을 넣을지 (프로젝트 루트 파일이라 **수정 전 묻기**)
- [ ] Build Settings 씬 목록에 새 씬을 넣을지 (프로젝트 설정 변경 → **묻고**, `SETUP.md`·`CHANGELOG.md` 기록)

## 1. 목표 (사용자 요청 2026-11-09)
새 정식 씬에 아래 두 묶음을 **복사**해 넣는다 (원본 씬은 바꾸지 않는다).
1. **`Prototype_FarmFishing` 씬에서**: 기본 도형 연못(낚시 기능), 텃밭 기능, 의뢰 게시판, 크리처(텃밭·연못 테스트용 임시), **모든 임시 UI**
2. **`Scenes/Tools/TileMapEditor` 씬에서**: 구운 바닥 `FarmFishing` + 사용자가 배치한 **기능 없는 배치 오브젝트**(나무·덤불·꽃·벤치 등)
- 기능 없는 배치 오브젝트는 **상위 오브젝트 하나**(예: `Decor`) 아래로 모아 한 번에 켜고 끌 수 있게

## 2. 지금 상태 (2026-11-09 직접 확인, 브랜치 `feature/tool-tile-editor`)
### 2-1. `Scenes/Prototype_FarmFishing.unity` (사용자가 직접 고친 씬 — **저장·재생성 금지**)
```
_Bootstrap [GameBootstrap]            ← 시스템 연결. UI·플레이어·텃밭·연못을 참조할 가능성 큼 → 같이 복사
Main Camera [QuarterViewCamera]
Directional Light (50, 330) Soft, Depth Bias 0.1 · Normal Bias 0
World
 ├ Ground (40×1×40 상자, BoxCollider)   ← 새 씬에는 안 가져옴 (구운 바닥이 대신)
 ├ School, GinkgoTrees                   ← 요청 목록에 없음 → 가져올지 사용자에게 확인
 ├ RequestBoard (-3, 0, 6) [RequestBoard]
 ├ FarmField (-10, 0, -6) [FarmField, FarmFieldView] — Cells(off)·Plots 36칸·Crops·PlacementPreview
 ├ Pond (7, 0, -5) [PondArea, FishingDriver] — Pond_Water·Pond_Bank(fbx, 배율 2,2,1.5, MeshCollider)·Blocker·Bobber·FishingLine 등
 └ Creature (11, 0, 4) [CreatureNpc]
Player (0, 0.1, 0) [CharacterController, PlayerMover, PlayerInteractor, PlayerAvatarAnimator, SocketHost, CharacterEquipment, InventoryEquipmentLink]
UI
 ├ EventSystem
 └ HUDCanvas — ControlsHint, InteractionPromptView, ScoreHud, QuestTracker, InventoryBar, Toast, AcquisitionPopup, FishingMinigame, CreatureDialog, RequestBoardWindow
```
- 메뉴 **`VoxelMagic/프로토타입 씬 다시 만들기`는 실행 금지** (사용자 수정이 사라진다)
- 기능이 돌려면 `_Bootstrap`·Player·Main Camera·Light·UI·EventSystem도 필요할 것 (추측: `GameBootstrap`이 참조를 묶음) → 새 씬에서 무엇을 함께 가져갈지 사용자와 확인

### 2-2. `Scenes/Tools/TileMapEditor.unity`
```
Maps
 ├ Map_FarmSample (off), Map_TestA (off)          ← 테스트 맵
 └ Map_FarmFishing (루트 70, 0, 0)                ← 정식 바닥
    └ FarmFishing_Floor ⟨Art/Map/Baked/FarmFishing/FarmFishing_Floor.prefab⟩ [GroundSurfaceSource, BoxCollider 20×0.5×26 m]
Compare [TileMapCompareRig]                       ← 작업 대상 맵을 따라 자동으로 옮겨짐!
 ├ Center: Main Camera, Chibi, RefCube_1m(off)
 └ East: Pond (사용자 배치, fbx 연못), FarmField (82.6, 0, 4.9) [FarmField, FarmFieldView] ← 기능 있는 오브젝트
씬 최상위: SM_* 27개 ⟨Art/Map/Tree/*.fbx⟩ — 나무·덤불·꽃·풀·벤치·표지판·피크닉·손수레·연못 가장자리 (기능 없는 배치)
```
- 칸 크기 1 m. `FarmFishing` 칠한 범위 약 x 0~20, y −8~18칸 (월드 x 70~90, z −8~18). 굽기 결과는 충돌체·칸 데이터·바닥 종류 포함
- ⚠️ **`Compare/East` 아래의 `Pond`·`FarmField`는 도구 창에서 작업 대상 맵을 바꾸면 함께 옮겨진다** (`TileMapCompareRig`). 복사 전에 위치가 맞는지 확인하고, 필요하면 `Compare` 밖으로 꺼낸다 (사용자에게 확인)
- 에디터 씬의 `FarmField`(82.6, 4.9)는 칸 격자(맵 루트 70, 칸 1 m)에 딱 맞지 않는다 → 텃밭 칸이 흙 칸과 맞는지 확인 (`FarmConfig` Grid Size 6 · Cell Size 1)

## 3. 작업 방법 (추천, 새 대화에서 설계로 정리 후 "제작 시작" 승인받기)
1. **새 씬 만들기**: 빈 씬 + 해(농장과 같은 값) 또는 프로토타입 씬의 Light 복사
2. **두 씬을 함께 열고 복사** (Multi-Scene, `TILEMAP_EDITOR_GUIDE.md` 참고)
   - 반드시 **Ctrl+C / Ctrl+V** (Hierarchy에서 다른 씬으로 끌면 **이동**이 되어 원본에서 빠진다)
   - **참조가 이어진 것들은 한 번에 같이 선택해 복사** (예: `_Bootstrap` + Player + UI + FarmField + Pond + RequestBoard + Creature + Main Camera). 따로 복사하면 서로 연결이 끊긴다 → 붙여 넣은 뒤 Inspector에서 빈 참조(None/Missing) 확인
   - 자식만 복사하면 부모 위치·배율이 빠진다 → 부모째 복사
   - 원본 씬은 **저장하지 않고** 닫는다 (Remove Scene → Don't Save)
3. **바닥**: `FarmFishing_Floor.prefab`을 새 씬에 놓는다 (Rotation 0, Scale 그대로). 위치는 배치 오브젝트와의 상대 위치가 에디터 씬과 같게 (에디터에서는 바닥 원점 = (70, 0, 0)). `Ground` 상자는 가져오지 않는다
4. **배치 오브젝트**: `SM_*`를 `Decor` 아래로 모아 복사. 바닥과 같은 기준으로 옮긴다 (예: 바닥과 `Decor`를 같은 부모 아래 두고 부모를 한 번 옮기기)
5. **위치 정렬**: 프로토타입의 기능 오브젝트(원점 근처)와 에디터 배치(x 70~90)는 좌표계가 다르다 → 기준(바닥 원점)을 정하고 한쪽을 옮긴다. 텃밭(FarmField)은 칸 격자에 맞춘다
6. **플레이 모드 확인**: 걷기(바닥 충돌체), 의뢰 받기, 심기·수확, 낚시, 크리처 대화, UI 표시, 맵 끝에서 떨어지지 않는지(범위 밖은 바닥 없음), 발밑 바닥 종류(선택)

## 4. 참고 사실
- 구운 바닥 프리팹: 메시 + 머티리얼 + `GroundSurfaceSource` + BoxCollider. Tilemap 없음. 다시 구우면 게임 씬의 프리팹도 자동 갱신
- 플레이어는 CharacterController + 중력 → 발밑에 충돌체 필요 (구운 바닥 BoxCollider로 확인됨, CHANGELOG 2026-11-08)
- 연못 FBX는 배율 (2, 2, 1.5) 유지 (사용자 결정). 낚시는 `PondArea`의 Water = `Pond_Water`의 MeshCollider
- 그림자: 해 Depth Bias 0.1 · Normal Bias 0 (`VoxelShadowSettings`)
- `Art/Map/Tree/*.fbx`(배치 오브젝트 원본)는 사용자가 넣은 에셋 — 커밋 여부 확인
- 타일 맵 에디터 작업(이 대화)은 `feature/tool-tile-editor`에 일부 미커밋 변경이 있을 수 있다 → 새 작업 시작 전 `git status`로 확인하고 사용자에게 알리기
