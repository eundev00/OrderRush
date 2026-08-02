using UnityEngine;

public class IngredientObject : MonoBehaviour, ICarriable
{
    public IngredientData Data { get; private set; }

    public void SetData(IngredientData ingredient)
    {
        Data = ingredient;
    }

    public void OnPickedUp(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void OnPutDown(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void AttachToSlot(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public CarriableType GetCarriableType()
    {
        return CarriableType.Ingredient;
    }

}
