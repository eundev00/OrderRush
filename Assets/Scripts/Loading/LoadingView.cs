using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LoadingView : MonoBehaviour
{
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] float _fadeDuration = 0.3f;

    private void Awake()
    {
        _canvasGroup.alpha = 0f;
    }

    public async UniTask FadeIn()
    {
        await _canvasGroup.DOFade(1f, _fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }

    public async UniTask FadeOut()
    {
        await _canvasGroup.DOFade(0f, _fadeDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
}
