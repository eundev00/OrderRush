# Order Rush

모바일 터치 기반 레스토랑 시뮬레이션 게임. PlateUp! 을 레퍼런스로, **행동 큐(Action Queue)** 를 중심에 둔 캐릭터 제어와 **이벤트 기반 서비스 아키텍처**로 구현했다.

플레이어는 신인 셰프가 되어 하루(Day) 단위로 영업한다. 손님을 받고 주문을 받고 요리해 서빙하며, 하루가 끝나면 번 코인으로 상점에서 카드를 구매해 가게를 성장시킨다. Day 가 올라갈수록 영업 시간과 손님 수가 늘어난다.

```
로비 → [스토리] → 영업 → [정산] → [상점] → 영업 → …
                    │
                    └ 손님 인내심 소진 → 실패 → 해당 Day 재시작
```

---

## 목차

1. [기술 스택](#1-기술-스택)
2. [아키텍처 개요](#2-아키텍처-개요)
3. [Scene 과 DI 스코프](#3-scene-과-di-스코프)
4. [부팅 시퀀스](#4-부팅-시퀀스)
5. [행동 큐 시스템](#5-행동-큐-시스템)
6. [캐릭터 3종의 제어 모델](#6-캐릭터-3종의-제어-모델)
7. [상호작용 · 주방 파이프라인](#7-상호작용--주방-파이프라인)
8. [Day 진행 시스템](#8-day-진행-시스템)
9. [손님 스폰과 인내심](#9-손님-스폰과-인내심)
10. [카드 · 상점 시스템](#10-카드--상점-시스템)
11. [팝업 시스템](#11-팝업-시스템)
12. [프레임 업데이트 관리](#12-프레임-업데이트-관리)
13. [이벤트 흐름](#13-이벤트-흐름)

---

## 1. 기술 스택

| 역할     | 사용 기술                                         |
| -------- | ------------------------------------------------- |
| 엔진     | Unity 6000.3.6f1 (URP 17.3)                       |
| DI       | VContainer 1.17.0                                 |
| UI 패턴  | MVP (Model = Service, View, Presenter)            |
| 이벤트   | MessagePipe (+ VContainer Integration)            |
| 비동기   | UniTask 2.1.0                                     |
| Reactive | UniRx (`ReactiveProperty`, `CompositeDisposable`) |
| 트위닝   | DOTween                                           |
---

## 2. 아키텍처 개요

### 3계층 구조

| 계층 | 설명 |
|---|---|
| **View** | MonoBehaviour. 표시와 입력 수신만. 게임 로직·서비스를 전혀 모른다. |
| **Presenter** | Model ↔ View 중재. 구독 정리 책임. VContainer 가 생성/소멸 관리. |
| **Model (Service)** | Service 레이어. 상태와 규칙 소유. 변경 사실을 이벤트로 발행한다. |

### 세 가지 결합 해소 장치

이 프로젝트의 설계는 "누가 누구를 직접 알아야 하는가"를 줄이는 방향으로 정리되어 있다.

**① MessagePipe — 도메인 사실의 방송**

서비스는 "무슨 일이 일어났다"만 발행하고, 그 결과로 무엇을 할지는 구독자가 각자 결정한다. 예를 들어 `DayProgressService` 는 하루가 끝났다는 사실(`DayEndedEvent`)만 발행하고, 계정 저장·팝업 표시·캐릭터 행동 중단·조리 중단은 각 구독자가 독립적으로 처리한다. 발행자는 구독자 목록을 모른다.

**② UniRx `ReactiveProperty` — 상태의 흐름**

`DayContext.TimeBarElapsed` 하나에 HUD 타임바, 손님 스폰 타이밍, 낮/밤 조명 보간이 각각 구독으로 붙는다. 타임바를 갱신하는 쪽은 구독자를 신경 쓰지 않는다.

**③ VContainer 스코프 — 생존 범위의 물리적 분리**

씬 스코프가 소멸하면 그 스코프에 등록된 모든 서비스와 구독이 함께 정리된다. "게임을 나갈 때 무엇을 해제해야 하는가"를 수동으로 관리하지 않는다.

### 이 아키텍처가 만드는 특징적 결과

**씬 재로드 없는 Day 리셋.** Day 실패 후 재시작이나 다음 Day 진입 시 씬을 다시 로드하지 않는다. 대신 `GameCleanupEvent` 를 방송하면 각 객체가 자기 상태를 스스로 초기화한다. 테이블은 접시를 파괴하고 좌석을 비우며, 조리도구는 재료를 버리고, 접시 렉은 수량을 복원하고, 손님 대기열은 비워진다. 리셋 로직이 상태를 가진 객체 자신에게 있으므로, 새 객체를 추가할 때 중앙 리셋 코드를 수정할 필요가 없다.

**행동 취소가 안전하다.** 모든 비동기 행동이 `CancellationToken` 을 받고 `try/finally` 로 정리하므로, 플레이어가 조리 중 다른 곳을 터치하면 진행 중인 작업이 즉시 중단되고 게이지·애니메이션·사운드가 원복된다.

---

## 3. Scene 과 DI 스코프

Bootstrap 씬만 상주하고, 로비와 게임플레이는 Additive 로 교체된다.

```
Bootstrap 씬 (빌드 인덱스 0, 항상 상주)
└── ProjectLifetimeScope ─── 앱 전체 싱글톤
        │
        ├── LobbyScene (Additive)
        │   └── LobbyLifetimeScope
        │
        └── GameplayScene (Additive)
            └── GameLifetimeScope
                    └── 레벨 맵 프리팹 (런타임 생성 후 DI 주입)
```

부모 스코프의 서비스는 자식에서 자동 주입되고, 자식 스코프가 소멸하면 그 스코프 서비스만 정리된다.

### 스코프 배치 기준

| 스코프                   | 성격                                                                                                          | 생존    |
| ------------------------ | ------------------------------------------------------------------------------------------------------------- | ------- |
| **ProjectLifetimeScope** | 런을 넘어 유지되어야 하는 것 — 계정(코인·해금 레시피·구매 카드), 저장, 게임 데이터, 사운드, 팝업, 프레임 루프 | 앱 전체 |
| **LobbyLifetimeScope**   | 로비 화면 전용                                                                                                | 로비 씬 |
| **GameLifetimeScope**    | 하루/한 판 단위로만 의미 있는 것 — Day 진행·흐름, 손님, 직원, 상점, 주방 스탯, 레벨, 카메라, 입력, HUD        | 게임 씬 |

게임 스코프의 카드 효과 누적치(`KitchenStatService`)는 게임 씬을 나가면 사라지고, 다음 진입 시 `CardEffectApplier` 가 저장된 구매 기록으로 재구성한다. 런타임 상태를 저장 데이터로부터 매번 다시 만들기 때문에, 저장할 것과 계산할 것의 경계가 분명하다.

### 주입 방식 세 가지

- 생성자 주입 — 순수 C# 서비스와 Presenter
- `[Inject]` 메서드 — MonoBehaviour (`CharacterBase`, `DiningTable`, `Plate` 등)
- `RegisterComponentInHierarchy` — 씬에 이미 배치된 View (`HudView`, `CameraDirector`)

런타임에 생성되는 오브젝트는 `SpawnFactory` / `LevelFactory` 를 거치며, 두 팩토리 모두 `Instantiate` 직후 `InjectGameObject` 를 호출해 프리팹 계층 전체에 의존성을 채운다. **프리팹을 직접 `Instantiate` 하면 DI 가 되지 않으므로 반드시 팩토리를 사용해야 한다.**

---

## 4. 부팅 시퀀스

### 앱 시작 (`AppBootstrap`, Project 스코프 엔트리포인트)

```
GameDataService.Initialize()   ScriptableObject 일괄 로드 (Config/Recipes/Cards/Days/DayNight)
SoundService.Initialize()      믹서 로드 + 전체 클립 프리로드 + 볼륨 설정 복원
AccountService.Initialize()    기본 레시피 주입 → 저장 데이터 로드 → DayEndedEvent 구독
PopupService.Initialize()      공통 팝업을 Root 리졸버로 등록
→ LobbyScene Additive 로드
```

공통 팝업을 **Root 리졸버** 소유로 등록하는 이유는, 씬 전환 중에도 살아 있어야 하는 팝업(로딩 등)을 지원하기 위함이다.

### 게임 시작 (`GameInitiator`, Game 스코프 엔트리포인트)

```
BGM 재생
DayProgressService.Initialize()     DaysData 참조, 프레임 루프 등록, PaymentEvent 구독
DayNightService.Initialize()        TimeBarElapsed 구독 시작
LevelContextPresenter.LoadLevelContext(1)   맵 프리팹 생성 + DI 주입
GameUIContextPresenter.Initialize()  팝업 캔버스 루트 지정 + DayEndedEvent 구독
CardEffectApplier.ApplyAllPurchasedCards()  저장된 카드를 티어까지 재적용
DayEventService.Initialize(day)     날씨 등 Day 이벤트 반영
DayFlowService.RunFirstDayAsync(day)  스토리 → 카메라 인트로 → 타임바 시작
```

`ScenePopupRegistrar` 가 `GameInitiator` 보다 먼저 등록되어 있어, 게임 시작 시점에 씬 팝업 카탈로그가 이미 준비된 상태다.

---

## 5. 행동 큐 시스템

모든 캐릭터 행동은 `IGameAction` 구현체로 표현되고 `ActionExecutor` 가 순차 실행한다.

```csharp
public interface IGameAction
{
    UniTask ExecuteAsync(CancellationToken ct);
}
```

이동·상호작용·착석·주문·대기·식사·퇴장이 모두 같은 계약을 따르므로, 캐릭터는 "무엇을 할지" 만 큐에 넣고 "어떻게 할지" 는 각 행동이 스스로 안다.

### 실행 루프

`ActionExecutor` 는 캐릭터 생성 시 시작되는 단일 UniTask 루프를 돈다.

```
while (루프 살아있음)
    큐에 항목이 있으면
        Dequeue → 행동별 CTS 생성 → await ExecuteAsync(token)
        예외/취소는 흡수 (한 행동의 실패가 루프를 죽이지 않음)
        큐가 비었으면 ExecutionCompleted 이벤트 발행
    UniTask.Yield()
```

**두 단계 취소 토큰**을 쓴다.

| 토큰            | 범위          | 취소 시점                                            |
| --------------- | ------------- | ---------------------------------------------------- |
| `_loopCts`      | 루프 전체     | 캐릭터 파괴 (`StopLoop`)                             |
| `_executionCts` | 현재 행동 1개 | 새 입력, 대기 행동 조기 종료 (`CancelCurrentAction`) |

이 분리 덕분에 "지금 하던 것만 취소하고 다음 행동은 계속" 과 "전부 정지" 를 구분할 수 있다. `Clear()` 는 현재 행동 취소 + 큐 비우기를 한 번에 수행한다.

### 도달 가능성을 먼저 검사하는 이동

`InteractAction` 은 상호작용 지점 후보를 거리순으로 정렬한 뒤 **NavMesh 경로가 `PathComplete` 인 첫 지점**을 선택한다. 물리적으로 가깝지만 도달할 수 없는 지점을 향해 캐릭터가 갇히는 문제를 방지한다. 도착 후 지점의 회전값을 그대로 적용해 조리대를 바라보는 방향까지 맞춘다.

### 무한 대기 행동이라는 패턴

주문 대기와 음식 대기는 `UniTask.WaitUntilCanceled(ct)` 로 영원히 기다린다. 종료 조건을 스스로 검사하지 않고, 외부에서 상태가 바뀔 때 `CancelCurrentAction()` 으로 깨운다.

```
손님 착석 → WaitForOrderAction (대기 중)
직원/플레이어가 주문 받음 → 현재 행동 취소 → OrderAction → WaitForFoodAction (대기 중)
음식 서빙됨 → 현재 행동 취소 → EatAction → LeaveAction
```

폴링이 사라지고, 상태 전이 지점이 "누가 취소를 호출하는가" 로 명확히 드러난다.

---

## 6. 캐릭터 3종의 제어 모델

세 캐릭터 모두 `CharacterBase` 를 상속해 **들고 있는 아이템(`ICarriable`) 관리, 집기/내려놓기 애니메이션 동기화, `ActionExecutor` 소유, Day 종료·정리 이벤트 구독**을 공유한다. 차이는 "무엇이 큐를 채우는가" 다.

### 플레이어 — 입력 이벤트 구동

```
PlayerInputHandler (ITickable)
  터치/클릭 레이캐스트
    IInteractable 히트 → InteractEvent 발행
    지면 히트 → NavMesh 샘플링 → MoveEvent 발행
PlayerCharacter
  이벤트 수신 → 큐 비우고 해당 행동 하나만 등록
```

입력 핸들러는 `PlayerCharacter` 를 모르고, 캐릭터는 입력 장치를 모른다. 레이캐스트는 Character 레이어를 제외하므로 캐릭터가 클릭을 가로막지 않는다. Day 가 끝나면 입력이 비활성화되고 `GameCleanupEvent` 로 복구된다.

### 손님 — 시퀀스 사전 등록

`CustomerCharacter` 는 상황별로 행동 묶음을 한꺼번에 큐에 넣는다.

| 트리거         | 등록되는 시퀀스              |
| -------------- | ---------------------------- |
| 빈 테이블 있음 | 이동 → 착석 → 주문 대기      |
| 빈 테이블 없음 | 대기열 위치로 이동 → 줄 서기 |
| 주문 받음      | 주문 확정 → 음식 대기        |
| 전원 서빙 완료 | 식사 → 퇴장                  |
| 인내심 소진    | 화내는 연출 → 퇴장           |
| Day 종료       | 퇴장                         |

### 직원 — 작업 큐 풀(pull) 모델

직원은 손님과 반대로, 자신이 할 일을 **가져온다**.

```
StaffManager                                StaffCharacter
  OrderNeededEvent   → 주문받기 작업          작업 완료
  PlateOnCounterEvent → 서빙 작업              → TryGetWork()
  DirtyPlateEvent    → 접시치우기 작업            작업 있음 → 행동 시퀀스 등록
                                                  없음 → 유휴 순회 + WorkAdded 구독
  Enqueue → WorkAdded 이벤트 ────────────────→ 유휴 행동 취소하고 즉시 재조회
```

각 작업은 `IsValid()` 를 갖고 있고, 꺼내는 시점에 유효성을 검사해 무효 작업을 폐기한다. "직원이 접시를 들고 갔더니 손님이 이미 나간" 류의 상태 변화를 자연스럽게 흡수한다. 서빙 작업은 접시의 매칭 레시피가 실제로 그 테이블의 미서빙 주문에 포함되는지까지 확인한다.

작업 배분에서 `StaffManager` 는 접시가 서빙 카운터에 올라오면 **가장 먼저 주문한 테이블**을 찾아 배정한다(주문 시각 비교). 직원이 여러 명이어도 각자 처리 가능한 작업 종류만 가져가므로, 홀/주방 분업 확장이 열려 있다.

Day 종료 시 직원은 즉시 사라지지 않고 **진행 중인 작업을 끝낸 뒤** 퇴장한다.

---

## 7. 상호작용 · 주방 파이프라인

### 상호작용 계약

```csharp
public interface IInteractable
{
    Transform[] GetInteractPointsSortedByDistance(Vector3 from);
    UniTask InteractAsync(CharacterBase character, CancellationToken ct);
    void SetHighlight(bool highlight);
}
```

핵심 설계는 **상호작용이 명령이 아니라 문맥 해석**이라는 점이다. 상호작용 대상은 "캐릭터가 무엇을 들고 있는가 × 자신이 무엇을 담고 있는가" 로 동작을 결정한다. 플레이어 입장에서는 어디를 눌러도 "지금 상황에서 자연스러운 일" 이 일어난다.

| 대상         | 빈 손                    | 재료 들고 있음                 | 접시 들고 있음                         |
| ------------ | ------------------------ | ------------------------------ | -------------------------------------- |
| 냉장고       | 재료 꺼내기              | —                              | —                                      |
| 접시 렉      | 새 접시 꺼내기           | 접시 꺼내 재료 담아 들기       | (깨끗하면) 렉에 반환                   |
| 화로         | 재료 회수 (조리 중단)    | 올려놓고 조리 시작             | 화로 위 재료를 접시에 담기             |
| 카운터       | 놓인 것 집기             | 내려놓기 / 접시 위에 재료 얹기 | 내려놓기 / 카운터 재료를 접시에 담기   |
| 싱크대       | 설거지 시작 or 접시 집기 | —                              | (더러우면) 내려놓고 설거지 → 자동 집기 |
| 테이블(손님) | 주문 받기                | —                              | 서빙                                   |
| 테이블(빈)   | 더러운 접시 회수         | —                              | —                                      |
| 쓰레기통     | —                        | 버리기                         | 버리기                                 |

### 조리 흐름

`Stove` 는 재료를 올린 순간부터 **조리 → 오버쿡 → 폐기**를 하나의 연속 비동기 흐름으로 실행한다.

```
조리 루프 (KitchenStatService.GetModifiedDuration() 초)
  → 재료 변환 (원본 파괴 → 결과 재료 생성, 슬롯에 부착)
  → 경고 게이지 전환
오버쿡 루프 (KitchenStatService.GetOvercookDuration() 초)
  → 시간 초과 시 재료 폐기 (검게 변색)
```

중간에 캐릭터가 재료를 집어가거나 Day 가 끝나면 `StopCooking()` 이 조리용 CTS 를 취소해 어느 단계에서든 안전하게 빠져나온다.

### 재료 변환은 데이터가 정의한다

조리 규칙이 코드에 없다. 각 재료가 "나는 어떤 변환으로 무엇이 되는가" 를 데이터로 갖고 있고, 도구는 자신이 지원하는 변환 타입이 그 재료에 있는지만 확인한다.

```
IngredientData
  └── Transitions[]
        ├── Type   (Cook / Overcook / Slice)
        └── Result (변환 결과 IngredientData)
```

`Stove` 는 `Cook` 변환이 있는 재료만 받는다. 슬라이서 등 새 도구는 `CookingToolBase` 를 상속해 지원 변환 타입만 바꾸면 되고, 새 메뉴 추가는 애셋 작업만으로 끝난다.

### 레시피 매칭

`Plate` 는 재료가 담길 때마다 전체 재료 목록으로 레시피 매칭을 시도해 `MatchedRecipeID` 를 갱신한다. 매칭은 "필요 재료 수가 같고 모두 포함" 조건이며, 같은 재료 중복 담기는 거부된다. 서빙 가능 여부, 직원 작업 배정, 코인 계산이 모두 이 ID 하나를 기준으로 동작한다.

### 완결 루프

```
냉장고 ─재료─→ 화로 ─조리─→ 접시(레시피 매칭) ─→ 서빙 카운터 ─→ 테이블
                                                                   │
                            접시 렉 ←─세척── 싱크대 ←─더러운 접시──┘
```

접시가 유한 자원(렉 수량)이라 세척 루프가 병목으로 작동하고, 그래서 세척 시간 단축 카드가 의미를 갖는다.

---

## 8. Day 진행 시스템

### 런타임 모델

`DayContext` 는 현재 하루의 상태를 담는 유일한 원본이다. 여러 시스템이 각자 사본을 두는 대신, 이 객체의 `ReactiveProperty` 를 구독한다.

```
DayContext
├── DayNumber        ReactiveProperty<int>    → HUD, 최대 손님 수 계산
├── TimeBarElapsed   ReactiveProperty<float>  → HUD 타임바, 손님 스폰, 낮/밤 조명
├── TimeBarDuration  float
├── EarnedCoins      ReactiveProperty<int>    → 정산 팝업
└── IsCompleted      bool
```

영업 중 획득한 코인은 `DayContext.EarnedCoins` 에만 쌓이고, 하루를 성공적으로 마쳐야 계정 코인에 반영된다. 실패하면 그날 수익이 사라지는 규칙이 자연스럽게 성립한다.

### 난이도 곡선

`DaysData` 가 Day 번호를 받아 계산한다. Day 별 값을 일일이 정의하지 않고 규칙으로 생성하므로, 데이터 몇 개만 바꿔 전체 곡선을 조정할 수 있다.

```
구간 인덱스 = (Day - 1) / 구간길이(3)

영업 시간   = 기본 100초 + 구간 인덱스 × 25초    → 1~3일: 100초, 4~6일: 125초 …
최대 손님   = 기본 4명   + 구간 인덱스 × 1명     → 1~3일: 4명,  4~6일: 5명 …
```

### 하루의 생애 (`DayFlowService`)

`DayFlowService` 는 연출·팝업·서비스 호출 순서를 아는 유일한 오케스트레이터다. `DayProgressService` 는 상태만 관리하고 흐름을 모른다.

```
첫 Day 진입
  InitDay → 손님/직원 서비스 초기화 → 스토리 팝업 → 카메라 인트로 → 타임바 시작

Day 종료 (DayEndedEvent 수신)
  카메라 아웃트로
  ├─ 성공 → 정산 팝업 (await 결과)
  │           Next → SetNextDay → 스토리 팝업 → 상점 팝업 → 인트로 → 타임바 시작
  │           Exit → 로비로
  └─ 실패 → 실패 팝업 (await 결과)
              Restart → 인트로 → RestartDay
              Exit    → 로비로
```

팝업이 `await` 로 사용자 선택을 반환하므로, 흐름 전체가 콜백 중첩 없는 하나의 선형 코드로 읽힌다.

### 성공/실패 판정 경로

| 결과     | 판정 주체         | 조건                                      |
| -------- | ----------------- | ----------------------------------------- |
| **성공** | `CustomerService` | 서빙 완료된 손님 누계 ≥ 그날 최대 손님 수 |
| **실패** | `DiningTable`     | 인내심 게이지 타임아웃                    |

**타임바가 0이 되어도 하루는 끝나지 않는다.** 타임바는 손님 입장 마감선일 뿐이고, 하루는 그날 배정된 손님 전원이 서빙을 마쳐야 끝난다.

### Day 전환과 리셋

`RestartDay` / `SetNextDay` 는 모두 `GameCleanupEvent` 를 발행한다. 씬 재로드가 없으므로 프레임 하락 없이 즉시 재시작되고, 각 객체는 자기 상태만 초기화한다. `CustomerService` 는 이 시점에 그날의 영업 시간·최대 손님 수를 다시 읽어온다.

---

## 9. 손님 스폰과 인내심

### 스폰 타이밍 분포

균등 간격이 아니라 **중반에 몰리는 곡선**을 쓴다. 실제 식당의 피크 타임처럼 난이도에 리듬을 만든다.

```
버퍼 = Config.SpawnBufferDuration          (앞뒤 여유 시간)
윈도우 = 영업 시간 - 버퍼 × 2
진행도 p = 손님 인덱스 / (최대 손님 - 1)

왜곡(p) = p + (강도 / 2π) × sin(2π × p)     강도 0 = 균등, 1 = 중반 집중
스폰 시각 = 버퍼 + 왜곡(p) × 윈도우
```

`TimeBarElapsed` 구독이 다음 스폰 시각을 넘는 프레임을 감지해 스폰을 트리거한다. 강도는 0~1 로 클램프되는데, 1을 넘으면 함수의 단조성이 깨져 스폰 순서가 뒤집히기 때문이다.

### 그룹과 대기열

그룹 크기는 최대 그룹 크기 이하로 무작위 결정되며, 그날 남은 손님 수를 넘지 않는다. 구성원은 서로 다른 캐릭터 프리팹으로 채워진다.

```
그룹 도착
├── 대기열 있음 → 무조건 대기열 뒤로 (선착순 보장)
├── 그룹 인원을 수용할 빈 테이블 있음 → 즉시 착석
└── 없음 → 대기열 추가
```

테이블이 완전히 비면(손님 없고 접시 없음) `DiningTable` 이 `CustomerService` 에 알리고, 맨 앞 그룹이 좌석 수를 충족하면 입장한다. 이후 남은 대기열은 위치가 재정렬된다.

### 인내심 (`TableGauge`)

인내심은 손님 개인이 아니라 **테이블 단위** 게이지다. 그룹 전체가 하나의 시한폭탄을 공유한다.

```
첫 손님 착석 → 게이지 시작 (벨 아이콘, 주문 대기)
주문 받음     → 음식 대기로 전환, 게이지 리셋 (시계 아이콘)
1명 서빙 완료 → 게이지 일부 회복
전원 서빙 완료 → 게이지 정지 → 식사 → 퇴장
타임아웃      → 전원 화내며 퇴장 + Day 실패
```

`TableGauge` 는 `IUpdatable` 로 중앙 업데이트 서비스에 등록되며, 필요할 때만 등록/해제되므로 대기 중이 아닌 테이블은 프레임 비용이 0이다.

### 낮/밤 연출

`DayNightService` 는 타임바 진행도를 3구간으로 나눠 실내외 조명과 앰비언트를 보간한다. 앞 1/3 은 완전한 낮, 중간 1/3 에서 전환, 뒤 1/3 은 완전한 밤이다. 실내 조명은 변화폭을 작게 잡아 플레이 가시성을 유지한다.

---

## 10. 카드 · 상점 시스템

### 카드 데이터와 효과의 분리

`CardData` 는 상점에서의 성질(가격, 등장 확률, 최대 티어)을, `CardEffectData` 파생 클래스는 실제 효과를 담는다. 상점 로직은 효과 종류를 몰라도 동작하고, 효과 적용은 가격 정책을 모른다.

```
CardData
├── Cost, PriceIncrement   실제 가격 = Cost + PriceIncrement × 보유수
├── MaxTier                최대 구매 횟수 (테이블 4, 버프 3, 메뉴 1)
├── Weight                 오퍼 등장 가중치
├── IsOneDayOnly           소유되지 않는 당일 한정 카드 (알바)
└── Effect  ──→ CardEffectData (다형성)
```

효과는 조리·세척 시간 단축, 오버쿡 허용 시간 연장, 레시피 해금, 테이블 추가, 당일 한정 직원 소환의 다섯 계열이다. 시간 단축과 오버쿡 연장은 티어가 쌓이며 누적되고, 테이블은 배치 지점 수만큼 늘어난다.

### 오퍼 구성 규칙 (`ShopService`)

3슬롯을 서로 다른 규칙으로 채워, 매번 다르면서도 예측 가능한 선택지를 만든다.

```
슬롯 0 : 테이블 카드 고정 (최대 티어 도달 시 SoldOut)
슬롯 1·2 : 비테이블 후보에서 가중치 무작위 2장
             ├── 최대 티어 도달 카드 제외
             ├── 같은 버프 라인은 후보에 1장만 (중복 오퍼 방지)
             └── 후보 부족 시 SoldOut
```

카드 색은 데이터가 아니라 상태에서 **파생**된다. 테이블은 노랑, 처음 사는 카드는 초록, 이미 보유한 카드(업그레이드)는 파랑. 플레이어가 색만 보고 "새 카드 / 강화" 를 구분할 수 있고, 데이터에 색을 중복 저장할 필요가 없다.

### 티어 = 중복 구매 횟수

구매 기록은 **중복을 허용하는 ID 리스트**다. 별도의 티어 필드 없이 리스트 내 등장 횟수가 곧 티어이며, 게임 재진입 시 등장 횟수만큼 효과를 재적용하면 누적 상태가 그대로 복원된다. 저장 데이터가 단순해지고 마이그레이션 부담이 없다.

```
효과값 = 기본값 + (티어 - 1) × 티어당 증가량
```

당일 한정 카드(`IsOneDayOnly`)는 구매 기록에 남지 않으므로 다음 진입 시 재적용되지 않는다.

### 주방 스탯 계산 (`KitchenStatService`)

카드 효과는 각 도구에 직접 적용되지 않고 이 서비스 하나에 누적된다. 화로와 싱크대는 매 조리/세척 시작 시 현재 값을 조회하므로, 카드 구매가 즉시 반영되고 상한 정책도 한 곳에서 관리된다.

| 메서드                  | 계산식                     | 한계             |
| ----------------------- | -------------------------- | ---------------- |
| `GetModifiedDuration()` | `기준 × (1 - 누적 단축률)` | 최소 기준의 50%  |
| `GetOvercookDuration()` | `기준 × (1 + 누적 연장률)` | 최대 기준의 150% |

---

## 11. 팝업 시스템

### 스코프 소유 모델

팝업의 핵심 설계는 **어느 리졸버가 이 팝업을 소유하는가**다.

```
RegisterPopup(키, 리졸버)   팝업 키 ↔ 소유 리졸버 등록
Open<Presenter>(키)
  ├── 프리팹 로드 → 캔버스 루트에 생성
  ├── 소유 리졸버의 자식 스코프 생성 (View 인스턴스 + Presenter 등록)
  ├── Presenter resolve → ShowAsync → 사용자 선택까지 await
  └── finally: 스코프 Dispose + 오브젝트 파괴
```

게임 씬 팝업은 게임 리졸버가 소유하므로 그 Presenter 가 게임 서비스(상점, 진행도)를 주입받을 수 있고, 공통 팝업은 Root 리졸버가 소유하므로 씬 전환에도 살아남는다.

`ScenePopupRegistrar` 는 씬 스코프의 엔트리포인트로서 시작 시 자기 씬 팝업들을 등록하고, 소멸 시 자기 소유 팝업을 모두 닫고 카탈로그에서 해제한다. **씬이 사라졌는데 팝업만 남는 고아 상태가 구조적으로 발생하지 않는다.** 등록을 스코프 구성 단계가 아니라 엔트리포인트 시작 단계에서 하는 이유는, 구성 시점에는 리졸버가 아직 완성되지 않았기 때문이다.

### await 반환 패턴

Presenter 베이스가 `UniTaskCompletionSource` 를 들고 있어 팝업 결과를 `await` 로 받는다. 인자·반환 타입 조합에 따라 4가지 오버로드를 제공한다.

```csharp
var action = await _popupService.Open<PopupCompletedPresenter, DayCompletedData, DayCompletedAction>(키, 데이터);
switch (action) { case Next: …; case Exit: …; }
```

호출자가 콜백 등록 없이 팝업 결과에 따라 분기할 수 있어, `DayFlowService` 의 하루 흐름 전체가 위에서 아래로 읽히는 코드가 된다.

---

## 12. 프레임 업데이트 관리

`UpdateSubscriptionService` 는 씬에 존재하는 단 하나의 MonoBehaviour 로, 모든 프레임 로직을 대신 호출한다. 개별 오브젝트의 `Update` 를 없애 Unity 의 매직 메서드 호출 오버헤드를 줄이고, 실행 순서와 등록 목록을 한 곳에서 파악할 수 있게 한다.

매 프레임(`IUpdatable`), LateUpdate, FixedUpdate, 그리고 지정 간격마다 호출되는 주기 갱신의 네 종류를 지원한다.

**반복 중 등록/해제 문제**를 두 가지 전략으로 처리한다. Update 목록은 역방향으로 순회하며 인덱스를 보정하고(즉시 반영), LateUpdate/FixedUpdate 목록은 대기 목록에 모아 다음 프레임에 반영한다. 콜백 안에서 구독을 해제해도 컬렉션 변경 예외나 항목 건너뜀이 발생하지 않는다.

현재 사용처는 `DayProgressService`(타임바 누적)와 `TableGauge`(인내심 감소)이며, 둘 다 필요한 순간에만 등록되고 끝나면 해제된다.

---

## 13. 이벤트 흐름

시스템 간 결합의 실제 배선도다. 화살표 왼쪽이 발행, 오른쪽이 반응이다.

```
[전역 브로드캐스트] — 구독자가 가장 많은 두 축

DayEndedEvent        하루 종료
  DayProgressService ─→ 계정 저장 / 하루 흐름 오케스트레이션(정산·실패 팝업)
                     ─→ 전 캐릭터 행동 중단, 테이블·조리도구 정지, 입력 차단

GameCleanupEvent     Day 재시작 · 다음 Day 진입
  DayProgressService ─→ 손님 서비스·직원 작업큐 초기화, Day 이벤트 재적용
                     ─→ 전 캐릭터·전 상호작용 오브젝트 자기 상태 리셋, 입력 복구


[게임플레이 배선]

PaymentEvent          식사 완료 ─→ 그날 획득 코인 누적
CustomerRemovedEvent  손님 퇴장 ─→ 서빙 카운트 → Day 성공 판정
OrderNeededEvent      손님 착석 ─→ 직원 "주문받기" 작업 등록
PlateOnCounterEvent   접시 배출 ─→ 직원 "서빙" 작업 등록
DirtyPlateEvent       빈 테이블 ─→ 직원 "접시치우기" 작업 등록
MoveEvent             지면 터치 ─→ 플레이어 이동
InteractEvent         대상 터치 ─→ 플레이어 상호작용
```

새 게임 오브젝트나 서비스를 추가할 때 **전역 브로드캐스트 두 개에 대한 대응(행동 중단 / 상태 초기화)을 구현하는 것이 사실상 필수 관례다.** 이걸 빼먹으면 Day 재시작 후 유령 상태가 남는다.

---

## 관련 문서

- `DESIGN.md` — 게임 기획서 (스토리, 밸런스 목표, 시스템 의도)
- `CLAUDE.md` — 코드 작업 규칙 (수정 프로세스, 네이밍 컨벤션)