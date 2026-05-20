using System.Collections.Generic;
using UnityEngine;

namespace OrderRush.Data
{
    [CreateAssetMenu(fileName = "CustomerCharacterDatas", menuName = "Order Rush/CustomerCharacterDatas")]
    public class CustomerCharacterDatas : ScriptableObject
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
