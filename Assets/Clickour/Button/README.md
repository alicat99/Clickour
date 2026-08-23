# Button

## 1. 기능 검증 방법

Game Scene의 서로 다른 크기 버튼에 커서를 닿게 하거나 겹친 뒤 클릭한다. 버튼은 홀드 상태 없이 누른 프레임에 곧바로 클릭 해제와 같은 결과를 내며, 커서는 YAML의 `button_jump_height_cells`만큼 위로 점프한다. 버튼 Collider 크기는 SpriteRenderer 크기와 함께 자유롭게 조절할 수 있다.

외형은 Material 3 Filled Button의 40dp 높이와 full corner pill을 2:1 SVG 캔버스에 적용한다. Game Scene WebGL 스크린샷에서 외곽선 없는 pill, 중앙 18dp 상당 상향 아이콘, 상하 24px 투명 여백을 확인한다.

## 2. 기능 사용법

클릭용 `Collider2D`와 `ButtonMechanic`을 같은 GameObject에 추가한다. 버튼은 밟는 발판이 아니므로 Collider를 Trigger로 둔다. 크기는 Transform 또는 SpriteRenderer의 Size와 Collider Size를 동일한 격자 배수로 맞춘다.

```csharp
var button = gameObject.AddComponent<ButtonMechanic>();
```

## 3. 코드 구조와 책임

| 파일 | 책임 |
|---|---|
| `ButtonMechanic.cs` | 즉시 해제형 클릭을 한 칸 수직 점프로 변환 |
| `Art/button.svg` | Material 3 pill과 상향 아이콘의 벡터 원본 |
| `Art/button.png` | Inkscape로 변환해 Scene에 사용하는 Sprite |

점프 높이는 Core의 물리 계산과 Balance의 단일 YAML에 의존한다.

형상 기준: https://github.com/material-components/material-components-android/blob/master/docs/components/Button.md
