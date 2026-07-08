using OrderRush.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [NotNull][SerializeField] Transform _root;
    [NotNull][SerializeField] RectTransform _gaugeContainer;

    protected override void Configure(IContainerBuilder builder)
    {

        // Services
        builder.Register<IDayProgressService, DayProgressService>(Lifetime.Scoped);
        builder.Register<CustomerService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<StaffManager>(Lifetime.Singleton);
        builder.Register<IShopService, ShopService>(Lifetime.Scoped);
        builder.Register<IKitchenStatService, KitchenStatService>(Lifetime.Scoped);
        builder.Register<CardEffectApplier>(Lifetime.Scoped);

        // Factories
        builder.Register<SpawnFactory>(Lifetime.Singleton);

        // UI Factories
        builder.Register<WorldUIFactory>(Lifetime.Singleton).WithParameter(_gaugeContainer);

        // Initiators
        builder.RegisterEntryPoint<GameInitiator>();
        builder.RegisterEntryPoint<PlayerInputHandler>();

        // HUD
        builder.RegisterComponentInHierarchy<HudView>();
        builder.RegisterEntryPoint<HudPresenter>();

        // Game UI
        builder.RegisterComponentInHierarchy<GameUIContext>();
        builder.RegisterEntryPoint<GameUIContextPresenter>();

        // Level
        builder.Register<LevelFactory>(Lifetime.Scoped).WithParameter(_root);
        builder.Register<LevelContextPresenter>(Lifetime.Scoped)
                .As<ILevelContextPresenter>()
                .WithParameter(_root);
    }
}
