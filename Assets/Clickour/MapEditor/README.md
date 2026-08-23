# Map Editor

## 1. 기능 검증 방법

`MapEditor` Scene을 실행하면 우측 `COMPONENTS` 패널에 Button, Switch, Slider, List prefab 카드가 보인다. 각 카드를 12×7 map canvas로 드래그하면 격자에 맞춰 새 요소가 배치되고 즉시 선택된다. 선택 요소는 본체 드래그로 이동하고 네 모서리 32px 파란 핸들로 최소 1×1까지 크기를 조절한다. `R`은 90° 회전, 방향키 또는 WASD는 한 칸 이동, Shift+방향키는 1/4칸 이동이다.

`Delete`로 삭제한 뒤 `Ctrl+Z`로 복원하고, `Ctrl+D`로 복제한 뒤 `Ctrl+Y`로 redo가 되는지 확인한다. `SNAP TO GRID`를 끄거나 드래그 중 Shift를 누르면 자유 배치된다. 요소끼리 겹칠 수 있어야 하며 List는 항상 다른 top-level 요소 아래에 그려져야 한다.

Button/Switch/Slider를 List 한 행 안에 네 모서리까지 완전히 포함되도록 놓는다. 선택 상태를 해제한 뒤 List를 이동하거나 회전하면 포함 요소가 같은 행과 함께 이동해야 한다. 한 모서리라도 행 밖에 있는 요소는 List 소속이 되지 않아야 한다.

각 완료 동작 후 `Assets/Clickour/Balance/StreamingAssets/clickour_balance.yaml`의 `map_data` 값이 바뀐다. WebGL에서는 Status가 `Session`, Editor/Standalone에서는 `Saved`로 시작한다. Edit Mode Test Runner의 `MapCodecTests`는 요소 위치, 크기, 회전과 List 행 소속의 Base64 왕복 및 행 경계에 정확히 맞는 요소의 포함 판정을 검증한다.

## 2. 기능 사용법

새 요소 종류는 `MapElementKind`와 Scene의 `MapEditorController.element_prefabs`에 prefab을 함께 추가한다. prefab root에는 `RectTransform`, `Image`, `MapEditorElement`가 필요하다. List prefab은 `MapListRows`에 고정 행 RectTransform 목록을 전달한다.

```csharp
using System;
using Clickour.Balance;
using Clickour.MapEditor;
using UnityEngine;

var document = MapCodec.Decode(BalanceDatabase.GetEncodedMap());
document.elements.Add(new MapElementData
{
    id = Guid.NewGuid().ToString("N"),
    kind = MapElementKind.Button,
    position = new Vector2(2, 1),
    size = new Vector2(2, 1)
});
BalanceDatabase.SetEncodedMap(MapCodec.Encode(document));
```

저장 좌표와 크기는 grid cell 단위이고 Scene 표현에서만 `cell_size` 픽셀로 변환된다. `owner_list_id`와 `owner_row`는 포함 요소의 행 소속을 나타낸다.

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `MapElementKind.cs` | 배치 가능한 요소 종류 |
| `MapElementData.cs` | 요소 ID, 변환, List 소속 저장 데이터 |
| `MapDocument.cs` | 맵 크기와 요소 목록 저장 루트 |
| `MapCodec.cs` | JSON map document와 YAML용 Base64 값 변환 |
| `MapEditorController.cs` | prefab 배치, 선택, 단축키, undo/redo, 저장, layer/ownership 조정 |
| `MapEditorElement.cs` | 개별 요소 선택, 본체 드래그, 모서리 근접 리사이즈 |
| `MapResizeHandle.cs` | 네 모서리의 WebGL pointer down/drag/up 전달 |
| `MapPaletteItem.cs` | 우측 palette drag preview와 drop |
| `MapListRows.cs` | 행 layout과 완전 포함 판정 |
| `Prefabs/*.prefab` | Scene에 참조된 네 요소 원형 |
| `Art/grid.svg` | 12×7 editor grid 벡터 원본 |
| `Art/grid.png` | Inkscape로 변환한 map canvas Sprite |
| `Art/back_button.svg` | Material pill과 leading arrow를 분리한 뒤로가기 버튼 원본 |
| `Art/list_preview.svg` | palette용 Material one-line List 축약 원본 |

`MapEditorController`가 정식 `MapDocument` 상태를 소유하고 Unity UI Component는 입력과 표현만 연결한다. MapEditor는 Balance와 Core 입력을 참조하지만 GameFlow 또는 실제 기믹 assembly를 참조하지 않는다. Scene 전환은 GameFlow가 MapEditor 내부를 알지 않은 채 담당한다.
