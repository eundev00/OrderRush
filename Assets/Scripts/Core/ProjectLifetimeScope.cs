using UnityEngine;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using Services.UpdateService;

public class ProjectLifetimeScope : LifetimeScope
{
    [SerializeField] UpdateSubscriptionService _updateService;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterMessagePipe();

        // UpdateSubscriptionService 등록
        builder.RegisterComponent(_updateService)
            .AsImplementedInterfaces()
            .AsSelf();
    }
}