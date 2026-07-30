using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomerService : IDisposable
{
    void Initialize();
    void OnTableAvailable(DiningTable table);
}
