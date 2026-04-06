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
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void OnPutDown(Transform slot)
    {
        transform.SetParent(slot);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void SetRuined()
    {
        IsRuined = true;

        // 탄 음식 색상 (거의 검은색)
        Color burnedColor = new Color(0.15f, 0.1f, 0.05f);
        var mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", burnedColor);      // Built-in RP
        mpb.SetColor("_BaseColor", burnedColor);  // URP
        _renderer.SetPropertyBlock(mpb);
    }
}
