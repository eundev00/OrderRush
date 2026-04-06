using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class StageLifetimeScope : LifetimeScope
{
    [SerializeField] Transform _injectObjectRoot;

    protected override void Configure(IContainerBuilder builder)
    {
        // Services
        builder.Register<IOrderService, OrderService>(Lifetime.Singleton);
        builder.Register<ILevelsDataService, LevelsDataService>(Lifetime.Singleton);

        // Factories
        builder.Register<GameObjectFactory>(Lifetime.Singleton);

        // Initiators
        builder.RegisterEntryPoint<GameInitiator>();
        builder.RegisterEntryPoint<PlayerInputHandler>();

        // Grid
        builder.RegisterComponentInHierarchy<GridSystem>();

        // Inject IInjectable components in hierarchy
        builder.RegisterBuildCallback(container =>
        {
            foreach (var obj in _injectObjectRoot.GetComponentsInChildren<MonoBehaviour>()
                                          .OfType<IInjectable>())
            {
                container.Inject(obj as MonoBehaviour);
            }
        });
    }
}
