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

## 참고 스킬

게임 아키텍처 및 시스템 상세 명세는 아래 스킬을 참조:
- `order-rush-architecture` - 기술 스택, VContainer Scope 구조, MVP 패턴, DI 라이프타임, UpdateSubscriptionService, MessagePipe
- `order-rush-game-systems` - 행동 큐, Day/Run 시스템, 손님/스폰, Account, 재료/레시피 데이터 구조