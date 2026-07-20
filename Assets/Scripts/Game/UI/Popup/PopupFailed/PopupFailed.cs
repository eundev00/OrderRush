using UnityEngine;
using UnityEngine.UI;

public class PopupDayFailed : PopupViewBase
{
    [NotNull][SerializeField] private Button _restartButton;
    [NotNull][SerializeField] private Button _exitButton;

    public Button RestartButton => _restartButton;
    public Button ExitButton => _exitButton;
}
