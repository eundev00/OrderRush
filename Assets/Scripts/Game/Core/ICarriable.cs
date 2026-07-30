using UnityEngine;

public interface ICarriable
{
    void AttachToSlot(Transform slot);

    CarriableType GetCarriableType();
}


public enum CarriableType
{
    None,
    Ingredient,
    Plate
}

public static class CarriableExtensions
{
    public static void DestroyObject(this ICarriable carriable)
    {
        if (carriable is Component component && component != null)
            Object.Destroy(component.gameObject);
    }
}
