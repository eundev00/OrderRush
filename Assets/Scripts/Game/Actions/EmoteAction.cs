using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EmoteAction : IGameAction
{
    private readonly CharacterBase _character;
    private readonly WorldUIFactory _worldUIFactory;
    private GameObject _emoteObj;

    public EmoteAction(
        CharacterBase character,
        WorldUIFactory worldUIFactory)
    {
        _character = character;
        _worldUIFactory = worldUIFactory;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
        {
            return;
        }

        try
        {
            _emoteObj = _worldUIFactory.Create(PrefabKeys.CharacterEmoteIcon);
            var emoteIcon = _emoteObj.GetComponent<CharacterEmoteIcon>();
            await emoteIcon.PlayPopupAnimation(ct);
        }
        finally
        {
            if (_emoteObj != null)
            {
                _worldUIFactory.Release(PrefabKeys.CharacterEmoteIcon, _emoteObj);
                _emoteObj = null;
            }
        }
    }
}
