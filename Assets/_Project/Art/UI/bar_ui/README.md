# 세로 상태 바 UI 에셋 (마력 / 기력, 픽셀아트, 16px 기준)

퀵슬롯(Sprout Lands 인벤토리 칸)과 같은 갈색 입체 프레임에, 마력(보라)과 기력(연두)이 아래에서부터 차오르는 세로 바입니다. 픽셀은 모두 새로 그렸고, 색 구성만 인벤토리 칸에 맞췄습니다(Sprout Lands 라이선스 제약을 받지 않음). 아이콘과 텍스트는 포함하지 않았습니다.

## 파일

| 파일 | 크기 | 용도 |
|---|---|---|
| `bar_frame_vertical.png` | 16×20 | 바 프레임 (9-slice, 세로로 늘림) |
| `bar_fill_mana.png` | 6×16 | 마력 채움 (보라) |
| `bar_fill_stamina.png` | 6×16 | 기력 채움 (연두) |
| `bar_cap_mana.png` | 6×2 | 마력 채움 윗면 하이라이트 (선택) |
| `bar_cap_stamina.png` | 6×2 | 기력 채움 윗면 하이라이트 (선택) |
| `bar_icon_plate.png` | 20×20 | 바 위에 놓을 아이콘 받침 (안쪽 12×12) |
| `_preview/` | | 확인용 이미지. 게임에는 쓰지 않음 |

## Unity 임포트 값

공통: Filter Mode = Point, Compression = None, Mesh Type = Full Rect, PPU = 16 (Canvas의 Reference Pixels Per Unit도 16)

| 스프라이트 | Border (L / T / R / B) | Image Type |
|---|---|---|
| bar_frame_vertical | 5 / 5 / 5 / 5 | Sliced (가로 16px 고정, 높이만 변경) |
| bar_fill_* | 없음 | Filled (Vertical, Origin = Bottom) |
| bar_cap_* | 없음 | Simple |
| bar_icon_plate | 없음 | Simple |

프레임의 가운데 영역은 한 색이라 어떤 높이로 늘려도 도트가 깨지지 않습니다(높이 20~140에서 직접 그린 결과와 픽셀 단위로 일치하는 것을 확인했습니다). 최소 높이는 20px입니다.
가로를 16px보다 넓히면 채움(6px)과 어긋나므로, 폭을 바꾸려면 채움 스프라이트도 새로 만들어야 합니다.

## 조합 방법

1. 프레임 Image를 만들고 높이를 H로 정합니다(정수 픽셀).
2. 채움 Image는 프레임의 **안쪽 영역**에 둡니다. 프레임의 위·아래·좌·우에서 각각 5px 안쪽이며, 폭은 6px, 최대 높이는 `H − 10`입니다.
3. 채움 Image는 Image Type = Filled, Fill Method = Vertical, Fill Origin = Bottom으로 두고, 값(0~1)을 Fill Amount에 넣습니다.
4. **Fill Amount는 정수 픽셀로 맞춥니다.** `fillAmount = round(value × (H − 10)) / (H − 10)`. 그렇지 않으면 채움 윗면이 반 픽셀 걸쳐 흐릿해 보입니다.
5. 윗면 하이라이트(cap)를 쓰려면 채움 윗면 위치에 놓고(채움의 맨 위 2행과 겹침), 채움 높이가 2px 미만이면 숨깁니다.
6. 아이콘 받침은 바 위쪽 중앙에 놓습니다(예: 바보다 좌우 2px씩 넓고, 바와 2px 간격). 안쪽 12×12에 아이콘을 둡니다.
7. 기준 해상도(640×360) 기준 예시: 프레임 높이 80px, 바 두 개 사이 간격 10px. 퀵슬롯·시간대 UI와 함께 둔 모습은 `_preview/hud_mock_3x.png`에 있습니다.

## 색 팔레트

프레임: 외곽선 `#90625d`(아래쪽 `#6b4b5b`), 테두리 `#dcb98a` / `#aa7959`, 몸통 `#c49a6c`, 안쪽 홈 `#2a1a28` / `#5a3d4f`, 빈 칸 `#3a2533`
마력(보라): `#dcc0f7` `#bb8cec` `#9a5fdc` `#7a42b8` `#5c2d94`
기력(연두): `#e4f8b4` `#c6ef84` `#a4dd5a` `#80c040` `#5a9a2e`
