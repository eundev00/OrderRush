# Order Rush

PlateUp! 스타일의 레스토랑 시뮬레이션 게임
주문부터 서빙까지 바쁜 레스토랑을 운영하세요.

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


## VContainer Scope 계층 구조

```
RootScene        ← index 0, 항상 존재
└── ProjectLifetimeScope

LobbyScene       ← Additive 로드/언로드
└── LobbyLifetimeScope (parent = ProjectLifetimeScope)

GameplayScene    ← Additive 로드/언로드
└── GameLifetimeScope (parent = ProjectLifetimeScope)
    └── 맵 프리펩 (level1, level2... DayData 기반 런타임 로드)
```

## 핵심 시스템

### 행동 큐 시스템 (ActionExecutor)
- 캐릭터의 모든 행동을 큐로 관리하여 순차 실행
- IGameAction 기반 행동 시스템 (MoveAction, InteractAction)
- 재클릭 시 현재 큐 취소 후 새 행동 시작

### 레벨 시스템

**LevelProgressService**
레벨 진행도 및 상태를 관리하는 서비스
- 레벨 데이터 로드 (LevelsData ScriptableObject)
- 최대 도달 레벨 추적 (PlayerPrefs 저장)
- 선택된 레벨 관리 (로비에서 설정, GameInitiator에서 사용)

**레벨 로딩 플로우**
```
GameInitiator.Start()
├── 1. LevelProgressService.LoadLevelsData() ← ScriptableObject 로드
├── 2. LevelProgressService.LoadMaxReachedLevel() ← PlayerPrefs에서 진행도 로드
├── 3. GetSelectedLevel() ← 선택된 레벨 번호 가져오기 (기본값: 1)
└── 4. LevelContextPresenter.LoadLevelContext(levelNumber)
    ├── LevelProgressService에서 LevelData 조회
    ├── LevelFactory로 레벨 맵 프리팹 생성
    └── 레벨 상태 초기화 (돈, 시간, 활성화)
```

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

**레시피 시스템**
```
RecipeData
├── RecipeName
├── Icon
└── List<IngredientData> RequiredIngredients  ← 완성에 필요한 재료들
```
