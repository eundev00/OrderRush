using UnityEngine;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using Services.UpdateService;
using OrderRush.Services;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] UpdateSubscriptionService _updateService;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterMessagePipe();
        builder.RegisterComponent(_updateService)
               .As<IUpdateSubscriptionService>();
        builder.Register<ResourcesLoaderService>(Lifetime.Singleton)
               .As<IResourcesLoaderService>();
        builder.Register<LocalStorageService>(Lifetime.Singleton)
               .As<ILocalStorageService>();
        builder.Register<GameDataService>(Lifetime.Singleton)
               .As<IGameDataService>();
        builder.Register<AccountService>(Lifetime.Singleton)
               .As<IAccountService>();
        builder.RegisterEntryPoint<AppBootstrap>();
    }

}