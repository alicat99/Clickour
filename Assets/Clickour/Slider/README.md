# Slider

## 1. 기능 검증 방법

Game Scene의 회색/파란색, 가로/세로 및 반대 방향 슬라이더를 사용한다. 핸들 이외의 트랙을 클릭해도 반응하지 않아야 한다. 핸들에 겹쳐 클릭한 채 방향키 또는 WASD를 누르면 월드 기준 입력이 슬라이더 축에 투영되어 가속하고, 떼면 현재 핸들 속도로 발사된다. 가로형은 한 칸 점프용 수직 속도가 추가되고 세로형은 축 속도만 사용한다. 파란형은 시작점부터 핸들까지 진한 파랑으로 채워지고 그 부분만 고체 발판이다.

홀드 시작부터 커서가 전체 슬라이더의 `exit_zone`을 벗어날 때까지 진행 영역과의 충돌이 무시되어, 핸들을 놓는 순간 켜진 Collider에 걸리지 않아야 한다.

## 2. 기능 사용법

루트에 `SliderMechanic`, 트랙과 진행 영역 SpriteRenderer, 진행 영역 `solid_collider`, 전체 크기 Trigger `exit_zone`을 둔다. 핸들 자식만 Clickable 레이어와 Trigger Collider를 사용한다. `Configure`의 `travel`은 핸들 중심이 이동할 수 있는 로컬 X 길이다. 네 방향은 루트 Transform 회전으로 지정한다.

```csharp
mechanic.Configure(SliderKind.Blue, handle, track, fill, solid, exit_zone, 4);
```

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `SliderKind.cs` | 회색/파란색 타입 |
| `SliderMechanic.cs` | 핸들 가속, 진행 상태, 발판 범위, 해제 발사 |
| `Art/*.svg` | 트랙, 핸들, 채움 영역의 벡터 원본 |
| `Art/*.png` | Inkscape 변환 Sprite |

충돌 예외는 Cursor 타입이 아니라 상호작용한 이동 Collider와 슬라이더 Collider 쌍에 적용되므로 이후 다른 충돌 가능 개체에도 같은 Core 계약을 확장할 수 있다.
