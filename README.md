# Order Rush

모바일 터치 기반 주방 시뮬레이션 게임

---

## 게임 개요

요리학교를 갓 졸업한 신인 셰프가 낡은 동네 식당을 인수하고, **15일간의 영업**으로 임대료를 납부하는 이야기.

**핵심 루프**
```
상점(카드 구매) → 영업(손님 응대) → 정산(코인 획득) → 반복
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

| 캐릭터 | Patience | 특징                          |
| ------ | -------- | ----------------------------- |
| 할머니 | 1.5x     | 항상 스테이크 주문            |
| 직장인 | 0.7x     | patience 짧음, 성공 시 팁 10% |
| 꼬마   | 1.0x     | 메뉴 랜덤                     |
| 일반   | 1.0x     | 기본                          |

### 상점 카드

매 Day 시작 전 3장 중 1장 구매. 리프레시 50/100/150 코인.

| 카드                                  | 지속   |
| ------------------------------------- | ------ |
| 도구 업그레이드 (조리·세척 시간 단축) | 영구   |
| 메뉴 추가 (버섯/어니언 스테이크)      | 영구   |
| 테이블 추가 (2인/4인)                 | 영구   |
| 알바 채용                             | 당일만 |

---

## 기술 스택

| 역할        | 라이브러리              |
| ----------- | ----------------------- |
| Engine      | Unity 6.3 (URP, iOS)    |
| DI          | VContainer 1.17.0       |
| UI 패턴     | MVP                     |
| 비동기      | UniTask                 |
| Reactive UI | UniRx 7.1.0             |
| 이벤트      | MessagePipe             |
| 이동        | AI Navigation (NavMesh) |
| 트위닝      | DOTween                 |

---

## 아키텍처

### Scene 구조

```
RootScene (index 0, 항상 존재)
└── ProjectLifetimeScope

LobbyScene (Additive)
└── LobbyLifetimeScope

GameplayScene (Additive)
└── GameLifetimeScope
```

### 핵심 시스템

**ActionExecutor (행동 큐)**
- 모든 캐릭터 행동을 FIFO 큐로 순차 실행
- `IGameAction` 기반 (MoveAction, InteractAction)
- 재클릭 시 현재 큐 취소 후 새 행동 시작
- UniTask 비동기 실행 + CancellationToken 안전 종료

**DayProgressService**
- `IUpdatable` → `UpdateSubscriptionService` 등록으로 타임바 매 프레임 업데이트
- `MessagePipe PaymentEvent` 구독 → `DayContext.EarnedCoins` 실시간 증가
- API: `StartDay()` / `CompleteDay()` / `RestartDay()` / `NextDay()`

**CustomerService**
- 균등 스폰: `spawnInterval = timeBarDuration / maxCustomers`
- `DayContext.TimeBarElapsed` 구독으로 스폰 타이밍 체크
- 빈 테이블 즉시 착석, 없으면 대기열(`waitingList`) 관리

**UpdateSubscriptionService**
- MonoBehaviour Update를 구독 패턴으로 추상화
- `IUpdatable` 구현 객체를 중앙 등록/관리

### DI Scope 라이프타임

| Scope                | 서비스                                                            | 생존    |
| -------------------- | ----------------------------------------------------------------- | ------- |
| ProjectLifetimeScope | AccountService, ResourcesLoaderService, UpdateSubscriptionService | 앱 전체 |
| GameLifetimeScope    | DayProgressService, CustomerService, LevelContextPresenter        | 게임 씬 |
| LobbyLifetimeScope   | 로비 UI Presenter                                                 | 로비 씬 |

---

## 데이터 구조

```
RunsData (ScriptableObject)
└── List<DaysData>
    ├── Rent, Difficulty Rules
    └── List<StoryPhaseData>

RecipesData (ScriptableObject)
└── List<RecipeData>
    ├── RecipeID, Coin
    └── List<IngredientData>
        └── List<IngredientTransition> (Cook/Overcook/Slice)
```

## 관련 문서
- [DESIGN.md](DESIGN.md) - 게임 기획서
- [CLAUDE.md](CLAUDE.md) - 개발 가이드

