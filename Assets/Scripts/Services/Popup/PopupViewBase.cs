using UnityEngine;

// =====================================================================
//  팝업 View 공통 베이스 — 모든 팝업 View 가 상속.
// ---------------------------------------------------------------------
//  - 순수 표시 계층. 게임/서비스 로직은 전혀 모른다(Presenter 담당).
//  - Show()/Hide() 는 기본 SetActive. 페이드/스케일 등 연출이 필요하면
//    파생 View 에서 오버라이드한다(기본은 즉시 표시).
//  - PopupService 가 프리팹을 로드→Instantiate 한 뒤 이 컴포넌트를 찾아
//    캔버스 아래에 배치하고 Presenter 와 짝지어 다룬다.
// =====================================================================
public abstract class PopupViewBase : MonoBehaviour
{
    public RectTransform RectTransform => (RectTransform)transform;

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
