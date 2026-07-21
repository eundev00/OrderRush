using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public interface IPopupService
{
    UniTask Initialize();
    void SetCanvasRoot(RectTransform canvasRoot);

    void RegisterPopup(string popupKey, IObjectResolver parent);
    void UnregisterPopup(string popupKey);
    void CloseOwnedBy(IObjectResolver parent);

    UniTask<TResult> Open<TPresenter, TArgs, TResult>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs, TResult>;

    UniTask Open<TPresenter, TArgs>(string popupKey, TArgs args)
        where TPresenter : PopupPresenterBase<TArgs>;

    UniTask<TResult> Open<TPresenter, TResult>(string popupKey)
        where TPresenter : PopupPresenterBaseNoArgs<TResult>;

    UniTask Open<TPresenter>(string popupKey)
        where TPresenter : PopupPresenterBase;

    void CloseTop();
    void CloseAll();

    bool HasOpenPopup { get; }
}
