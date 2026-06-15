---
name: order-rush-game-systems
description: Order Rush의 게임플레이 시스템 명세 - 행동 큐 시스템(ActionExecutor), Day/Run 데이터 구조 및 계산 로직, 타임바/손님 스폰 시스템, 손님 캐릭터 타입 및 Patience/팁 시스템, Account 시스템, 재료 변환 및 레시피 데이터 구조. 게임플레이 로직 구현/수정, 밸런스 조정, ScriptableObject 데이터 구조 변경, 손님/주문/결제 관련 기능 작업 시 참조.
---

# Order Rush - 게임 시스템

## 행동 큐 시스템 (ActionExecutor)
- 캐릭터의 모든 행동을 큐로 관리하여 순차 실행
- IGameAction 기반 행동 시스템 (MoveAction, InteractAction)
- 재클릭 시 현재 큐 취소 후 새 행동 시작

### 상세 동작 방식
- FIFO 큐 기반으로 행동을 순차적으로 처리
- 각 행동은 UniTask로 비동기 실행되며 완료 대기
- 새로운 클릭 시 현재 실행 중인 행동을 취소하고 큐를 비움
- MoveAction 완료 후 자동으로 InteractAction 실행 (체인 처리)
- 행동 취소 시 CancellationToken을 통한 안전한 종료

---

## Day/Run 시스템

### 데이터 구조
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

### DaysData 계산 로직
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

### DayProgressService
- IUpdatable 구현 → UpdateSubscriptionService에 등록하여 타임바 매 프레임 업데이트
- MessagePipe PaymentEvent 구독 → DayContext.EarnedCoins 실시간 증가
- API: StartDay(), CompleteDay(), RestartDay(), NextDay(), CompleteRun()

### 게임 초기화 플로우
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

---

## 타임바 & 손님 스폰 시스템

### 타임바 업데이트
- DayProgressService가 IUpdatable 구현
- ManagedUpdate()에서 매 프레임 Time.deltaTime을 DayContext.TimeBarElapsed에 누적
- TimeBarElapsed는 ReactiveProperty → 구독자들에게 자동 알림

### 균등 스폰 로직
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

### 스폰 처리
- SpawnFactory로 CustomerCharacter 프리팹 생성
- 그룹 사이즈: Random.Range(1, maxGroupSize + 1)
- 빈 테이블 있으면 즉시 착석, 없으면 대기열(waitingList)에 추가
- 대기열: CalculateWaitingPosition()으로 일렬 배치

---

## 손님 캐릭터 시스템

### CustomerCharacterData (ScriptableObject)
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

### Patience 계산
- 실제 patience 시간 = 기본 patience 시간 × PatienceMultiplier
- 그룹의 경우 가장 낮은 배율 적용
- 단계별 patience: 입장 대기 / 주문 대기 / 음식 대기

### 팁 시스템
- GivesTip = true인 캐릭터 (Worker) 서빙 성공 시
- MessagePipe PaymentEvent로 추가 팁 발행

---

## Account 시스템

### Account 모델
```
├── Coins (ReactiveProperty<int>)
└── OwnedRecipeIDs (List<int>)
```

### AccountService
- LocalStorageService를 통해 개별 필드 영구 저장 (SaveInt, SaveString 사용)
- 코인 관리: AddCoins(), SpendCoins(), TrySpendCoins()
- 레시피 관리: AddOwnedRecipe(), GetRandomOwnedRecipe()
- 보유 레시피 캐시: `_ownedRecipes` (랜덤 선택 O(1) 최적화)
- 초기화 시 기본 레시피(IsDefaultRecipe = true) 자동 추가
- 런 간 코인 이월 지원

---

## 재료 & 레시피 데이터 구조

### 재료 변환 시스템
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

### 레시피 시스템
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

### 코인 지급 플로우
- 손님에게 서빙 완료 시 RecipeData.Coin 값을 PaymentEvent로 발행
- DayProgressService가 구독하여 DayContext.EarnedCoins 증가
- 팁 발생 시 추가 PaymentEvent 발행 (GivesTip = true인 캐릭터)
