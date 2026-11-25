# quest_ui — 수락한 의뢰 드롭다운 에셋 (1차)

책 UI와 같은 크림색 종이 톤. 가장자리는 깔끔하고, 찢어진 곳은 64px 주기마다 변 하나에 한 군데(패널 한 변에 한두 개)뿐이다. 얼룩은 없다. Claude 제작(제약 없음). 설계는 `ui_spec.md` 10장.

## 파일

| 파일 | 크기 | 용도 |
|---|---|---|
| quest_bar_paper.png | 76×76 | 드롭다운 **바** 배경 (조금 진한 종이색) |
| quest_panel_paper.png | 76×76 | 펼친 **목록 패널** 배경 |
| quest_detail_paper.png | 76×76 | **자세히 패널** 배경 (무늬가 다른 변형) |
| quest_toast_paper.png | 76×76 | **완료 알림 창** 배경 (조금 밝은 종이색) |
| buttons/arrow_down_{normal,hover,pressed}.png | 12×12 | 접힌 상태의 아래 화살표 버튼 |
| buttons/arrow_up_{normal,hover,pressed}.png | 12×12 | 펼친 상태의 위 화살표 버튼 |
| buttons/close_{normal,hover,pressed}.png | 12×12 | 닫기(X) 버튼. 바와 자세히 패널 둘 다 사용 |
| scrollbar/scroll_track.png | 8×16 | 스크롤바 트랙, 9-slice 경계 L0 / T4 / R0 / B4 (세로로만 늘림) |
| scrollbar/scroll_handle.png | 6×16 | 스크롤바 핸들, 9-slice 경계 L0 / T3 / R0 / B3 (세로로만 늘림) |
| _preview/ | | 확인용 이미지 (게임에서 쓰지 않음) |

## 종이 패널 4종 사용법 (Image Type = Tiled + 스프라이트 Border)

- 스프라이트 Border는 **L6 / T6 / R6 / B6** (Sprite Editor에서 입력). Mesh Type Full Rect, Point, 압축 없음, PPU 16.
- Image Type은 **Tiled**, Fill Center 켬. 중앙 64×64와 사방 가장자리(길이 64)가 반복되어 임의의 가로·세로 크기를 채운다. 찢김은 64px 주기로 반복되고 이음매는 보이지 않는다(`_preview/panel_height_check_2x.png`).
- 마지막 반복 조각은 잘려서 들어가지만, 무늬가 원래 불규칙해서 티가 나지 않는다.
- 크기는 정수 픽셀. 최소 크기는 12×12(양쪽 Border 합).
- 모서리 4개는 반복되지 않으며 1~2px만 둥글게 깎여 있다.
- **확인 필요**: Unity의 Tiled가 Border 영역도 길이 방향으로 반복해서 그리는지 에디터에서 한 번 확인한다. 반복하지 않고 늘리는 경우에는 이 스프라이트를 영역별로 잘라(모서리 4개, 가장자리 4개, 중앙) 조각으로 쓰는 스크립트를 만든다. 영역 좌표는 위 규칙(모서리 6, 가장자리 64, 중앙 64)이다.

## 조합 예 (잠정 치수, 최종 값은 레이아웃 확정 때)

- 바: 144×22. 왼쪽 5px 위치에 화살표 버튼(12×12), 오른쪽 끝에서 5px 떨어진 곳에 X 버튼(12×12). 제목 텍스트는 그 사이.
- 목록 패널: 바 바로 아래에 붙인다. 가로 144 고정. 높이 = 12(양쪽 가장자리) + 2×위아래 패딩(4) + 항목들. 위·아래 패딩은 같은 값.
- 스크롤바: 패널 오른쪽 안쪽. 트랙 폭 8, 핸들 폭 6(트랙 안쪽 1px 여백).
- 호버는 `hover` 스프라이트, 눌림은 `pressed` 스프라이트(Button의 Sprite Swap)를 쓴다.

## 팔레트

- 종이 `#F4E8CC`, 가장자리 안쪽 띠 `#E9D6A8`, 외곽선 `#5C4128`
- 바는 `#EAD8B0` 계열(조금 진함), 토스트는 `#F7EBCF` 계열(조금 밝음). 책 UI의 PAPER 팔레트와 같은 계열이다.
- 버튼 외곽선 `#5C4128`, 화살표 `#5C4128`, X `#8C2F2A`
