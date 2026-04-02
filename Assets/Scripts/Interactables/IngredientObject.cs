using UnityEngine;

public class IngredientObject : MonoBehaviour, ICarriable
{
    [SerializeField] Renderer _renderer;
    public IngredientData Data { get; private set; }
    public bool IsRuined { get; private set; }

    public void SetData(IngredientData ingredient)
    {
        Data = ingredient;
    }

    public void OnPickedUp(Transform slot)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnPutDown(Transform slot)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void SetRuined()
    {
        IsRuined = true;
        var mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", new Color(0.3f, 0.2f, 0.1f));
        _renderer.SetPropertyBlock(mpb);
    }
}
