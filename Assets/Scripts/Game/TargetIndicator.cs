using System;
using DG.Tweening;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class TargetIndicator : MonoBehaviour, IInitializable
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");

    [NotNull][SerializeField] Transform _ring;
    [SerializeField] float _groundOffsetY = 0.02f;

    [Header("Expand")]
    [SerializeField] float _startScale = 0.3f;
    [SerializeField] float _endScale = 1.4f;
    [SerializeField] float _expandDuration = 0.45f;
    [SerializeField] Ease _expandEase = Ease.OutCubic;

    [Header("Fade")]
    [SerializeField] float _fadeDuration = 0.45f;
    [SerializeField] Ease _fadeEase = Ease.Linear;

    ISubscriber<MoveEvent> _moveSubscriber;
    IDisposable _subscription;
    Material _material;
    Color _color;
    Sequence _sequence;

    [Inject]
    public void Construct(ISubscriber<MoveEvent> moveSubscriber)
    {
        _moveSubscriber = moveSubscriber;
    }

    public void Initialize()
    {
        _material = _ring.GetComponent<Renderer>().material;
        _color = _material.HasProperty(BaseColorId)
            ? _material.GetColor(BaseColorId)
            : _material.GetColor(ColorId);

        _ring.gameObject.SetActive(false);
        _subscription = _moveSubscriber.Subscribe(e => Show(e.Destination));
    }

    void OnDestroy()
    {
        _sequence?.Kill();
        _subscription?.Dispose();
    }

    void Show(Vector3 position)
    {
        _sequence?.Kill();

        transform.position = position + Vector3.up * _groundOffsetY;
        _ring.localScale = Vector3.one * _startScale;
        SetAlpha(_color.a);
        _ring.gameObject.SetActive(true);

        _sequence = DOTween.Sequence();
        _sequence.Append(_ring.DOScale(_endScale, _expandDuration).SetEase(_expandEase));
        _sequence.Join(DOVirtual.Float(_color.a, 0f, _fadeDuration, SetAlpha).SetEase(_fadeEase));
        _sequence.OnComplete(() => _ring.gameObject.SetActive(false));
    }

    // URP(_BaseColor) / Built-in(_Color) 둘 다 대응
    void SetAlpha(float alpha)
    {
        var color = _color;
        color.a = alpha;

        if (_material.HasProperty(BaseColorId))
            _material.SetColor(BaseColorId, color);
        if (_material.HasProperty(ColorId))
            _material.SetColor(ColorId, color);
    }
}
