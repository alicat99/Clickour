# Balance

## 1. 기능 검증 방법

`Game`, `Debug`, `MapEditor` Scene의 `BalanceBootstrap`이 `Content`를 비활성화한 채 `StreamingAssets/clickour_balance.yaml`을 읽고 다시 활성화한다. Play Mode에서 Debug Scene의 각 입력 행을 수정하면 같은 키의 런타임 값이 바뀐다. `map_data`는 Debug Scene에 노출되지 않으며 MapEditor Scene에서만 갱신된다. Edit Mode Test Runner의 `BalanceDocumentTests`는 1-depth YAML의 파싱, 값 변경, 순서 보존 직렬화를 검증한다.

## 2. 기능 사용법

Scene 진입점에 `BalanceBootstrap`을 두고 실제 콘텐츠 루트를 `content_root`로 지정한다. 콘텐츠가 활성화된 뒤 다른 기능은 `BalanceDatabase.GetFloat("grid_cell_size")`처럼 값을 읽는다. 편집기는 `BalanceDatabase.Set` 또는 `SetEncodedMap`을 호출한 뒤 `Save`를 호출한다. WebGL에서는 현재 세션에만 반영되며 원본 빌드 파일은 수정하지 않는다.

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `BalanceDocument.cs` | 1-depth YAML의 순서와 키-값 상태 소유 |
| `BalanceDatabase.cs` | 단일 YAML 로드, 타입 변환, 런타임 변경 및 저장 |
| `BalanceBootstrap.cs` | 비동기 StreamingAssets 로드 전 Scene 콘텐츠 활성화 방지 |
| `StreamingAssets/clickour_balance.yaml` | 게임 밸런스와 인코딩된 맵 데이터의 단일 원본 |

`map_data`는 Base64로 인코딩된 맵 문자열이며 일반 밸런스 키와 분리해 Debug UI에서 숨긴다.
