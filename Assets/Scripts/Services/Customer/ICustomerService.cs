using System.Collections.Generic;
using UnityEngine;

public interface ICustomerService
{
    void Initialize();
    void OnTableAvailable(DiningTable table);
}
