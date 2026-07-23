using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EmoteAction : IGameAction
{
    private readonly CustomerCharacter _character;

    public EmoteAction(CustomerCharacter character)
    {
        _character = character;
    }

    public async UniTask ExecuteAsync(CancellationToken ct)
    {
        if (_character == null || _character.gameObject == null)
        {
            return;
        }

        try
        {
            _character.EmoteIcon.Show();
            await _character.EmoteIcon.PlayPopupAnimation(ct);
        }
        finally
        {
            _character.EmoteIcon.Hide();
        }
    }
}
