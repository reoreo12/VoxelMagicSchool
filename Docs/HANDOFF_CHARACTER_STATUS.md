# 인수인계 — 캐릭터 시스템 현재 상태 (2026-11-11)

> 새 대화는 이 문서와 `CLAUDE.md`부터 읽는다. 설계·결정의 자세한 내용은 `Docs/DESIGN_CHARACTER.md`, 날짜별 변경은 `Docs/CHANGELOG.md`.
> 처음 계획서였던 `HANDOFF_CHARACTER_SYSTEMS.md`는 2026-11-06 기준이라 **이 문서가 더 최신**이다. 저장 슬롯 확장은 `HANDOFF_SAVE_LOAD.md`.

## 0. 사용자·작업 방식
- 자체 엔진 경험 있음, Unity는 배우는 중 → Unity 방식을 쓰던 개념(Container/Part 등)과 **비교해서** 쉽게. 더 나은 Unity·객체 지향·최적화 방식이 있으면 제안 → 수락하면 만든다
- 답변은 한국어. 설계 먼저 → 사용자 "제작 시작" 뒤에만 스크립트·씬·에셋. 기능 하나마다 플레이 확인 → 확인한 것/추측을 나눠 보고 → 사용자 확인
- 커밋·푸시는 사용자가 요청할 때만(사용자가 직접 커밋하는 경우가 많음, 메시지 초안을 부탁하기도 함). 커밋 전 브랜치 확인. 작업 브랜치: `feature/character-system`
- 기존 파일 덮어쓰기·지우기, `Assets/_Project/`·`Docs/` 밖 수정 전에는 묻는다. 메뉴 `VoxelMagic/프로토타입 씬 다시 만들기`는 실행 금지 (씬 생성 도구는 고쳐 두되 실행 안 함)

## 1. 지금 있는 것
| 영역 | 상태 | 핵심 |
|---|---|---|
| 소켓·장착 | 완성 | `SocketId`·`SocketProfile`·`AttachmentDefinition`·`EquipmentState`(Core) / `SocketHost`·`CharacterEquipment`·`HumanoidSocketSource`(Gameplay). 칩비형 손 소켓은 **위팔 본 기준**(팔 메시가 위팔에만 붙어 있어서, 설계 8번). 플레이 중 프로필·장착물 값 바꾸면 바로 반영(에디터 전용) |
| 지팡이 | 완성 | `Prefabs/Attachments/Wand_Temp`(메시 `Art/Objects/Wand/Magic_Staff_v2.fbx`, 길이 0.58), `Attachment_WandTemp`, 지팡이 칸 고를 때만 손에 듦, 낚싯줄 시작점 = 지팡이 끝 |
| 플레이어 데이터 유지·저장 | 완성 (슬롯 0) | `PlayerProgress`·`GameSession`·`SaveData`·`SaveMapper`·`DefinitionCatalog`(Core) / `GameSaveSystem`·`JsonFileSaveStore`(Gameplay). 씬 이동·종료 때 저장, 밭 포함. 슬롯 선택은 `HANDOFF_SAVE_LOAD.md` |
| 이동 | 완성 | 복도·텃밭 공용 `LocomotionStateMachine`(Idle/Walk/Run) + 이동기 `ICharacterMotor`(복도 `SideScrollMotor`, 3D 맵 `PlanarMotor`). 텃밭 Shift 달리기, 이동은 카메라 Yaw 기준 |
| 이동 애니메이션 | 완성 | `CharacterAnimatorView`(상태 바뀔 때만 크로스페이드, 복도·텃밭 공용), `AvatarAnimationSpeed`(아바타 값 × 장면 배율) |
| 동작 애니메이션 | 완성 | 공용 `Animations/Avatars/Base/CharacterBase.controller` + 아바타별 `.overrideController`. 동작: 던지기·기다리기·올리기 왕복·낚아채기·물 주기. `CharacterActionView`(낚시 상태·물 주기 → Animator), `CharacterAnimationEvents`(클립 이벤트 → 게임 로직: CastRelease·WaterApply) |
| 스탯 | **없음 (보류)** | 새 캐릭터 추가·전투 시스템이 생길 때 (사용자 결정). 들어갈 자리: `PlayerProgress` |
| NPC·몬스터 조종 분리 | **없음 (보류)** | 설계 5-3 |

## 2. 사용자가 조절하는 값 (모두 데이터 에셋, 플레이 중 바꾸면 대부분 바로 반영)
| 무엇 | 에셋 |
|---|---|
| 3D 맵(복도 제외) 걷기·달리기 이동 속도, 가속·감속, 이동 애니 재생 배율 | `Data/Config/MoveConfig` (사용자 값: 걷기 3·달리기 4.5, 배율 1.6·1.6) |
| 복도 이동 | `Data/SideScroll/CharacterMoveConfig` |
| 이동 애니 섞기 시간 | `Data/SideScroll/CharacterAnimationConfig` Blend Seconds |
| 동작 재생 속도·섞기·안전 시간 | `Data/Config/CharacterActionConfig` (던지기 2·올리기 1·낚아채기 2.5·물 주기 1) |
| 클립 자르기·이벤트 순간·뒤집기 | `Data/Avatars/ClipImportPlan` → 메뉴 `VoxelMagic/아바타/동작 클립 준비` 다시 실행 (던지기 0~120 CastRelease 0.52, 올리기 120~끝, 물 주기 84~168 WaterApply 0.4) |
| 동작 이어짐 섞기 | `CharacterActionConfig` Chain Blend → 메뉴 `VoxelMagic/아바타/공용 Controller 다시 만들기` |
| 소켓 위치·각도 / 장착물 놓는 위치 | `Data/Characters/SocketProfiles/SocketProfile_Chibi` / `Data/Characters/Attachments/Attachment_WandTemp` |
| 찌 날아가는 시간·놓는 신호 대기 | `Data/Config/FishingConfig` Cast Seconds · Cast Release Timeout |
| 카메라 각도(이동 방향도 따라감) | `Data/Config/PlayerConfig` Camera Yaw/Pitch (사용자가 시험 중) |

## 3. 메뉴 (VoxelMagic/…)
- `아바타/동작 클립 준비 (ClipImportPlan)` · `아바타/공용 Controller 다시 만들기 (모든 아바타)` · `아바타/1~3`(모델 준비·Humanoid 임포트·복도 캐릭터, 3은 이제 Override를 만듦)
- `캐릭터/텃밭 플레이어 이동·애니 통일`(MoveConfig·애니 뷰·Override·동작 뷰 연결) · `캐릭터/농장 플레이어에 소켓·장착 붙이기` · `캐릭터/기본 소켓·지팡이 데이터 만들기`
- `저장/정의 목록 채우기(+부트스트랩 연결)` · `저장/저장 폴더 열기` · `저장/저장 지우기(슬롯 0)`

## 4. 남은 일·알아 둘 점
- **어떤 FBX든 장착·애니가 되게** (사용자 목표): 새 아바타 소켓 프로필 자동 생성(손 근처 정점이 붙은 본을 기준 본으로), Student_Male 계열 프로필 없음(랩에서 안내만)
- 사람형·크리처 모두 **칩비형(팔·다리 한 조각)**으로 만들 예정 → 소켓은 위팔 기준이 기본
- 획득 정보 UI가 낚아채기 동작과 동시에 뜸 (속도는 올렸음). 원하면 낚아채기가 끝난 뒤 UI를 띄우는 방식 제안 가능
- 벽에 막혀도 키를 누르면 걷기 애니가 계속됨 (입력 기준 상태, 복도와 같은 규칙)
- `CharacterMoveConfig`에 공용 값과 복도 전용 값이 섞여 있음 → 3D 맵이 더 생기면 나눌 것
- `PlayerConfig`의 Move Speed·Walk Animation Reference Speed·Walk Start Speed·Animation Blend Seconds는 텃밭에서 더 이상 안 씀 (옛 `PlayerAvatarAnimator`는 꺼 둠, Prototype 씬이 아직 사용)
- `Student_Chibi_v2.controller`(옛 방식)는 Prototype·TileMapEditor 씬이 써서 남겨 둠
- 애니메이션 랩 목록에서 긴 클립 이름이 두 줄로 겹쳐 보임 (표시만)
- 새 아이템·의뢰를 만들면 `저장/정의 목록 채우기` 다시 실행

## 5. Claude가 확인할 때의 요령 (이전 대화에서 겪은 것)
- MCP: `set_active_instance VoxelMagicSchool@2d926515` 후 `Application.dataPath`로 프로젝트 확인. 연결이 끊기면 다시 설정
- **MCP가 같은 `execute_code`를 두 번 실행하는 일이 잦다** → 상태를 바꾸는 시험 코드는 `SessionState` 표시로 중복 실행을 막는다
- 에디터에 포커스가 없으면 가상 키가 입력 액션에 안 들어가고 프레임도 느리다 → 플레이 모드에서 일시정지 후 `Animator.Update`·세션 `Tick`·뷰 `Update`(리플렉션)를 0.05초씩 직접 진행해 확인. 실제 키 조작은 사용자에게 부탁
- `execute_code`는 C# 6(CodeDom): 지역 함수·튜플 안 됨(`System.Func` 사용), `in` 매개변수는 `ref`로 넘김. `AssetDatabase.DeleteAsset`은 막혀 있음 → 지울 때는 사용자 승인 후 `git rm` + 새로 고침
- 플레이를 끄면 저장 파일이 생기므로 시험 뒤 `JsonFileSaveStore().Delete(0)`로 지운다
- 씬을 바꾸기 전 `isDirty` 확인, 씬 저장 뒤 `git diff`로 의도한 변경만 있는지 확인
