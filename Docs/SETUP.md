# 설정 · 패키지 기록

프로젝트 설정이나 패키지를 바꿀 때마다 여기에 기록한다.

## 기본 환경 (2026-10-29 확인)
- Unity 6000.0.38f1, URP 17.0.3
- 입력 처리: Both (구 Input Manager + 새 Input System 1.13.0) — 변경하지 않음

## 패키지

| 날짜 | 패키지 | 버전 / 출처 | 이유 | 승인 |
|---|---|---|---|---|
| 2026-10-30 | MCP for Unity (`com.coplaydev.unity-mcp`) | `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.3.0` | Claude Code가 에디터를 조작(씬 구성, 플레이 모드, 콘솔 확인)하기 위함. STT_Prototype과 같은 버전으로 고정 | 사용자 승인 |

## 에셋 가져오기

| 날짜 | 내용 | 위치 | 이유 | 승인 |
|---|---|---|---|---|
| 2026-10-30 | Pretendard 폰트 v1.3.9 (Regular) | `Assets/_Project/Fonts/Pretendard/` (원본 otf, `OFL.txt`) | 한글 TMP 고정 폰트 에셋의 원본. GitHub `orioncactus/pretendard` 저장소에서 받음. SIL OFL 1.1이라 게임에 넣어 배포 가능 | 사용자 승인 |
| 2026-10-30 | TMP Essential Resources | `Assets/TextMesh Pro/` (Fonts, Resources, Shaders, Sprites) | 한글 TMP 폰트 사용. uGUI 2.0에 포함된 리소스를 가져온 것이라 외부 다운로드 없음 | 사용자 승인 |

## 어셈블리 (asmdef)

| 이름 | 위치 | 참조 |
|---|---|---|
| VoxelMagic.Core | `Assets/_Project/Scripts/Core` | 없음 (UnityEngine만) |
| VoxelMagic.Gameplay | `Assets/_Project/Scripts/Gameplay` | Core, Unity.InputSystem, Unity.TextMeshPro |
| VoxelMagic.UI | `Assets/_Project/Scripts/UI` | Core, Unity.TextMeshPro, UnityEngine.UI |
| VoxelMagic.Editor | `Assets/_Project/Scripts/Editor` (에디터 전용) | Core, Unity.TextMeshPro |
| VoxelMagic.Tests | `Assets/_Project/Tests` (EditMode 테스트) | Core, Test Runner, NUnit |

## 연결 메모
- 한글 폰트: `Fonts/Pretendard SDF.asset`은 고정(Static) 아틀라스라 플레이해도 파일이 바뀌지 않는다. 문자 목록에 없는 글자를 쓰려면 `VoxelMagic/한글 폰트 다시 굽기`를 실행한다.
- MCP는 stdio 방식, 포트 6400. STT_Prototype 에디터를 같이 열면 두 에디터가 함께 보이므로, 세션 시작 시 `VoxelMagicSchool` 인스턴스를 지정한다.
