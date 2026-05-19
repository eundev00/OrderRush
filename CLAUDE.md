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

**상세 동작 방식**
- FIFO 큐 기반으로 행동을 순차적으로 처리
- 각 행동은 UniTask로 비동기 실행되며 완료 대기
- 새로운 클릭 시 현재 실행 중인 행동을 취소하고 큐를 비움
- MoveAction 완료 후 자동으로 InteractAction 실행 (체인 처리)
- 행동 취소 시 CancellationToken을 통한 안전한 종료

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

### MVP 패턴 상세

**Model**
- 비즈니스 로직과 데이터 관리를 담당
- Service 레이어로 구현 (LevelProgressService, InventoryService 등)
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
- 글로벌 이벤트: 레벨 클리어, 게임 오버, 설정 변경 등
- 로컬 이벤트: 재료 상태 변경, 주문 완료, UI 업데이트 등
- 필터링 가능: 특정 조건의 이벤트만 구독

**사용 패턴**
- Service에서 상태 변경 시 이벤트 발행
- Presenter가 이벤트 구독하여 View 업데이트
- IDisposable 패턴으로 구독 정리 보장

### DI Scope별 서비스 라이프타임

**ProjectLifetimeScope**
- 앱 전체에서 공유되는 싱글톤 서비스
- 사운드 매니저, 설정 서비스, 데이터 로더 등
- 앱 시작부터 종료까지 유지
- 모든 하위 Scope에서 접근 가능

**LobbyLifetimeScope**
- 로비 씬 전용 서비스
- 레벨 선택 UI, 로비 상태 관리 등
- 로비 씬 언로드 시 함께 소멸
- ProjectScope의 서비스 주입 가능

**GameLifetimeScope**
- 게임플레이 관련 서비스
- 플레이어 컨트롤러, 레벨 컨텍스트, 주문 시스템 등
- 게임 씬 언로드 시 함께 소멸
- ProjectScope의 서비스 주입 가능

**설계 원칙**
- 각 Scope는 자신의 책임 범위 내 서비스만 등록
- 부모 Scope의 서비스는 자식에서 자동 주입
- Scope 소멸 시 모든 서비스 자동 정리

---

