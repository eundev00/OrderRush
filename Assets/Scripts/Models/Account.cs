using System.Collections.Generic;
using UniRx;

namespace OrderRush.Models
{
    public class Account
    {
        public ReactiveProperty<int> Coins { get; } = new(0);
        public List<int> OwnedRecipeIDs { get; set; } = new();
    }
}
