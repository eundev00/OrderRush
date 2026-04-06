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
├── LobbyLifetimeScope        ← 로비 씬 (추후 구현)
└── StageLifetimeScope        ← 게임 플레이 씬, 스테이지마다 생성/소멸
```

### 등록 내용
**ProjectLifetimeScope**
- MessagePipe
- UpdateSubscriptionService (AsImplementedInterfaces)
- IResourcesLoaderService, ResourcesLoaderService (Singleton)
- Launcher (EntryPoint) ← 첫 씬 로드 담당

**StageLifetimeScope**
- IOrderService, OrderService (Singleton)
- GameObjectFactory (Singleton)
- PlayerInputHandler (EntryPoint)
- IInjectable 자동 주입 (BuildCallback)

---

## 핵심 시스템

### 행동 큐 시스템 (ActionExecutor)
- 캐릭터의 모든 행동을 큐로 관리하여 순차 실행
- IGameAction 기반 행동 시스템 (MoveAction, InteractAction)
- 재클릭 시 현재 큐 취소 후 새 행동 시작

### 재료 & 레시피 데이터 구조

**재료 변환 시스템**
```
IngredientData (재료)
├── IngredientName
├── Icon, PrefabName
└── List<IngredientTransition>  ← 이 재료가 변할 수 있는 모든 경로
    ├── TransitionType (Cook/Overcook/Slice)
    ├── Result (IngredientData)  ← 변환 후 결과 재료
    ├── Duration                  ← 변환 소요 시간
    └── OverDuration              ← 과도 시간 (타는 시간)
```

**관계 예시**
- 생고기 IngredientData
  - Cook → 익은 고기 IngredientData (Duration: 5초)
  - Overcook → 탄 고기 IngredientData (OverDuration: 3초)
- 양파 IngredientData
  - Slice → 썬 양파 IngredientData

**레시피 시스템**
```
RecipeData
├── RecipeName
├── Icon
└── List<IngredientData> RequiredIngredients  ← 완성에 필요한 재료들
```

---

