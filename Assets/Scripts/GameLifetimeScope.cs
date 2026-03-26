using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] GameObject _playerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        // Services
        builder.Register<ICookingService, CookingService>(Lifetime.Singleton);
        builder.Register<IOrderService, OrderService>(Lifetime.Singleton);

        // Input Handler
        builder.RegisterEntryPoint<PlayerInputHandler>();

        builder.RegisterBuildCallback(container =>
        {
            container.InjectGameObject(_playerPrefab); // Addressables 로드 예정
        });
    }
}