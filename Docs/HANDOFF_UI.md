# UI 작업 인수인계 (2026-11-24 기준)

브랜치 `feature/tool-UI-editor`. 세션을 새로 시작하거나 대화가 압축된 뒤에는 **이 문서 → `UI_SPEC.md` 13장 → `CHANGELOG.md` 위쪽 UI 항목** 순서로 읽고 이어간다.

- **기능 연결(마력·기력, 시간대, 점수, 의뢰, 퀵슬롯, 책 메뉴)과 새 UI 제작 틀은 `Docs/UI_INTEGRATION_GUIDE.md`**에 정리돼 있다. 게임 규칙 쪽에서 UI에 기능을 붙이거나 다른 대화가 UI를 이어 만들 때 먼저 읽는다.

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
| (f) 의뢰 드롭다운 목록(가상 스크롤)·완료 알림 | 완료 | 아래 커밋 |
| 확인 창(예/아니요) | 2026-11-26 텍스처 창 `UI_Confirm`(`confirm_ui/confirm_panel.png` + 책 페이지 버튼)으로 교체, **커밋 전** | — |
| 자세히 패널(10-5) | 책 "의뢰 목록" 탭으로 대체(사용자 결정). 2026-11-26 목록 마우스 올림·스크롤바 끌기·고른 칸 새 그림·오른쪽 글자 크기 단계·구분선·대사 들여쓰기, **커밋 전** | — |
| (b) 책 틀 + 의뢰 목록 탭 + 페이지 넘김(5-3)·탭 휠 | 완료(사용자 확인) | 아래 커밋 |
| (b) 책 메뉴 탭(배치만: 왼쪽 장 표지, 오른쪽 메인 화면으로·게임 종료·책 닫기, 책 닫기만 동작) | 완료(`22c9c88` + 장 표지는 커밋 전) | — |
| (b) 책 나머지 탭 내용(환경 설정·마법 스펠·마법진) | 대기 | — |
| 대사 UI(6장, 초상화 대사창) | 완료(`1738420`). 2026-11-24 초상화 72·이름표 72×14로 확대(A안, 패널 높이 96), 배율 1.25, ▼ 여백 맞춤, 이름 세로 가운데, '그것' 초상화 7종(72로 줄여 구움) 적용. 2026-11-26 크리처 대화 메뉴·의뢰 고르기를 대사창 안 선택지로(6-4), **커밋 전**. 말풍선(초상화 없는 대사)은 범위 밖 | — |
| 확장 인벤토리(7-4) | 나중(퀵슬롯은 확장 고려해서만 만듦) | — |

## 2. 확정된 결정 (요약, 자세한 것은 UI_SPEC 13장)

- uGUI + TMP. **기준 높이 480**(1080p 2배, 가상 960×540). PPU 16. 배율 = floor(화면 높이 ÷ 480).
- 폰트: **갈무리11 / 갈무리11 Bold(12px)**. 고정(Static) 아틀라스 + 예비 Dynamic. 작은 글씨 후보 갈무리9(10px)·7(8px)은 Bold 없음.
- 시간 정지: 책·가방·게시판 = timeScale 0, 크리처 대화·수업 = 게임 시계만. 팝업 뒤 딤 = 검정 알파 0.6(2026-11-20 사용자 결정), 바깥 클릭 = 닫기.
- 텍스트는 `Data/UI/Texts_ko`(TextTable) 키로만. 키 이름은 `UiTextKeys`.
- 왼쪽 위: 아이콘 2배(32), 의뢰 바 폭 144·높이 = 줄 높이 16 + 여백 3×2, 제목 `수락한 의뢰(n)`(HudTitle 스타일, 띄어쓰기 +2px).
- 퀵슬롯: 칸 32×36, 아이콘 24(0.8배로 줄임), 선택 프레임 보간. 휠은 **퀵슬롯 위에서만 칸 변경, 게임 화면 위는 카메라 줌**.
- 상태 바: 기력(왼쪽) → 마력(오른쪽), 바 1.5배(원래 16×80 비율), 아이콘 1.125배(바보다 조금 넓게), 가장자리 8, 바 사이 4(확대 후 6).
- 시간대: 화면 오른쪽 위 1.5배(0.5 단위만, 1.2에서는 조각 사이 틈이 생겼음), 틀 뒤 받침으로 틈 가림, 틀은 v2(날짜 판 가운데·잎 없음), 시간대 그림은 틀보다 앞(8,8), 날짜는 HudTitle 굵게. 화살표 회전 중심 = 룰렛 반원 중심. 화살표는 정시마다 스텝 회전. 툴팁은 공용 `UI_Tooltip`(Overlay).
- 책·가방 버튼과 점수 숫자·밑줄은 **외곽선 v2** 채택(2026-11-20, 배경과 구분). 메뉴 `VoxelMagic/UI/외곽선 버전/`으로 원본과 바꿔 볼 수 있음. 아이콘 20px × 2 = 40.
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
- 시간대 UI: 날짜 판은 `D-n`(8일 플레이, 1일째 D-8, 테마 `Play Days` 임시 — 규칙 쪽으로 옮길 것). 게임 시계 없음(`GameContext.SetTime(IGameTime)`). [미정] 구간 시각(임시 방과후 16~19, 밤 19~24)·화살표 각도(임시 ±75°)·임박 효과(종류, 방과후→밤 시작 시각)·현실 시간 대비 속도. 런타임 회전 화살표가 도트가 어긋나 보이면 각도별로 미리 구운 화살표로 바꿀 것(7-3 방식 2 도구). 시간대 조각도 Sprout 폴더라 git 제외.
- Sprout 조각(퀵슬롯 칸·선택 프레임)을 아틀라스에 넣을지(Sprout은 git 제외라 팀원 쪽에서 비게 됨).
- `bar_ui/icon/`(옛 폴더)에 `bar_ui/icons/`와 같은 기력 아이콘이 남아 있음 — 지울지 사용자 결정.
- 기존 프로토타입 UI: `InventoryBar`·`ScoreHud`·`QuestTrackerPanel`은 세 씬에서 꺼 둠(스크립트는 아직 남음). `ControlsHintView`(왼쪽 아래 조작 안내)가 완료 알림과 겹침 → 정리 필요. 모두 바꾼 뒤 스크립트 정리. `ControlsHintView` 안내 문구도 그때 확인.
- 책 메뉴 탭: 메인 화면·게임 종료는 기능이 생기면 `MenuBookPage.MainMenuRequested`·`QuitRequested`에 연결(확인 창 [잠정]). 버튼은 Claude가 그린 깔끔한 9-slice `book_ui/buttons/btn_page_*`(사용자가 바꿀 수 있음), 글자는 여백이 좌우·상하 같게 자동 정렬.
- 책 탭 휠 전환은 페이지 넘김과 함께 넣음(2026-11-23).
- 의뢰 바 X = 바·목록 끄기, 책 의뢰 탭 체크 상자로 다시 켜기(연결됨). 켜짐 상태 저장은 아직(환경 설정 저장 때).
- 입력 막기 판: 책에서 확인. 판을 누르면 맨 위 화면이 닫힌다(바깥 클릭 = 닫기).
- 실제 마우스 휠 동작(퀵슬롯 위/게임 화면 위)은 사용자가 직접 확인해야 함.

## 5. 작업할 때 알아 둘 것 (함정)

- 치트: F7 = 진행 중 의뢰의 모자란 재료 1개, F12 = Inspector에서 고른 아이템, F10 = 의뢰 게시판, F11 = 의뢰 전달, 대사창에서 `]`/`[` = 초상화 표정 넘기기. 의뢰 목록 여러 건은 메뉴 `VoxelMagic/UI/미리보기/`(가짜, 의뢰 바 개수는 실제 값 그대로).
- 클릭을 받는 위젯을 새 층에 둘 때는 그 층 Canvas에 GraphicRaycaster가 있는지 확인한다(HudAnimated에 없어서 완료 알림 X가 안 눌렸음).
- 플레이 확인 때 **자동 저장을 끈다**(`GameSaveSystem.AutoSaveEnabled = false`). 안 끄면 테스트로 받은 아이템·점수가 저장 파일에 남는다.
- 캡처는 `manage_camera screenshot`, 폴더 `Temp/UiCaptures`(프로젝트 안이어야 함, git 제외). Unity 창이 포커스가 없으면 게임 화면이 다시 그려지지 않아 캡처가 이전 화면일 수 있다(같은 캡처를 두 번 찍어 비교하거나, 값만 뽑아 확인).
- 컴파일·재연결 직후 Unity가 **플레이 모드에 들어가 있거나 다른 씬(UIEditor)이 열려 있는** 경우가 있었다. 에셋을 고치기 전에 `isPlaying`·활성 씬·dirty를 확인한다.
- `EditorSceneManager.NewScene` 전에 읽은 에셋 참조는 풀릴 수 있다 → 새 씬을 연 뒤 다시 읽는다.
- Unity `spriteBorder`는 (L, **B**, R, **T**) 순서(`UiImportRule`이 변환).
- 편집 모드에서 테마 변경은 위젯이 한 박자 뒤(delayCall)에 적용(OnValidate 안에서 RectTransform을 못 바꿈).
- 가운데 정렬은 앵커 0.5를 쓰지 않고 왼쪽 끝 기준 정수 px(`PixelLayout.PlaceBottomCenter`, 점수 줄)로 계산한다.
- Sprout Lands 폴더(원본·`(수정본)`)는 `.gitignore`.
- 컴파일 직후 Input System "Map index…" 오류가 콘솔에 몇 개 뜨는 일이 있었으나 플레이 중에는 재현되지 않음.
