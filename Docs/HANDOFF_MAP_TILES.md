# 인수인계 — 텃밭·낚시 맵 바닥: 타일 에디터 테스트 씬

> 작성: 2026-11-05 (이전 대화에서 사용자 정리본을 프로젝트 상태와 맞춰 검토). 새 대화는 이 문서와 `CLAUDE.md`부터 읽는다.
> 관련 문서: `GAMEPLAY_PROTOTYPE_SPEC.md`(프로토타입 1 지침), `Docs/DESIGN.md`(프로토타입 1 설계), `Docs/SETUP.md`, `Docs/CHANGELOG.md`

## 0. 시작 전에 (사용자 확인)
- [ ] 농장 씬(`Scenes/Prototype_FarmFishing.unity`)에 사용자가 직접 한 **연못 교체가 저장되지 않았을 수 있음** (2026-11-05 확인: 씬 dirty). Unity에서 저장
- [ ] `feature/prototype-farm-fishing` 브랜치에 **커밋 안 된 것**: 농장 씬 변경, `Assets/_Project/Art/Map/`(연못 FBX·타일 팩 전체). 새 작업 브랜치를 만들기 전에 커밋할지 사용자에게 묻는다
- [ ] 타일 팩의 출처·라이선스 (사용자에게 받아 `SETUP.md`에 기록)

## 1. 목표 (사용자 요청)
- 높이(Y 단차) 없는 평평한 3D 복셀 맵의 바닥을 **타일로 칠할 수 있는 타일 에디터 테스트 씬**
- 타일 팩 `Art/Map/Tile/farm`을 가져와 **바로 칠할 수 있게 팔레트까지** 준비 (경계는 **Rule Tile**만, Auto Tile은 쓰지 않음)
- 범위 밖: 실제 맵 씬 적용, 샘플 구역 이후 칠하기, 걸을 수 없는 경계(절벽·나무 줄·물·울타리), z-fighting, 낮/밤, 충돌

### 역할
| Claude | 사용자 |
|---|---|
| 설계 → 승인 후 테스트 씬, 타일 팩 임포트 설정, Tile Palette·Rule Tile 등록, 비교용 오브젝트, 확인 1~4 테스트, 샘플 구역(약 10×10칸) | 실제 바닥 칠하기·배치 결정, 타일 크기·연못 배율 결정 |

## 2. 프로젝트 규칙 (CLAUDE.md 요약 — 반드시 지킬 것)
- 답변은 한국어. **설계 먼저**: 코드·씬·에셋을 만들기 전에 설계 문서(새로 `Docs/DESIGN_TILEMAP.md` 제안)에 정리해 설명하고, 사용자가 **"제작 시작"**이라고 한 뒤에만 만든다
- 작업 파일은 `Assets/_Project/`, `Docs/` 안에만. 그 밖(예: `Assets/Settings/`, `ProjectSettings/`, `Packages/manifest.json`)을 바꾸기 전에는 반드시 묻는다
- **패키지 설치 전에 묻는다**, 설치·설정 변경은 즉시 `SETUP.md`·`CHANGELOG.md`에 기록
- 스크립트 맨 위에 용도 주석(1~3줄), 숫자 값은 설정(ScriptableObject)에서 조절, 의존 방향 `UI → Core`, `Gameplay → Core` (Editor 도구는 `VoxelMagic.Editor` 어셈블리)
- 기능 하나 끝날 때마다 플레이 모드 확인 → 짧게 보고 → 사용자 확인. 커밋·푸시는 사용자 요청 시에만
- 기존 파일을 덮어쓰거나 지우기 전에 묻는다 (특히 `Prototype_FarmFishing.unity`는 사용자가 직접 고친 씬. 메뉴 `VoxelMagic/프로토타입 씬 다시 만들기`를 실행하면 사용자 수정이 사라진다 → **실행 금지**)
- 브랜치: 작업별로 나눈다. 새 작업이므로 `main`에서 새 브랜치(예: `feature/map-tiles`)를 제안하고 사용자 승인 후 만든다. 지금 브랜치: `feature/prototype-farm-fishing`, 다른 브랜치: `feature/prototype-corridor`, `feature/tool-animation-lab`

## 3. 확인된 사실 (2026-11-05, 이전 대화에서 직접 측정)
### 3-1. 크기 — 치비와 연못의 복셀 크기가 다르다
| 대상 | 씬 배율 | 렌더러 크기 (m) | 복셀 수 | 1복셀 |
|---|---|---|---|---|
| 치비 (농장 `Player/Model`) | 1 | 키 1.633 · 폭 1.40 | 24 × 21 | **약 0.067 m** |
| `Pond_Water` | (2, 2, 1.5), 회전 X 270 | 6.0 × 4.0, 두께 0.2 | 45 × 30, 높이 약 3 | 가로 **0.133 m** |
| `Pond_Bank` | (2, 2, 1.5) | 7.2 × 6.13, 높이 0.7 | 54 × 46, 높이 7 | 가로 **0.133 m**, 세로 **0.1 m** |

- 연못 FBX 원본의 1복셀은 **0.067 m로 치비와 같다**. 사용자가 씬에서 배율을 바꿔(가로 2배, 세로 1.5배) 지금은 연못 복셀이 치비보다 크고, 정육면체도 아니다 → **사용자에게 알리고 맞출지 결정받는다** (새 대화 시작 때 다시 측정해 값이 그대로인지 확인)
- 복도 맵 치비는 프리팹 배율 0.8 (1복셀 약 0.053 m). 농장은 배율 1. 맵 연결 때 맞출 문제 (이번 범위 밖, 참고만)

### 3-2. 타일 팩 (`Assets/_Project/Art/Map/Tile/`)
- 모든 타일 **32 × 32 px** 낱장 PNG. 테마 9개(autumn, beach, cafe_shop, fantasy_village, farm, home, pets, town_park, winter), 이번 대상은 **farm**
- farm: `floors/` 12장 (barn_floor_planks, barnyard_stones, dark_garden_soil, dry_dirt_path, farm_grass, flower_meadow, hay_floor, meadow_grass, muddy_farm_path, stepping_stone_path, straw_covered_ground, tilled_soil_rows), `walls/` 5장, `transitions/` 3세트:
  - `meadow_grass_to_dark_garden_soil`, `meadow_grass_to_muddy_farm_path`, `meadow_grass_to_straw_covered_ground`
  - 세트마다 12장: 변 `n s e w`, 바깥 모서리 `ne nw se sw`, 안쪽 모서리 `ine inw ise isw`. 가운데 칸은 `floors/`의 같은 이름 타일
  - 이름의 방향(예: `_n`)이 "북쪽이 풀"인지 "북쪽이 흙"인지는 **그림을 보고 확인** 필요
- **아직 .meta 없음** (Unity가 임포트하지 않은 상태). 임포트 설정 전 확인
- 연못: `Art/Map/Pond/` — `Pond_Bank.fbx`, `Pond_Water.fbx`, `pond_palette.png`, `Pond_Water_Mat.mat` (사용자 제작, 같은 원점)

### 3-3. 패키지·렌더링
- Unity **6000.0.38f1**, URP **17.0.3**, 렌더러 **Forward+** (오브젝트당 추가 조명 4개 제한은 적용되지 않음). 라이트 쿠키 켜짐
- `Packages/manifest.json`: `com.unity.modules.tilemap`만 있음. **`Window > 2D > Tile Palette` 메뉴 없음** (확인함)
  - Tile Palette 창 = **2D Tilemap Editor** (`com.unity.2d.tilemap`), Rule Tile = **2D Tilemap Extras** (`com.unity.2d.tilemap.extras`) → **패키지 2개** 필요. Package Manager에서 6000.0.38f1용 버전 확인 후 **설치 전에 사용자에게 알린다**
  - 2D Sprite 패키지도 없음 (예전 작업에서 확인: Unity 6에서 텍스처 임포터로 칸을 자르는 옛 방법이 없어 우회함). 이번 타일은 낱장이라 자를 필요 없음
- 농장 씬 해: 그림자 Soft, **Depth Bias 0.1 · Normal Bias 0** (`Editor/VoxelShadowSettings`, 복셀 그림자 구멍 대응) — 확인함
- 화면 전체 도트 렌더링 `UI/Rendering/PixelRenderView` 있음 (복도·랩에서 사용, 농장 씬에는 아직 없음)
- 농장 로직 그리드: `Data/Config/FarmConfig` Grid Size 6 · **Cell Size 1 m**, 텃밭 시작점 (−10, 0, −6) (`PrototypeSceneBuilder.FarmOrigin`). 바닥 Ground = 40 × 40 상자 하나
- 연못 낚시: `Pond`의 PondArea → Water = `Pond_Water`의 **MeshCollider** (사용자가 직접 설정)

## 4. 이전 대화의 검토 결과 (설계에 반영할 것)
### 4-1. 확인 2번(Tilemap + URP Lit)은 거의 확실히 안 된다 — 굽기 방식 추천
- URP Lit은 그림을 `_BaseMap`에서 읽는데 스프라이트·타일맵은 `_MainTex`로 넘긴다 → 그림이 안 나옴. 스프라이트 메시는 **법선이 없어** 조명 계산도 틀린다. Unity 6 스프라이트는 색·좌우 반전도 렌더러 값(`unity_SpriteColor`·`unity_SpriteProps`)으로 넘긴다 (복도 작업에서 같은 문제를 겪음: `Shaders/SpriteLit3D.shader` 참고)
- 테스트는 짧게 보여 주고(사실 확인용), 본 방식은 **"Tilemap = 칠하기 전용, 굽기 도구 = 평면 메시 + 아틀라스"**를 추천:
  - Tilemap은 에디터에서만 보이고 게임에서는 끈다
  - 굽기 도구: 칠한 칸을 읽어 위를 향한 평면 메시(예: 16 × 16칸 조각)와 사용한 타일만 모은 아틀라스(여백·가장자리 늘이기 포함)를 만든다 → URP Lit(불투명)
  - 장점: 조명·그림자·나중의 낮/밤을 일반 3D 바닥처럼 받음, 조각당 그리기 1번(가벼움), 다시 칠하고 굽기만 누르면 수정 끝, 아틀라스 여백으로 확인 4번(이음새)도 함께 해결, 칸 정보를 나중에 걸을 수 없는 칸·충돌에 재사용
  - 같은 종류 타일의 변형 섞기는 굽을 때 **위치로 정해지는 무작위**(다시 구워도 같은 결과)로

### 4-2. 타일 크기 — 두 목표가 서로 부딪힌다 (사용자가 결정)
32 px 타일에서 텍셀 = 1복셀(0.067 m)로 맞추면 한 칸 2.13 m → 치비 0.75칸 높이라 "캐릭터 1~1.5칸"과 맞지 않는다.

| 후보 | 한 칸 | 치비 높이 | 도트 밀도 |
|---|---|---|---|
| A | 32복셀 = 2.13 m | 0.75칸 | 바닥 = 캐릭터 (일치) |
| B | 16복셀 = 1.07 m | 1.5칸 | 바닥이 2배 촘촘 |
| C | 16복셀 + 그림을 16 px로 줄임 | 1.5칸 | 일치, 대신 타일 정보 절반 |

- 테스트 씬에서 후보를 나란히 보여 주고(치비·연못 옆), 도트 렌더링(P)을 켠 모습도 함께
- 어느 크기든 **바닥 칸 = 텃밭 로직 칸(크기·원점)**이어야 심기 미리보기가 흙 칸과 맞는다 (지금 Cell Size 1 m → B에 가까움. 바꾸면 `FarmConfig`도 함께)

### 4-3. Rule Tile 한계
- 경계 세트가 "풀 → X" 한 쌍이라 **풀을 바탕으로 깔고 흙·길·짚을 위에 칠하는** 규칙이어야 한다. X끼리 맞닿는 경계 그림은 없다
- 12장 구성이라 1칸짜리 가는 줄·대각선만 닿는 모양은 맞는 그림이 없다 → 샘플 구역에서 보여 주고 사용자에게 알린다

### 4-4. 그 밖
- 확인 1번(XZ 방향): Grid의 Cell Swizzle `XZY`로 바닥 방향 칠하기 가능할 것으로 예상 (실제 확인 필요)
- 확인 3번(그림자 줄무늬): Normal Bias 0 + 넓은 평면 + 해가 낮을 때 생길 수 있음 → 해 각도를 바꿔 가며 확인
- 확인 4번(이음새): 텍스처 Point·Mip Map 끔·압축 없음, 스프라이트 Full Rect. 굽기 방식이면 아틀라스 여백으로 해결
- 타일 팩 임포트 설정(Point·Mip Map 끔·압축 없음)은 farm부터 (전체 387장을 한 번에 바꿀지는 묻기)

## 5. 작업 순서 (새 대화)
1. MCP 연결: `set_active_instance VoxelMagicSchool@2d926515` 후 `Application.dataPath`가 `C:/Unity_Project/NCAI/VoxelMagicSchool/Assets`인지 확인 (도메인 리로드·플레이 진입 뒤마다 다시)
2. 0번 체크리스트 확인, 브랜치 제안
3. 치비·연못 크기 다시 측정 → 3-1 차이 보고, farm 타일 그림 확인(경계 방향 이름 뜻)
4. 패키지 2개 버전 확인 → 사용자에게 알리고 승인 후 설치
5. **설계 문서**(`Docs/DESIGN_TILEMAP.md`) 작성: 테스트 씬 구성, 팔레트·Rule Tile, 4-1 굽기 방식, 타일 크기 후보, 확인 1~4 방법 → **"제작 시작" 승인 대기**
6. 승인 후: 테스트 씬(`Scenes/Sandbox/` 아래 제안) + 팔레트 + 확인 1~4 + 타일 크기 후보 보여 주기 + 샘플 구역 약 10 × 10칸
7. 보고 (확인한 것 / 추측 구분) 후 사용자 확정 대기. 이후 단계는 요청 시에만

## 6. 정할 것 (사용자)
| 항목 | 선택지 | 언제 |
|---|---|---|
| 연못 배율 | 치비에 맞춰 (1, 1, 1)로 / 지금 (2, 2, 1.5) 유지 | 새 대화의 크기 보고 뒤 |
| 타일 크기 | A / B / C (4-2) | 테스트 씬에서 보고 |
| 본 방식 | Tilemap 직접 / 굽기 방식(추천) | 확인 2번 결과를 보고 |
| 작업 브랜치 | `main`에서 새 브랜치 (예: `feature/map-tiles`) | 시작할 때 |

## 7. 새 대화 첫 메시지 예시
```text
Docs/HANDOFF_MAP_TILES.md와 CLAUDE.md를 읽고 시작하자. 먼저 0번 체크리스트와 MCP 연결을 확인하고, 치비·연못 크기를 다시 재서 알려 줘. 그다음 패키지 버전을 확인해서 설치 전에 알려 주고, 설계 문서부터 정리해 줘. 내가 "제작 시작"이라고 하기 전에는 씬이나 에셋을 만들지 마.
```
