# 인수인계 — 저장·불러오기 확장 (슬롯 선택 · 불러오기 화면)

> 작성: 2026-11-10 (캐릭터 시스템 단계 2-① 직후). 새 대화는 이 문서와 `CLAUDE.md`부터 읽는다.
> 이 문서는 **지금 있는 저장 구조 설명 + 확장 제안서**다. 새 대화는 이것을 바탕으로 설계를 정리해 설명하고, 사용자가 **"제작 시작"**이라고 해야 만든다.
> 관련: `Docs/DESIGN_CHARACTER.md` 9-2(저장 설계), `Docs/SETUP.md` "저장 파일", `Docs/CHANGELOG.md` 2026-11-10

---

## 0. 사용자에 대해
- 자체 엔진 경험이 있고 Unity는 배우는 중 → Unity 방식을 쓰던 개념과 **비교해서** 쉽게 설명한다
- 사용자가 예로 든 방식이 아니라도 Unity·객체 지향·최적화 관점에서 더 나은 방식이 있으면 제안 → 수락하면 만든다
- 답변은 한국어. 커밋·푸시는 사용자가 요청할 때만, 커밋 전 브랜치 확인

## 1. 목표 (다음 대화)
1. **저장 슬롯 고르기**: 여러 슬롯(예: 3개)에 저장, 슬롯마다 요약(저장 시각·점수·플레이 위치 등) 표시
2. **불러오기 화면**: 게임 시작 시 "새 게임 / 이어하기 / 슬롯 선택", 슬롯 지우기
3. (사용자와 정할 것) 수동 저장 버튼·자동 저장 시점, 마지막 씬·도착 지점까지 저장해 그 자리에서 이어하기, 시작 화면 씬을 따로 둘지

## 2. 지금 구조 (2026-11-10 만든 것)
```
[Core]  PlayerProgress ── 인벤토리 · 점수 · 도감 · 의뢰 · 밭 보관본(FarmPlotState)   ← 씬을 넘어 유지
        GameSession (정적) ── Progress 하나 보관, Capture() 때 Capturing 이벤트(씬 소속 데이터 옮겨 담기)
        SaveData (JSON용, Version 1) ── 칸(ItemId·개수)·선택 칸·점수·도감 ID·완료/진행 의뢰 ID·밭(SeedId·칸·물)·SavedAtUtc
        SaveMapper ── ToData(progress) / Apply(data, progress, catalog) → 모르는 ID 목록
        DefinitionCatalog (SO, Data/Save/) ── ID → 아이템·의뢰 에셋 (의뢰 ID = 에셋 이름)
        ISaveStore ── Exists / TryRead / Write / Delete (슬롯 번호)
[Gameplay] JsonFileSaveStore ── persistentDataPath/Saves/save_<슬롯>.json, 임시 파일 → 바꿔치기
           GameSaveSystem (정적) ── ActiveSlot(지금 0 고정), GetOrCreateProgress, Save, AutoSave, 종료 때 저장
           GameBootstrap ── 진행 상태 받기 → GameContext 만들기 → 연결 → 밭 다시 심기, Capturing 때 밭 보관
           SceneTransitionDirector ── 페이드 아웃 뒤·로드 전에 AutoSave
[Editor]  메뉴 VoxelMagic/저장/ ── 정의 목록 채우기(+부트스트랩 연결) · 저장 폴더 열기 · 저장 지우기(슬롯 0)
```
- 흐름: 텃밭 씬 시작 → `GameSaveSystem.GetOrCreateProgress` → (세션에 있으면 그대로 / 슬롯 파일 있으면 불러오기 / 없으면 새 게임 + 시작 아이템) → `GameSession.Begin`
- 복도 씬은 플레이어 데이터를 쓰지 않는다. 게임을 복도에서 시작하면 진행 상태가 없어 저장도 안 됨 (텃밭에 가야 생김) — 시작 화면을 만들면 여기서 진행 상태를 먼저 정하게 바꿀 것
- 단위 테스트: `Tests/SaveSystemTests.cs` (JSON 왕복, 모르는 ID, 이벤트, 의뢰 재지급 없음, 점수 범위, 세션 보관)

## 3. 확장 제안 (새 대화에서 다듬기)
| 이름 | 종류 | 역할 |
|---|---|---|
| `SaveSlotSummary` | Core 데이터 | 슬롯 목록 화면용 요약 (저장 시각, 점수, 씬 이름 등). `SaveData`에서 만들거나 작은 `meta` 파일로 따로 둬서 목록을 빨리 읽기 |
| `GameSaveSystem.SelectSlot(int)` / `StartNewGame(slot)` / `LoadSlot(slot)` | 기존 확장 | `ActiveSlot` 바꾸기, `GameSession.Clear()` 후 새로 만들기·불러오기 |
| `ISaveStore.ListSlots()` | 기존 확장 | 슬롯 목록 (파일 이름 규칙 `save_<n>.json`) |
| 시작 화면 씬 + UI | 새 | 새 게임 / 이어하기 / 슬롯 고르기·지우기. 고른 뒤 `SceneTransitionDirector`로 이동 |
| `SaveData` 버전 2 | 형식 변경 | 마지막 씬·도착 지점 ID 등 — `Version` 올리고 1 → 2 변환 함수(`SaveMigrations`)로 옛 파일도 읽기 |
- 슬롯 개수·자동 저장 여부 같은 숫자·설정은 `SaveConfig`(SO)로 (코드에 박지 않기)

## 4. 알아 둘 점·주의
- **씬을 넘는 데이터(인벤토리·점수·의뢰·도감)를 구독하는 UI·부품은 반드시 OnDestroy에서 구독을 푼다.** 안 그러면 사라진 씬의 UI가 계속 불린다 (2026-11-10 `QuestTrackerPanel`·`RequestBoardWindow` 고침)
- 새 아이템·의뢰 에셋을 만들면 `VoxelMagic/저장/정의 목록 채우기`를 다시 실행해야 불러오기 때 찾을 수 있다 (자동화 검토: 에셋 저장 시 자동 갱신)
- 의뢰 ID는 에셋 이름이다 → **의뢰 에셋 이름을 바꾸면 옛 저장의 의뢰 진행을 못 찾는다** (필요하면 의뢰에도 `_id` 필드 추가)
- `Application.quitting`은 에디터에서 플레이를 끌 때도 불려서 저장된다. 시험할 때 새 게임으로 시작하려면 `저장 지우기(슬롯 0)` 또는 부트스트랩 `_autoSave` 끄기
- 스탯(캐릭터 시스템 단계 3)이 생기면 `PlayerProgress`에 넣고 `SaveData`·`SaveMapper`에 칸 추가 + 버전 올리기
- 저장 위치는 프로젝트 밖(`AppData/LocalLow/...`)이라 git에 안 들어간다. Player Settings 회사·제품 이름을 바꾸면 위치가 바뀐다 (프로젝트 설정 변경은 사용자에게 먼저 묻기)

## 5. 새 대화 첫 메시지 예시
```text
Docs/HANDOFF_SAVE_LOAD.md와 CLAUDE.md를 먼저 읽고 시작하자. 이번 대화에서는 저장 슬롯 선택과 불러오기(시작) 화면을 만들 거야.
지금 브랜치를 확인해서 알려 주고, 문서 2번 구조를 코드에서 다시 확인한 뒤, 설계를 정리해서 1번 3)의 정할 것들을 나한테 물어봐 줘.
"제작 시작"이라고 하기 전에는 스크립트·씬·에셋을 만들지 마.
```
