using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Order Rush/Ingredient", order = 50)]
public class IngredientData : ScriptableObject
{
    public string IngredientName;
    public Sprite Icon;
    public string PrefabName;

    [Header("Transitions")]
    public List<IngredientTransition> Transitions = new();

    public IngredientData GetResult(TransitionType transitionType)
    {
        foreach (var transition in Transitions)
        {
            if (transition.Type == transitionType)
                return transition.Result;
        }
        return null;
    }
}
