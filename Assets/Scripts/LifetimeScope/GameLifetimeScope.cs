using OrderRush.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [NotNull][SerializeField] Transform _root;
    [NotNull][SerializeField] RectTransform _gaugeContainer;
    [NotNull][SerializeField] Light _outdoorLight;
    [NotNull][SerializeField] Light _indoorLight;

    protected override void Configure(IContainerBuilder builder)
    {

        // Services
        builder.Register<IDayProgressService, DayProgressService>(Lifetime.Scoped);
        builder.Register<IDayNightService, DayNightService>(Lifetime.Scoped)
            .WithParameter("outdoorLight", _outdoorLight)
            .WithParameter("indoorLight", _indoorLight);
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

        // Game UI — 게임 씬 팝업을 게임 리졸버로 등록(팝업 Presenter 가 게임 서비스를 주입받도록)
        builder.RegisterComponentInHierarchy<GameUIContext>();
        builder.RegisterInstance(new ScenePopupKeys(
            PrefabKeys.PopupCompleted,
            PrefabKeys.PopupCardShop,
            PrefabKeys.PopupFailed
        ));
        builder.RegisterEntryPoint<ScenePopupRegistrar>();
        builder.RegisterEntryPoint<GameUIContextPresenter>();

        // Level
        builder.Register<LevelFactory>(Lifetime.Scoped).WithParameter(_root);
        builder.Register<LevelContextPresenter>(Lifetime.Scoped)
                .As<ILevelContextPresenter>()
                .WithParameter(_root);
    }
}
