using System.Collections.Generic;
using UnityEngine;

namespace OrderRush.Data
{
    [CreateAssetMenu(fileName = "CustomerCharactersData", menuName = "Order Rush/Customer Characters Data")]
    public class CustomerCharactersData : ScriptableObject
    {
        [SerializeField] private List<CustomerCharacterData> _characters = new();

        public List<CustomerCharacterData> Characters => _characters;

        public CustomerCharacterData GetCharacter(CustomerCharacterType characterType)
        {
            return _characters.Find(character => character.CharacterType == characterType);
        }

        public int GetTotalCharacterCount() => _characters.Count;
    }
}
