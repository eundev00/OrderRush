using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EmoteAction : IGameAction
{
    private const float DISPLAY_DURATION = 1f;

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
        _emoteIconPresenter = _emoteIconFactory.Create(
            _character.transform,
            new Vector3(0, 1.5f, 0),
            null
        );
        _emoteIconPresenter.Show();

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(DISPLAY_DURATION), cancellationToken: ct);
        }
        finally
        {
            if (_emoteIconPresenter != null)
            {
                _emoteIconFactory.Release(_emoteIconPresenter);
                _emoteIconPresenter = null;
            }
        }
    }
}
