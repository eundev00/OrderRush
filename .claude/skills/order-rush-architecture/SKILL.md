---
name: order-rush-architecture
description: Order Rush 프로젝트의 기술 스택, VContainer Scope 계층 구조, MVP 패턴, DI Scope별 서비스 라이프타임, UpdateSubscriptionService, MessagePipe 이벤트 시스템에 대한 레퍼런스. Order Rush 코드 작업 시, 특히 새 서비스/Presenter 추가, Scope 등록 위치 결정, 이벤트 발행/구독 구조 설계, 프레임 업데이트 로직 구현 시 참조.
---

# Order Rush - 아키텍처

행동 큐 시스템 기반의 모바일 레스토랑 시뮬레이션. MVP 패턴, VContainer DI, MessagePipe 이벤트로 구현한 PlateUp! 스타일 게임.

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

## DI Scope별 서비스 라이프타임

### ProjectLifetimeScope
- 앱 전체에서 공유되는 싱글톤 서비스
- AccountService (코인, 레시피 언락 - 런 간 이월)
- LocalStorageService (영구 저장)
- ResourcesLoaderService (ScriptableObject 로드)
- UpdateSubscriptionService (프레임 루프 관리)
- 앱 시작부터 종료까지 유지
- 모든 하위 Scope에서 접근 가능

### LobbyLifetimeScope
- 로비 씬 전용 서비스
- 로비 UI Presenter 등
- 로비 씬 언로드 시 함께 소멸
- ProjectScope의 서비스 주입 가능

### GameLifetimeScope
- 게임플레이 관련 서비스
- DayProgressService (Day 진행 관리)
- CustomerService (손님 스폰 관리)
- LevelContextPresenter (레벨 맵 관리)
- PlayerController, OrderSystem 등
- 게임 씬 언로드 시 함께 소멸
- ProjectScope의 서비스 주입 가능

### 설계 원칙
- 각 Scope는 자신의 책임 범위 내 서비스만 등록
- 부모 Scope의 서비스는 자식에서 자동 주입
- Scope 소멸 시 모든 서비스 자동 정리

---

## MVP 패턴 상세

### Model
- 비즈니스 로직과 데이터 관리를 담당
- Service 레이어로 구현 (DayProgressService, AccountService, CustomerService 등)
- 상태 변경 시 이벤트 발행 (MessagePipe 사용)
- VContainer를 통해 Presenter에 주입

### View
- UI 표시 및 사용자 입력 수신
- UniRx를 통한 Reactive 바인딩으로 상태 자동 반영
- MonoBehaviour 기반, Presenter로부터 명령 수신
- 비즈니스 로직 없이 순수 UI 처리만 담당

### Presenter
- Model과 View를 중재하는 중간 계층
- View의 이벤트를 받아 Model 호출
- Model의 상태 변경을 View에 전달
- VContainer를 통해 생명주기 관리
- IDisposable 구현으로 구독 정리

---

## UpdateSubscriptionService

### 개념
- MonoBehaviour의 Update 호출을 구독 패턴으로 추상화
- 매 프레임 업데이트가 필요한 로직을 중앙에서 관리
- Update 메서드 호출 횟수를 최소화하여 성능 최적화

### 사용 방식
- IUpdatable 인터페이스를 구현한 객체를 등록
- Presenter에서 구독하여 매 프레임 로직 실행
- Dispose 시 자동으로 구독 해제
- VContainer와 통합되어 생명주기 자동 관리

### 장점
- MonoBehaviour 없이도 프레임 업데이트 가능
- 중앙 집중식 관리로 디버깅 용이
- 불필요한 Update 호출 제거

---

## MessagePipe 이벤트 시스템

### Pub/Sub 패턴
- 발행자와 구독자가 느슨하게 결합
- 이벤트 기반 아키텍처로 모듈 간 의존성 최소화
- VContainer Integration으로 DI 지원

### 이벤트 종류
- 글로벌 이벤트: Day 완료, Day 실패, Run 완료 등
- 로컬 이벤트: PaymentEvent (코인 획득), 재료 상태 변경, 주문 완료 등
- 필터링 가능: 특정 조건의 이벤트만 구독

### PaymentEvent 사용 예시
```
// 발행: 손님 서빙 완료 시
publisher.Publish(new PaymentEvent { Amount = recipe.Coin });

// 구독: DayProgressService에서 코인 누적
subscriber.Subscribe(evt => currentDayContext.EarnedCoins.Value += evt.Amount);
```

### 사용 패턴
- Service에서 상태 변경 시 이벤트 발행
- Presenter가 이벤트 구독하여 View 업데이트
- IDisposable 패턴으로 구독 정리 보장
