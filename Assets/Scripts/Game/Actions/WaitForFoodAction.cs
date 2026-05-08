using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WaitForFoodAction : IGameAction
{
    private OrderIconFactory _orderIconFactory;
    private CustomerCharacter _character;
    private OrderIconPresenter _orderIconPresenter;
    public WaitForFoodAction(CustomerCharacter character, OrderIconFactory orderIconFactory)
    {
        _character = character;
        _orderIconFactory = orderIconFactory;
    }
    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        Debug.Log($"[WaitForFoodAction] START - Character: {_character.name}");

        if (_character.Order == null)
        {
            Debug.LogError($"[WaitForFoodAction] Order is null! Character: {_character.name}");
            return;
        }

        if (_character.Order.Recipe == null)
        {
            Debug.LogError($"[WaitForFoodAction] Recipe is null! Character: {_character.name}");
            return;
        }

        try
        {
            _orderIconPresenter = _orderIconFactory.Create(_character.transform, new Vector3(0, 1.5f, 0));
            _orderIconPresenter.SetIcon(_character.Order.Recipe.Icon);
            _orderIconPresenter.Show();

            await UniTask.WaitUntilCanceled(ct);
        }
        finally
        {
            if (_orderIconPresenter != null)
            {
                _orderIconFactory.Release(_orderIconPresenter);
                _orderIconPresenter = null;
            }
        }
    }

}
