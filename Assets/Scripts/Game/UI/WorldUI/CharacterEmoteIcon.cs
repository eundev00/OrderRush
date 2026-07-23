using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEmoteIcon : MonoBehaviour
{
    [NotNull][SerializeField] private Image _iconImage;

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public async UniTask PlayPopupAnimation(CancellationToken ct)
    {
        transform.localScale = Vector3.zero;

        var tween1 = transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
        ct.Register(() => tween1.Kill());
        await tween1.AsyncWaitForCompletion();

        var tween2 = transform.DOScale(1f, 0.1f);
        ct.Register(() => tween2.Kill());
        await tween2.AsyncWaitForCompletion();

        await UniTask.Delay(TimeSpan.FromSeconds(Constants.kEmoteIconSeconds), cancellationToken: ct);
    }
}
