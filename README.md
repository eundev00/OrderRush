# Order Rush

모바일 환경에서 터치 기반 입력과 행동 큐 시스템을 활용한 자동화 주방 시뮬레이션 게임

## 기술 스택

| 역할 | 라이브러리 |
|------|-----------|
| DI | VContainer 1.17.0 |
| UI 패턴 | MVP (Model-View-Presenter) |
| 비동기 | UniTask |
| Reactive UI | UniRx 7.1.0 (View 전용) |
| 프레임 루프 | UpdateSubscriptionService |
| 이벤트 | MessagePipe |


## VContainer Scope 계층 구조

```
ProjectLifetimeScope          ← 앱 전체 생존
└── GameLifetimeScope         ← 게임플레이 생존
    └── StageLifetimeScope    ← 맵마다 생성/소멸 (추후 확장)
```

### 확장 시 (Lobby 추가 등)
```
ProjectLifetimeScope
├── LobbyLifetimeScope        ← 로비 씬 진입 시 생성, 나가면 소멸
└── GameLifetimeScope
    └── StageLifetimeScope
```

- `LobbyLifetimeScope`와 `GameLifetimeScope`는 형제 관계 (동시에 존재하지 않음)
- 자식 Scope는 부모 Scope의 의존성을 주입받을 수 있음
