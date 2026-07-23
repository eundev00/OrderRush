using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class OrderBubble : MonoBehaviour
{
    [NotNull][SerializeField] private Image _recipeIcon;

    private void Awake()
    {
        Hide();
    }

    public void Show(RecipeData recipe)
    {
        if (_recipeIcon != null && recipe != null)
        {
            _recipeIcon.sprite = recipe.Icon;
        }

        gameObject.SetActive(true);

        transform.localScale = Vector3.zero;
        transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(1f, 0.1f));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
