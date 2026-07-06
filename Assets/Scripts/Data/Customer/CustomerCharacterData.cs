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

    [CreateAssetMenu(fileName = "CustomerCharacterData", menuName = "Order Rush/Customer/CustomerCharacterData")]
    public class CustomerCharacterData : ScriptableObject
    {
        [Header("Character Info")]
        [SerializeField] private CustomerCharacterType _characterType;

        [Header("Patience")]
        [SerializeField] private float _patienceMultiplier = 1.0f;

        [Header("Behavior")]
        [SerializeField] private bool _givesTip;
        [NotNull][SerializeField] private RecipeData _preferredRecipe;

        public CustomerCharacterType CharacterType => _characterType;
        public float PatienceMultiplier => _patienceMultiplier;
        public bool GivesTip => _givesTip;
        public RecipeData PreferredRecipe => _preferredRecipe;
    }
}
