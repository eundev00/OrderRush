using UnityEngine;

namespace OrderRush.Data
{
    public enum CustomerCharacterType
    {
        Normal,
        Kind,
        Worker,
        Wild
    }

    [CreateAssetMenu(fileName = "CustomerTraitData", menuName = "Order Rush/Customer/Customer Trait Data")]
    public class CustomerTraitData : ScriptableObject
    {

        [Header("Patience")]
        [SerializeField] private float _patienceMultiplier = 1.0f;

        [Header("Behavior")]
        [SerializeField] private float _givesTip = 1.0f;
        [SerializeField] private bool _onlyDefaultRecipe = false;

        public float PatienceMultiplier => _patienceMultiplier;
        public float GivesTip => _givesTip;
        public bool OnlyDefaultRecipe => _onlyDefaultRecipe;
    }
}
