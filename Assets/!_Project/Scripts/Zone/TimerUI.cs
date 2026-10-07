using UnityEngine;
using TMPro;

/// <summary>
/// TimerUI — показывает время игры на Canvas.
/// Формат: минуты и секунды, например "Время: 2:35".
/// </summary>
public class TimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _timeText;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (GameTimer.Instance == null)
        {
            _timeText.text = "Время: —";
            return;
        }

        int totalSeconds = Mathf.FloorToInt(GameTimer.Instance.ElapsedTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        _timeText.text = $"Время: {minutes}:{seconds:D2}";
    }
}
