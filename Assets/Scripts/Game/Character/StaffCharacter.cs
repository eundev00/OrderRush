using VContainer;

public abstract class StaffCharacter : CharacterBase
{
    private StaffManager _staffManager;
    private ILevelService _levelService;
    private bool _isLeaving;
    private bool _workInProgress;

    public NavMeshMover Mover => _mover;
    public CharacterAnimator Animator => _animator;
    protected ServingCounter[] ServingCounters => _levelService.Context.ServingCounters;
    protected Counter[] KitchenCounters => _levelService.Context.KitchenCounters;

    [Inject]
    public void Construct(StaffManager staffManager, ILevelService levelService)
    {
        _staffManager = staffManager;
        _levelService = levelService;
    }

    protected void Start()
    {
        _actionExecutor.ExecutionCompleted += OnActionCompleted;
        TryGetWork();
    }

    public void TryGetWork()
    {
        var work = _staffManager.TryDequeue(CanHandle);
        if (work != null)
        {
            _workInProgress = true;
            EnqueueWork(work);
        }
        else
        {
            _workInProgress = false;
            EnqueueAction(new StaffIdleAction(_mover, _levelService.Context.StaffIdlePoints));
            _staffManager.WorkAdded += OnWorkAdded;
        }
    }

    private void OnWorkAdded()
    {
        _staffManager.WorkAdded -= OnWorkAdded;
        _actionExecutor.Clear();
    }

    private void OnActionCompleted()
    {
        _workInProgress = false;

        if (_isLeaving)
            EnqueueExit();
        else
            TryGetWork();
    }

    protected override void OnDayEnded()
    {
        if (_isLeaving)
            return;

        _isLeaving = true;
        _staffManager.WorkAdded -= OnWorkAdded;

        // 진행 중인 작업이 있으면 끝낸 뒤 OnActionCompleted에서 퇴장, 유휴면 즉시 퇴장
        if (!_workInProgress)
        {
            _actionExecutor.Clear();
            EnqueueExit();
        }
    }

    private void EnqueueExit()
    {
        _actionExecutor.ExecutionCompleted -= OnActionCompleted;
        EnqueueAction(new StaffLeaveAction(this, _levelService.Context.SpawnPoint.position, _mover, _animator));
    }

    protected abstract bool CanHandle(WorkType type);
    protected abstract void EnqueueWork(WorkItem item);

    protected override void OnDestroy()
    {
        _actionExecutor.ExecutionCompleted -= OnActionCompleted;
        _staffManager.WorkAdded -= OnWorkAdded;
        base.OnDestroy();
    }
}
