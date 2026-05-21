# Order Rush

행동 큐 시스템 기반의 모바일 레스토랑 시뮬레이션. MVP 패턴, VContainer DI, MessagePipe 이벤트로 구현한 PlateUp! 스타일 게임.
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

### 코드 작성 규칙
1. **Debug.Log 최소화**
   - 중요하거나 디버깅에 꼭 필요한 경우에만 추가
   - 불필요한 로그는 작성하지 말 것

2. **주석 최소화**
   - 코드만으로 이해하기 어려운 복잡한 로직에만 추가
   - 자명한 코드에는 주석을 달지 말 것

3. **네이밍 규칙**
   - 시간/초 단위 변수는 `Duration` 접미사 사용
   - 예: `_baseTimeBarDuration`, `_timeBarDurationIncrease`
   - 모든 데이터 클래스는 `Data` 접미사 사용
   - 예: `DaysData`, `StoryPhaseData`, `RecipeData`
   - 데이터 컨테이너 클래스는 `[복수명사]Data` 패턴 사용
   - 예: `RecipeData` → `RecipesData`, `CustomerCharacterData` → `CustomerCharactersData`
   - 예: `RunsData` (여러 DaysData를 담는 컨테이너)

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
    └── 맵 프리펩 (LevelFactory로 런타임 생성)
```

---

## 핵심 시스템

### 행동 큐 시스템 (ActionExecutor)
- 캐릭터의 모든 행동을 큐로 관리하여 순차 실행
- IGameAction 기반 행동 시스템 (MoveAction, InteractAction)
- 재클릭 시 현재 큐 취소 후 새 행동 시작

**상세 동작 방식**
- FIFO 큐 기반으로 행동을 순차적으로 처리
- 각 행동은 UniTask로 비동기 실행되며 완료 대기
- 새로운 클릭 시 현재 실행 중인 행동을 취소하고 큐를 비움
- MoveAction 완료 후 자동으로 InteractAction 실행 (체인 처리)
- 행동 취소 시 CancellationToken을 통한 안전한 종료

### Day/Run 시스템

**데이터 구조**
```
RunsData (ScriptableObject 컨테이너)
└── List<DaysData>

DaysData (ScriptableObject)
├── RunNumber (int)
├── Rent (int) - 임대료
├── Difficulty Rules
│   ├── baseTimeBarDuration = 100초
│   ├── timeBarDurationIncrease = 25초
│   ├── baseCustomers = 4명
│   ├── customerIncrease = 1명
│   └── daysInterval = 3일
└── List<StoryPhaseData> - 스토리 페이즈

DayContext (런타임 모델)
├── DayNumber (int)
├── TimeBarElapsed (ReactiveProperty<float>)
├── TimeBarDuration (float)
├── EarnedCoins (ReactiveProperty<int>)
├── SpawnedCustomers (ReactiveProperty<int>)
└── IsCompleted (bool)
```

**DaysData 계산 로직**
```csharp
// 타임바 시간 계산
int GetTimeBarDuration(int dayNumber)
{
    int intervalIndex = (dayNumber - 1) / daysInterval;
    return baseTimeBarDuration + (intervalIndex * timeBarDurationIncrease);
}
// Day 1-3: 100초, Day 4-6: 125초, Day 7-9: 150초...

// 최대 손님 수 계산
int GetMaxCustomers(int dayNumber)
{
    int intervalIndex = (dayNumber - 1) / daysInterval;
    return baseCustomers + (intervalIndex * customerIncrease);
}
// Day 1-3: 4명, Day 4-6: 5명, Day 7-9: 6명...
```

**DayProgressService**
- IUpdatable 구현 → UpdateSubscriptionService에 등록하여 타임바 매 프레임 업데이트
- MessagePipe PaymentEvent 구독 → DayContext.EarnedCoins 실시간 증가
- API: StartDay(), CompleteDay(), RestartDay(), NextDay(), CompleteRun()

**게임 초기화 플로우**
```
GameInitiator.StartAsync()
├── 1. DayProgressService.Initialize()
│   ├── ResourcesLoaderService로 DaysData 로드 (DataKeys.Run1_Days)
│   ├── UpdateSubscriptionService에 IUpdatable 등록
│   └── MessagePipe PaymentEvent 구독 시작
│
├── 2. DayProgressService.StartDay(1)
│   ├── DayContext 인스턴스 생성
│   │   ├── DayNumber = 1
│   │   └── TimeBarDuration = DaysData.GetTimeBarDuration(1)
│   └── _isDayActive = true
│
├── 3. LevelContextPresenter.LoadLevelContext(1)
│   ├── LevelFactory.CreateLevelContext(levelNumber) 호출
│   └── LevelContext View 참조 저장 (DiningTables, SpawnPoint, WaitingPoint)
│
└── 4. CustomerService.Initialize()
    ├── DayContext 및 DaysData 참조 획득
    ├── 스폰 간격 계산: timeBarDuration / maxCustomers
    └── DayContext.TimeBarElapsed 구독 (스폰 타이밍 체크)
```

### 타임바 & 손님 스폰 시스템

**타임바 업데이트**
- DayProgressService가 IUpdatable 구현
- ManagedUpdate()에서 매 프레임 Time.deltaTime을 DayContext.TimeBarElapsed에 누적
- TimeBarElapsed는 ReactiveProperty → 구독자들에게 자동 알림

**균등 스폰 로직**
```
CustomerService.Initialize()
├── spawnInterval = timeBarDuration / maxCustomers 계산
├── nextSpawnIndex = 0 초기화
└── DayContext.TimeBarElapsed.Subscribe(CheckAndSpawn)

CheckAndSpawn(elapsed)
├── nextSpawnTime = nextSpawnIndex * spawnInterval 계산
├── if (elapsed >= nextSpawnTime)
│   ├── TrySpawn() 호출
│   └── nextSpawnIndex++ 증가
```

**스폰 처리**
- SpawnFactory로 CustomerCharacter 프리팹 생성
- 그룹 사이즈: Random.Range(1, maxGroupSize + 1)
- 빈 테이블 있으면 즉시 착석, 없으면 대기열(waitingList)에 추가
- 대기열: CalculateWaitingPosition()으로 일렬 배치

### 손님 캐릭터 시스템

**CustomerCharacterData (ScriptableObject)**
```
├── CustomerCharacterType (enum)
│   ├── Normal (일반 손님)
│   ├── Kind (할머니)
│   ├── Worker (직장인)
│   └── Wild (꼬마)
├── PatienceMultiplier (float) - patience 배율
├── GivesTip (bool) - 팁 지급 여부
└── PreferredRecipe (RecipeData) - 선호 레시피
```

**Patience 계산**
- 실제 patience 시간 = 기본 patience 시간 × PatienceMultiplier
- 그룹의 경우 가장 낮은 배율 적용
- 단계별 patience: 입장 대기 / 주문 대기 / 음식 대기

**팁 시스템**
- GivesTip = true인 캐릭터 (Worker) 서빙 성공 시
- MessagePipe PaymentEvent로 추가 팁 발행

### Account 시스템

**Account 모델**
```
├── Coins (ReactiveProperty<int>)
└── OwnedRecipeIDs (List<int>)
```

**AccountService**
- LocalStorageService를 통해 개별 필드 영구 저장 (SaveInt, SaveString 사용)
- 코인 관리: AddCoins(), SpendCoins(), TrySpendCoins()
- 레시피 관리: AddOwnedRecipe(), GetRandomOwnedRecipe()
- 보유 레시피 캐시: `_ownedRecipes` (랜덤 선택 O(1) 최적화)
- 초기화 시 기본 레시피(IsDefaultRecipe = true) 자동 추가
- 런 간 코인 이월 지원

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
RecipeData (ScriptableObject)
├── RecipeID (int)
├── RecipeName (string)
├── Icon (Sprite)
├── Coin (int) - 판매 가격
├── IsDefaultRecipe (bool) - 초기 언락 여부
└── List<IngredientData> RequiredIngredients  ← 완성에 필요한 재료들

RecipesData (ScriptableObject 컨테이너)
└── List<RecipeData> Recipes
```

**코인 지급 플로우**
- 손님에게 서빙 완료 시 RecipeData.Coin 값을 PaymentEvent로 발행
- DayProgressService가 구독하여 DayContext.EarnedCoins 증가
- 팁 발생 시 추가 PaymentEvent 발행 (GivesTip = true인 캐릭터)

### MVP 패턴 상세

**Model**
- 비즈니스 로직과 데이터 관리를 담당
- Service 레이어로 구현 (DayProgressService, AccountService, CustomerService 등)
- 상태 변경 시 이벤트 발행 (MessagePipe 사용)
- VContainer를 통해 Presenter에 주입

**View**
- UI 표시 및 사용자 입력 수신
- UniRx를 통한 Reactive 바인딩으로 상태 자동 반영
- MonoBehaviour 기반, Presenter로부터 명령 수신
- 비즈니스 로직 없이 순수 UI 처리만 담당

**Presenter**
- Model과 View를 중재하는 중간 계층
- View의 이벤트를 받아 Model 호출
- Model의 상태 변경을 View에 전달
- VContainer를 통해 생명주기 관리
- IDisposable 구현으로 구독 정리

### UpdateSubscriptionService

**개념**
- MonoBehaviour의 Update 호출을 구독 패턴으로 추상화
- 매 프레임 업데이트가 필요한 로직을 중앙에서 관리
- Update 메서드 호출 횟수를 최소화하여 성능 최적화

**사용 방식**
- IUpdatable 인터페이스를 구현한 객체를 등록
- Presenter에서 구독하여 매 프레임 로직 실행
- Dispose 시 자동으로 구독 해제
- VContainer와 통합되어 생명주기 자동 관리

**장점**
- MonoBehaviour 없이도 프레임 업데이트 가능
- 중앙 집중식 관리로 디버깅 용이
- 불필요한 Update 호출 제거

### MessagePipe 이벤트 시스템

**Pub/Sub 패턴**
- 발행자와 구독자가 느슨하게 결합
- 이벤트 기반 아키텍처로 모듈 간 의존성 최소화
- VContainer Integration으로 DI 지원

**이벤트 종류**
- 글로벌 이벤트: Day 완료, Day 실패, Run 완료 등
- 로컬 이벤트: PaymentEvent (코인 획득), 재료 상태 변경, 주문 완료 등
- 필터링 가능: 특정 조건의 이벤트만 구독

**PaymentEvent 사용 예시**
```
// 발행: 손님 서빙 완료 시
publisher.Publish(new PaymentEvent { Amount = recipe.Coin });

// 구독: DayProgressService에서 코인 누적
subscriber.Subscribe(evt => currentDayContext.EarnedCoins.Value += evt.Amount);
```

**사용 패턴**
- Service에서 상태 변경 시 이벤트 발행
- Presenter가 이벤트 구독하여 View 업데이트
- IDisposable 패턴으로 구독 정리 보장

### DI Scope별 서비스 라이프타임

**ProjectLifetimeScope**
- 앱 전체에서 공유되는 싱글톤 서비스
- AccountService (코인, 레시피 언락 - 런 간 이월)
- LocalStorageService (영구 저장)
- ResourcesLoaderService (ScriptableObject 로드)
- UpdateSubscriptionService (프레임 루프 관리)
- 앱 시작부터 종료까지 유지
- 모든 하위 Scope에서 접근 가능

**LobbyLifetimeScope**
- 로비 씬 전용 서비스
- 로비 UI Presenter 등
- 로비 씬 언로드 시 함께 소멸
- ProjectScope의 서비스 주입 가능

**GameLifetimeScope**
- 게임플레이 관련 서비스
- DayProgressService (Day 진행 관리)
- CustomerService (손님 스폰 관리)
- LevelContextPresenter (레벨 맵 관리)
- PlayerController, OrderSystem 등
- 게임 씬 언로드 시 함께 소멸
- ProjectScope의 서비스 주입 가능

**설계 원칙**
- 각 Scope는 자신의 책임 범위 내 서비스만 등록
- 부모 Scope의 서비스는 자식에서 자동 주입
- Scope 소멸 시 모든 서비스 자동 정리

---

