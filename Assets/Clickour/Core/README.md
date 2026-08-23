# Cursor Core

## 1. 기능 검증 방법

Game Scene의 `Cursor`에는 `Rigidbody2D`, `Collider2D`, `CollisionExitGate`, `CursorController`, `CursorInteractor`가 있다. Play Mode에서 마우스를 좌우로 움직이면 커서가 수평 가속으로 따라가며 중력과 파란 발판 충돌은 유지된다. 클릭 가능한 두 Collider가 동시에 후보일 때, 접촉 중인 후보가 겹친 후보보다 먼저 선택된다. Edit Mode Test Runner의 `ClickSelectionPolicyTests`가 이 우선순위를 검증한다.

## 2. 기능 사용법

클릭 대상은 `ICursorClickTarget`을 구현한다. `HoldsCursor`가 `false`면 누른 프레임에 `Press`, `Release`가 연속 호출된다. 홀드 대상은 `Press`에서 `BeginFixedHold` 또는 `BeginSlowFall`을 호출하고 `Release`에서 `ReleaseWithVelocity`를 호출한다. 발사 원본과의 충돌을 미뤄야 할 때 `IgnoreUntilExited`에 고체 Collider 목록과 전체 이탈 범위를 전달한다.

```csharp
public void Release(CursorController cursor)
{
    cursor.ReleaseWithVelocity(Vector2.up * cursor.JumpVelocity(1));
}
```

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `ICursorClickTarget.cs` | 모든 클릭형 기믹이 따르는 입력 수명주기 |
| `ClickCandidate.cs` | 클릭 후보 데이터 |
| `ClickSelectionPolicy.cs` | 접촉 우선, 거리 차선 선택 규칙 |
| `CursorInteractor.cs` | 마우스 입력과 물리 후보를 클릭 대상으로 연결 |
| `CursorController.cs` | 커서의 수평 이동, 홀드 상태, 점프 속도 |
| `CollisionExitGate.cs` | 움직이는 개체별 Collider 무시와 이탈 후 복원 |
| `Art/cursor.svg` | 플레이어 커서의 벡터 원본 |
| `Art/platform.svg` | 파란 충돌 발판의 벡터 원본 |
| `Art/*.png` | Inkscape로 변환한 Scene Sprite |

Core는 Balance만 참조한다. 각 기믹 assembly는 Core를 참조하며 Core는 구체 기믹을 알지 못한다.
