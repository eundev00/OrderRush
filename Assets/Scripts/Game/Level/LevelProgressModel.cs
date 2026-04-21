using System;
using UniRx;

public class LevelProgressModel : IDisposable
{
    private readonly ReactiveProperty<int> _currentMoney = new(0);
    private readonly ReactiveProperty<float> _remainingTime = new(0f);
    private readonly ReactiveProperty<bool> _isLevelActive = new(false);

    private int _targetMoney;
    private float _timeLimit;

    // 외부 구독용 (읽기 전용)
    public IReadOnlyReactiveProperty<int> CurrentMoney => _currentMoney;
    public IReadOnlyReactiveProperty<float> RemainingTime => _remainingTime;
    public IReadOnlyReactiveProperty<bool> IsLevelActive => _isLevelActive;

    // 목표 데이터 (읽기 전용)
    public int TargetMoney => _targetMoney;
    public float TimeLimit => _timeLimit;

    // 레벨 초기화
    public void Initialize(int targetMoney, float timeLimit)
    {
        _targetMoney = targetMoney;
        _timeLimit = timeLimit;
        _currentMoney.Value = 0;
        _remainingTime.Value = timeLimit;
        _isLevelActive.Value = true;
    }

    // 금액 추가
    public void AddMoney(int amount)
    {
        if (!_isLevelActive.Value) return;
        _currentMoney.Value += amount;
    }

    // 남은 시간 업데이트
    public void UpdateRemainingTime(float remainingTime)
    {
        _remainingTime.Value = remainingTime;
    }

    // 레벨 종료
    public void SetLevelInactive()
    {
        _isLevelActive.Value = false;
    }

    // 클리어 여부 확인
    public bool IsCompleted() => _currentMoney.Value >= _targetMoney;

    // 실패 여부 확인 (시간 초과)
    public bool IsTimeExpired() => _timeLimit > 0 && _remainingTime.Value <= 0;

    public void Dispose()
    {
        _currentMoney?.Dispose();
        _remainingTime?.Dispose();
        _isLevelActive?.Dispose();
    }
}
