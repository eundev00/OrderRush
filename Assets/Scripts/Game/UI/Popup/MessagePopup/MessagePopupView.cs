using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 메시지 팝업 View — 텍스트 + 확인 버튼만.
public class MessagePopupView : PopupViewBase
{
    [NotNull][SerializeField] private TMP_Text _messageText;
    [NotNull][SerializeField] private Button _confirmButton;

    public Button ConfirmButton => _confirmButton;

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }
}
