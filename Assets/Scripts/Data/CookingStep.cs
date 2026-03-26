using System;

[Serializable]
public class CookingStep
{
    public string stepName;
    public string requiredTool;    // 예: "Knife", "Pan", "Oven"
    public float duration;          // 0이면 시간 제한 없음
    public IngredientState resultState;
}
