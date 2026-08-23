# List UI

## 1. 기능 검증 방법

Game Scene의 세로 목록은 각 행 마지막 칸의 햄버거를 홀드하면 커서 중력이 YAML의 `list_vertical_hold_gravity_scale`로 낮아지고, 낙하 중 다른 행 중심을 통과할 때 순서가 바뀐다. 위/아래 방향키와 W/S는 이동에 사용하지 않는다. 가로 목록은 햄버거 홀드 중 좌/우 방향키 또는 A/D로 이동하며 다른 열 중심을 통과하면 순서가 바뀐다. 행 또는 열 내부의 라벨과 샘플 UI는 부모 항목과 함께 움직여야 한다.

외형은 Material 3 one-line List item의 56dp 행, 16dp 좌우 여백, surface 배경과 24dp 상당 trailing drag icon anatomy를 따른다. Game Scene WebGL 스크린샷에서 행마다 별도 카드 외곽선이 없고, 얇은 divider와 trailing hamburger만 보이는지 확인한다.

## 2. 기능 사용법

목록 루트에 `ReorderableList`를 두고 순서대로 항목 Transform을 전달한다. 각 항목의 진행 방향 마지막 칸에 `ListHandle`, Trigger Collider와 햄버거 Sprite를 두고 `Configure`로 소유 목록과 항목을 연결한다. 항목 내부의 다른 UI는 항목 Transform 자식으로 둔다.

```csharp
list.Configure(false, rows);
handle.Configure(list, row.transform);
```

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `ReorderableList.cs` | 슬롯 순서, 드래그 항목, 방향별 커서 이동과 재배치 |
| `ListHandle.cs` | 햄버거 Collider의 클릭 수명주기를 목록으로 전달 |
| `Art/list_panel.svg` | 목록 외곽 시각 원본 |
| `Art/list_row.svg` | 항목 시각 원본 |
| `Art/hamburger.svg` | 클릭 핸들 시각 원본 |
| `Art/*.png` | Inkscape 변환 Sprite |

항목의 모든 콘텐츠는 같은 부모 Transform을 공유한다. 목록은 내부 콘텐츠 타입을 알지 않으므로 버튼, 스위치 등 다른 기능을 자식으로 포함할 수 있다.

형상 기준: https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/list_tile.dart
