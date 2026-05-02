using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitForOrderAction : IGameAction
{

    private readonly CustomerCharacter _customer;
    public WaitForOrderAction(CustomerCharacter customer)
    {
        _customer = customer;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        float elapsed = 0f;

        while (elapsed < CustomerCharacter.WAIT_TIME_LIMIT)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / CustomerCharacter.WAIT_TIME_LIMIT;
            _customer.SetWaitGauge(progress);
            await UniTask.Yield();
        }

        // 시간 초과 → 이탈 처리
        _customer.OnWaitTimeout();

        await UniTask.CompletedTask;
    }
}

/*
// 게이지 시스템 적용 예시 (주석)
//
// 사용법:
// 1. GaugeFactory 주입
// 2. ExecuteAsync에서 게이지 생성 및 표시
// 3. 매 프레임 UpdatePosition() + SetProgress() 호출
// 4. finally에서 Factory.Release()

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitForOrderAction : IGameAction
{
    private readonly CustomerCharacter _customer;
    private readonly GaugeFactory _gaugeFactory;
    private readonly Sprite _bellIcon;  // 종 아이콘

    public WaitForOrderAction(
        CustomerCharacter customer,
        GaugeFactory gaugeFactory,
        Sprite bellIcon)
    {
        _customer = customer;
        _gaugeFactory = gaugeFactory;
        _bellIcon = bellIcon;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        GaugePresenter gaugePresenter = null;

        try
        {
            // 게이지 생성
            gaugePresenter = _gaugeFactory.Create(
                _customer.transform,
                new Vector3(0, 2f, 0),  // 손님 머리 위
                _bellIcon);
            gaugePresenter.Show();

            float elapsed = 0f;

            while (elapsed < CustomerCharacter.WAIT_TIME_LIMIT)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / CustomerCharacter.WAIT_TIME_LIMIT;

                // 좌표 업데이트 + 진행도 설정
                gaugePresenter.UpdatePosition();
                gaugePresenter.SetProgress(progress);

                // 색상 변화 (초록 → 빨강)
                Color color = Color.Lerp(Color.green, Color.red, progress);
                gaugePresenter.SetColor(color);

                await UniTask.Yield(ct);
            }

            // 시간 초과 → 이탈 처리
            _customer.OnWaitTimeout();
        }
        finally
        {
            // 게이지 정리
            if (gaugePresenter != null)
            {
                _gaugeFactory.Release(gaugePresenter);
            }
        }

        await UniTask.CompletedTask;
    }
}
*/
