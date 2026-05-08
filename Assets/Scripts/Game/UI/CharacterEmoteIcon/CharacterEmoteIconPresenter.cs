using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CharacterEmoteIconPresenter : BaseUIPresenter<CharacterEmoteIconView>
{
    private const float DISPLAY_DURATION = 1.5f;

    public CharacterEmoteIconPresenter(Camera mainCamera, RectTransform canvasRectTransform, CharacterEmoteIconView view, Transform target, Vector3 offset)
        : base(mainCamera, canvasRectTransform, view, target, offset)
    {

    }

    public void SetIcon(Sprite sprite)
    {
        if (_isDisposed || View == null) return;
        View.SetIcon(sprite);
    }

    public async UniTask PlayPopupAnimation(CancellationToken ct)
    {
        if (_isDisposed || View == null) return;

        var transform = View.transform;

        transform.localScale = Vector3.zero;

        var tween1 = transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
        ct.Register(() => tween1.Kill());
        await tween1.AsyncWaitForCompletion();

        var tween2 = transform.DOScale(1f, 0.1f);
        ct.Register(() => tween2.Kill());
        await tween2.AsyncWaitForCompletion();

        await UniTask.Delay(TimeSpan.FromSeconds(DISPLAY_DURATION), cancellationToken: ct);
    }
}
