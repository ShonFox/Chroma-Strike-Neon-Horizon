using UnityEngine;
using TMPro;

/// <summary>
/// ScoreUI — показывает счёт на экране.
/// Вешается на Text (TextMeshPro) на Canvas.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(int score)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (ScoreManager.Instance != null)
        {
            _scoreText.text = $"Очки: {ScoreManager.Instance.Score}";
        }
    }
}
