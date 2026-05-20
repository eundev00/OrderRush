# Order Rush

PlateUp! 스타일의 레스토랑 시뮬레이션 게임
주문부터 서빙까지 바쁜 레스토랑을 운영하세요.

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

---

## 관련 문서
- [DESIGN.md](DESIGN.md) - 게임 기획서
- [CLAUDE.md](CLAUDE.md) - 개발 가이드

