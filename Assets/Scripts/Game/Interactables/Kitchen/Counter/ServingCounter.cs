using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServingCounter : MonoBehaviour
{
    [SerializeField] Counter[] _slots;

    public bool HasPlate => _slots.Any(s => s.HasItem && s.CurrentItem is Plate);

    public Counter FindSlotWithMatchingPlate(List<RecipeData> pendingRecipes)
    {
        foreach (var slot in _slots)
        {
            if (!slot.HasItem || slot.CurrentItem is not Plate plate) continue;

            var ingredientDatas = plate.PlacedIngredients.Select(i => i.Data).ToList();

            foreach (var recipe in pendingRecipes)
            {
                if (recipe.IsComplete(ingredientDatas))
                    return slot;
            }
        }

        return null;
    }
}
