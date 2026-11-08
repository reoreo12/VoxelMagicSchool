# 설계 — 캐릭터 공용 시스템 (소켓·장착물 · 애니메이션 · 스탯)

> 작성: 2026-11-06. 바탕: `Docs/HANDOFF_CHARACTER_SYSTEMS.md` 4번 추천 구조.
> 상태: 2026-11-06 "제작 시작" 승인. 단계 1-a(농장) 완료·사용자 확인. **단계 1-b(애니메이션 랩) 완료 — 사용자 확인 대기.**
> 지팡이 각도: 지금 값(Idle에서 앞으로 31° 들림)으로 유지 (사용자 결정). 동작 애니메이션이 들어오면 조절할 값을 그때 정리한다 (5-1 "각도 조절 메모").
> 이번 문서의 상세 범위는 **단계 1 (농장 플레이어 오른손 지팡이)**. 애니메이션·스탯은 큰 틀만 적고 해당 단계에서 다시 다듬는다.

---

## 1. 다시 확인한 사실 (2026-11-06, 이 대화에서 직접 확인)

| 항목 | 확인 방법 | 결과 |
|---|---|---|
| Unity 연결 | MCP `set_active_instance VoxelMagicSchool@2d926515` → `Application.dataPath` | `C:/Unity_Project/NCAI/VoxelMagicSchool/Assets`, Unity 6000.0.38f1, 플레이 중 아님, 컴파일 중 아님. 열린 씬은 `Scenes/Tools/AnimationLab.unity` |
| 브랜치 | `git branch --show-current` | `feature/character-system`, 변경 없음 |
| 칩비 손 본 | `Student_Chibi_v2.fbx`의 ModelImporter Humanoid 매핑 | `RightHand→RightHand`, `LeftHand→LeftHand`, `RightLowerArm→RightForeArm`, `Head→Head` 있음. **Neck 매핑 없음**. 리그 `optimizeGameObjects: 0`(본 Transform이 그대로 남아 있음 → 본 아래에 자식을 붙일 수 있다) |
| 농장 씬 플레이어 | `Prototype_FarmFishing.unity` YAML 읽기 (씬은 열지 않음) | 루트 `Player` 아래에 `Model`(칩비 FBX 프리팹 인스턴스, guid `cbb686…`), `Wand`(로컬 (0.35, 1.15, 0.4), 크기 0.05×0.05×0.58, X −31°), `WandTip`(로컬 (0.35, 1.3, 0.65)) — 셋 다 **플레이어 루트의 자식**이라 손을 따라가지 않는다. `PlayerAvatarAnimator._animator`는 이 Model의 Animator |
| 낚싯줄 참조 | `FishingDriver.cs` | `[SerializeField] Transform _wandTip` — 던지기 시작(`Casting` 진입 시 위치 한 번 읽음)과 **매 프레임 줄 시작점**(`_line.SetPosition(0, _wandTip.position)`)에 사용. 씬에서 `_wandTip` → 위 `WandTip` |
| 씬 생성 도구 | `PrototypeSceneBuilder.BuildPlayer` | 위 `Wand`·`WandTip`을 만들고 `fishingDriver.SetRef("_wandTip", …Find("WandTip"))`로 연결 |
| 아이템·인벤토리 | `ToolDefinition`(`ToolKind.Wand`), `Inventory.SelectionChanged`(int) | 인벤토리 선택 변경 이벤트가 이미 있어서 "지팡이 칸을 고르면 손에 듦"을 붙이기 쉽다 |

**추측(아직 확인 안 함)**: 칩비 `RightHand` 본의 로컬 축 방향(손바닥이 어느 축을 보는지)은 FBX마다 달라서 지팡이 회전 보정 값은 플레이 모드에서 눈으로 맞춰야 한다.

---

## 2. 사용자 엔진(Container/Part) ↔ Unity 방식 쉽게 비교

### 2-1. 한 장 요약
```
사용자 엔진                               Unity (이 설계)
──────────────────────────────           ──────────────────────────────────────────
ContainerObject (Player)                  캐릭터 루트 GameObject
  상속: Player / Npc / Monster             + 컴포넌트 조립 (소켓, 장착, 스탯, 이동, 조종)
                                           Player/NPC/몬스터 = "어떤 조종 컴포넌트 + 어떤 데이터"
PartObject                                 
  ├ Body   → 소켓 위치 Get 함수            Model(FBX) + ISocketSource (본 Transform을 "한 번" 찾아 줌)
  ├ Weapon → 매 프레임 Get으로 위치 갱신    장착물 프리팹을 손 본 아래 "자식"으로 한 번 붙임 → 이후 엔진이 자동으로 따라감
  └ Effect                                 AttachmentBase 하위 (같은 붙이기 규칙)
코드 안의 캐릭터별 값                       ScriptableObject 데이터 (소켓 프로필·장착물 정의·스탯 프로필)
```

### 2-2. 핵심 차이 3가지
1. **"매 프레임 Get" → "한 번 부모 지정"**
   - Unity의 `Transform`은 씬 그래프다(부모가 움직이면 자식의 월드 위치를 엔진이 계산). 애니메이션이 손 본을 움직이면, 그 자식인 지팡이도 C++ 쪽에서 같이 갱신된다.
   - 그래서 사용자 엔진의 `GetSocketTransform()`은 **장착하는 순간 한 번**만 쓴다. 매 프레임 스크립트 `Update`가 없으니 가볍고, 애니메이션 갱신 순서와 어긋나서 한 프레임 늦게 따라가는 떨림도 없다.
2. **상속 계층 → 컴포넌트 조합**
   - Unity에서는 모든 것이 이미 `GameObject`이고 기능은 `MonoBehaviour` 컴포넌트로 붙인다. `Player : ContainerObject` 같은 클래스 대신 **같은 부품을 붙이고 데이터만 바꾼다**. 몬스터에 지팡이를 쥐여 주고 싶으면 상속을 바꿀 필요 없이 같은 컴포넌트를 붙이면 된다.
3. **Part 종류 차이는 두 축으로 나눔**
   - "소켓을 **어떻게 찾나**"(사람형 뼈 / 몬스터 임의 위치) → 인터페이스 `ISocketSource` (다형성)
   - "붙은 물건이 **무엇을 하나**"(지팡이 끝 제공, 이펙트 재생…) → 추상 클래스 `AttachmentBase` (공통 동작 상속)

---

## 3. 문서 4번 제안에서 바꾸거나 더하는 점 (제안 — 사용자 승인 필요)

| # | 문서 4번 | 이 설계의 제안 | 이유 (Unity · 객체 지향 · 최적화) |
|---|---|---|---|
| A | `SocketHost`가 소켓 관리 **와** 장착물 붙이기·떼기, `EquipmentView`가 상태 보고 붙이기 | **`SocketHost`는 소켓 찾기·캐시만**, 붙이기·떼기·프리팹 생성은 **`CharacterEquipment` 하나**가 한다 | 두 부품이 "붙이기"를 나눠 가지면 책임이 겹친다. 소켓 = "어디에", 장착 = "무엇을" 으로 딱 나누면 몬스터처럼 장착 없이 소켓만 쓰는 경우(이펙트 위치 등)도 깔끔 |
| B | 장착 해제 시 장착물 제거 | 한 번 만든 장착물 인스턴스는 **정의별로 캐시해 두고 `SetActive`로 켜고 끈다** | 인벤토리 칸을 바꿀 때마다 `Instantiate/Destroy`하면 GC·생성 비용. 캐릭터당 장착물은 몇 개뿐이라 메모리 부담 작음 (작은 오브젝트 풀) |
| C | `CharacterRoot`·`ICharacterBrain`을 처음부터 | **단계 1에서는 만들지 않는다.** 소켓·장착만 만들고 기존 `PlayerMover`는 그대로. 조종 분리는 NPC/몬스터를 실제로 만드는 단계 5에서 | 필요 전에 틀부터 만들면 쓰이지 않는 추상화가 생긴다(YAGNI). 소켓·장착 부품은 루트 조립 지점이 없어도 독립적으로 동작하게 설계 |
| D | 낚시가 `IHasTip`으로 장착된 지팡이 끝 조회 | `FishingDriver`는 `Transform _wandTip` 대신 **`ICastOrigin`**(시작점 제공자) 하나를 참조. 플레이어의 `CharacterEquipment`가 이를 구현해 "지금 든 장착물 중 끝(Tip)이 있는 것"을 돌려준다. 장착이 바뀔 때만(이벤트) 끝 Transform을 다시 캐시 | 낚시는 "지팡이"를 몰라도 되고(결합 감소), 나중에 손에서 바로 마법을 쏘는 캐릭터도 같은 인터페이스. 매 프레임 `GetComponent` 없음 |
| E | 장착물 잡는 보정만 | 보정을 **두 겹**으로: 소켓 보정(아바타별: 손바닥 중심·방향, `SocketProfile`) + 손잡이 보정(장착물별: 물건의 어디를 쥐나, `AttachmentDefinition`) | 칩비처럼 손이 큰 아바타가 늘어도 지팡이 데이터는 그대로, 새 지팡이가 늘어도 아바타 데이터는 그대로 (N×M 조합 대신 N+M) |
| F | 인벤토리와 장착 연결 방식 미정 | `Inventory`와 `EquipmentState`는 서로 모르고, 둘을 잇는 작은 부품 **`InventoryEquipmentLink`**(Gameplay)가 `SelectionChanged`를 받아 장착을 바꾼다 (질문 4의 답에 따라 켜고 끔) | Core 클래스끼리 직접 묶지 않아 단위 테스트가 쉽고, NPC는 인벤토리 없이 기본 장착만 쓸 수 있다 |
| G | — | 리그의 **Optimize Game Objects는 끈 상태 유지** (지금 0). 켜면 본 Transform이 숨겨져 손 아래에 붙일 수 없게 되므로, 나중에 최적화로 켤 때는 "Extra Transforms to Expose"에 손 본을 넣어야 한다 | 미리 적어 두지 않으면 나중에 최적화하다 지팡이가 사라지는 버그가 생긴다 |

---

## 4. 단계 1 상세 설계 — 소켓·장착물

### 4-1. 오브젝트 구조 (농장 플레이어, 실행 중)
```
Player                                  [CharacterController] [PlayerMover] [PlayerInteractor] [PlayerAvatarAnimator]
│                                       [SocketHost] [CharacterEquipment : ICastOrigin] [InventoryEquipmentLink]
├─ Model (칩비 FBX)                      [Animator] [HumanoidSocketSource : ISocketSource]
│   └─ … RightHand (본)
│        └─ Socket_RightHand            ← SocketHost가 시작 시 한 번 생성 (SocketProfile의 위치·회전 보정)
│             └─ Grip                    ← CharacterEquipment가 장착물별 손잡이 보정 적용
│                  └─ Wand_Temp (프리팹 인스턴스) [WandAttachment]
│                       └─ Tip           ← 낚싯줄 시작점
├─ Wand      (기존, 비활성화 — 삭제하지 않음)
└─ WandTip   (기존, 그대로 둠 — ICastOrigin이 없을 때의 대비용)
```

### 4-2. 클래스

**Core** (Unity 씬 객체 없이 값·식별자·이벤트만, 단위 테스트 대상)

| 이름 | 종류 | 공개 | 역할 |
|---|---|---|---|
| `SocketId` | ScriptableObject (빈 이름표) | `DisplayName` | 소켓 식별자. 에셋 참조로 비교하므로 문자열 오타가 없고 코드 수정 없이 늘린다 |
| `SocketProfile` | ScriptableObject | `TryGet(SocketId, out SocketBinding)` | 아바타별 소켓 목록. `SocketBinding` = 소켓 ID, 사람형 본(`HumanBodyBones`, 없으면 Transform 이름), 위치·회전 보정 |
| `AttachmentDefinition` | ScriptableObject | `Prefab`, `Socket`, `GripPosition`, `GripRotation` | 장착물 데이터 |
| `EquipmentState` | 일반 C# 클래스 | `Get(SocketId)`, `Equip(def)`, `Unequip(SocketId)`, `event Changed(SocketId, AttachmentDefinition old, AttachmentDefinition now)` | 소켓별 장착 상태. 같은 소켓에 새 물건을 끼우면 이전 것은 자동 해제 |

기존 파일 변경 (덮어쓰기가 아니라 필드 추가):
- `AvatarSetup` + `[SerializeField] SocketProfile _socketProfile` (읽기 전용 `SocketProfile`)
- `ToolDefinition` + `[SerializeField] AttachmentDefinition _attachment` (선택, 비어 있으면 손에 아무것도 안 듦)

> `HumanBodyBones`는 `UnityEngine` 기본 enum이라 Core에서 써도 의존 규칙에 어긋나지 않는다 (Animator 객체는 쓰지 않음).

**Gameplay** (MonoBehaviour)

| 이름 | 역할 |
|---|---|
| `ISocketSource` (인터페이스) | `bool TryGetAnchor(SocketBinding, out Transform)` — 소켓을 붙일 기준 Transform을 준다 |
| `HumanoidSocketSource` | Animator의 `GetBoneTransform(HumanBodyBones)`로 찾음 (칩비·Student_Male 공통) |
| `TransformSocketSource` | 모델 안의 이름으로 찾음 (몬스터용, 단계 5에서 만들어도 됨) |
| `SocketHost` | `Awake`에서 `SocketProfile` 순서대로 `Socket_<이름>` 오브젝트를 기준 본 아래에 **한 번** 만들고 `Dictionary<SocketId, Transform>`에 캐시. `TryGetSocket(SocketId, out Transform)` |
| `AttachmentBase` (추상) | `OnAttached(owner)`, `OnDetached()` 가상 메서드, `Owner` 읽기 전용 |
| `WandAttachment : AttachmentBase` | `[SerializeField] Transform _tip` → `Tip` 공개. `ITipProvider` 구현 |
| `ITipProvider` (인터페이스) | `Transform Tip` — 끝이 있는 장착물 공통 |
| `CharacterEquipment` | `EquipmentState`를 갖고 `Changed` 이벤트에 맞춰 프리팹 인스턴스를 소켓의 `Grip` 아래에 붙이고 켜고 끔(캐시, 3-B). `ICastOrigin` 구현: 장착 변경 시 `ITipProvider`를 찾아 끝 Transform 캐시. 시작 장착물(`_defaultAttachments`) 지원 |
| `ICastOrigin` (인터페이스) | `bool TryGetOrigin(out Vector3 position)` |
| `InventoryEquipmentLink` | `IGameContextBinder`로 `Inventory`를 받아 `SelectionChanged` 시 선택 아이템의 `AttachmentDefinition`을 장착/해제 (질문 4 답에 따라) |

`FishingDriver` 변경: `[SerializeField] MonoBehaviour _castOriginSource`(ICastOrigin, 인스펙터에서 타입 검사) 추가. 있으면 그것을, 없거나 끝이 없으면 기존 `_wandTip`을 쓴다 → **기존 씬이 깨지지 않는다**.

상속 깊이: `MonoBehaviour → AttachmentBase → WandAttachment` (우리 코드 기준 1단계). 3단계 필요 없음.

### 4-3. 데이터·에셋 (모두 `Assets/_Project/` 안)
| 경로 | 내용 |
|---|---|
| `Data/Characters/Sockets/Socket_RightHand.asset` (+ 질문 1 답에 따라 다른 소켓) | `SocketId` |
| `Data/Characters/SocketProfiles/SocketProfile_Chibi.asset` | 칩비 오른손 보정 |
| `Data/Characters/Attachments/Attachment_WandTemp.asset` | 임시 지팡이 정의 |
| `Prefabs/Attachments/Wand_Temp.prefab` | 지금 씬의 `Wand`와 같은 상자(7-1) + `Tip` + `WandAttachment`. 콜라이더 제거 (손에 든 물건이 CharacterController·상호작용에 걸리지 않게) |
| `Data/Avatars/Student_Chibi_v2` | `_socketProfile` 연결 |
| `Data/Items/Tool_Wand` | `_attachment` 연결 |

### 4-4. 농장 씬에 붙이는 방법 (씬 다시 만들기 메뉴는 쓰지 않음)
- 새 에디터 메뉴 **`VoxelMagic/캐릭터/농장 플레이어에 소켓·장착 붙이기`** — 지금 열린 씬의 `Player`를 찾아서:
  1. `Model`에 `HumanoidSocketSource`, `Player`에 `SocketHost`·`CharacterEquipment`·`InventoryEquipmentLink` 추가 (이미 있으면 건너뜀 — 여러 번 실행해도 안전)
  2. `FishingDriver._castOriginSource` ← `CharacterEquipment`
  3. 기존 `Wand` 오브젝트는 **비활성화만** (삭제 안 함), `WandTip`은 대비용으로 둠
  4. 씬을 dirty 표시만 하고 **저장은 사용자가** (Ctrl+S) — 자동 저장하지 않음
- 같은 메뉴를 쓸 수 없을 때를 위한 수동 순서도 `Docs/CHARACTER_GUIDE.md`(새 파일)에 적는다
- `PrototypeSceneBuilder.BuildPlayer`도 같은 구조로 고쳐 두지만 **실행하지 않는다**

### 4-5. 단위 테스트 (`Tests/CharacterEquipmentTests.cs`)
- 빈 상태에서 `Get` → null
- `Equip` → `Changed(socket, null, def)` 한 번
- 같은 소켓에 다른 물건 `Equip` → `Changed(socket, old, new)`, 이전 것 해제
- 같은 물건 다시 `Equip` → 이벤트 없음
- `Unequip` → `Changed(socket, def, null)`, 빈 소켓 `Unequip` → 이벤트 없음
- `SocketProfile.TryGet` 없는 ID → false

### 4-6. 플레이 확인 항목 (단계 1 완료 조건)
| 확인 | 방법 |
|---|---|
| 지팡이가 오른손에 붙어 Idle·Walk 중 손을 따라감 | 플레이 → 씬 뷰 스크린샷 (Idle, Walk 각각 `EditorApplication.Step`으로 프레임 진행) |
| 소켓·장착 계층이 4-1 구조와 같음 | 플레이 중 계층 조회 |
| 인벤토리 선택에 따라 들고/내림 (질문 4 답에 따라) | 코드로 `Inventory.Select` 호출 후 활성 상태 확인 |
| 낚싯줄 시작점 = 지팡이 `Tip` 위치 | 낚시 상태를 코드로 진행 → `LineRenderer` 0번 점과 `Tip.position` 비교 |
| 물 주기·낚시 기존 동작 | 단위 테스트 전체 통과 + 사용자 실제 플레이 |
| 콘솔 에러·경고 없음 | `read_console` |

---

## 5. 뒤 단계 큰 틀 (해당 단계에서 다시 설계)

### 5-1. 애니메이션 (단계 4)
- 이동(Idle/Walk/Run)은 Base 레이어 그대로. 위에 **Action 레이어**(Avatar Mask로 상체만, 필요하면 전신)를 두고 "한 번 재생" 동작
- `CharacterActionId`: **ScriptableObject 이름표를 추천** (SocketId와 같은 방식, 코드 수정 없이 동작 추가). `AvatarSetup`에 동작 → 클립 목록
- 게임 로직은 "동작 요청"만, 애니메이션 뷰가 재생. 던지는 순간 같은 타이밍은 애니메이션 이벤트 → 게임 로직
- 임포트: 한 번 재생 클립은 반복 끄기 (데이터의 목록 기준 — 파일 이름 규칙보다 실수가 적다)
- Unity 6의 `Animator.StringToHash`로 상태·파라미터 해시 캐시
- **각도 조절 메모 (2026-11-06)**: 지팡이 각도는 "손 본 기준 고정 회전"이라, 손목이 위로 꺾이는 동작(던지기 등)에서는 지팡이도 그만큼 더 올라간다. 조절할 곳은 두 곳:
  - `SocketProfile_<아바타>`의 회전 보정 — 그 아바타의 모든 장착물에 적용 (손 모양 기준)
  - `Attachment_<장착물>`의 놓는 회전·위치 — 그 장착물만 (예: 지팡이만 덜 들리게)
  - 흔한 방식: 소켓은 "주먹이 쥔 방향 그대로"(중립)로 두고, 각도 연출은 애니메이션이 맡게 한다. 동작 클립이 들어오면 랩에서 비교해 정한다

### 5-2. 스탯 (단계 3)
- 문서 4-4 그대로: `StatDefinition`·`StatProfile`(SO), `StatBlock`(일반 C#, 기본값 + 보정치, `Changed` 이벤트), `StatsHolder`(MonoBehaviour)
- 보정치 계산은 "더하기 합 → 곱하기 합" 순서 고정, 값은 바뀔 때만 다시 계산(더티 플래그) — 매 프레임 계산 없음
- 스탯 종류는 단계 3 전에 사용자에게 묻는다

### 5-3. 조종 분리 (단계 5)
- `ICharacterBrain`(이동·행동 의도) — `PlayerBrain`(입력) / `NpcBrain`·`MonsterBrain`(AI). 지금 `PlayerMover`를 "의도 → 이동"과 "입력 → 의도"로 나눔

---

## 6. 사용자 결정
| 질문 | 답 (2026-11-06) | 설계 반영 |
|---|---|---|
| 소켓 목록 | **오른손만** | `Socket_RightHand` 하나. 나머지는 필요할 때 에셋만 추가 |
| 임시 지팡이 | **지금 동작하는 Wand를 그대로 소켓에 붙인다.** 나중에 지팡이 FBX가 오면 메시만 바꾼다 | 7-1 |
| 인벤토리 연결 | **지팡이 칸을 고를 때만 손에 듦.** 낚시·물 주기 중 Idle 자세여도 신경 쓰지 않음 — 소켓에만 잘 붙어 있으면 나중에 동작 애니메이션이 와도 계속 손을 따라간다 | 7-2 |
| 적용 범위 | **농장 + 애니메이션 랩.** 농장: 손에 붙이고 기존 농사·연못 기능 그대로. 랩: 물 주기·찌 던지기 없이 모습만 확인, **장착물 접이식 패널**에서 장착물 내리기·바꾸기 | 7-3, 7-4 |
| 스탯 종류 | 단계 3 전에 다시 묻는다 | — |
| 스탯·동작 애니메이션 시기 (2026-11-06) | **미룸.** 새 캐릭터를 추가하거나 전투 시스템이 생길 때 진행 (단계 3·4·5 보류) | 5번 |
| 3번 제안 A~G | (대기 — "제작 시작" 때 함께 확인) | — |
| 지팡이를 프리팹으로 (7-1) | **프리팹으로 만든다** (필요하면 나중에 Unpack) | 7-1 그대로 |
| 랩 씬 덮어쓰기 | **허용.** 랩 브랜치·씬은 최신 상태로 저장(커밋)돼 있음. **기존 패널(아바타·클립·재생 등)도 모두 접이식으로** | 7-4 |
| Tools → Gameplay 참조 | **허용.** 랩은 농장에서 쓰는 소켓·장착 부품을 그대로 가져다 쓴다 | 7-4 |
| CLAUDE.md 작업 표 | **이 작업 줄 추가** (2026-11-06 반영) | — |

---

## 7. 답을 반영한 단계 1 세부 (4번보다 이쪽이 우선)

### 7-1. 지팡이 = 지금 Wand 그대로
- 씬의 `Wand`는 기본 Cube 메시 + 머티리얼 에셋 `M_Wand`(갈색) + 크기 0.05×0.05×0.58 + X −31°. `WandTip`은 이 상자의 **앞쪽 끝**과 정확히 같은 점이다 (상자 중심 + 회전된 +Z × 0.29 = (0, 0.149, 0.249) ≈ WandTip − Wand = (0, 0.15, 0.25)).
- **추천**: 같은 메시·머티리얼·크기로 `Prefabs/Attachments/Wand_Temp.prefab`을 만든다 (구조: `Wand_Temp`[WandAttachment] → `Mesh`(Cube, 0.05×0.05×0.58, 콜라이더 없음) + `Tip`(상자 앞 끝)). 
  - 이유: 농장·애니메이션 랩 두 곳이 **같은 프리팹 하나**를 쓰고, 지팡이 FBX가 오면 이 프리팹의 `Mesh`만 바꾸면 두 곳 모두 바뀐다. 랩에서 내리기·바꾸기도 프리팹이 있어야 가능하다.
  - 씬 오브젝트 `Wand`를 그대로 손 밑으로 옮기는 방법도 있지만, 그러면 농장 씬에만 존재해서 랩과 공유가 안 되고 FBX 교체도 두 번 해야 한다.
- 농장 씬의 원래 `Wand`는 **비활성화만**(삭제 안 함), `WandTip`은 대비용으로 남긴다.
- 쥐는 각도: 첫 값은 "손바닥에서 앞으로 비스듬히" 정도로 넣고, 플레이 모드에서 칩비 `SocketProfile`의 회전 보정을 눈으로 맞춘다 (값은 데이터라 코드 수정 없음).

### 7-2. 인벤토리 연결과 낚시·물 주기
- `InventoryEquipmentLink`: 선택 아이템이 `ToolDefinition`이고 `_attachment`가 있으면 장착, 아니면 그 소켓 해제.
- 확인한 사실: 물 주기(`FarmService` 85행)와 낚시(`PondArea` 23행)는 **지팡이를 선택했을 때만** 된다 → 이 기능들을 쓸 때는 항상 지팡이가 손에 있다.
- 낚시 중 다른 칸으로 바꿔 지팡이가 사라지는 경우: `ICastOrigin`이 "끝 없음"을 돌려주고 `FishingDriver`는 기존 `WandTip`으로 돌아간다 (에러 없이 동작, 줄 시작점만 몸 쪽으로 이동). 이 동작이 싫으면 알려 달라.
- 동작 애니메이션은 이번 단계에서 다루지 않는다. 지팡이는 손 본의 자식이므로 나중에 Cast·물 주기 클립이 들어와도 추가 코드 없이 손을 따라간다.

### 7-3. 농장 적용
- 4-4 메뉴 그대로 (`VoxelMagic/캐릭터/농장 플레이어에 소켓·장착 붙이기`, 씬 저장은 사용자가).
- 완료 조건 = 4-6 + "물 주기·낚시가 손에 든 지팡이로 그대로 동작".

### 7-4. 애니메이션 랩 적용
- 랩은 아바타를 바꿀 때마다 모델을 새로 `Instantiate`한다(`AnimationLab.SelectAvatar`) → 새 모델이 생길 때마다 `HumanoidSocketSource`·`SocketHost`·`CharacterEquipment`를 붙이고 **이전 장착 상태를 그대로 다시 적용**(아바타를 바꿔도 같은 지팡이를 들고 있음). 아바타의 `SocketProfile`이 비어 있으면 상태 줄에 "이 아바타는 소켓 프로필이 없음"을 표시.
- 게임 기능(인벤토리·물 주기·낚시)은 붙이지 않는다. `InventoryEquipmentLink`도 붙이지 않는다.
- **장착물 패널 (접이식)**: 제목 줄을 누르면 펼치고 접힘.
  - 장착물 목록: "없음" + 프로젝트의 모든 `AttachmentDefinition` (에디터에서는 자동 검색, 빌드용 예비 목록은 `AnimationLabConfig`) — 고르면 그 장착물의 소켓에 장착, "없음"이면 해제
  - 표시 토글: 장착물 보이기/숨기기 (단축키는 기존 단축키와 겹치지 않게 정함)
  - 현재 장착 정보: `소켓: 오른손 · 장착: 임시 지팡이`
- **모든 패널 접이식 (결정)**: 접이식 패널 부품 하나(`LabFoldout` — 제목 줄 버튼 + 내용 영역 켜고 끄기 + 화살표 ▶/▼, 펼침 상태는 랩 실행 중 유지)를 만들고 장착물 패널과 기존 패널(아바타·클립 목록·재생·카메라·도트 렌더링 등) 모두에 쓴다. 패널이 접히면 아래 패널이 위로 올라오도록 세로 레이아웃(`VerticalLayoutGroup` + `ContentSizeFitter`)으로 묶는다. 처음 펼침 상태는 `AnimationLabConfig`에서 정한다.
- **랩 씬 덮어쓰기 (결정: 허용)**: 랩 UI는 `AnimationLabSceneBuilder`(메뉴 "애니메이션 랩 씬 다시 만들기")가 씬째 새로 만든다. 빌더를 고친 뒤 이 메뉴로 `Scenes/Tools/AnimationLab.unity`를 다시 만든다. 다시 만든 뒤 `git diff`로 씬 변경을 보고, UI 말고 사라진 것이 있으면 보고한다 (커밋돼 있어 되돌릴 수 있음).
- 어셈블리 (결정: 허용): 랩이 농장과 **같은** 소켓·장착 부품(`HumanoidSocketSource`·`SocketHost`·`CharacterEquipment`, Gameplay 어셈블리)을 그대로 쓰도록 `VoxelMagic.Tools.asmdef` 참조에 `VoxelMagic.Gameplay`를 추가한다 (Tools → Gameplay → Core, 순환 없음. 게임 코드는 Tools를 모름). 랩 전용 소켓 코드는 만들지 않는다. 변경은 `SETUP.md`·`CHANGELOG.md`에 기록.

### 7-5. 제작 순서 (단계 1)
1. Core: `SocketId`·`SocketProfile`·`AttachmentDefinition`·`EquipmentState` + `AvatarSetup`/`ToolDefinition` 필드 추가 + 단위 테스트 → 테스트 실행
2. Gameplay: 소켓·장착 부품 + `FishingDriver` 변경 → 컴파일 확인
3. 데이터·프리팹 (`Wand_Temp`, 칩비 프로필, 정의) + 농장 적용 메뉴 → **농장 플레이 확인 → 보고 → 사용자 확인**
4. 애니메이션 랩 장착물 패널 → **랩 플레이 확인 → 보고 → 사용자 확인**
5. `PrototypeSceneBuilder` 수정(실행 안 함), `CHANGELOG.md`·`SETUP.md`(asmdef 참조 변경 기록)·`CHARACTER_GUIDE.md`

---

## 8. 소켓 기준 본 원칙 (2026-11-06 결정)
- 이 게임의 사람형·크리처 아바타는 모두 **칩비형(팔·다리가 한 조각, 어깨·다리에 틈)**으로 만든다 (사용자 결정).
- 칩비형은 팔 메시가 위팔 본에만 붙어 있어 팔꿈치·손목 본의 회전이 화면에 보이지 않는다. 손 본에 소켓을 달면 그 보이지 않는 회전을 장착물만 따라가 Run 같은 활발한 동작에서 크게 어긋난다 (측정: Run 124.5°/32.3cm).
- 그래서 **소켓은 "보이는 손이 실제로 따라가는 본"에 단다** — 칩비형 손 소켓은 `RightUpperArm`/`LeftUpperArm`. 보정 값은 Idle에서 주먹 위치에 오도록 잡는다.
- 새 아바타 소켓 프로필 자동화("어떤 FBX든") 때: 손 근처 정점이 붙은 본을 찾아 기준 본으로 고르면 칩비형·일반 리깅 모두 처리된다.
