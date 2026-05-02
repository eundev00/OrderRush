# Order Rush

모바일 환경에서 터치 기반 입력과 행동 큐 시스템을 활용한 자동화 주방 시뮬레이션 게임.
---

## 작업 규칙

### 코드 수정 프로세스
1. **수정 전 반드시 수정 계획 설명**
   - 어떤 파일을 수정할 것인지
   - 현재 로직이 어떻게 동작하는지
   - 어떻게 변경할 것인지
   - 변경 후 예상되는 동작

2. **사용자 승인 후 수정**
   - 계획 설명 후 사용자의 승인을 받은 뒤에만 코드 수정
   - 승인 없이 Edit/Write 도구를 사용하지 말 것

### 플랜 작성 규칙
1. **기본 구조 설계 먼저**
   - 플랜에서는 전체 아키텍처와 구조만 설명
   
2. **코드는 구현 단계에서**
   - 플랜에는 구체적인 코드를 작성하지 말 것
   - 구조 설계 완료 후 실제 구현 시 코드 작성
   
---

## 기술 스택

| 역할        | 라이브러리                 |
| ----------- | -------------------------- |
| DI          | VContainer 1.17.0          |
| UI 패턴     | MVP (Model-View-Presenter) |
| 비동기      | UniTask                    |
| Reactive UI | UniRx 7.1.0 (View 전용)    |
| 프레임 루프 | UpdateSubscriptionService  |
| 이벤트      | MessagePipe                |
| 이동        | AI Navigation (NavMesh)    |
| 트위닝      | DOTween                    |

---

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

---

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

**로비에서 레벨 선택 시 (추후 구현)**
```csharp
// 로비에서
_levelProgressService.SetSelectedLevel(selectedLevel);
SceneManager.LoadScene("Gameplay");
// → GameInitiator가 선택된 레벨을 자동으로 로드
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

