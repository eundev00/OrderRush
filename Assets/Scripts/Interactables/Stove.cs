using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Stove : CookingToolBase
{
    public override string DisplayName => "Stove";

    public override async UniTask InteractAsync(CharacterBase character, CancellationToken ct)
    {
        Debug.Log($"[Stove] InteractAsync 호출됨 - IsHolding: {character.IsHolding}, IsOccupied: {IsOccupied}");

        // 캐릭터가 아무것도 안 들고 있고 재료가 있으면 집기
        if (!character.IsHolding && IsOccupied)
        {
            var ingredientObject = _ingredientSlot.GetComponentInChildren<IngredientObject>();
            RemoveIngredient();
            ingredientObject.OnPickedUp(character.ItemSlot);
            character.PickUp(ingredientObject);
            Debug.Log($"[Stove] 재료 집음: {ingredientObject.Data.IngredientName}");
            return;
        }

        // 캐릭터가 재료 들고 있으면 올리기
        if (character.IsHolding && !IsOccupied)
        {
            Debug.Log("[Stove] 재료 올리기 시도");
            var ingredientObject = character.PutDown() as IngredientObject;
            if (ingredientObject != null)
            {
                ingredientObject.transform.SetParent(_ingredientSlot);
                ingredientObject.transform.localPosition = Vector3.zero;
                PlaceIngredient(ingredientObject.Data, ingredientObject);
            }
        }

        Debug.Log($"[Stove] IsCooking: {IsCooking}, IsOccupied: {IsOccupied}");

        // 재료 없으면 무시
        if (!IsOccupied)
        {
            Debug.Log($"[{DisplayName}] 재료가 없습니다.");
            return;
        }

        // 이미 조리 중이면 무시
        if (IsCooking)
        {
            Debug.Log($"[{DisplayName}] 이미 조리 중입니다.");
            return;
        }

        // Cook 전환 가능 여부 체크
        var transition = CurrentIngredient.Transitions.Find(t => t.Type == TransitionType.Cook);
        if (transition == null)
        {
            Debug.Log($"[{DisplayName}] 조리할 수 없는 재료입니다.");
            return;
        }

        Debug.Log($"[Stove] 조리 시작: {transition.Duration}초");
        StartCookingTimer(transition);
        await UniTask.CompletedTask;
    }
}