# 인수인계 — 복도 캐릭터를 칩비(Student_Chibi_v2)로 교체

> 작성: 2026-11-04 (이전 대화 마무리). 새 대화에서 이 문서부터 읽고 시작한다.
> 프로젝트 규칙: `CLAUDE.md` / 이 작업의 지침·설계: `CORRIDOR_SIDESCROLL_SPEC.md`, `Docs/DESIGN_CORRIDOR.md`(12·13번), 메뉴 사용법: `Docs/AVATAR_GUIDE.md`

## 1. 이어갈 작업
복도 씬(`Assets/_Project/Scenes/Corridor_SideScroll.unity`)의 플레이어를 **Student_Chibi_v2**로 바꾼다. 사용자가 메뉴 `VoxelMagic/아바타/` 1 → 2 → 3 순서로 직접 적용해 보고, 막히면 MCP로 함께 확인한다.

## 2. 지금 상태 (2026-11-04 확인)
| 항목 | 상태 |
|---|---|
| 작업 브랜치 | `feature/prototype-corridor` (로컬 = 원격 `4d910ca`) |
| 랩 브랜치 | `feature/tool-animation-lab` (로컬 = 원격 `f3c1c48`, 사용자가 복도 브랜치를 병합함) |
| 칩비 파일 | `Art/Characters/Student_Male/Model/` 안에 `Student_Chibi.fbx`, `Student_Chibi_v2.fbx`, `chibi_palette.png` (Student_Male과 **같은 폴더**) |
| 칩비 아바타 데이터 | `Data/Avatars/Student_Chibi`, `Student_Chibi_v2` 이미 있음 (애니메이션 랩이 자동 준비). 키 1.6, Idle/Walking/Standard Run 연결됨, **Controller·복도 프리팹은 아직 없음**, 2D 스프라이트 없음 |
| v2 임포트 | Humanoid, 배율 1.066667. Hips 매핑을 `Student_Armature` → `Hips`로 **사용자가 수동 수정**함 (.meta에 저장) |
| Mixamo 클립 | 이미 Humanoid (메뉴 2는 다시 안 해도 됨) |
| 랩 발 IK | `LabClipPlayer.SetApplyFootIK(false)` — 복도 Animator Controller도 기본으로 발 IK 꺼짐이라 같은 조건 |
| 지금 복도 캐릭터 | `SideScrollCharacter_Student_Male_v05` (2D 스프라이트 있음, 2D로 시작, V로 3D) |
| 농장 맵 플레이어 | `PlayerConfig` 아바타 = Student_Male_v05 (이번 작업과 무관) |

## 3. 메뉴 진행 시 주의 (중요)
1. **메뉴 1(모델 준비)은 v2에 다시 돌리지 않아도 된다.** 아바타 데이터가 이미 있고 Humanoid다. 다시 돌리면 임포트를 다시 하면서(배율 재계산 + Humanoid 재설정) **수동으로 고친 Hips 매핑이 풀릴 수 있다.** 돌렸다면 모델 임포트 설정 → Rig → Configure에서 Hips가 `Hips` 본인지 꼭 확인한다.
2. **메뉴 2(애니메이션 임포트)도 생략 가능** (Mixamo 3개가 이미 Humanoid·반복·루트 굽기 완료).
3. **메뉴 3**: Project 창에서 `Data/Avatars/Student_Chibi_v2`를 선택 → `3. 선택한 아바타로 복도 캐릭터 만들기`.
   - 만들어지는 것: `Animations/Avatars/Student_Chibi_v2.controller`, `Prefabs/SideScroll/SideScrollCharacter_Student_Chibi_v2.prefab`, `CorridorConfig` 캐릭터 지정
   - "복도 씬을 다시 만들까요?" → 다시 만들기
4. **2D가 없는 캐릭터라 항상 3D로 보인다.** 다만 `CorridorConfig`의 "2D로 시작"이 켜져 있어서, 화면 오른쪽 위 "2D 캐릭터 (V)" 토글은 **체크된 채로 3D가 보이는 불일치**가 생긴다 (알려진 작은 문제, 아래 5번).
5. **같은 폴더 문제**: 칩비가 Student_Male 폴더에 있어서, 나중에 Student_Male의 2D 스프라이트 준비(`선택한 2D 스프라이트 준비`)를 다시 돌리면 **칩비 아바타에도 Student_Male 2D가 연결**된다 (같은 캐릭터 폴더의 아바타를 모두 연결하는 규칙). 피하려면 칩비 파일을 **Unity 안에서** `Art/Characters/Student_Chibi/Model/`로 옮긴다 (GUID 유지, 아바타 이름은 파일 이름 그대로).
6. 되돌리기: `Data/SideScroll/CorridorConfig`의 캐릭터 프리팹을 `SideScrollCharacter_Student_Male_v05`로 바꾸고 `VoxelMagic/복도/복도 씬 다시 만들기`.

## 4. 적용 후 확인할 것
- 키가 큐브/v05와 비슷한지 (목표 1.6), 발이 바닥 띠 위(y ≈ 0.12)에 있는지
- Idle·Walk·Run 전환, A/D 방향 전환(즉시 180°), 양 끝 문 앞에서 멈춤
- 발 미끄러짐: 다리가 짧아 걷기·달리기가 미끄러져 보이면 플레이 중 `Data/Avatars/Student_Chibi_v2`의 Walk·Run 재생 속도 조절 (랩의 "디딤발 속도" 제안값을 시작값으로)
- 콘솔 에러 0, 단위 테스트(현재 69개) 통과
- 칩비 특성(감수하기로 함): Neck 본 없음, 팔·다리 한 조각이라 무릎·팔꿈치 굽힘 안 보임, 손을 머리 위로 올리면 머리 통과

## 5. 남은 일·알려진 문제
| 항목 | 내용 |
|---|---|
| 2D 없는 캐릭터의 토글 불일치 | `CharacterViewSwitcher`가 스프라이트가 없으면 3D로 보이지만 `CharacterViewState`는 2D로 남음 → 토글 체크 표시가 틀림. 고치려면: 스프라이트가 없으면 상태를 3D로 맞추거나 패널 토글을 비활성화 |
| `_BlenderShots` 폴더 | `4d910ca`에 섞여 커밋됨 (Blender 캡처 PNG). 필요 없으면 `git rm -r Assets/_BlenderShots Assets/_BlenderShots.meta` 후 커밋 (복도·랩 브랜치 둘 다) |
| 칩비 2D | 필요해지면 아틀라스 + json(Student_Male과 같은 형식) → `선택한 2D 스프라이트 준비` → 메뉴 3 다시 |
| 앞으로 Blender 내보내기 | 아마추어 루트 노드 없이 `Hips`가 루트가 되게. 임포트 후 Rig → Configure에서 Hips 매핑 확인 |
| 복도 도트 모드 분홍 선 | 타일 이미지 유리 경계 색 (이미지 확인 필요) |
| 랩 브랜치 | 복도 브랜치를 병합해서 받기 (파일 복사 금지: .meta/GUID·폴더 구조 어긋남) |

## 6. 작업 규칙 요약 (이전 대화에서 정한 것)
- 답변은 한국어. 설계 먼저 → "제작 시작" 승인 후 제작. 기능 하나 끝날 때마다 플레이 확인 → 짧게 보고 → 커밋 여부 질문
- **MCP 연결 확인**: 작업 시작·컴파일·플레이 진입 뒤마다 `set_active_instance VoxelMagicSchool@2d926515` 후 `Application.dataPath`가 `C:/Unity_Project/NCAI/VoxelMagicSchool/Assets`인지 확인 (STT_Prototype 에디터가 같이 연결될 수 있음)
- 에디터에 포커스가 없으면 가상 키 입력이 안 들어감 → 상태 머신에 입력 값을 직접 넣어 확인하고, 실제 조작은 사용자 플레이로 확인
- 씬 생성 도구는 새 씬을 연 **뒤에** 에셋을 읽는다 (먼저 읽으면 참조가 풀림)
- 브랜치: 복도 = `feature/prototype-corridor`, 랩 = `feature/tool-animation-lab`, 농장 = `feature/prototype-farm-fishing`. 커밋 전에 브랜치 확인
- 커밋은 사용자 승인 후. `_BlenderShots`는 사용자가 정하기 전까지 건드리지 않음

## 7. 새 대화 첫 메시지 예시
```text
Docs/HANDOFF_CHIBI_CORRIDOR.md 읽고 이어서 하자. 복도 캐릭터를 Student_Chibi_v2로 바꾸려고 메뉴 3을 실행할 거야. 먼저 MCP 연결과 현재 상태(브랜치, 아바타 데이터, Hips 매핑) 확인해 줘.
```
