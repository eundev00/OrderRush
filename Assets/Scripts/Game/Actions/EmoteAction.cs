using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EmoteAction : IGameAction
{
    private readonly CharacterBase _character;
    private readonly WorldUIFactory _worldUIFactory;
    private CharacterEmoteIcon _emoteIcon;

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
            _emoteIcon = _worldUIFactory.Create<CharacterEmoteIcon>(
                PrefabKeys.CharacterEmoteIcon,
                _character.transform,
                new Vector3(0, 1.5f, 0));
            await _emoteIcon.PlayPopupAnimation(ct);
        }
        finally
        {
            if (_emoteIcon != null)
            {
                _worldUIFactory.Release(PrefabKeys.CharacterEmoteIcon, _emoteIcon);
                _emoteIcon = null;
            }
        }
    }
}
