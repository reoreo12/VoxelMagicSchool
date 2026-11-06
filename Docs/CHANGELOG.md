# 변경 기록

## 2026-11-06 — 복도: 칩비 2D 아틀라스 연결 (V로 3D ↔ 2D)
- 파일 (사용자 제공 webp 1536×1024, Idle 4 · Walk 6 · Run 6, 오른쪽을 보는 그림): 압축으로 흐려진 알파를 정리(128 미만 → 투명, 이상 → 불투명)해 `Art/Characters/Student_Chibi/Sprites/Student_Chibi_sheet.png`로 저장, 칸 정보 `Student_Chibi_sprites.json`(칸 256×300, 줄마다 발바닥이 기준점 (140, 4)에 오게 잘라냄)
- 칩비 전용 폴더에 둔 이유: Student_Male 폴더에 두면 기존 `Student_Male_2D`를 덮어쓰고 v02~v05에도 연결됨
- `Data/Avatars/Student_Chibi_2D`: 목표 키 1.28(복도 칩비 0.8배와 같게) → PPU 210, Idle 3fps · Walk 8fps · Run 9fps(반복). 기존 2D 준비 도구(`SpriteCharacterImporter`)로 만들고, 같은 폴더에 모델이 없어 `Student_Chibi_v2` 아바타에 직접 연결
- 칩비 복도 프리팹 다시 만듦(메뉴 3과 같은 처리) → `Sprite2D` 자식 + 2D/3D 전환 포함, Model 배율 0.8 다시 적용, 복도 씬 다시 만듦 (밤 조명·사용자 값 그대로). 시작은 3D("2D로 시작" 꺼짐)
- 코드 변경 없음
- 확인 (한 프레임씩 진행): V → 2D(3D 렌더러 꺼짐)·V → 3D, 오른쪽 걷기 Walk 프레임 진행·반전 없음, 왼쪽 걷기 반전(flipX), 달리기 Run 프레임 진행, 발 y 0.10(바닥 띠 안), 높이 1.43(칸 포함) / 콘솔 에러 0
- 알아 둘 점: 2D 스프라이트는 조명 없는 머테리얼이라 **밤에도 밝게** 보임 (3D 칩비·복도는 어두워짐)

## 2026-11-06 — 타일 에디터: 구운 상태로 되돌리기 버튼
- 설계: `DESIGN_TILEMAP.md` 11-11 (사용자 요청·승인)
- Editor: `TileMapLibrary.RevertToBaked`/`BakedCellsOf` — 칠한 칸을 마지막 굽기 칸 데이터로 그 자리에서 바꿈 (Ctrl+Z 가능, 굽기 결과 파일은 읽기만). 도구 창 굽기 영역 "구운 상태로 되돌리기" (확인 창, 칸 데이터 없으면 꺼짐 + 이유 표시)
- 확인: 시험 맵 굽기 → 짚·바깥 칸으로 잘못 칠하기 → 되돌리기 → 구웠을 때와 모든 칸·결과 그림 같음, 맵 위치·구운 바닥 연결 그대로, 칸 데이터 파일 안 바뀜 / Ctrl+Z → 잘못 칠한 상태로 돌아감 / FarmSample은 칸 데이터가 없어 버튼 꺼짐 / 테스트 92개 통과, 콘솔 에러 0. 시험 맵·결과는 지움
- 정리: 이전 시험 맵 `_SurfaceTest`가 에디터 다시 실행(Redo)으로 씬에 되살아나 있던 것을 지움 (굽기 결과는 이미 없어 프리팹이 빠진 상태였음)
- 문서: 조작법 3장에 G 단축키 충돌(Shift+G 권장)·채우기 동작(실제 확인), 9장에 되돌리기

## 2026-11-06 — 타일 에디터 여러 맵 4·5단계: 바닥 종류 정보 + 단계별 조작법
- 설계: `DESIGN_TILEMAP.md` 11-10. 사용자 요청으로 4단계 뒤 확인 없이 조작법까지 한 번에 진행
- Core: `Tiles/GroundSurface`(바닥 종류, 표시 이름), `GroundSurfaceTable`(타일 → 종류, 빠진 줄만 더하기), `GroundSurfaceMap`(칸마다 종류 1바이트 + 종류 목록, 칸·로컬 위치로 찾기) + 테스트 4개 (전체 92개 통과). `TileMapConfig`에 바닥 종류 표 참조
- Gameplay: `GroundSurfaceSource`(구운 프리팹 루트, 월드 위치 → 종류), `GroundSurfaceQuery`(씬 전체 찾기 `TryGet`), `GroundSurfaceProbe`(확인용: 플레이 중 발밑 종류를 왼쪽 위에 표시, 에디터 씬 치비에 붙임)
- Editor: `GroundSurfaceBuilder` — 메뉴 `VoxelMagic/타일 맵/바닥 종류 표 만들기·채우기 (farm)`: `Data/Tiles/Surfaces/Surface_{Grass,Soil,Mud,Straw,Stone,Wood}`(풀·흙·진흙·짚·돌·나무판) + `Data/Tiles/GroundSurfaceTable`(51줄: 바닥 12 + Rule Tile 3 + 경계 36, Rule Tile·경계는 재료 이름으로), 설정에 연결. `TileFloorBaker`가 `<맵>_Surfaces.asset` 저장 + 프리팹 루트에 `GroundSurfaceSource`, 표에 없는 타일은 경고. 도구 창에 바닥 종류 수·표 열기/만들기. 씬 생성 도구가 치비에 확인 표시를 붙임 (지금 씬에도 추가)
- 확인: 표 다시 실행 → 새 줄 0 (고친 줄 보존) / 시험 맵(8×6 풀 + 흙·짚) 굽기 → 바닥 종류 3종, 표에 없는 칸 0, 795바이트 / 플레이 모드 찾기: 풀·흙 가운데·흙 경계 칸(흙)·짚 맞음, 맵 밖·옛 굽기(FarmSample) 위는 없음 / 10만 번 찾기 14 ms / 치비를 흙 위로 옮기면 "발밑 바닥: 흙" 표시(한글 정상) / 콘솔 에러 0. 시험 맵·결과·임시 캡처는 지움. `FarmSample`은 사용자 요청으로 다시 굽지 않음 (칸 데이터·바닥 종류 없음)
- 문서: `TILEMAP_EDITOR_GUIDE.md`를 단계별 조작법으로 다시 씀 (처음 준비 · 씬 열기 · 새 맵 · 칠하기 · 여러 맵 · 칸 크기 · 굽기 · 바닥 종류 · 게임 씬 적용 · 다시 불러오기 · 확인 도구 · 문제 해결 · 메뉴)

## 2026-11-06 — 타일 에디터 여러 맵 3단계: 칸 데이터 저장 · 구운 맵 다시 불러오기
- 설계: `DESIGN_TILEMAP.md` 11-5·11-6
- Core: `Tiles/TileMapCells` (칸 위치 → 칠한 타일, 같은 타일은 한 번만 두고 번호로) + 테스트 4개 (전체 88개 통과)
- Editor: `TileFloorBaker`가 `<맵>_Cells.asset`도 저장 (Rule Tile 칸은 Rule Tile 자체로, 덮어쓰기 확인 문구에 칸 데이터 추가). `TileMapLibrary` — `LoadBakedMap`(칸 데이터대로 새 맵을 칠하고 구운 프리팹 연결, 칠하기 보기로 시작, 같은 이름이 씬에 있으면 그 맵을 돌려줌, 음수 칸이 있어도 다른 맵과 안 겹치게 배치, Ctrl+Z 한 번), `BakedMaps`, `FindCells`(프리팹·칸 데이터·폴더 어느 것을 골라도), `FindMap`. 도구 창 "구운 맵 불러오기" — 구운 맵 목록(칸 수 / 씬에 있음 → [선택] / 칸 데이터 없음), Project에서 고른 파일로 불러오기, 다른 맵 숨기기
- 확인: 시험 맵(8×6 풀 + 흙·짚 Rule Tile + 음수 칸 + 경계 낱장, 50칸) 굽기 → 칸 데이터 50칸·4종 → 씬에서 지우고 불러오기 → **50칸 모두 타일·결과 그림까지 같음**, 구운 프리팹 연결, 칠하기 보기 / 같은 맵 다시 불러오기 → 새로 안 만듦 / 고친 뒤 다시 굽기 → 칸 데이터 갱신(흙 9 → 12칸), 구운 바닥 인스턴스 그대로 / 플레이 모드 정상, 콘솔 에러 0. 시험 맵과 결과는 지움
- 남은 것: `FarmSample`은 칸 데이터가 없어 아직 불러오기 목록에서 "칸 데이터 없음" → 한 번 다시 구우면 생김 (기존 결과 덮어쓰기라 사용자 확인 후)

## 2026-11-06 — 타일 에디터 여러 맵 2단계: 맵 목록 · 새 맵
- 설계: `DESIGN_TILEMAP.md` 11-2~11-4
- Core: `Tiles/TileMapPlacement` (새 맵 자리 = 기존 맵들 동쪽 끝 + 간격) + 테스트 4개 (전체 84개 통과). `TileMapConfig`에 새 맵 기본 크기 40×40·간격 4칸 추가
- Editor: `TileMapLibrary` — `NewMap`(이름 검사, 겹치지 않게 배치, 바탕 타일 채우기, 다른 맵 숨기기, Ctrl+Z 한 번으로 취소), `SetVisible`, `AllMaps`, `SuggestName`. `TileMapEditorWindow` — 맵 목록(체크 = 보이기, ▶ 작업 대상, 칸 크기·구움 여부, [선택] = 보이기 + 선택 + 씬 뷰 맞춤), "작업 대상 말고 다른 맵 모두 숨기기", "새 맵"(이름·크기·바탕 타일·다른 맵 숨기기 → 만든 뒤 선택하고 칠하기 시작), 창 스크롤
- 확인: 새 맵 40×40(풀) → (14, 0, 0)에 1,600칸, FarmSample 숨겨짐 / 두 번째 새 맵(8×6, 빈 맵) → (58, 0, 0), 숨긴 맵도 자리 계산에 들어감 / 같은 이름 거부 / Ctrl+Z 두 번 → 두 맵 모두 사라지고 FarmSample 다시 보임 / 창 그리기 에러 없음, 맵 선택 → 비교 오브젝트가 그 맵 가운데로 / 플레이 모드 정상, 콘솔 에러 0. 시험 맵은 지움
- 참고: 컴파일 직후 콘솔에 Unity 내부 경고 `Assertion failed ... kDontSaveInEditor` 1번 (스택 없음). 이후 작업에서는 다시 안 나옴 — Tile Palette 창이 열린 채 다시 컴파일할 때 Unity가 내는 것으로 추정

## 2026-11-06 — 타일 에디터 여러 맵 1단계: 맵 하나 = 루트 하나
- 설계: `DESIGN_TILEMAP.md` 11-1·11-7 ("제작 시작" 승인)
- 구조: `Maps/Map_<이름>`(TileMapDocument + TileGridScaler) > `Grid`(X 90°, 배율 = 칸 크기) > `Paint`, 구운 바닥은 맵 루트 아래. 비교 오브젝트는 `Compare`(TileMapCompareRig) > `Center`(카메라·치비·상자)·`East`(연못)
- Gameplay: `TileMapDocument`(Grid·Paint 참조, 칠한 범위·칸 → 월드 위치), `TileGridScaler`(맵 루트로 옮김, 자기 Grid·구운 바닥 배율만, 위치는 안 건드림), 새 `TileMapCompareRig`(작업 대상 맵 가운데·동쪽 끝으로)
- Editor: `TileFloorBaker`(맵 하나만 굽고 그 루트 아래에 놓음), `TileMapEditorWindow`(선택한 오브젝트가 속한 맵 = 작업 대상, 비교 오브젝트·Tile Palette 대상도 따라감), 새 `TileMapLibrary`(맵 만들기·이름 검사/바꾸기·옛 구조 옮기기 메뉴 `VoxelMagic/타일 맵/옛 구조 씬을 여러 맵 구조로 옮기기`), 씬 생성 도구도 새 구조로
- 지금 씬: 다시 만들지 않고 구조만 옮김 (칠한 100칸·구운 바닥 그대로, Ctrl+Z 한 번으로 되돌릴 수 있게 묶음)
- 확인: 맵 루트를 (20, 0, 3)으로 옮기면 칸·구운 바닥·비교 오브젝트가 같이 옮겨짐 / 칸 크기 2 → Grid·구운 바닥 배율 2 / 루트를 끄면 칠한 칸·구운 바닥 함께 숨겨짐 / 시험 맵(6×6)을 만들어 그것만 구움 → FarmSample 결과 파일 안 바뀜, 구운 바닥은 시험 맵 루트 아래 / 선택 기준 대상: 시험 맵 Paint 선택 → 시험 맵, FarmSample 조각 선택 → FarmSample / 같은 이름 거부 / 플레이 모드: 두 맵 모두 칠하기 층 꺼지고 구운 바닥 보임, 콘솔 에러 0 / 테스트 80개 통과. 시험 맵과 그 굽기 결과는 지움

## 2026-11-05 — 타일 에디터 4단계: 굽기 방식 확정, 칠하기 편의, 사용법
- 사용자 결정: 본 방식 = **굽기** (플레이 전에 한 번 구워 파일로 저장, 게임은 파일만 씀). 땅은 런타임에 바꾸지 않고, 물 주기 등 변화는 텃밭 칸 오브젝트(나중에 MagicaVoxel FBX)에서만 → `DESIGN_TILEMAP.md` 10장
- 도구 창: "칠하기 시작"(Tile Palette 열기 + 팔레트 Farm + 대상 Paint 자동 선택 + 칠하기 보기), 에디터 씬이 아닐 때 "타일 맵 에디터 씬 열기" 버튼. 메뉴 `VoxelMagic/타일 맵/타일 맵 에디터 씬 열기` 추가
- 문서: `Docs/TILEMAP_EDITOR_GUIDE.md` (사용법)
- 확인: "칠하기 시작" → palette Farm · target Paint 선택됨, 기본 붓으로 칠하기 됨(시험 칸은 되돌림) / 구운 프리팹을 빈 씬에 놓으면 Transform·MeshFilter·MeshRenderer만으로 그대로 보임 (범위 (0,0,0)~(10,0,10)) / 콘솔 에러 0

## 2026-11-05 — 타일 에디터 3단계: 굽기 + 바닥 프리팹 + 도구 창, 확인 1~4
- 설계: `DESIGN_TILEMAP.md` 7·8장
- Core: `Tiles/TileAtlasLayout` (아틀라스 배치·UV, 여백 제외) + 테스트 4개 (`TileAtlasLayoutTests`, 전체 80개 통과)
- Gameplay: `TileMapDocument` (맵 이름·구운 칸 크기·구운 바닥, 플레이 중 구운 바닥 켬), `PaintOnlyTilemap` (플레이 중 칠하기 층 끔), `TileGridScaler`가 구운 바닥 배율도 맞춤
- Editor: `TileFloorBaker` (칠한 칸 → 아틀라스 PNG·URP Lit 머티리얼·16칸 조각 메시·`<맵>_Floor.prefab`, 이전 결과가 있으면 덮어쓰기 전에 확인 창, 이번에 없는 조각 메시는 지움), `TileMapEditorWindow` (메뉴 `VoxelMagic/타일 맵/타일 맵 에디터 창`: 맵 이름, 칸 크기·후보 버튼, 굽기, 보기 전환, 해 높이·방향). 씬 생성 도구가 두 컴포넌트도 붙이게 함 (지금 씬에는 다시 만들지 않고 컴포넌트만 추가)
- 결과: `Art/Map/Baked/FarmSample/` — 칸 100, 타일 24종, 조각 1, 아틀라스 180×180 (여백 2px), 프리팹 루트 배율 1 (= 칸 1 m)
- 확인 1 (방향): 칸 (0,0) = 월드 (0.5, 0, 0.5), 위쪽 = +Z
- 확인 2 (Tilemap + URP Lit): **예상과 달리 그림이 나오고 그림자도 받음**. 평균 색 차이는 작음 (구운 바닥 (115,129,72) / Lit Tilemap (117,131,78) / Unlit Tilemap (119,133,70)), 화소 28%가 조금씩 다름. 확인 후 원래 머티리얼(Sprite-Unlit-Default)로 되돌림
- 확인 3 (그림자 줄무늬): 해 높이 50°·20°·10°에서 줄무늬 안 보임 (그림자만 길어짐)
- 확인 4 (이음새): 여백 0px과 2px로 각각 구워 먼 시점·낮은 시점 비교 → 차이 화소 0~1개, 이음새 안 보임 (Point·밉 없음이라). 여백 2px는 안전 여유로 유지. 비교용 임시 결과 `_SeamTest_Pad0`은 지움
- 플레이 모드: '칠하기' 보기로 두고 들어가도 칠하기 층 꺼지고 구운 바닥만 보임, 조각 렌더러 1개, 콘솔 에러 0. 칸 크기 2.133으로 바꾸면 구운 바닥도 바로 2.133배 (되돌림)

## 2026-11-05 — 타일 에디터 2단계: 타일 맵 에디터 씬 + 10×10 샘플
- 사용자 정리: 테스트 씬이 아니라 **타일 맵 에디터 씬** (칠하고 구워서 다른 게임 플레이 씬에 땅으로 씀) → `DESIGN_TILEMAP.md` 0·2·3·7장 반영
- Gameplay: `Tiles/TileGridScaler` ([ExecuteAlways]) — `TileMapConfig.CellSize`를 FloorGrid 배율로 바로 반영, 샘플 가운데·동쪽 끝 기준점도 옮김
- Editor: `Tiles/TileMapEditorSceneBuilder` — 메뉴 `VoxelMagic/타일 맵/타일 맵 에디터 씬 만들기` (씬이 있으면 덮어쓰기 전에 확인 창)
- 씬: `Scenes/Tools/TileMapEditor.unity` — 해(농장과 같은 값), FloorGrid(X 90°) + Paint Tilemap 한 층, 10×10 샘플(흙 4×4·길 2칸 폭·짚 ㄱ자·1칸 폭 줄·대각선 칸), 치비(Student_Chibi_v2, 배율 1)·1 m 상자·연못(2, 2, 1.5)
- 고침: 씬 생성 도구가 새 씬을 열기 전에 설정을 읽어, 씬 전환 때 에셋이 내려가며 설정 참조가 비던 문제 → 새 씬을 연 뒤 읽음
- 확인: 칸 (0,0) 가운데 = 월드 (0.5, 0, 0.5), (9,9) = (9.5, 0, 9.5), 팔레트 위쪽 = +Z(북) / 칸 크기 1 → 2.133으로 바꾸면 Grid 배율·기준점이 바로 따라옴(씬 다시 만들기 불필요), 되돌림 / 플레이 모드: 칠한 칸 유지, 치비 Idle, 콘솔 에러 0 / Tilemap은 조명·그림자를 받지 않음(예상대로 → 굽기 단계에서 해결)

## 2026-11-05 — 타일 에디터 1단계: 타일 팩 임포트 설정 + farm 타일·Rule Tile·팔레트
- 브랜치: `feature/tool-tile-editor` · 설계: `DESIGN_TILEMAP.md` 5·6장 ("제작 시작" 승인)
- Core: `TileMapConfig` (칸 크기 1 m, 후보 2.133/1.067/1, 타일 32 px, 샘플 10, 조각 16, 여백 2 px, 바뀌면 `Changed`) → `Data/Tiles/TileMapConfig.asset`
- Editor: `Tiles/FarmTileSetBuilder` — 메뉴 `VoxelMagic/타일 맵/타일 팩 임포트 설정 적용 (전체)`, `VoxelMagic/타일 맵/farm 타일·Rule Tile·팔레트 만들기` (이미 있으면 덮어쓰기 전에 확인 창). `VoxelMagic.Editor.asmdef`에 `Unity.2D.Tilemap.Extras`·`Unity.2D.Tilemap.Editor` 참조 추가
- 에셋: 타일 팩 387장 임포트 설정 변경(.meta 수정, GUID 유지), `Art/Map/TileAssets/farm/` (Tile 48 = 바닥 12 + 경계 36, Rule Tile 3), `Art/Map/TilePalettes/Farm.prefab` (54칸)
- 확인: 387장 모두 설정 일치. 미리보기 장면(저장 안 됨)에서 Rule Tile이 바깥·안쪽 모서리·변을 맞게 고름, 경계는 칸 가운데라 칠한 영역이 둘레로 반 칸씩 줄어 보임, 1칸 폭 줄은 맞는 그림이 없어 경계 없이 끊김(4-3 예상대로) / 콘솔 에러 0. 씬이 아직 없어 플레이 모드 확인은 다음 단계에서

## 2026-11-05 — 타일 에디터: 타일맵 패키지 2개 설치
- 브랜치: `feature/tool-tile-editor`
- 설계: `DESIGN_TILEMAP.md` (설계안, "제작 시작" 승인 전)
- 패키지: 2D Tilemap Editor 1.0.0, 2D Tilemap Extras 4.1.2 설치 (`Packages/manifest.json`, `packages-lock.json`), 사용자 승인
- 확인: 메뉴 `Window > 2D > Tile Palette` 열림, `RuleTile` 형식 있음, 콘솔 에러 0

## 2026-11-04 — 복도 밤 조명 나누기: 바닥 빛 그림 + 천장 조명(칩비)
- 설계: DESIGN_CORRIDOR.md 17번 (사용자 결정: 미리보기는 빼고 조명만 분리)
- 이유: 사용자가 점 조명을 바닥으로 내려 쓰니(간격 5.7, 높이 −0.5, 세기 1.3) 바닥 빛 모양은 맞지만 칩비를 아래에서 올려 비추고, 쿠키 진하기를 따로 조절할 수 없었음
- `Lamp_i` 아래 `FloorGlow`(조명이 아닌 도트 원 스프라이트, 더하기 합성) + `CeilingLight`(천장 스포트, 아래로 보다가 벽 쪽으로 25° 기울여 칩비 앞면까지)
- `CorridorConfig`: 점 조명 항목 → 바닥 빛(가운데 높이 0.2·벽 앞 0.05·가로 2.4·세로 0.4·색·진하기 0.35·도트 32픽셀·4단계) + 천장 조명(높이 2.7·z −0.8·기울기 25·각도 60·거리 4·세기 4·색) + 벽이 천장 조명 받는 배율(기본 0). 간격·끝 여유는 그대로(사용자 값 5.7 → 7개), 색·쿠키 크기·단계는 이름만 바뀌어 값 이어짐
- Core: `SideScrollMath.DotCircleLevel`(계단식 도트 원 밝기, 테스트 1개 → 전체 76개 통과)
- 셰이더: 새 `Shaders/SpriteAdditive.shader`(조명 없는 더하기 합성, 머테리얼 `Art/Materials/CorridorFloorGlow.mat`), `SpriteLit3D`에 `_CorridorLampOnWall` 배율
- 고침: 두 셰이더가 Unity 6 스프라이트 색·좌우 반전(`unity_SpriteColor`·`UnityFlipSprite`)을 반영하지 않아 바닥 빛이 흰색으로 진하게 나옴 → URP `Core2D.hlsl` 사용
- `CorridorLighting`: 밤에 바닥 빛·천장 조명 켜고, 벽 배율 전역 값 설정. 쿠키 그림 `lamp_dot_cookie.png` 삭제 → `Art/Lights/lamp_floor_glow.png`
- 사용자가 플레이하며 정한 최종 값 (`CorridorConfig`): 끝 여유 2.5, 바닥 빛 높이 0.1·세로 0.2·진하기 0.15·도트 80픽셀·6단계, 천장 조명 세기 6 (나머지는 위 기본값)
- 확인 (한 프레임씩 진행): 밤 — 칩비 발밑에 따뜻한 계단식 도트 타원, 칩비가 천장 조명으로 밝아짐, 벽에는 추가 빛 없음, 창밖 밤 배경 그대로 / 낮 — 바닥 빛·천장 조명 꺼짐, 오른쪽 문 정상 / 콘솔 에러 0

## 2026-11-04 — 복도: 낮/밤 전환(N) + 밤 배경 + 도트 점 조명 (3D 조명)
- 설계: DESIGN_CORRIDOR.md 16번. 처음 설계(2D Renderer + Sprite Light)는 사용자 결정으로 **3D 조명**으로 바꿈 → URP·프로젝트 설정 변경 없음
- 셰이더: `Shaders/SpriteLit3D.shader` (`VoxelMagic/Sprite Lit 3D`) — 스프라이트 그림 × (전역 `_CorridorAmbient` + 3D 추가 조명, Forward+·쿠키 지원). 해는 쓰지 않아 낮 화면은 이전과 같음. 머테리얼 `Art/Materials/CorridorSpriteLit.mat`을 복도 타일에 사용, 창밖 배경은 계속 조명 없는 스프라이트
- Core: `TimeOfDayState`(낮/밤 + 이벤트), `SideScrollContext.TimeOfDay`, `SideScrollMath.LampPositions`(간격 이하로 고르게, 양 끝 여유), `CorridorConfig` 낮/밤(밤으로 시작 끔, 낮/밤 배경 세트, 복도 밝기 색 낮 흰색·밤 (0.12, 0.14, 0.26), 밤 해 ×0.15·주변광 ×0.3)·점 조명(간격 4, 끝 여유 1.5, 높이 1.9, z −2, 각도 34, 거리 6, 세기 5, 따뜻한 색, 쿠키 32픽셀·4단계), 문구 "밤 (N)"·조작법에 N
- 데이터: `BackdropSet_Night`(bg_night_loop, 하늘 대체 색 남색). 이미지 임포트는 기존 배경 설정 그대로
- 입력: `SideScroll` 맵에 `ToggleNight`(N) 추가
- Gameplay: `CorridorLighting` — 배경 세트 교체, 전역 복도 밝기, 해 세기·주변광(3D 캐릭터), 점 조명 켜고 끄기. 플레이를 멈추면 낮 값으로 되돌림
- UI: `NightTogglePanel` — "카메라 고정 (C)" 아래 "밤 (N)" 토글
- Editor: 복도 씬 생성 도구가 타일 조명 머테리얼, 점 조명(스포트, 그림자 없음) 10개, 도트 원 쿠키 `Art/Lights/lamp_dot_cookie.png`(Cookie·Point·압축 없음)·밤 배경 세트, HUD 토글을 만듦. 조작법 글상자 높이 230
- Tests: `TimeOfDayTests` 4개 (전체 75개 통과)
- 확인 (한 프레임씩 진행): 낮 화면이 이전과 같음 → 밤: 창밖 밤 배경, 복도 어두워짐, 벽에 빛 원 10개(첫 값 각도 60은 원이 겹쳐 34로 줄이고 세기 3 → 5), 해 0.15, 토글 표시 일치, 도트 렌더링(P)과 함께 빛 원 가장자리도 도트 / 콘솔 에러 0. 밤으로 처음 바꿀 때 에디터가 셰이더 변형을 컴파일하는 몇 프레임 동안 화면이 하늘색으로 보임 (에디터 전용, 빌드에는 없음)

## 2026-11-04 — 농장 맵: 플레이어 아바타를 칩비(Student_Chibi_v2)로 교체
- 브랜치: `feature/prototype-farm-fishing`에 `feature/prototype-corridor` 병합 (빨리 감기, 충돌 없음)
- `PlayerConfig` 아바타 `Student_Male_v05` → `Student_Chibi_v2` (복도와 같은 Controller, Idle/Walk만 사용), 메뉴 `VoxelMagic/프로토타입 씬 다시 만들기`로 씬 다시 생성
- 확인: 플레이어 Model = Student_Chibi_v2.fbx, Humanoid, 키 1.63(렌더러 범위), 시작 Idle, 움직임 감지 시 Walk 재생 속도 파라미터 갱신 / 콘솔 에러 0
- 문서: DESIGN.md 12번 (칩비 교체 + 지금 기능에 필요한 애니메이션 목록, 연결에 필요한 작업)

## 2026-11-04 — 복도: 오른쪽 문 포털 (E → 농장·낚시 맵)
- 설계: DESIGN_CORRIDOR.md 15번 승인 후 제작
- 설정 변경 (사용자 승인): Build Settings 씬 목록에 `Corridor_SideScroll`, `Prototype_FarmFishing` 추가 → SETUP.md
- Core: `InteractionFocusState`(지금 대상의 안내 문구 + 변경 이벤트), `SideScrollContext.Interaction`, `CorridorConfig` 포털 설정(켜기·씬 이름·오른쪽 끝에서 거리 2.56·키 1.0·지름 0.6·트리거 반지름 0.2·색·검사 높이 0.5), 문구 "농장으로 가기"·조작법에 E
- 입력: `SideScroll` 맵에 `Interact`(E) 추가 (프로토타입 1 `Gameplay` 맵은 그대로)
- Gameplay: `ScenePortal`(`IInteractable`, 트리거 CapsuleCollider, 목록에 없는 씬이면 에러 로그), `SideScrollInteractor`(발 + 0.5 높이 점의 트리거 겹침 검사 → 대상·안내, E로 실행), `SideScrollInputReader.InteractPressed`
- UI: `SideScrollPromptView` — 화면 아래 가운데 "E  농장으로 가기" (대상이 있을 때만)
- Editor: 복도 씬 생성 도구가 포털(루트: 포털 + 트리거, 자식 Visual: 반투명 원기둥)·머테리얼 `Art/Materials/CorridorPortal.mat`(URP Unlit 반투명)·안내 UI 생성, 조작법 글상자 높이 200. `SetString` 확장 메서드 추가
- Tests: `InteractionFocusTests` 1개 (전체 71개 통과)
- 확인 (한 프레임씩 진행): x 32.987(범위 밖) 안내 없음 → x 33.28(포털 중심) "E  농장으로 가기" 표시 → E(상호작용 실행) → `Prototype_FarmFishing` 열림(플레이어·UI 정상) / 콘솔 에러 0

## 2026-11-04 — 복도: 카메라 따라가기 토글 (고정 ↔ 부드럽게, C)
- 설계: DESIGN_CORRIDOR.md 14번 승인 후 제작
- Core: `CameraFollowState`(고정/부드럽게 + 변경 이벤트), `SideScrollContext.CameraFollow`, `CorridorConfig` "카메라 고정으로 시작"(기본 켬), 조작법에 C 추가
- 입력: `SideScroll` 맵에 `ToggleCameraFollow`(C) 추가
- Gameplay: `SideScrollCamera` — 고정이면 매 프레임 플레이어 x로 바로(복도 끝 제한은 그대로), 부드럽게면 기존 SmoothDamp
- UI: `CameraFollowPanel` — "2D 캐릭터 (V)" 아래 "카메라 고정 (C)" 토글. 씬 생성 도구는 토글 패널을 같은 함수로 쌓도록 정리, 조작법 글상자 높이 140 → 170
- 칩비 크기: 사용자가 씬에서 Model 배율 0.8로 줄인 것을 프리팹(`SideScrollCharacter_Student_Chibi_v2`)으로 옮김 (씬을 다시 만들어도 유지. 메뉴 3을 다시 실행하면 1로 돌아감)
- Tests: `CameraFollowTests` 1개 (전체 70개 통과)
- 확인 (한 프레임씩 진행): 부드럽게 → 캐릭터 7.08 / 카메라 6.03(늦게 따라감), 고정 → 캐릭터 8.563 = 카메라 8.563, 오른쪽 끝 → 카메라 32.481(허용 최대값)에서 멈춤, 토글 표시 일치 / 콘솔 에러 0

## 2026-11-04 — 복도: 플레이어 3D 모델을 칩비(Student_Chibi_v2)로 교체, 2D 끔
- 메뉴 1·2는 건너뜀 (v2는 랩에서 이미 Humanoid로 준비됨, 직접 고친 Hips 매핑 유지), 메뉴 3 기능(`BuildCharacter` + `UseInCorridor`) 실행
- 생성: `Animations/Avatars/Student_Chibi_v2.controller`, `Prefabs/SideScroll/SideScrollCharacter_Student_Chibi_v2.prefab` (2D 버전 없음 → `Sprite2D` 없이 3D만)
- `CorridorConfig`: 캐릭터 프리팹 v05 → Student_Chibi_v2, 2D로 시작 켬 → 끔. 복도 씬 다시 생성
- 확인 (상태 머신에 입력 값 직접): 키 약 1.63(렌더러 범위), Idle → Walk(오른쪽 1.52 m/s) → Run(왼쪽) → 왼쪽 끝 2.56에서 멈춤·Idle, 2D 상태 꺼짐 / 콘솔 에러 0
- 되돌리기: `CorridorConfig` 캐릭터 프리팹을 `SideScrollCharacter_Student_Male_v05`로, 2D로 시작 켜고 복도 씬 다시 만들기

## 2026-11-03 — 복도: 2D 스프라이트 캐릭터 + 2D/3D 전환 (V)
- 설계: DESIGN_CORRIDOR.md 13번 승인 후 제작
- Core: `SpriteCharacterSet`(상태별 프레임·fps·반복·목표 키) + `SpriteClipMath`(시간 → 프레임), `AvatarSetup._sprites`(2D 버전 연결), `CharacterViewState`(2D/3D + 변경 이벤트), `SideScrollContext.CharacterView`, `CorridorConfig`에 2D로 시작(기본 켬)·스프라이트 정렬 순서(10), 조작법에 V 추가
- 입력: `SideScroll` 맵에 `ToggleCharacterView`(V) 추가
- Gameplay: `SpriteFrameAnimator`(상태 이벤트 → 프레임 재생, 회전 정면 고정, 왼쪽이면 좌우 반전), `CharacterViewSwitcher`(2D면 3D 렌더러 끄고 스프라이트 켬, Animator는 계속)
- UI: `CharacterViewPanel` — 복도 HUD 도트 렌더링 패널 아래 "2D 캐릭터 (V)" 토글
- Editor: `SpriteCharacterImporter`(메뉴 `VoxelMagic/아바타/선택한 2D 스프라이트 준비`), 복도 캐릭터 만들기(메뉴 3)가 2D 버전이 있으면 `Sprite2D` 자식과 전환 컴포넌트를 함께 넣음, 복도 HUD에 토글 패널
- 생성: `Data/Avatars/Student_Male_2D`(Idle 8프레임 6fps · Walk 8프레임 10fps · Run 8프레임 12fps, PPU 51.25 = Idle 82px → 키 1.6), Student_Male v02~v05 아바타에 연결, v05 복도 프리팹·복도 씬 다시 생성. 스프라이트 PNG 임포트: Sprite·Point·압축 없음·Mip Map 끔
- Tests: `CharacterViewTests` 9개 (전체 69개 통과)
- 확인 (상태 머신에 입력 값 직접): 2D로 시작(3D 렌더러 꺼짐), 걷기 → Walk 프레임, 왼쪽 달리기 → Run 프레임·좌우 반전, 루트가 90° 돌아도 스프라이트 회전 (0,0,0), 발 y 0.08(바닥 띠 안), 정렬 순서 10 / V → 3D(애니메이션 Run 이어짐) → V → 2D / 콘솔 에러 0

## 2026-11-03 — 폴더 구조 정리: 캐릭터별로 묶기
- 사용자 요청: 3D 모델(`Art/Characters/`)과 2D 스프라이트(`Art/Sprites/Characters/`)가 따로 흩어져 복잡했음
- 새 구조: `Art/Characters/<캐릭터>/Model/`(3D 모델·텍스처), `Art/Characters/<캐릭터>/Sprites/`(2D 아틀라스·json). `Animations/`는 모든 캐릭터가 같이 쓰는 클립이라 그대로 따로 둠
- 이동 (Unity 안에서 옮겨 GUID 유지): Student_Male v02~v05 `.fbx`·`_tex.png` → `Model/`, 사용자가 넣은 `Student_Male_Idle/Walk/Run.png`·`Student_Male_sprites.json` → `Sprites/`. 빈 `Art/Sprites/` 갈래 삭제
- `AvatarPipeline` 아바타 이름 규칙: 모델 폴더 이름 → **캐릭터 폴더 이름**(Art/Characters 바로 아래), 모델이 여러 개면 파일 이름 (기존과 같음)
- 확인: 아바타 데이터 4개 모델 경로 새 위치, 복도 프리팹(v02·v05) 메시·아바타 연결 정상, PlayerConfig 아바타 정상, 새 모델 자동 준비 0개 / 콘솔 에러 0
- 문서: AVATAR_GUIDE.md, DESIGN_CORRIDOR.md, SETUP.md 경로 갱신

## 2026-11-03 — 복셀 아바타 그림자 구멍 수정 (씬 조명 Normal Bias 0)
- 원인: 복셀 메시는 면마다 정점이 따로라, URP 그림자 Normal Bias가 면을 법선 방향 안쪽으로 밀 때 면 사이가 벌어져 그림자에 구멍이 생김
- 비교 (같은 위치·같은 카메라 7곳: 아바타·게시판·학교 벽·은행나무·연못 돌·텃밭·크리처, 현재 0.5 vs Normal 0): 바뀐 픽셀 0.03~0.46%, **모두 어두워지는 쪽**(밝아진 곳 0) — 아바타 그림자 구멍이 메워지고 다른 물체 그림자 가장자리가 한 줄 정도 넓어짐. 빛 받는 바닥·벽·돌 표면에 새 얼룩(Shadow Acne) 없음 → 롤백하지 않음
- 적용: `Editor/VoxelShadowSettings`(씬 조명만 파이프라인 설정 대신 Depth Bias 0.1·Normal Bias 0), 프로토타입 씬·애니메이션 랩 씬 생성 도구에서 사용, 두 씬 다시 생성. 프로젝트 URP 설정은 그대로
- Editor asmdef에 `Unity.RenderPipelines.Universal.Runtime` 참조 추가
- 확인: 근접 캡처에서 아바타 그림자 꽉 참 / 콘솔 에러 0
- 참고: 노말 맵은 화면에 그릴 때의 음영에만 쓰이고 그림자 모양에는 쓰이지 않아 이 문제와 관계없음. 근본 해결은 모델에서 정점 합치기(Weld)지만, 복셀의 각진 음영이 바뀔 수 있어 지금은 하지 않음

## 2026-11-03 — 프로토타입 1: 마우스 스크롤 카메라 줌, 기본 거리 가깝게
- `GameInputReader`: 스크롤은 낚시 미니게임 중이면 잡기 바, 창이 열려 있지 않은 평소에는 `ZoomRequested` 이벤트(카메라 줌)
- `QuarterViewCamera`: 스크롤 위 = 가까이, 아래 = 멀리. 거리를 줌 범위로 제한하고 따라가기와 같은 부드러움으로 다가감
- `PlayerConfig`: 카메라 기본 거리 14 → **8**, 줌 범위 4~16, 스크롤 한 칸 1 (에셋 값도 8로 변경)
- 씬 다시 생성 (카메라에 입력 연결)
- 확인: 시작 거리 8, 위로 10칸 → 4에서 멈춤, 아래로 20칸 → 16에서 멈춤, 부드럽게 4까지 다가감 / 콘솔 에러 0
- 그림자 구멍 원인 확인: 아바타 메시(정점 20,122 · 삼각형 11,902)는 복셀 면마다 정점이 따로 있는 구조라, URP 그림자 Normal Bias(조명 0.4)가 면을 법선 방향으로 안쪽으로 밀 때 면 사이가 벌어져 구멍이 생긴다. 플레이 중 조명만 Normal Bias 0으로 바꾸자 구멍이 사라짐 (저장하지 않은 실험)

## 2026-11-03 — 프로토타입 1: 플레이어를 아바타(Student_Male_v05)로 임시 교체
- 설계: DESIGN.md 11번, 사용자가 이동 애니메이션 Walk 선택
- Core: `PlayerConfig`에 아바타(AvatarSetup, 비우면 기본 도형)·Walk 기준 속도(1 m/s)·Walk 시작 속도(0.1)·섞기 시간(0.15초) 추가, 아바타 = `Student_Male_v05`
- Gameplay: `PlayerAvatarAnimator`(움직이면 Walk, 멈추면 Idle — 농사·낚시·대화는 Idle. Walk 재생 속도 = 실제 이동 속도 ÷ 기준 속도)
- Editor: `PrototypeSceneBuilder.BuildPlayer`가 아바타가 있으면 몸통·머리·넥타이 도형 대신 아바타 모델(`Model`, Animator = v05 Controller, 루트 이동 끔)을 넣음. 지팡이·지팡이 끝·입질 표시·CharacterController는 그대로. 프로토타입 씬 다시 생성
- 확인: 시작 Idle → 4 m/s 이동 시 Walk(재생 속도 약 3.3~4배) → 멈추면 Idle, 낚시 상태 Idle, Model 위치 (0,0,0) 유지 / 콘솔 에러 0 / 단위 테스트 60개 통과
- 참고: 아바타 키 1.6이라 기존 도형(머리까지 약 2.1)보다 작아 보임, 지팡이는 몸 앞 고정 위치. 되돌리려면 `PlayerConfig` 아바타를 비우고 `VoxelMagic/프로토타입 씬 다시 만들기`

## 2026-11-03 — 복도 맵 도트 렌더링 On/Off (화면 전체)
- 설계: DESIGN_CORRIDOR.md 12-10, 사용자가 B(화면 전체) + 토글·슬라이더 선택
- 공용화: 랩의 `LabPixelView` → `UI/Rendering/PixelRenderView` (같은 .meta로 옮겨 기존 씬 참조 유지), 랩도 같은 컴포넌트 사용. `VoxelMagic.Sandbox`·`Sandbox.Editor` asmdef에 `VoxelMagic.UI` 참조 추가
- Core: `PixelRenderState`(켜짐·도트 크기·범위 제한·변경 이벤트), `SideScrollContext.PixelRender`, `CorridorConfig`에 시작 시 켜기·도트 크기(4)·범위(2~8), 조작법 문구에 P 추가
- 입력: `GameControls`의 `SideScroll` 맵에 `TogglePixel`(P) 추가 — 기존 `Gameplay` 맵은 그대로. `SideScrollInputReader`가 P로 상태 토글
- UI: `PixelRenderPanel`(토글·슬라이더·표시 ↔ 상태 ↔ 뷰), 복도 HUD에 패널·EventSystem·GraphicRaycaster·전체 화면 출력 RawImage 추가
- Tests: `PixelRenderStateTests` 4개 (전체 60개 통과)
- 확인: P(상태 토글) → 켜짐·UI 토글 체크·480×222 (4배), 슬라이더 6 → 320×148, 글자 표시 갱신 / 화면 도트 렌더링 캡처 확인 / P 바인딩 `<Keyboard>/p`, 기존 Gameplay 맵 액션 10개 그대로 / 콘솔 에러 0 / TMP 폴백 폰트 변경 없음
- 참고: 도트 모드에서 창틀·난간 일부에 분홍 선이 보임 — 원본 타일 이미지의 유리 경계 색이 낮은 해상도에서 드러나는 것으로 보임 (이미지 확인 필요)

## 2026-11-03 — 복도 캐릭터를 Student_Male_v05로 교체
- 애니메이션 랩에서 v05 + Idle/Walking/Standard Run 확인 후 (사용자), 메뉴 3과 같은 처리로 복도에 적용
- 생성: `Animations/Avatars/Student_Male_v05.controller`(Idle/Walk/Run + 속도 파라미터), `Prefabs/SideScroll/SideScrollCharacter_Student_Male_v05.prefab`
- `CorridorConfig` 캐릭터 프리팹 → `SideScrollCharacter_Student_Male_v05`, 복도 씬 다시 생성 (v02 프리팹·데이터는 그대로 남김)
- 확인 (상태 머신에 입력 값 직접): Walk·Run 애니메이션 상태 일치, A 한 프레임에 Y 270°, 양 끝 2.56·33.28에서 Idle, Y 0.12·Z 0 고정, Model 위치 (0,0,0) 유지 / 콘솔 에러 0
- 재생 속도는 아직 기본값 1 (`Data/Avatars/Student_Male_v05`에서 플레이 중 조절)

## 2026-11-03 — 애니메이션 랩: 도트 렌더링 On/Off
- 설계: DESIGN_CORRIDOR.md 12-9 (A. 저해상도 렌더 텍스처, 프로젝트 설정·URP 렌더러 변경 없음) 승인 후 제작
- Sandbox: `LabPixelView`(카메라를 화면 ÷ 도트 크기 해상도의 Point 필터 텍스처에 그려 화면 맨 뒤 RawImage로 확대, 창 크기 바뀌면 다시 생성), 랩 UI에 도트 렌더링 토글(P)·도트 크기 슬라이더(2~8, 기본 4)·해상도 표시, `AnimationLabConfig`에 도트 크기 범위·기본값, 재생 패널 폭 1320 → 1500
- 확인: 켜면 1920×888 화면에서 480×222(4배)·240×111(8배)로 그려지고 UI는 선명 / 끄면 카메라가 화면에 직접 그림, RawImage 꺼짐 / 마우스 입력을 막지 않음(Raycast Target 끔) / 콘솔 에러 0
- 참고: 바닥 격자 같은 가는 선은 도트가 클수록 끊겨 보인다 (확인용 화면 특성)
- 아바타 자동 준비 보완: 아바타 데이터가 있어도 모델이 Humanoid가 아니면 다시 준비 (`Student_Male_v05.fbx`의 .meta가 기본값으로 다시 만들어져 Generic이 된 것을 발견 → 다시 준비해 Humanoid·배율 1.6 복구)

## 2026-11-03 — 애니메이션 랩: 새 캐릭터 모델 자동 준비
- 랩 시작·새로고침 때 `Art/Characters/` 아래에서 AvatarSetup이 없는 .fbx를 찾아 자동으로 준비(Humanoid·키 1.6 배율·텍스처·AvatarSetup·클립 자동 연결)하고 아바타 목록에 추가, 아래쪽에 "새 모델 N개를 준비해 목록에 추가했습니다" 표시
- `AvatarPipeline.PrepareNewModels()`, `AnimationLabBridge.PrepareNewModels` 추가
- 확인: 사용자가 넣은 `Student_Male_v04.fbx` → 플레이 시작 때 `Data/Avatars/Student_Male_v04` 생성, 목록 Student_Male / v03 / v04, v04 키 1.60·Humanoid 뼈 22개 / 콘솔 에러 0
- 주의: `Art/Characters/` 안의 .fbx는 모두 캐릭터(Humanoid)로 처리된다. 소품 모델은 다른 폴더에 둔다

## 2026-11-03 — 아바타 버전 비교 (Student_Male_v03)
- 파일 (사용자): `Art/Characters/Student_Male/Student_Male_v03.fbx`, `Student_Male_v03_tex.png` (v02와 같은 폴더)
- `AvatarPipeline` 이름 규칙: 이미 이 모델을 쓰는 AvatarSetup이 있으면 그것을 갱신, 없으면 폴더에 모델이 하나일 때 폴더 이름·여러 개일 때 파일 이름 → v03 준비가 기존 `Student_Male`(v02)을 덮어쓰지 않음
- v03 준비: `Data/Avatars/Student_Male_v03` 생성, Humanoid, 배율 1.6 (키 1.6), 텍스처 Point, Idle/Walk/Run 자동 연결
- 애니메이션 랩: 아바타를 바꿔도 클립·재생 시간·일시정지·카메라 유지 (확인: Run 0.40초 일시정지 상태로 v02 ↔ v03 전환, 카메라 이동 0)
- 복도 캐릭터는 그대로 v02 (`SideScrollCharacter_Student_Male`)

## 2026-11-03 — 프로토타입 2: 아바타 파이프라인 · 애니메이션 랩 · Student_Male 복도 적용
- 파일 (사용자): `Art/Characters/Student_Male/`(Student_Male_v02.fbx, 텍스처 png), `Animations/Mixamo/`(Y Bot@Idle·Walking·Standard Run, Without Skin, In Place)
- 임포트 설정: Mixamo 3개 Humanoid·반복 재생·루트 굽기, 클립 이름 Idle/Walking/Standard Run / Student_Male Humanoid, 배율 1.58 (키 1.012 → 1.6), 텍스처 Point·압축 없음·Mip Map 끔
- Core: `AvatarSetup`(SO), `CorridorConfig`에 캐릭터 프리팹 필드
- Gameplay: `AvatarAnimationSpeed`(상태별 재생 속도를 Animator 파라미터로, 플레이 중 반영)
- Editor: `AvatarPipeline` 메뉴 `VoxelMagic/아바타/` 1~3 (모델 준비 / 애니메이션 Humanoid 임포트 / 복도 캐릭터 만들기). 복도 씬 생성기는 설정의 캐릭터 프리팹을 우선 사용
- Sandbox(새 asmdef `VoxelMagic.Sandbox`, `VoxelMagic.Sandbox.Editor`): 애니메이션 랩 — `AnimationLab`, `LabClipPlayer`(Playable 재생·섞기·반복·시간 이동), `FootSlideMeter`, `LabCamera`, `AnimationLabBridge`, `AnimationLabConfig`, 씬 생성 메뉴 `VoxelMagic/아바타/애니메이션 랩 씬 다시 만들기`
- 생성: `Data/Avatars/Student_Male`, `Animations/Avatars/Student_Male.controller`(Idle/Walk/Run + 속도 파라미터), `Prefabs/SideScroll/SideScrollCharacter_Student_Male`, `Scenes/Sandbox/AnimationLab.unity`, `Data/Sandbox/AnimationLabConfig`, `Materials/Sandbox/M_Lab_*`
- 문서: `Docs/AVATAR_GUIDE.md`(새 아바타 넣는 순서)
- 랩 확인: 키 1.60(큐브와 같음), Humanoid 뼈 22개 매핑 / Idle→Walk 섞기 진행률 표시 / 반복 끄면 끝(0.733초)에서 멈춤, 켜면 다시 반복 / 시간 이동 0.3초 + 1프레임 → 0.333초 / A↔B: Idle ↔ Walking / 디딤발 속도 걷기 약 1.0 m/s·달리기 약 2.15 m/s, 골반 이동 폭 0.03~0.06 m (제자리) / 한글 글자 빠짐 없음
- 복도 확인 (상태 머신에 입력 값 직접): Idle→Walk→Run 애니메이션 상태 일치, A 한 프레임에 Y 270°, 양 끝 2.56·33.28에서 Idle, Y 0.12·Z 0 고정, Model 위치 (0,0,0) 유지 (루트 이동 없음) / 콘솔 에러·경고 0 / 단위 테스트 56개 통과
- 문제와 처리: 랩 UI 생성 중 기본 폰트로 한글을 먼저 그려 `Assets/TextMesh Pro/.../LiberationSans SDF - Fallback.asset`이 바뀜 → git으로 되돌리고, 폰트를 먼저 지정하도록 수정
- 미확인 (직접 확인 필요): 실제 키·마우스 조작, 재생 속도·이동 속도 자연스러움, 달리기 자세에서 바지·몸통 메시가 꺾여 보이는 정도 (모델 가중치 문제일 수 있음)

## 2026-11-02 — 프로토타입 2 제작 2단계: 캐릭터 (모델 · 상태 머신 · 이동 · 애니메이션 · HUD)
- 입력: `GameControls.inputactions`에 `SideScroll` 맵 추가 (Move = 1D Axis A/D·←/→, 둘 다 누르면 0 / Run = Left Shift). 기존 `Gameplay` 맵은 그대로 (추가된 줄만 있음)
- Core: `MoveIntent`·`Facing`·`CharacterStateId`, `ICharacterState`·`CharacterStateBase`·`IdleState`·`MoveStateBase`·`WalkState`·`RunState`, `CharacterStateMachine`(StateChanged·FacingChanged 이벤트), `SideScrollMotor`(x만), `CharacterMoveConfig`, `CharacterAnimationConfig`, `SideScrollTexts`. `SideScrollContext`에 이동 설정·상태 머신 추가
- Gameplay: `SideScrollInputReader`, `SideScrollCharacter`(위치 Y·Z와 회전 Y ±90°를 매 프레임 덮어씀), `CharacterAnimatorView`(상태 이벤트 → CrossFade)
- UI: `SideScrollHintView`(왼쪽 아래 조작법), `StateDebugView`(오른쪽 위 현재 상태, 설정으로 끔)
- Editor: `SideScrollCharacterBuilder`(메뉴 `VoxelMagic/복도/캐릭터 프리팹 다시 만들기`), `CharacterAnimationBuilder`(메뉴 `VoxelMagic/복도/캐릭터 애니메이션 다시 만들기`), 씬 생성기에 캐릭터·입력·HUD 추가
- 생성: `Prefabs/SideScroll/SideScrollCharacter.prefab`(키 1.6, 루트 = 로직 + 캡슐 충돌체 하나, `Model` 자식 = 큐브 모델 + Animator), `Animations/SideScroll/`(Idle·Walk·Run 클립, Controller — 전이 없음), `Materials/SideScroll/M_SS_*` 6개, `Data/SideScroll/CharacterMoveConfig`·`CharacterAnimationConfig`
- Tests: `CharacterStateMachineTests` 12개, `SideScrollMotorTests` 6개 추가 (전체 56개 통과)
- 플레이 확인 (상태 머신에 입력 값을 직접 넣어 진행): 오른쪽 걷기 Idle→Walk, 속도 1.6, 카메라 x 따라감 / Shift → Run 속도 3.6, 앞으로 기울기·팔다리 ±45° / A 한 프레임에 Y 회전 90 → 270(-90) 즉시 / 양 끝 x 2.56·33.28에서 Idle로 멈춤, 카메라 3.36·32.48로 제한 / 26초 이동 내내 캐릭터 Y 0.12·Z 0 고정, X·Z축 회전 0 / 상태 표시 문구 "상태: 걷기" 등으로 바뀜 / 콘솔 에러 0
- 확인 방법 한계: 에디터가 포커스를 받지 않아 가상 키 입력이 입력 액션에 들어가지 않았다. 입력 바인딩(A/D·←/→·Shift만 있고 W/S 없음)은 설정을 읽어 확인했고, 실제 키 조작과 Point 필터 깜빡임은 직접 플레이로 확인 필요
- 참고: 일시정지 상태로 캡처해 화면의 상태 글자가 갱신되지 않은 채 찍혔다 (값은 바뀐 것을 확인)

## 2026-11-02 — 프로토타입 2 제작 1단계: 복도 맵 (임포트 · 복도 · 카메라 · 패럴랙스)
- 설계: `Docs/DESIGN_CORRIDOR.md` (사용자 결정 반영 후 "제작 시작" 승인)
- 이미지 임포트 설정 (`Art/Corridor` 4장, `Art/Backdrop` 1장): Sprite·Single·Full Rect, PPU 300, 기준점 Bottom Center, Point 필터, 압축 없음, Mip Map 끔, Alpha Is Transparency, Clamp, 최대 크기 4096 (배경 폭 2688이 줄지 않게)
- Core: `CorridorLayout`, `BackdropSet`, `CorridorConfig`(SO), `CorridorBounds`, `SideScrollMath`(패럴랙스·반복 배치·카메라 제한·직교 크기), `SideScrollContext`/`ISideScrollBinder`
- Gameplay: `CorridorBootstrap`(조립), `CorridorBuilder`(타일 배치, 에디터 미리보기는 씬에 저장 안 함), `ParallaxBackdrop`(계수 0.3, 3장 반복), `SideScrollCamera`(직교 크기 1.44, x만 따라가기, 복도 범위 제한)
- Editor: `CorridorSpriteImportTool`(메뉴 `VoxelMagic/복도/이미지 임포트 설정 적용`), `CorridorSceneBuilder`(메뉴 `VoxelMagic/복도/복도 씬 다시 만들기`)
- 데이터: `Data/SideScroll/CorridorConfig`, `CorridorLayout`(문→창문→민벽→창문+사물함→창문→민벽→문(반전)), `BackdropSet_Day`
- 씬: `Scenes/Corridor_SideScroll.unity`
- Tests: `SideScrollMathTests` 10개 추가 (전체 38개 통과)
- 플레이 확인: 복도 7칸 x 2.56~33.28 간격 5.12로 이음매 없음, 마지막 문 반전 / 화면 세로에 복도 높이 꼭 맞음(직교 크기 1.44) / 유리 뒤로 숲·하늘 비침, 하이라이트 점 보임 / 카메라 대상 x 15로 이동 시 배경 화면 이동량 = 카메라 이동량 × 0.3 (계산값과 일치), 빈틈 없음 / 양 끝에서 카메라 x 3.36·32.48로 제한, 복도 밖 안 보임 / 콘솔 에러·경고 0
- 문제와 처리: 씬 생성 중 열려 있던 `Prototype_FarmFishing.unity`가 TMP 내부 값(m_TextStyleHashCode 등)만 바뀐 채 저장됨 → 의도한 변경이 아니라 git 원래 상태로 되돌림
- 미확인: Point 필터 깜빡임은 캐릭터가 걸어 움직일 때 확인 예정 (지금은 에디터 비활성 상태라 연속 움직임을 볼 수 없음)

## 2026-10-30 — 한글 폰트를 Pretendard 고정 아틀라스로 교체
- 이유: 맑은 고딕 동적(DynamicOS) 에셋은 플레이할 때마다 글자가 추가돼 Git 변경으로 잡히고, 맑은 고딕은 게임에 넣어 배포할 수 없는 폰트
- 추가: `Fonts/Pretendard/Pretendard-Regular.otf`(v1.3.9, SIL OFL 1.1, 라이선스 `OFL.txt`), 문자 목록 `KoreanCharset.txt`(영문·숫자·기호, 한글 자모, KS X 1001 한글 2,350자)
- 생성: `Fonts/Pretendard SDF.asset` — 고정(Static) 모드, 2048×2048 아틀라스 1장, 샘플링 32pt, 여백 4, 글자 2,558개, 약 9.7MB. 문자 목록 + Core 스크립트·데이터 에셋에 쓰인 글자를 함께 굽는다
- `KoreanFontAssetBuilder`: OS 폰트 대신 폰트 파일로 고정 에셋을 굽도록 변경, 메뉴 `VoxelMagic/한글 폰트 다시 굽기`. 다시 구워도 GUID를 유지한다. 문구를 새로 쓸 때 문자 목록에 없는 글자(예: 똠)가 생기면 다시 굽는다
- 씬·프리팹(`UI_InventorySlot`, `UI_RequestEntry`) 다시 생성해 새 폰트 연결, `MalgunGothic SDF.asset` 삭제
- 확인: 플레이 모드에서 TMP 텍스트 51개 모두 Pretendard, 빠진 글자 없음, 한글 정상 표시 / 플레이 후 폰트 에셋 파일 해시 그대로 / 콘솔 에러·경고 0 / 단위 테스트 28개 통과

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
- 참고: 한글 폰트 에셋(DynamicOS)은 플레이할 때 글자가 추가되며 파일이 바뀌므로 커밋에서 제외하고 있다 (→ 같은 날 Pretendard 고정 폰트로 교체해 해결)

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
