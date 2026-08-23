# Switch

## 1. 기능 검증 방법

Game Scene에는 회색/파란색과 0°, 90°, 180°, 270° 예제가 있다. 2×1 또는 1×2 트랙의 핸들에 커서를 겹쳐 클릭하면 클릭 지점에서 고정된다. 버튼을 떼면 핸들이 반대편으로 토글되고 그 이동 방향으로 발사된다. 가로 스위치는 한 칸 수직 점프 속도와 그 두 배의 수평 속도를 사용한다. 세로 스위치는 같은 두 배 속도를 축 방향으로 사용한다. 파란 타입은 지정된 활성 핸들 위치에서만 진한 파랑과 고체 Collider가 된다.

## 2. 기능 사용법

루트에 `SwitchMechanic`, 트랙용 `SpriteRenderer`, 활성 상태 발판용 `BoxCollider2D`, 전체 범위 Trigger `exit_zone`을 둔다. 핸들 자식에는 Trigger Collider를 둔다. `Configure`로 참조와 타입을 연결하고 루트 회전으로 네 방향을 정한다.

```csharp
mechanic.Configure(SwitchKind.Blue, handle, track, solid, exit_zone);
```

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `SwitchKind.cs` | 회색/파란색 상태 타입 |
| `SwitchMechanic.cs` | 홀드, 토글, 방향별 발사, 파란 발판 상태 |
| `Art/switch_track.svg` | 트랙 시각 원본 |
| `Art/switch_handle.svg` | 핸들 시각 원본 |
| `Art/*.png` | Inkscape 변환 Sprite |

파란 발판이 켜지는 프레임에는 움직이는 커서 Collider와의 충돌을 먼저 무시하고, 전체 스위치 범위를 벗어나면 Core가 충돌을 복원한다.
