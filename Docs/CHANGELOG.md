# 변경 기록

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
