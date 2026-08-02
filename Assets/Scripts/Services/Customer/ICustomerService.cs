using System;

public interface ICustomerService : IDisposable
{
    void Initialize();
    void OnTableAvailable(DiningTable table);
}
