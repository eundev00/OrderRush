using UnityEngine;

public class GameUIContext : MonoBehaviour
{
    [NotNull][SerializeField] PopupCompleted _popupCompleted;
    [NotNull][SerializeField] PopupDayFailed _popupDayFailed;


    public PopupCompleted PopupCompleted => _popupCompleted;
    public PopupDayFailed PopupDayFailed => _popupDayFailed;

}
