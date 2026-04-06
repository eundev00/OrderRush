using UnityEngine;

public interface ICarriable
{
    void OnPickedUp(Transform slot);
    void OnPutDown(Transform slot);
}
