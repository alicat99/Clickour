# Game Flow

## 1. 기능 검증 방법

Build Settings의 첫 Scene인 `Game`을 실행한다. `~` 키를 누르면 `Debug` Scene으로 이동한다. Debug에는 `map_data`를 제외한 YAML 1-depth 키가 키당 한 행으로 표시되며 값 칸의 편집을 끝내면 즉시 런타임 값에 반영된다. `MAP EDITOR` 버튼은 prefab palette 기반 `MapEditor` Scene으로 이동한다. `BACK TO DEBUG` 버튼으로 Debug로 돌아간다.

WebGL에서는 빌드 원본을 쓸 수 없으므로 Debug/Map 편집이 현재 실행 세션에만 유지된다. Editor 및 Standalone에서는 원본 YAML에 저장된다. 맵 편집과 인코딩의 검증 방법은 `Assets/Clickour/MapEditor/README.md`에 둔다.

## 2. 기능 사용법

세 Scene 모두 `BalanceBootstrap`과 비활성 시작 `Content` 루트를 사용한다. Game의 `SceneNavigator`만 `debug_shortcut_enabled`를 켠다. Debug의 `DebugBalancePanel.Row`는 Scene에 배치된 키 라벨과 입력 필드를 연결한다. MapEditor Scene은 GameFlow가 전환만 담당하고 편집 책임은 독립 MapEditor assembly에 둔다.

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `SceneNavigator.cs` | Game/Debug/MapEditor 전환과 `~` 단축키 |
| `DebugBalancePanel.cs` | 고정 키 행과 YAML 런타임 값 연결 |
| `Editor/ClickourWebGLBuild.cs` | 필수 최적화 설정과 순차 WebGL 빌드 경로 적용 |
| `Art/background.svg` | 격자 배경 벡터 원본 |
| `Art/background.png` | Inkscape 변환 배경 Sprite |
| `Scenes/*.unity` | 런타임 생성 없이 배치된 세 실행 Scene |

GameFlow는 Scene 전환과 빌드를 조립하지만 기믹 또는 맵 편집 구현을 소유하지 않는다.

WebGL 플레이테스트 빌드는 `Clickour > Build Next WebGL`을 실행한다. 이 명령은 기존 폴더를 덮어쓰지 않고 다음 `Builds/WebNNN`을 선택한다.
기능 폴더에 둔 단일 YAML은 빌드 완료 후 산출물의 `StreamingAssets` 루트로 복사된다.
