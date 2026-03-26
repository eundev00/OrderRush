# Order Rush

모바일 환경에서 터치 기반 입력과 행동 큐 시스템을 활용한 자동화 주방 시뮬레이션 게임.
클라이언트 시스템 설계 능력을 보여주는 포트폴리오 프로젝트.

---

## 기술 스택

| 역할 | 라이브러리 |
|------|-----------|
| DI | VContainer 1.17.0 |
| UI 패턴 | MVP (Model-View-Presenter) |
| 비동기 | UniTask |
| Reactive UI | UniRx 7.1.0 (View 전용) |
| 프레임 루프 | UpdateSubscriptionService |
| 이벤트 | MessagePipe |
| 이동 | AI Navigation (NavMesh) |
| 트위닝 | DOTween |

---

## VContainer Scope 계층 구조

```
ProjectLifetimeScope          ← VContainerSettings Root로 등록, 앱 전체 생존
└── GameLifetimeScope         ← Gameplay 씬, 게임플레이 생존
    └── StageLifetimeScope    ← 맵마다 생성/소멸 (추후 확장)
```

### 등록 내용
**ProjectLifetimeScope**
- MessagePipe
- UpdateSubscriptionService (AsImplementedInterfaces)
- AppBootstrap (EntryPoint) ← 첫 씬 로드 담당

**GameLifetimeScope**
- CharacterStateMachine (EntryPoint, IStartable, ITickable)
- IdleState, MoveState, WorkState (Scoped)
- InjectGameObject(_playerObject) ← 추후 Addressables로 변경 예정


### 확장 시 (Lobby 추가 등)
```
ProjectLifetimeScope
├── LobbyLifetimeScope        ← 로비 씬 진입 시 생성, 나가면 소멸
└── GameLifetimeScope
    └── StageLifetimeScope
```

---

## 씬 구조

```
Scenes/
└── Gameplay     ← 메인 게임 씬 (Build Settings 인덱스 0)
```

Bootstrap 씬 없음. VContainerSettings가 ProjectLifetimeScope 자동 초기화.

### Gameplay 씬 오브젝트 계층
```
Gameplay
├── Main Camera
├── Directional Light
├── Stage
│   ├── Floor         ← NavMesh Bake 대상
│   └── NavMeshSurface
├── Characters
│   └── Player
│       ├── Model     ← 3D 메시 (시각 전용)
│       └── (NavMeshAgent, NavMeshMover, PlayerInputHandler 컴포넌트)
├── GameLifetimeScope
└── EventSystem
```

---

## 구현된 시스템

### 이동 시스템
- `NavMeshMover` — NavMeshAgent 래퍼, MoveToAsync(UniTask) / MoveDirect / Stop
- `PlayerInputHandler` — MonoBehaviour + IUpdatable, 터치/마우스 입력 처리
  - 에디터: 마우스 클릭 이동
  - 모바일: 터치 이동
  - IInteractable 클릭 → InteractWith()
  - 바닥 클릭 → MoveToPosition()
- 동적 장애물: `NavMeshObstacle + Carve` 방식

### 인터랙션 시스템
```csharp
public interface IInteractable
{
    string DisplayName { get; }
    Transform InteractPoint { get; }  // 캐릭터가 설 위치
    UniTask InteractAsync(CancellationToken ct);
}
```
- InteractPoint: 오브젝트마다 상호작용 위치 지정 (PlateUp 방식)
- NavMesh.SamplePosition으로 막힌 경우 이동 불가 처리
- Gizmo로 에디터에서만 시각화

### 캐릭터 상태 머신
```
ICharacterState
├── Enter()
├── Exit()
└── Update()

CharacterStateMachine (IStartable, ITickable)
├── 시작 시 IdleState로 초기화
└── ChangeState(ICharacterState)

상태
├── IdleState  ← 대기
├── MoveState  ← 이동 중, 도착 시 nextState로 전환
└── WorkState  ← 상호작용 중, 완료 시 IdleState로 전환
```

**상태 전환 흐름**
```
바닥 클릭 → MoveState(nextState: IdleState)
오브젝트 클릭 → MoveState(nextState: WorkState) → WorkState → IdleState
이동 중 재클릭 → 기존 CancellationToken 취소 → 새 이동 시작
```

---

## 데이터 설계 (ScriptableObject)

### IngredientState (enum)
- Raw / Processed / Cooked / Burnt

### 구조
```
RecipeData (ScriptableObject)
├── recipeName
├── List<Ingredient> ingredients
└── List<CookingStep> steps

Ingredient (ScriptableObject)
├── ingredientName
├── IngredientState initialState
└── Sprite icon

CookingStep (Serializable 클래스)
├── stepName
├── requiredTool
├── float duration  (0이면 시간 없음)
└── IngredientState resultState
```

---

## 폴더 구조

```
Assets/
├── _Core/          ← 공통 인터페이스 (IInteractable 등)
├── Characters/
├── Data/           ← ScriptableObject 에셋
├── Interaction/
├── Materials/
├── Prefabs/
├── Resources/
├── Scenes/
├── Scripts/
│   ├── Character/  ← NavMeshMover, PlayerInputHandler, StateMachine
│   ├── Core/       ← ProjectLifetimeScope, GameLifetimeScope, AppBootstrap
│   ├── Data/       ← RecipeData, Ingredient, CookingStep, IngredientState
│   └── Interaction/← IInteractable, TestInteractable
└── Plugins/        ← DOTween, UniRx
```

---

## 개발 진행 상황

- ✅ Phase 1 — NavMesh 이동
- ✅ Phase 2 — 터치/마우스 입력
- ✅ Phase 3 — IInteractable 인터랙션
- 🔲 Phase 4 — 단일 레시피 구현 (스테이크 굽기)
- 🔲 Phase 5 — 행동 큐 시스템
- 🔲 Phase 6 — Grid 기반 가구 배치
- 🔲 Phase 7 — UI 및 전체 흐름