# 인수인계 — 캐릭터 공용 시스템 (소켓·장착물 · 애니메이션 · 스탯)

> 작성: 2026-11-06 (이전 대화). 새 대화는 이 문서와 `CLAUDE.md`부터 읽는다.
> 이 문서는 **추천 구조 제안서**다. 새 대화는 이 제안을 바탕으로 설계 문서 `Docs/DESIGN_CHARACTER.md`를 쓰고, 사용자가 **"제작 시작"**이라고 해야 만든다.
> 관련 문서: `Docs/DESIGN.md`(프로토타입 1, 12번에 필요한 애니메이션 목록), `Docs/DESIGN_CORRIDOR.md`(프로토타입 2), `Docs/AVATAR_GUIDE.md`, `Docs/SETUP.md`, `Docs/CHANGELOG.md`

---

## 0. 사용자에 대해 (설명 방식)
- 사용자는 **자체 엔진을 만들어 본 경험**이 있고 **Unity는 새로 배우는 중**이다. Unity 용어·방식은 쉬운 말로, 사용자가 쓰던 개념과 **비교해서** 설명한다 (1-2 표 참고)
- 사용자는 자체 엔진 방식을 예로 들 뿐 그대로 쓰라는 뜻이 아니다. **Unity·객체 지향·최적화 관점에서 더 나은 방식을 제안하고 설명 → 사용자가 수락하면 그대로 만든다**
- 답변은 한국어. 처음 나오는 전문 용어는 한 줄로 풀어 쓴다

## 1. 목표
### 1-1. 무엇을 만드나
**플레이어·NPC·몬스터·등장 캐릭터가 똑같이 쓰는 캐릭터 공용 시스템.** 같은 부품을 붙이고 데이터만 바꿔 서로 다른 캐릭터를 만든다.
1. **소켓·장착물**: 캐릭터 몸의 지정 위치(오른손 등)에 물건(지팡이, 이후 다른 무기·소품·이펙트)을 붙인다. **첫 결과물: 농장 맵 플레이어 오른손에 임시 도형 지팡이**
2. **애니메이션**: 지금은 Idle/Walk/Run만 있음 → 동작(던지기·물 주기·심기 등) 애니메이션을 같은 방식으로 추가할 수 있는 구조
3. **스탯**: 플레이어와 같은 방식으로 몬스터·NPC도 쓰는 스탯(체력 등) 시스템
- 순서·범위는 5번 단계표. 단계마다 설계 → 승인 → 제작 → 플레이 확인 → 보고

### 1-2. 사용자의 자체 엔진 구조와 Unity 방식 비교
사용자 엔진: `GameObject` → `ContainerObject`(Player) / `PartObject`(Body·Weapon·Effect). 무기(Part)가 플레이어(Container)의 바디(Part) 소켓 위치(손)를 **Get 함수로 받아** 붙음.

| 사용자 엔진 | Unity 추천 | 차이와 이유 |
|---|---|---|
| 매 프레임 소켓 위치를 Get으로 받아 무기 위치 갱신 | 무기를 손 본(bone) 아래의 **자식 Transform으로 한 번 붙임** | Unity의 Transform은 부모-자식 계층이라, 애니메이션으로 손이 움직이면 자식도 엔진이 자동으로 따라 움직인다. 매 프레임 코드가 필요 없다(가볍고 떨림 없음). Get은 "붙일 Transform을 한 번 알려 주기"로만 쓴다 |
| `ContainerObject` → `Player` 상속 | 캐릭터 루트에 **역할 컴포넌트들을 조립** (소켓 관리, 장착, 스탯, 이동, 조종) | Unity는 모든 것이 이미 `GameObject`. Player·NPC·몬스터를 클래스로 나누지 않고 **어떤 컴포넌트를 붙이고 어떤 데이터를 넣느냐**로 나눈다 (상속보다 조합) |
| `PartObject` → `Body`/`Weapon`/`Effect` 상속 | Body = 모델 + "소켓 찾기" 인터페이스, Weapon·Effect = **장착물 기반 클래스**의 하위 | 소켓 찾는 방식(사람형 뼈/몬스터 임의 위치)은 **인터페이스로 다형성**, 장착물 공통 동작은 **추상 기반 클래스** |
| 캐릭터별 값이 코드에 | **ScriptableObject 데이터** (캐릭터 정의·소켓·장착물·스탯) | 새 몬스터·무기를 코드 없이 데이터로 추가 |

## 2. 프로젝트 규칙 (CLAUDE.md 요약 + 사용자 결정)
- **설계 먼저**: `Docs/DESIGN_CHARACTER.md`에 설계 → 설명 → 사용자가 "제작 시작"이라고 한 뒤에만 스크립트·씬·에셋을 만든다
  - CLAUDE.md의 작업 표에 이 작업 줄을 추가하려면 CLAUDE.md가 허용 폴더 밖이므로 **먼저 묻는다**
- 설계 원칙: 캡슐화(private 필드 + 읽기 전용 공개, 상태 변화는 이벤트), 상속(공통 동작은 추상 기반 클래스), 다형성(인터페이스로 통일), 데이터는 ScriptableObject, 핵심 로직은 일반 C# 클래스(단위 테스트)
  - **상속 깊이 (2026-11-06 사용자 결정)**: 기본은 2단계 이하. **구조상 다른 방도가 없고 정말 필요하며 객체 지향을 크게 해치지 않으면 3단계도 허용** — 이때는 설계 문서에 이유를 적고 설명한다
- 의존 방향: `UI → Core`, `Gameplay → Core`. Core는 다른 어셈블리를 참조하지 않는다 (Core에서 `Transform`·`Animator` 같은 Unity 씬 객체 대신 값·식별자·이벤트만 다룸. `UnityEngine` 기본 타입·ScriptableObject는 지금도 Core에서 사용 중)
- 스크립트 맨 위 용도 주석(1~3줄), 숫자는 설정·데이터에서, UI 문구는 한국어(한글 폰트 확인)
- 작업 파일은 `Assets/_Project/`, `Docs/` 안에만. 패키지 설치·외부 다운로드·프로젝트 설정 변경·위 폴더 밖 수정 전에는 묻는다. 설정을 바꾸면 `SETUP.md`·`CHANGELOG.md`에 기록
- 기능 하나 끝날 때마다 플레이 모드 확인 → 짧게 보고 → 사용자 확인. 커밋·푸시는 사용자가 요청할 때만, 커밋 전 지금 브랜치 확인
- 브랜치: 이 작업용 브랜치는 **사용자가 만든다** (새 대화는 만들지 말고 시작할 때 이름만 확인)

## 3. 확인된 사실 (2026-11-06, 이전 대화에서 확인)
### 3-1. 아바타·본
| 항목 | 값 |
|---|---|
| 플레이어 아바타 | `Student_Chibi_v2` (`Art/Characters/Student_Male/Model/Student_Chibi_v2.fbx`, Humanoid). 데이터 `Data/Avatars/Student_Chibi_v2` (`AvatarSetup`: 모델·목표 키 1.6·Idle/Walk/Run 클립·Controller·복도 프리팹·2D 스프라이트) |
| Humanoid 손 본 | 칩비: `RightHand`·`LeftHand`·`RightForeArm` **있음**, **Neck 없음** / Student_Male_v05: 모두 있음 → `Animator.GetBoneTransform(HumanBodyBones.RightHand)`로 두 모델 모두 같은 코드로 손을 찾을 수 있다 |
| 칩비 특성 | 팔·다리가 한 조각(팔꿈치·무릎 안 굽음), 손이 큼 → 같은 지팡이라도 아바타마다 잡는 위치 보정이 다름 |
| 크기 | 농장 플레이어 Model 배율 1(키 약 1.63), 복도 칩비 프리팹 Model 배율 0.8. 1복셀 ≈ 0.067 m |

### 3-2. 지금 지팡이·낚싯줄 (농장 맵)
- `Editor/PrototypeSceneBuilder.BuildPlayer`가 플레이어 루트에 **몸에 고정된 상자 `Wand`**(로컬 (0.35, 1.15, 0.4), 크기 0.05×0.05×0.58, X −31°)와 **`WandTip`**(로컬 (0.35, 1.3, 0.65))을 만든다
- 낚시 `Gameplay/Fishing/FishingDriver._wandTip`이 이 `WandTip`을 참조해 낚싯줄 시작점·던지기 시작점으로 쓴다 → **지팡이를 손으로 옮기면 낚싯줄 시작점도 손에 든 지팡이 끝을 따라가야 한다**
- 지팡이는 데이터 `Items/Tool_Wand`(`ToolDefinition`, 인벤토리 1번 칸)이고, 물 주기·낚시 조건에 쓰인다
- ⚠ **농장 씬(`Scenes/Prototype_FarmFishing.unity`)은 사용자가 직접 고친 씬**(연못 FBX·콜라이더 등). 메뉴 `VoxelMagic/프로토타입 씬 다시 만들기`를 실행하면 사라진다 → **실행 금지**. 씬에 붙이는 방법은 "지금 씬의 플레이어에 컴포넌트·소켓을 붙이는 에디터 메뉴" 또는 "사용자가 따라 할 수동 순서"로 설계한다 (씬 생성 도구도 함께 고쳐 두되 실행은 하지 않음)

### 3-3. 지금 캐릭터 관련 코드 (재사용·정리 대상)
| 위치 | 내용 |
|---|---|
| `Gameplay/Player/PlayerMover` | 카메라 기준 WASD 이동 (플레이어 입력 전용) |
| `Gameplay/Player/PlayerInteractor` | 가까운 상호작용 대상 안내, E 처리 |
| `Gameplay/Player/PlayerAvatarAnimator` | 움직이면 Walk, 멈추면 Idle (농장) |
| `Core/SideScroll/CharacterStateMachine`·`CharacterStates` | 복도 캐릭터 상태 패턴 (Idle/Walk/Run) |
| `Gameplay/SideScroll/SideScrollCharacter`·`CharacterAnimatorView`·`AvatarAnimationSpeed`·`CharacterViewSwitcher`·`SpriteFrameAnimator` | 복도 캐릭터 (3D/2D 전환 포함) |
| `Core/Avatars/AvatarSetup`·`SpriteCharacterSet` | 아바타 데이터 (모델·클립·재생 속도·2D) |
| `Editor/Avatars/AvatarPipeline` | 메뉴 `VoxelMagic/아바타/` 1~3 (모델 준비·애니메이션 임포트·복도 캐릭터 만들기) |
| 스탯 | **아직 없음** (`ScoreBook`의 실기·필기 점수는 학교 점수라 캐릭터 스탯과 별개) |

### 3-4. 애니메이션 (DESIGN.md 12번)
- 지금 클립: Mixamo Idle·Walking·Standard Run (Humanoid, 반복). 메뉴 2는 **모든 클립을 반복**으로 임포트 → 한 번 재생 클립 규칙 필요
- 필요한 동작 목록(순위 포함)은 DESIGN.md 12-1: 던지기(Cast)·낚시 대기·감기(Reel)·잡음·물 주기(Spell)·심기·뽑기·실망·놀람·건네기·대화·읽기
- 애니메이션 랩: `Scenes/Tools/AnimationLab.unity` (어셈블리 `VoxelMagic.Tools`) — 새 아바타·클립 확인용

### 3-5. 렌더링·기타
- Unity 6000.0.38f1, URP 17.0.3, Forward+. MCP 연결: `set_active_instance VoxelMagicSchool@2d926515` 후 `Application.dataPath` = `C:/Unity_Project/NCAI/VoxelMagicSchool/Assets` 확인 (컴파일·플레이 진입 뒤마다 다시)
- 에디터에 포커스가 없으면 가상 키 입력이 안 들어간다 → 상태·입력 값을 코드로 직접 넣고 `EditorApplication.Step()`으로 한 프레임씩 진행해 확인, 실제 조작은 사용자 플레이로
- `execute_code`는 `autoReferenced: false`인 어셈블리(`VoxelMagic.Tools` 등)의 타입을 이름으로 직접 못 쓴다 → 리플렉션·타입 이름 문자열로
- 씬 생성 도구는 새 씬을 연 **뒤에** 에셋을 읽는다 (먼저 읽으면 참조가 풀림)

## 4. 추천 구조 (제안 — 설계 문서에서 다듬고 사용자 승인)
### 4-1. 큰 그림
```
캐릭터 루트 GameObject (Player / NPC / Monster 모두 같은 틀)
├─ [CharacterRoot]      캐릭터 정의 데이터 연결, 부품들을 한 번 연결해 주는 조립 지점 (God 객체 아님)
├─ [조종: ICharacterBrain]  PlayerBrain(입력) / 나중에 NpcBrain·MonsterBrain(AI) → 이동·행동 "의도"만 낸다
├─ [이동]                 의도 → 실제 이동 (지금 PlayerMover를 조종과 분리)
├─ [SocketHost]           소켓 목록·장착 관리, 붙음/떨어짐 이벤트
├─ [EquipmentView]        Core의 장착 상태를 보고 장착물 프리팹을 소켓에 붙이고 뗌
├─ [StatsHolder]          Core의 StatBlock을 갖고 이벤트를 UI·연출에 전달
└─ Model (FBX)            [Animator] [ISocketSource 구현] [애니메이션 뷰]
     └─ …RightHand 본
          └─ Socket_RightHand (실행 중 생성, 위치·회전 보정 적용)
               └─ Wand (장착물 프리팹) [WandAttachment] → Tip
```
- Player·NPC·몬스터의 차이 = **조종 컴포넌트 + 데이터**(캐릭터 정의·스탯 프로필·기본 장착물). 클래스 상속으로 나누지 않는다

### 4-2. 소켓·장착물
| 이름 | 종류 | 어셈블리 | 역할 |
|---|---|---|---|
| `SocketId` | ScriptableObject (빈 식별자 에셋) | Core | 소켓 이름표 (`RightHand`, `LeftHand`, `Back`, `Head`…). 문자열 오타 없이, 코드 수정 없이 늘릴 수 있음 |
| `SocketProfile` | ScriptableObject | Core | 아바타별 소켓 목록: 소켓 ID → (사람형 본 이름 또는 직접 둔 Transform 이름) + 위치·회전 보정. `AvatarSetup`이 참조 (아바타마다 손 크기·각도가 달라서) |
| `AttachmentDefinition` | ScriptableObject | Core | 장착물 데이터: 프리팹, 붙을 소켓 ID, 잡는 위치 보정. 아이템(`ToolDefinition` 등)이 선택적으로 참조 → "지팡이 아이템을 들면 지팡이 모양이 손에" |
| `EquipmentState` | 일반 C# | Core | 소켓 ID → 장착 중인 장착물. `Equip/Unequip`, `Changed` 이벤트. 인벤토리 선택과 연결. **단위 테스트** |
| `ISocketSource` | 인터페이스 | Gameplay | "이 소켓 ID의 Transform을 줘". 구현: `HumanoidSocketSource`(Animator `GetBoneTransform`), `TransformSocketSource`(몬스터처럼 모델에 직접 둔 빈 오브젝트) — **다형성 지점** |
| `SocketHost` | MonoBehaviour | Gameplay | 시작할 때 프로필대로 소켓 Transform을 **한 번** 만들고 캐시. 장착물을 자식으로 붙이고 떼기. 이벤트 |
| `AttachmentBase` | 추상 MonoBehaviour | Gameplay | 장착물 공통: 붙을 때·뗄 때 처리, 소유 캐릭터 참조 |
| `WandAttachment` | `AttachmentBase` 하위 | Gameplay | 지팡이 끝(`Tip`) 제공 → 낚시가 `WandTip` 대신 **장착된 지팡이의 끝**을 쓰게 (예: `IHasTip` 인터페이스로 조회) |
- 상속: `AttachmentBase` → `WandAttachment`(1단계). 이펙트 등이 더 생겨도 1단계로 충분할 것으로 예상
- **첫 결과물**: 임시 도형 지팡이 프리팹(기본 도형 + 끝 위치) + 칩비 `SocketProfile`(오른손 보정) → 농장 플레이어 오른손에 붙어 걷기 애니메이션에서 손과 함께 움직임, 낚싯줄이 지팡이 끝에서 시작

### 4-3. 애니메이션 (뒤 단계)
- 기본 이동(Idle/Walk/Run)은 그대로, 그 위에 **동작 레이어**(상체만 움직이는 Avatar Mask 사용 검토)로 "한 번 재생" 동작
- `CharacterActionId`(Core, ScriptableObject 또는 enum — 설계 때 결정) + `AvatarSetup`에 동작 → 클립 목록
- 게임 로직은 "동작 요청" 이벤트만 내고, 애니메이션 뷰가 재생. 타이밍이 필요한 곳(던지는 순간)은 애니메이션 이벤트 → 게임 로직 전달
- 임포트 규칙: 한 번 재생 클립은 반복 끄기 (파일 이름 규칙 또는 데이터)
- Animator 파라미터는 해시(`Animator.StringToHash`)로 한 번 계산해 쓴다

### 4-4. 스탯 (뒤 단계)
| 이름 | 종류 | 역할 |
|---|---|---|
| `StatDefinition` | ScriptableObject (Core) | 스탯 종류: 한국어 이름, 최소·최대·기본값, 자원형(현재/최대, 예: 체력)인지 |
| `StatProfile` | ScriptableObject (Core) | 캐릭터별 기본 스탯 묶음 (플레이어·몬스터 A·NPC B…) |
| `StatBlock` | 일반 C# (Core) | 실행 중 값: 기본값 + 보정치(더하기·곱하기, 출처·지속 시간), `Changed` 이벤트. **플레이어·몬스터 모두 같은 클래스** — 단위 테스트 |
| `StatsHolder` | MonoBehaviour (Gameplay) | 프로필로 `StatBlock`을 만들고 다른 부품에 제공 |
- 어떤 스탯이 필요한지(체력·마나·기력 등)는 게임 설계에 달렸으니 **사용자에게 묻는다**

### 4-5. 최적화 원칙
- 소켓·본은 시작할 때 한 번 찾고 캐시, 장착은 부모 지정 한 번 (매 프레임 위치 계산·`GetComponent` 없음)
- 상태 변화는 이벤트로 (매 프레임 확인하지 않음)
- 데이터(ScriptableObject)는 여러 캐릭터가 공유, 실행 중 값만 캐릭터마다
- 자주 생기고 사라지는 이펙트는 나중에 오브젝트 풀 검토

## 5. 단계 (각 단계: 설계 → 승인 → 제작 → 플레이 확인 → 보고 → 사용자 확인)
| 단계 | 내용 | 결과 확인 |
|---|---|---|
| 0 | 설계 문서 `Docs/DESIGN_CHARACTER.md` (4번 제안 다듬기, 사용자 질문 정리) | 사용자 "제작 시작" |
| 1 | 소켓·장착물 Core·Gameplay + 단위 테스트 + 임시 도형 지팡이 + 칩비 소켓 프로필 → **농장 플레이어 오른손에 지팡이**, 낚싯줄을 지팡이 끝에서 (지금 씬에 붙이는 메뉴/순서) | 플레이: 걷기 중 지팡이가 손을 따라감, 낚시 줄 시작점, 물 주기·낚시 기존 동작 그대로 |
| 2 | 같은 부품을 복도 칩비(3D)·애니메이션 랩 미리보기에도 적용 검토 (사용자 요청 시) | 3D/2D 전환 시 지팡이 숨김 등 |
| 3 | 스탯 Core + 캐릭터 정의 데이터 (+ 필요하면 간단한 표시) | 단위 테스트, 플레이에서 값 변화 이벤트 |
| 4 | 동작 애니메이션 구조 (DESIGN.md 12-1 순위 1부터) | 랩·농장에서 동작 재생 |
| 5 | NPC 또는 몬스터 예시 하나에 같은 부품 적용 (조종만 다름) | 같은 지팡이·스탯 재사용 확인 |

## 6. 사용자에게 물을 것 (설계 단계)
- 소켓 이름 목록 (처음엔 `RightHand`만? `LeftHand`·`Back`·`Head`도?)
- 지팡이 모양·크기 (임시 도형: 길이·굵기·색), 손에 쥐는 각도 기준
- 스탯 종류 (3단계 전에)
- 장착을 인벤토리 선택과 연결할지 (지팡이 칸을 고르면 손에 듦 / 항상 듦)
- 첫 적용 범위: 농장만 / 복도도

## 7. 새 대화 첫 메시지 예시
```text
Docs/HANDOFF_CHARACTER_SYSTEMS.md와 CLAUDE.md를 먼저 읽고 시작하자. 이번 대화에서는 플레이어·NPC·몬스터가 함께 쓰는 캐릭터 공용 시스템(소켓·장착물, 애니메이션, 스탯)을 진행할 거야. 첫 목표는 농장 맵 플레이어 오른손 소켓에 임시 도형 지팡이를 붙이는 거야.

작업 브랜치는 내가 만들어 뒀어. 브랜치는 새로 만들지 말고, 커밋·푸시는 내가 요청할 때만 해 줘.

나는 자체 엔진 경험은 있지만 Unity는 배우는 중이야. 문서 4번의 추천 구조를 내 자체 엔진 구조(Container/Part)와 비교해서 쉽게 설명해 주고, Unity·객체 지향·최적화 관점에서 더 좋은 점이 있으면 제안해 줘.

순서:
1. MCP 연결과 지금 브랜치를 확인해서 알려 줘.
2. 문서 3번 사실(칩비 손 본, WandTip·FishingDriver 참조)을 다시 확인해 줘.
3. 설계를 Docs/DESIGN_CHARACTER.md에 정리하고, 문서 6번 질문을 나한테 물어봐 줘.

지킬 것:
- 내가 "제작 시작"이라고 하기 전에는 스크립트·씬·에셋을 만들지 마.
- 메뉴 "VoxelMagic/프로토타입 씬 다시 만들기"는 실행하지 마. 내가 직접 고친 농장 씬이 사라져.
- 기존 파일을 덮어쓰거나 지우기 전, 그리고 Assets/_Project/와 Docs/ 밖을 바꾸기 전에는 먼저 물어봐.
- 기능 하나가 끝날 때마다 플레이 모드로 확인하고, 확인한 것과 추측을 나눠서 보고해 줘.
```
