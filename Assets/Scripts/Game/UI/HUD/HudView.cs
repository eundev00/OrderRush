using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudView : MonoBehaviour
{
    [NotNull][SerializeField] private TMP_Text _coinText;
    [NotNull][SerializeField] private TMP_Text _dayText;
    [NotNull][SerializeField] private TMP_Text _maxText;
    [NotNull][SerializeField] private Image _timerFill;

    public void SetCoin(int value)
    {
        _coinText.text = value.ToString();
    }

    public void SetDay(int dayNumber)
    {
        _dayText.text = $"Day {dayNumber}";
    }

    public void SetMaxCustomers(int maxCustomers)
    {
        _maxText.text = $"Max {maxCustomers}";
    }

    public void SetTimeGauge(float ratio)
    {
        _timerFill.fillAmount = ratio;
    }

}
