# 설정 · 패키지 기록

프로젝트 설정이나 패키지를 바꿀 때마다 여기에 기록한다.

## 기본 환경 (2026-10-29 확인)
- Unity 6000.0.38f1, URP 17.0.3
- 입력 처리: Both (구 Input Manager + 새 Input System 1.13.0) — 변경하지 않음

## Git 브랜치 (2026-11-03 사용자 결정)

| 브랜치 | 올리는 작업 | 비고 |
|---|---|---|
| `feature/prototype-farm-fishing` | 프로토타입 1: 의뢰·농사·낚시 루프 (`Prototype_FarmFishing`) | 마지막 커밋 `4b9921f`. 이후 농장 맵 아바타·줌·그림자(`3c98754`)는 복도 브랜치에 들어가 있음 |
| `feature/prototype-corridor` | 프로토타입 2: 횡스크롤 복도 (`Corridor_SideScroll`), 복도 캐릭터 | 지금 작업 중 |
| `feature/tool-animation-lab` | 툴: 애니메이션 랩(`Scenes/Tools/AnimationLab`, `Scripts/Tools/`), 아바타 파이프라인 | `3c98754`에서 분기. **앞으로 랩 관련 변경은 여기에만** |

- 세 브랜치는 공용 코드(Core, 아바타 파이프라인, 폰트 등)를 함께 쓴다. 한 브랜치의 공용 변경이 다른 브랜치에 필요하면 그때 병합 여부를 사용자에게 묻는다.
- 커밋 전에 지금 브랜치가 작업 종류와 맞는지 확인하고, 다르면 브랜치를 바꿀지 먼저 묻는다.

## 패키지

| 날짜 | 패키지 | 버전 / 출처 | 이유 | 승인 |
|---|---|---|---|---|
| 2026-10-30 | MCP for Unity (`com.coplaydev.unity-mcp`) | `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.3.0` | Claude Code가 에디터를 조작(씬 구성, 플레이 모드, 콘솔 확인)하기 위함. STT_Prototype과 같은 버전으로 고정 | 사용자 승인 |
| 2026-11-05 | 2D Tilemap Editor (`com.unity.2d.tilemap`) | 1.0.0 (Unity 레지스트리, 6000.0.38f1에 맞는 유일한 버전) | 타일 에디터 테스트 씬: `Window > 2D > Tile Palette` 창으로 바닥 칠하기 (`DESIGN_TILEMAP.md`) | 사용자 승인 |
| 2026-11-05 | 2D Tilemap Extras (`com.unity.2d.tilemap.extras`) | 4.1.2 (Unity 레지스트리 추천 버전. 최신 10.0.1은 이 에디터와 맞지 않음) | Rule Tile로 풀↔흙 경계 자동 배치 (`DESIGN_TILEMAP.md` 6장) | 사용자 승인 |

## 에셋 가져오기

| 날짜 | 내용 | 위치 | 이유 | 승인 |
|---|---|---|---|---|
| 2026-10-30 | Pretendard 폰트 v1.3.9 (Regular) | `Assets/_Project/Fonts/Pretendard/` (원본 otf, `OFL.txt`) | 한글 TMP 고정 폰트 에셋의 원본. GitHub `orioncactus/pretendard` 저장소에서 받음. SIL OFL 1.1이라 게임에 넣어 배포 가능 | 사용자 승인 |
| 2026-10-30 | TMP Essential Resources | `Assets/TextMesh Pro/` (Fonts, Resources, Shaders, Sprites) | 한글 TMP 폰트 사용. uGUI 2.0에 포함된 리소스를 가져온 것이라 외부 다운로드 없음 | 사용자 승인 |

## 입력 액션

| 날짜 | 변경 | 이유 | 승인 |
|---|---|---|---|
| 2026-11-02 | `Input/GameControls.inputactions`에 `SideScroll` 맵 추가 (Move: 1D Axis A/D·←/→, Run: Left Shift). 기존 `Gameplay` 맵은 그대로 | 복도 횡스크롤 조작. W/S는 바인딩 자체가 없음 | 사용자 승인 |
| 2026-11-04 | `SideScroll` 맵에 `ToggleCameraFollow`(C), `Interact`(E) 추가 | 복도 카메라 고정/부드럽게 전환, 오른쪽 문 포털 | 사용자 승인 |
| 2026-11-04 | `SideScroll` 맵에 `ToggleNight`(N) 추가 | 복도 낮/밤 전환 | 사용자 승인 |

## 빌드 씬 목록 (Build Settings)

| 날짜 | 변경 | 이유 | 승인 |
|---|---|---|---|
| 2026-11-04 | `Scenes/Corridor_SideScroll.unity`, `Scenes/Prototype_FarmFishing.unity` 추가 (기존 `Assets/Scenes/SampleScene.unity`는 그대로 0번) | 복도 오른쪽 문 포털에서 실행 중 농장 씬을 열기 위해 (`SceneManager.LoadScene`은 목록에 있는 씬만 열 수 있음) | 사용자 승인 |

## 임포트 설정

| 날짜 | 대상 | 설정 | 바꾸는 법 |
|---|---|---|---|
| 2026-11-02 | `Art/Corridor/*.png`, `Art/Backdrop/*.png` | Sprite·Single·Full Rect, PPU 300, Bottom Center, Point, 압축 없음, Mip Map 끔, Alpha Is Transparency, Clamp, 최대 크기 4096 | `Data/SideScroll/CorridorConfig` 값 수정 → 메뉴 `VoxelMagic/복도/이미지 임포트 설정 적용` |
| 2026-11-04 | `Art/Lights/lamp_floor_glow.png` (복도 씬 생성 도구가 그림, 흰색 + 계단식 알파 원) | Sprite·Single, PPU = 그림 크기(1유닛), 가운데 기준점, Point, Clamp, 압축 없음, Mip Map 끔 | `CorridorConfig` 바닥 빛 도트 크기·단계 수정 → 복도 씬 다시 만들기 |
| 2026-11-05 | `Art/Map/Tile/**/*.png` (타일 팩 9개 테마 387장, 사용자 결정: 팩 전체) | Sprite·Single·Full Rect, PPU 32, 가운데 기준점, Point, 압축 없음, Mip Map 끔, Alpha Is Transparency, Clamp | `Data/Tiles/TileMapConfig`의 타일 픽셀 수정 → 메뉴 `VoxelMagic/타일 맵/타일 팩 임포트 설정 적용 (전체)` |

## 아바타 임포트 설정 (메뉴 `VoxelMagic/아바타/`로 적용)

| 날짜 | 대상 | 설정 |
|---|---|---|
| 2026-11-03 | `Animations/Mixamo/*.fbx` | Rig Humanoid (이 모델로 Avatar 생성), 클립 이름 = `@` 뒤, 반복 재생·Loop Pose 켬, 루트 회전·높이·앞뒤 이동 Bake Into Pose |
| 2026-11-03 | `Art/Characters/Student_Male/Model/Student_Male_v02.fbx` (11-03 폴더 정리로 Model/로 이동) | Rig Humanoid, 배율 1.58 (목표 키 1.6, `Data/Avatars/Student_Male`) |
| 2026-11-03 | `Art/Characters/Student_Male/Model/*.png` | Point, 압축 없음, Mip Map 끔 |

## 조명 그림자 (씬별)

| 날짜 | 씬 | 설정 | 이유 |
|---|---|---|---|
| 2026-11-03 | Prototype_FarmFishing, Tools/AnimationLab (11-06 Sandbox에서 옮김) | 조명 그림자 보정을 파이프라인 대신 직접: Depth Bias 0.1, Normal Bias 0 (`Editor/VoxelShadowSettings`) | 복셀 아바타 그림자 구멍. 프로젝트 URP 설정(`PC_RPAsset`)은 그대로 |

## 어셈블리 (asmdef)

| 이름 | 위치 | 참조 |
|---|---|---|
| VoxelMagic.Core | `Assets/_Project/Scripts/Core` | 없음 (UnityEngine만) |
| VoxelMagic.Gameplay | `Assets/_Project/Scripts/Gameplay` | Core, Unity.InputSystem, Unity.TextMeshPro |
| VoxelMagic.UI | `Assets/_Project/Scripts/UI` | Core, Unity.TextMeshPro, UnityEngine.UI |
| VoxelMagic.Editor | `Assets/_Project/Scripts/Editor` (에디터 전용) | Core, Unity.TextMeshPro |
| VoxelMagic.Tests | `Assets/_Project/Tests` (EditMode 테스트) | Core, Test Runner, NUnit |
| VoxelMagic.Tools (2026-11-03, 11-06 `VoxelMagic.Sandbox`에서 이름 변경) | `Assets/_Project/Scripts/Tools` (애니메이션 랩, 자동 참조 끔) | Core, Unity.InputSystem, Unity.TextMeshPro, UnityEngine.UI |
| VoxelMagic.Tools.Editor (2026-11-03, 11-06 `VoxelMagic.Sandbox.Editor`에서 이름 변경) | `Assets/_Project/Scripts/Tools/Editor` (에디터 전용) | Tools, Core, Gameplay, Editor, TMP, UI, InputSystem |

## 연결 메모
- 한글 폰트: `Fonts/Pretendard SDF.asset`은 고정(Static) 아틀라스라 플레이해도 파일이 바뀌지 않는다. 문자 목록에 없는 글자를 쓰려면 `VoxelMagic/한글 폰트 다시 굽기`를 실행한다.
- MCP는 stdio 방식, 포트 6400. STT_Prototype 에디터를 같이 열면 두 에디터가 함께 보이므로, 세션 시작 시 `VoxelMagicSchool` 인스턴스를 지정한다.
