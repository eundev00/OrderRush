using System.Linq;
using OrderRush.Data;
using OrderRush.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] Transform _root;
    [SerializeField] RectTransform _gaugeContainer;
    [SerializeField] RunsData _runsData;

    protected override void Configure(IContainerBuilder builder)
    {
        // Data
        builder.RegisterInstance(_runsData);

        // Services
        builder.Register<ILevelProgressService, LevelProgressService>(Lifetime.Singleton);
        builder.Register<IRunProgressService, RunProgressService>(Lifetime.Scoped);
        builder.Register<CustomerService>(Lifetime.Singleton).AsImplementedInterfaces();

        // Factories
        builder.Register<SpawnFactory>(Lifetime.Singleton);

        // UI Factories
        builder.Register<KitchenGaugeFactory>(Lifetime.Singleton).WithParameter(_gaugeContainer);
        builder.Register<TableGaugeFactory>(Lifetime.Singleton).WithParameter(_gaugeContainer);
        builder.Register<OrderIconFactory>(Lifetime.Singleton).WithParameter(_gaugeContainer);
        builder.Register<CharacterEmoteIconFactory>(Lifetime.Singleton).WithParameter(_gaugeContainer);

        // Initiators
        builder.RegisterEntryPoint<GameInitiator>();
        builder.RegisterEntryPoint<PlayerInputHandler>();

        // HUD
        builder.RegisterComponentInHierarchy<HudView>();
        builder.RegisterEntryPoint<HudPresenter>();

        // Level
        builder.Register<LevelFactory>(Lifetime.Scoped).WithParameter(_root);
        builder.Register<LevelContextPresenter>(Lifetime.Scoped)
                .As<ILevelContextPresenter>()
                .WithParameter(_root);
    }
}
