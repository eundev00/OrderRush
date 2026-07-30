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
        builder.Register<IDayProgressService, DayProgressService>(Lifetime.Singleton);
        builder.Register<IDayNightService, DayNightService>(Lifetime.Scoped)
            .WithParameter("outdoorLight", _outdoorLight)
            .WithParameter("indoorLight", _indoorLight);
        builder.Register<CustomerService>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<StaffManager>(Lifetime.Singleton);
        builder.Register<IShopService, ShopService>(Lifetime.Singleton);
        builder.Register<IKitchenStatService, KitchenStatService>(Lifetime.Scoped);
        builder.Register<CardEffectApplier>(Lifetime.Scoped);
        builder.Register<IStoryFlowService, StoryFlowService>(Lifetime.Scoped);
        builder.Register<IDayFlowService, DayFlowService>(Lifetime.Scoped);
        builder.Register<DayEventService>(Lifetime.Singleton).AsImplementedInterfaces();

        // Factories
        builder.Register<SpawnFactory>(Lifetime.Singleton);

        // UI Factories
        builder.Register<WorldUIFactory>(Lifetime.Singleton).WithParameter(_gaugeContainer);

        // TargetIndicator
        builder.RegisterComponentInHierarchy<TargetIndicator>()
               .AsImplementedInterfaces();

        // Camera Director
        builder.RegisterComponentInHierarchy<CameraDirector>()
               .AsImplementedInterfaces();

        // HUD
        builder.RegisterComponentInHierarchy<HudView>();
        builder.RegisterEntryPoint<HudPresenter>();

        // Game UI — 게임 씬 팝업을 게임 리졸버로 등록(팝업 Presenter 가 게임 서비스를 주입받도록)
        builder.RegisterComponentInHierarchy<GameUIContext>();
        builder.RegisterInstance(new ScenePopupKeys(
            PrefabKeys.PopupCompleted,
            PrefabKeys.PopupCardShop,
            PrefabKeys.PopupFailed,
            PrefabKeys.PopupStory
        ));
        builder.RegisterEntryPoint<ScenePopupRegistrar>();
        builder.Register<GameUIContextPresenter>(Lifetime.Scoped);

        // Initiators — ScenePopupRegistrar 이후에 등록 (팝업 등록 완료 후 게임 시작)
        builder.RegisterEntryPoint<GameInitiator>();
        builder.RegisterEntryPoint<PlayerInputHandler>();

        // Level
        builder.Register<LevelFactory>(Lifetime.Scoped).WithParameter(_root);
        builder.Register<LevelContextPresenter>(Lifetime.Scoped)
                .As<ILevelContextPresenter>()
                .WithParameter(_root);
    }
}
