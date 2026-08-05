using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using MessagePipe;
using System;

public class PlayerCharacter : CharacterBase
{
    IDisposable _subscription;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private SpawnFactory _spawnFactory;
    private IResourcesLoaderService _resourceLoader;

    protected override void Awake()
    {
        base.Awake();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    [Inject]
    public void Construct(
        ISubscriber<MoveEvent> moveSubscriber,
        ISubscriber<InteractEvent> interactSubscriber,
        ISubscriber<SpawnTestItemEvent> spawnTestItemSubscriber,
        SpawnFactory spawnFactory,
        IResourcesLoaderService resourceLoader)
    {
        _spawnFactory = spawnFactory;
        _resourceLoader = resourceLoader;

        var bag = DisposableBag.CreateBuilder();
        moveSubscriber.Subscribe(OnMoveEvent).AddTo(bag);
        interactSubscriber.Subscribe(OnInteractEvent).AddTo(bag);
        spawnTestItemSubscriber.Subscribe(_ => OnSpawnTestItem()).AddTo(bag);
        _subscription = bag.Build();
    }

    protected override void OnGameCleanup()
    {
        base.OnGameCleanup();
        ClearActions();
        WarpTo(_initialPosition);
        transform.rotation = _initialRotation;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _subscription?.Dispose();
    }

    void OnMoveEvent(MoveEvent e)
    {
        _actionExecutor.Clear();
        _actionExecutor.Enqueue(new MoveAction(_mover, e.Destination, _animator));
    }

    void OnInteractEvent(InteractEvent e)
    {
        _actionExecutor.Clear();
        _actionExecutor.Enqueue(new InteractAction(_mover, e.Target, this, _animator));
    }

    async void OnSpawnTestItem()
    {
        if (IsHolding) return;

        var plate = await _spawnFactory.Create<Plate>(PrefabKeys.GetPrefabPath(PrefabKeys.Plate));

        var ingredientData = await _resourceLoader.LoadAsync<IngredientData>(DataKeys.GetDataPath(DataKeys.Steak));
        var ingredient = await _spawnFactory.Create<IngredientObject>(PrefabKeys.GetPrefabPath(PrefabKeys.Steak));
        ingredient.SetData(ingredientData);

        plate.TryPlaceOntoOther(ingredient);
        await PickUp(plate);
    }
}
