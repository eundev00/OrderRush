using System;
using MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

// =====================================================================
//  게임 도메인 이벤트 → 효과음 매핑 전담 (게임 스코프 엔트리포인트)
// ---------------------------------------------------------------------
//  - SoundService(순수 재생 엔진)는 게임 이벤트를 모른다. "무슨 이벤트가
//    무슨 소리를 내는가"의 지식은 오직 이 클래스에만 모인다.
//  - 구독자는 생성자로 나열하지 않고 IObjectResolver 로 그때그때 resolve 한다.
//    → 새 효과음 이벤트가 늘어도 생성자는 그대로, Start()에 Bind<T>() 한 줄만 추가.
//    (MessagePipe+VContainer 는 ISubscriber<T> 를 오픈 제네릭으로 등록하므로 resolve 가능)
//  - 이미 발행 중인 이벤트만 구독하므로 게임플레이 코드는 수정하지 않는다.
//  - UI 클릭/조리/픽업처럼 자연 이벤트가 없는 것은 호출부에서 직접
//    ISoundService.PlaySfx(...) 를 호출한다 (여기 넣지 않음).
//
//  ⚠ DI 등록(GameLifetimeScope 의 RegisterEntryPoint<GameAudioBinder>())은 이후 단계.
// =====================================================================
public class GameAudioBinder : IStartable, IDisposable
{
    private readonly ISoundService _sound;
    private readonly IObjectResolver _resolver;
    private readonly CompositeDisposable _disposables = new();

    public GameAudioBinder(ISoundService sound, IObjectResolver resolver)
    {
        _sound = sound;
        _resolver = resolver;
    }

    public void Start()
    {
        // 이벤트 ↔ 효과음 매핑 (새 매핑은 여기 한 줄씩 추가 — 생성자는 안 바뀜)
        // Bind<PaymentEvent>(_ => _sound.PlaySfx(AudioKeys.Sfx_Coin));
        // Bind<OrderNeededEvent>(_ => _sound.PlaySfx(AudioKeys.Sfx_Bell));
        // Bind<CustomerRemovedEvent>(e => _sound.PlaySfx(e.WasServed ? AudioKeys.Sfx_CustomerHappy : AudioKeys.Sfx_CustomerAngry));
        // Bind<PlateOnCounterEvent>(_ => _sound.PlaySfx(AudioKeys.Sfx_Plate));
    }

    private void Bind<T>(Action<T> handler)
    {
        var subscriber = _resolver.Resolve<ISubscriber<T>>();
        subscriber.Subscribe(handler).AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
