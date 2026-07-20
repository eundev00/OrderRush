using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopupView : PopupViewBase
{
    [NotNull][SerializeField] private TMP_Text _messageText;
    [NotNull][SerializeField] private Button _confirmButton;
    [NotNull][SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_Text _confirmButtonText;
    [SerializeField] private TMP_Text _cancelButtonText;

    public Button ConfirmButton => _confirmButton;
    public Button CancelButton => _cancelButton;

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }

    public void SetButtonLabels(string confirmLabel, string cancelLabel)
    {
        if (_confirmButtonText != null)
            _confirmButtonText.text = confirmLabel;

        if (_cancelButtonText != null)
            _cancelButtonText.text = cancelLabel;
    }
}
