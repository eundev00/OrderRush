using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EmoteAction : IGameAction
{
    private readonly CharacterBase _character;
    private readonly CharacterEmoteIconFactory _emoteIconFactory;
    private CharacterEmoteIconPresenter _emoteIconPresenter;

    public EmoteAction(
        CharacterBase character,
        CharacterEmoteIconFactory emoteIconFactory)
    {
        _character = character;
        _emoteIconFactory = emoteIconFactory;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
        {
            return;
        }

        try
        {
            _emoteIconPresenter = _emoteIconFactory.Create(_character.transform, new Vector3(0, 1.5f, 0));
            _emoteIconPresenter.Show();
            await _emoteIconPresenter.PlayPopupAnimation(ct);
        }
        finally
        {
            if (_emoteIconPresenter != null)
            {
                _emoteIconPresenter.Hide();
                _emoteIconFactory.Release(_emoteIconPresenter);
                _emoteIconPresenter = null;
            }
        }
    }
}
