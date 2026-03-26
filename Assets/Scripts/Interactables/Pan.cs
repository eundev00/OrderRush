using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;


public class Stove : CookingToolBase
{
    [Header("Cooking Step")]
    [SerializeField] CookingStep _defaultCookingStep; // Inspector에서 설정 (추후 RecipeData에서 가져오기)

    public override string DisplayName => "Pan";

    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        if (_currentIngredient == null)
        {
            Debug.Log($"[{DisplayName}] 재료가 없습니다.");
            return;
        }

        if (IsCooking)
        {
            Debug.Log($"[{DisplayName}] 이미 조리 중입니다.");
            return;
        }

        if (_defaultCookingStep == null)
        {
            Debug.LogError($"[{DisplayName}] CookingStep이 설정되지 않았습니다!");
            return;
        }

        StartCookingTimer(_defaultCookingStep);
        await UniTask.CompletedTask;
    }
}
