# UI 작업 인수인계 (2026-11-20 기준)

브랜치 `feature/tool-UI-editor`. 세션을 새로 시작하거나 대화가 압축된 뒤에는 **이 문서 → `UI_SPEC.md` 13장 → `CHANGELOG.md` 위쪽 UI 항목** 순서로 읽고 이어간다.

- 기준 문서: `Docs/UI_SPEC.md`. **13장 "설계 단계 결정"이 앞 장의 [잠정]·[미정]보다 우선**한다. README와 어긋나면 UI_SPEC을 따른다.
- 문서의 `Assets/UI/` = 실제 `Assets/_Project/Art/UI/`.
- 진행 방식: 단계마다 만들고 → 플레이 모드 캡처로 확인 → 보고 → 사용자 피드백(크기·간격은 대부분 사용자가 정함) → 커밋. 커밋은 사용자가 말할 때만.

## 1. 진행 상황

| 단계 | 상태 | 커밋 |
|---|---|---|
| 준비(임포트 규칙·아틀라스·Tiled 확인) | 완료 | `e2d61cc` (사용자 커밋) |
| 공통 기반(화면 스택·시간 정지·테마·UI_Root) | 완료 | `c5e1b02` |
| 에디터 씬·픽셀 폰트·위젯 프리셋·UI 작업대 | 완료 | `c5e1b02` |
| (a) 왼쪽 위 책·가방 버튼 + 의뢰 바 | 완료 | `717da48` |
| (d) 퀵슬롯 + 마력·기력 바 | 완료 | `6f0cb06` |
| (e) 점수 HUD | 완료 (글자색·숫자 가독성은 사용자 결정 대기) | `21da0c3` |
| 시간대(날씨) UI (8장) | 배치 완료(게임 시계 없음 → `GameContext.SetTime`으로 연결 예정, 회전 화살표 모양은 열린 질문) | 아래 커밋 |
| (f) 의뢰 드롭다운 목록(가상 스크롤)·완료 알림 → 자세히 패널 | **다음** | — |
| (b) 책 UI(탭 4개, 페이지 넘김 5-3) | 대기 | — |
| 확장 인벤토리(7-4) | 나중(퀵슬롯은 확장 고려해서만 만듦) | — |

## 2. 확정된 결정 (요약, 자세한 것은 UI_SPEC 13장)

- uGUI + TMP. **기준 높이 480**(1080p 2배, 가상 960×540). PPU 16. 배율 = floor(화면 높이 ÷ 480).
- 폰트: **갈무리11 / 갈무리11 Bold(12px)**. 고정(Static) 아틀라스 + 예비 Dynamic. 작은 글씨 후보 갈무리9(10px)·7(8px)은 Bold 없음.
- 시간 정지: 책·가방·자세히/게시판 = timeScale 0, 크리처 대화·수업 = 게임 시계만. 딤 없음(입력 막기 판은 투명).
- 텍스트는 `Data/UI/Texts_ko`(TextTable) 키로만. 키 이름은 `UiTextKeys`.
- 왼쪽 위: 아이콘 2배(32), 의뢰 바 폭 144·높이 = 줄 높이 16 + 여백 3×2, 제목 `수락한 의뢰(n)`(HudTitle 스타일, 띄어쓰기 +2px).
- 퀵슬롯: 칸 32×36, 아이콘 24(0.8배로 줄임), 선택 프레임 보간. 휠은 **퀵슬롯 위에서만 칸 변경, 게임 화면 위는 카메라 줌**.
- 상태 바: 기력(왼쪽) → 마력(오른쪽), 바 1.5배(원래 16×80 비율), 아이콘 1.125배(바보다 조금 넓게), 가장자리 8, 바 사이 4(확대 후 6).
- 시간대: 화면 오른쪽 위 1.5배(0.5 단위만, 1.2에서는 조각 사이 틈이 생겼음), 틀 뒤 받침으로 틈 가림, 틀은 v2(날짜 판 가운데·잎 없음), 시간대 그림은 틀보다 앞(8,8), 날짜는 HudTitle 굵게. 화살표 회전 중심 = 룰렛 반원 중심. 화살표는 정시마다 스텝 회전. 툴팁은 공용 `UI_Tooltip`(Overlay).
- 점수: 화면 위 가운데, 필기 → 실기, 숫자 v2·2배, 밑줄 필기 v1·실기 v2. 위치 손 조정은 테마 `Scores › Written/Practical Offsets`(묶음·글자·숫자 각각 x·y).

## 3. 어디에 무엇이 있나

- 테마(모든 크기·간격·색·스프라이트): `Data/UI/UiTheme.asset` — `Top Left`, `Quick Slots`, `Status Bars`, `Scores`, `Text Styles`, `Buttons`, `Panels`.
- 문구: `Data/UI/Texts_ko.asset`. 폰트 굽기 설정: `Data/UI/PixelFontSet.asset`. 임포트 규칙: `Data/Tools/UI/UiImportRules.asset`. 작업대 설정: `Data/Tools/UI/UiWorkbenchConfig.asset`.
- 프리팹: `Prefabs/UI/UI_Root.prefab`(층: HudStatic 0 / HudDynamic 10 / HudAnimated 20 / Popup 100 / Overlay 200), `Prefabs/UI/HUD/*`, 위젯 프리셋 `Prefabs/UI/Widgets/*`.
- UI_Root가 들어간 씬: `FarmPond`, `BackMountain`, `Creature_Cave`(부트스트랩 `_bindRoots`에 연결). 개발용 `Scenes/Tools/UIEditor.unity`(폰트 비교판).
- 코드: Core `Screens/`(ScreenStack·TimePause·HudState·ScreenPolicy), `Common/TextTable`·`IPlayerVitals`, `Requests/IAcceptedRequestList`. UI `Common/`(테마·스케일러·라우터·PixelLayout), `Widgets/`(Themed*), `HUD/`, `Requests/QuestBarView`. 에디터 `Tools/Editor/UI/`(빌더·검사기·작업대), `Editor/Fonts/`.
- 메뉴 `VoxelMagic/UI/`: 임포트 규칙 적용, 픽셀 폰트 다시 굽기, 테마 기본값·위젯 프리셋, UI 루트 프리팹, **HUD 만들기/**(왼쪽 위·퀵슬롯·상태 바·점수·시간대·툴팁), UI 에디터 씬 열기, **UI 작업대**(해상도·검사·굽기·테마).
- HUD 빌더를 다시 실행하면 해당 HUD 프리팹을 새로 만들고 UI_Root 안의 같은 이름을 바꾼다(테마 값은 덮어쓰지 않음, 비어 있는 것만 채움).

## 4. 열린 질문 · 할 일

- **점수 상한**: 지금 `ScoreBook`은 100점 상한(게임 규칙). 사용자가 "추후 바꾸자"고 함(UI 9-1은 상한 없는 점수를 가정, 4자리까지 표시 준비됨). 규칙 쪽 작업.
- 점수: 이름 글자색(임시 밝은 종이색), 어두운 붉은 숫자가 어두운 배경에서 잘 안 보임 → 사용자 결정 대기.
- 마력·기력 값 모델 없음(`GameContext.SetVitals`로 연결 예정) → 지금은 가득 찬 바.
- 시간대 UI: 게임 시계 없음(`GameContext.SetTime(IGameTime)`). [미정] 구간 시각(임시 방과후 16~19, 밤 19~24)·화살표 각도(임시 ±75°)·날짜 패널 내용(임시 `N일째`)·임박 효과(종류, 방과후→밤 시작 시각)·현실 시간 대비 속도. 런타임 회전 화살표가 도트가 어긋나 보이면 각도별로 미리 구운 화살표로 바꿀 것(7-3 방식 2 도구). 시간대 조각도 Sprout 폴더라 git 제외.
- Sprout 조각(퀵슬롯 칸·선택 프레임)을 아틀라스에 넣을지(Sprout은 git 제외라 팀원 쪽에서 비게 됨).
- `bar_ui/icon/`(옛 폴더)에 `bar_ui/icons/`와 같은 기력 아이콘이 남아 있음 — 지울지 사용자 결정.
- 기존 프로토타입 UI: `InventoryBar`·`ScoreHud`는 세 씬에서 꺼 둠, `QuestTrackerPanel`(오른쪽 위)은 (f)에서 교체. 모두 바꾼 뒤 스크립트 정리. `ControlsHintView` 안내 문구도 그때 확인.
- 의뢰 바 X 버튼: 환경 설정의 "의뢰 목록 표시" 토글과 함께 기능 연결.
- 입력 막기 판(UiScreenRouter)은 첫 팝업(자세히 패널·책) 때 실제로 클릭이 막히는지 확인.
- 실제 마우스 휠 동작(퀵슬롯 위/게임 화면 위)은 사용자가 직접 확인해야 함.

## 5. 작업할 때 알아 둘 것 (함정)

- 플레이 확인 때 **자동 저장을 끈다**(`GameSaveSystem.AutoSaveEnabled = false`). 안 끄면 테스트로 받은 아이템·점수가 저장 파일에 남는다.
- 캡처는 `manage_camera screenshot`, 폴더 `Temp/UiCaptures`(프로젝트 안이어야 함, git 제외).
- 컴파일·재연결 직후 Unity가 **플레이 모드에 들어가 있거나 다른 씬(UIEditor)이 열려 있는** 경우가 있었다. 에셋을 고치기 전에 `isPlaying`·활성 씬·dirty를 확인한다.
- `EditorSceneManager.NewScene` 전에 읽은 에셋 참조는 풀릴 수 있다 → 새 씬을 연 뒤 다시 읽는다.
- Unity `spriteBorder`는 (L, **B**, R, **T**) 순서(`UiImportRule`이 변환).
- 편집 모드에서 테마 변경은 위젯이 한 박자 뒤(delayCall)에 적용(OnValidate 안에서 RectTransform을 못 바꿈).
- 가운데 정렬은 앵커 0.5를 쓰지 않고 왼쪽 끝 기준 정수 px(`PixelLayout.PlaceBottomCenter`, 점수 줄)로 계산한다.
- Sprout Lands 폴더(원본·`(수정본)`)는 `.gitignore`.
- 컴파일 직후 Input System "Map index…" 오류가 콘솔에 몇 개 뜨는 일이 있었으나 플레이 중에는 재현되지 않음.
