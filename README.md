# Order Rush

모바일 터치 기반 주방 시뮬레이션 게임 — PlateUp! 스타일, 행동 큐 시스템 + MVP + VContainer DI

---

## 게임 개요

요리학교를 갓 졸업한 신인 셰프가 낡은 동네 식당을 인수하고, **15일간의 영업**으로 임대료를 납부하는 이야기.

**핵심 루프**
```
영업(손님 응대) → 정산(Day 클리어) → 카드 구매(상점 팝업) → 다음 Day 영업 → 반복
```

| 구간      | 스토리                      |
| --------- | --------------------------- |
| Day 1~3   | 첫 오픈, 설레는 날들        |
| Day 4~6   | 단골 생기기 시작            |
| Day 7~10  | 맛집 블로거 방문, 손님 폭발 |
| Day 11~13 | 임대료 고지서 도착          |
| Day 14~15 | 단골 손님들이 모두 몰려온다 |

### 게임 규칙

- **타임바**: Day 1 기준 100초, 3일마다 25초 증가
- **최대 손님**: Day 1 기준 4명, 3일마다 1명 증가
- **실패 조건**: 손님 patience 소진 → 해당 Day 재시작
- **런 종료**: Day 15 클리어 후 임대료 납부 (코인 부족 시 카드 1장 몰수)

### 손님 캐릭터

| 타입   | Patience | 특징                           |
| ------ | -------- | ------------------------------ |
| Normal | 1.0x     | 기본                           |
| Kind   | 1.5x     | 항상 스테이크 주문    |
| Worker | 0.7x     | patience 짧음, 팁 10% |
| Wild   | 1.0x     | 메뉴 랜덤               |

patience 단계: **입장 대기 → 주문 대기 → 음식 대기** (각 단계 별 독립 타이머)

### 상점 카드

매 Day 시작 전 3장 중 1장 구매. 리프레시 50 / 100 / 150 코인.

| 카드                         | 효과                       | 지속 |
| ---------------------------- | -------------------------- | ---- |
| 도구 업그레이드 (ToolUpgrade) | 조리·세척 시간 % 단축      | 영구 |
| 메뉴 추가 (Menu)              | 레시피 언락                | 영구 |
| 테이블 추가 (Table)           | 2인/4인 테이블 런타임 생성 | 영구 |
| 알바 채용 (StaffHire)         | 당일 한정 보조 직원        | 당일 |
| 오버쿡 연장 (OvercookExtend)  | 오버쿡 허용 시간 % 연장    | 영구 |

---

---

## 기술 스택

| 역할        | 라이브러리              |
| ----------- | ----------------------- |
| Engine      | Unity 6.3 (URP, iOS)    |
| DI          | VContainer 1.17.0       |
| UI 패턴     | MVP                     |
| 비동기      | UniTask                 |
| Reactive UI | UniRx 7.1.0 (View 전용) |
| 프레임 루프 | UpdateSubscriptionService |
| 이벤트      | MessagePipe             |
| 이동        | AI Navigation (NavMesh) |
| 트위닝      | DOTween                 |

---

## 아키텍처

### Scene 구조

```
RootScene (index 0, 항상 존재)
└── ProjectLifetimeScope

LobbyScene (Additive 로드/언로드)
└── LobbyLifetimeScope (parent = ProjectLifetimeScope)

GameplayScene (Additive 로드/언로드)
└── GameLifetimeScope (parent = ProjectLifetimeScope)
    └── 맵 프리펩 (LevelFactory로 런타임 생성)
```

### DI Scope 라이프타임

| Scope                | 주요 서비스                                                              | 생존    |
| -------------------- | ------------------------------------------------------------------------ | ------- |
| ProjectLifetimeScope | AccountService, LocalStorageService, ResourcesLoaderService, UpdateSubscriptionService | 앱 전체 |
| LobbyLifetimeScope   | LobbyPresenter                                                           | 로비 씬 |
| GameLifetimeScope    | DayProgressService, CustomerService, KitchenStatService, CardEffectApplier, LevelContextPresenter, GameUIContextPresenter | 게임 씬 |

### MVP 패턴

- **Model (Service)**: 비즈니스 로직 + 상태 관리. MessagePipe로 이벤트 발행
- **View (MonoBehaviour)**: 순수 UI 표시 + 입력 수신. UniRx 바인딩으로 자동 갱신
- **Presenter**: Model ↔ View 중재. IDisposable로 구독 정리

---

## 핵심 시스템

### 행동 큐 (ActionExecutor)

```
캐릭터 클릭 → ActionExecutor.EnqueueActions()
├── 기존 큐 취소 (CancellationToken)
├── MoveAction (NavMesh 이동)
└── InteractAction (도착 후 상호작용)
```

- FIFO 큐 기반, UniTask 비동기 순차 실행
- 재클릭 시 즉시 현재 행동 취소 → 새 행동 시작

**IGameAction 구현체**

| 행동            | 설명                              |
| --------------- | --------------------------------- |
| MoveAction      | NavMesh 경유 이동                 |
| InteractAction  | InteractableBase.InteractAsync() |
| SitAction       | 착석 연출                         |
| OrderAction     | 주문서 생성                       |
| WaitForOrderAction | 주문 patience 대기             |
| WaitForFoodAction  | 음식 patience 대기             |
| EatAction       | 식사 연출                         |
| LeaveAction     | 퇴장                              |
| WaitInLineAction | 대기열 위치 유지                  |
| EmoteAction     | 감정 표시 아이콘                  |

---

### Day/Run 시스템

**데이터 계층**
```
RunsData (ScriptableObject 컨테이너)
└── List<DaysData>
    ├── RunNumber, Rent
    ├── baseTimeBarDuration = 100초
    ├── timeBarDurationIncrease = 25초
    ├── baseCustomers = 4명
    ├── customerIncrease = 1명
    ├── daysInterval = 3일
    └── List<StoryPhaseData>

DayContext (런타임 모델)
├── DayNumber
├── TimeBarElapsed (ReactiveProperty<float>)
├── TimeBarDuration (float)
├── EarnedCoins (ReactiveProperty<int>)
├── SpawnedCustomers (ReactiveProperty<int>)
└── IsCompleted
```

**DayProgressService API**

| 메서드         | 동작                          |
| -------------- | ----------------------------- |
| StartDay(n)    | DayContext 생성, 타임바 시작  |
| CompleteDay()  | Day 성공 처리                 |
| RestartDay()   | 실패 시 현재 Day 재시작       |
| NextDay()      | 다음 Day로 진행               |
| CompleteRun()  | 런 종료, 임대료 정산          |

**게임 초기화 플로우**
```
GameInitiator.StartAsync()
├── DayProgressService.Initialize()   ← DaysData 로드, UpdateService 등록
├── DayProgressService.StartDay(1)    ← DayContext 생성
├── LevelContextPresenter.LoadLevelContext(1)  ← 맵 프리펩 생성
└── CustomerService.Initialize()      ← 스폰 간격 계산, TimeBarElapsed 구독
```

---

### 손님 스폰 시스템 (CustomerService)

**균등 스폰**
```
spawnInterval = timeBarDuration / maxCustomers
nextSpawnTime = nextSpawnIndex * spawnInterval

TimeBarElapsed >= nextSpawnTime → TrySpawn() → nextSpawnIndex++
```

- 그룹 크기: `Random.Range(1, maxGroupSize + 1)`
- 빈 테이블 있으면 즉시 착석, 없으면 `waitingList` 대기
- 대기 위치: `CalculateWaitingPosition()` 일렬 배치

---

### 주방 시스템

**재료 변환 구조**
```
IngredientData
└── List<IngredientTransition>
    ├── TransitionType (Cook / Overcook / Slice)
    ├── Result (IngredientData)
    ├── Duration (변환 소요 시간)
    └── OverDuration (타는 허용 시간)
```

**Stove 조리 흐름**
```
StartCooking()
├── [조리 루프] cookDuration = KitchenStatService.GetModifiedDuration()
│   ↑ ToolUpgrade 카드로 단축 가능 (최소 50%)
├── CompleteTransition() → 재료 변환
└── [오버쿡 루프] overcookDuration = KitchenStatService.GetOvercookDuration()
    ↑ OvercookExtend 카드로 연장 가능 (상한 없음)
    → 시간 초과 시 IngredientObject.SetRuined()
```

**KitchenStatService 계산식**

| 메서드                | 공식                                        |
| --------------------- | ------------------------------------------- |
| `GetModifiedDuration()` | `base × (1 - reducePercent)`, 최소 50%   |
| `GetOvercookDuration()` | `base × (1 + extendPercent)`, 최대 base × 1.5 |

base = `GameConfig.ToolProcessSeconds` (기본 5초)

**DishSink 설거지 흐름**
```
InteractAsync()
├── 더러운 접시 들고 있음 → 싱크대에 내려놓고 설거지 시작
├── 빈 손 + 싱크대에 더러운 접시 → 설거지 재시작
└── 빈 손 + 싱크대에 깨끗한 접시 → 접시 집기
```

---

### 카드 시스템

**CardEffectApplier**
- `ApplyEffect(CardEffectData)`: EffectType switch로 효과 분기
- `ApplyAllPurchasedCards()`: 게임 재시작 시 구매 카드 전체 재적용

**카드 효과 ScriptableObject**

| 클래스               | EffectType      | 필드                   |
| -------------------- | --------------- | ---------------------- |
| UpgradeCardEffect    | ToolUpgrade     | DurationReducePercent  |
| MenuCardEffect       | Menu            | Recipe (RecipeData)    |
| TableAdditionEffect  | Table           | TablePrefabName        |
| StaffCardEffect      | StaffHire       | (미구현)               |
| OvercookCardEffect   | OvercookExtend  | ExtendPercent          |

---

### Account 시스템

**Account 모델**
```
Account
├── Coins (ReactiveProperty<int>)
├── OwnedRecipeIDs (List<int>)
├── PurchasedCardIDs (List<int>)
├── CurrentRun (int)
└── CurrentDay (int)
```

**AccountService 주요 API**

| 메서드                | 동작                                   |
| --------------------- | -------------------------------------- |
| `AddCoins(int)`       | 코인 증가 + 즉시 저장                  |
| `TrySpendCoins(int)`  | 코인 부족 시 false 반환                |
| `AddOwnedRecipe(id)`  | 레시피 언락 + 캐시 갱신                |
| `GetRandomOwnedRecipe()` | 보유 레시피 O(1) 랜덤 선택          |
| `ResetAll()`          | PlayerPrefs 전체 삭제 + 인메모리 초기화 |

- `LocalStorageService`(PlayerPrefs 래퍼)로 영구 저장
- 런 간 코인 이월 지원
- 초기화 시 `IsDefaultRecipe = true` 레시피 자동 추가

---

### 이벤트 (MessagePipe)

| 이벤트               | 발행 시점              | 주요 구독자             |
| -------------------- | ---------------------- | ----------------------- |
| PaymentEvent         | 손님 서빙 완료, 팁 발생 | DayProgressService     |
| DayEndedEvent        | Day 완료/실패          | GameUIContextPresenter  |
| CustomerRemovedEvent | 손님 퇴장              | CustomerService         |
| TableAvailableEvent  | 테이블 빈 자리 발생    | CustomerService         |
| InteractEvent        | 플레이어 클릭          | PlayerInputHandler      |
| MoveEvent            | 플레이어 이동 요청     | PlayerInputHandler      |
| GameCleanupEvent     | 씬 정리                | 각 서비스               |

---

## 데이터 구조 요약

```
ScriptableObject 계층
├── GameConfig             ← patience, 도구 처리 시간, 팁 비율, 리프레시 비용
├── RunsData → List<DaysData>
│   └── DaysData → List<StoryPhaseData>
├── RecipesData → List<RecipeData>
│   └── RecipeData → List<IngredientData>
│       └── IngredientData → List<IngredientTransition>
├── CardsData → List<CardData>
│   └── CardData → CardEffectData (다형성)
└── CustomerCharactersData → List<CustomerCharacterData>
```

---

## 관련 문서

- [DESIGN.md](DESIGN.md) - 게임 기획서
- [CLAUDE.md](CLAUDE.md) - 개발 규칙 및 시스템 상세 설계
