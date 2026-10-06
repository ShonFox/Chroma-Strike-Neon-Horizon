using System;
using UnityEngine;

/// <summary>
/// ScoreManager — хранит очки игры и раздаёт события об их изменении.
/// Вешается на один пустой объект в корне сцены (например, "ScoreManager").
/// Singleton: любой скрипт может обратиться через ScoreManager.Instance.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    // Событие для UI: передаёт текущий счёт.
    public event Action<int> OnScoreChanged;

    private int _score;

    public int Score => _score;

    private void Awake()
    {
        // Классический singleton: второй экземпляр уничтожаем.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Объект переносится между сценами и не уничтожается при их смене.
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount)
    {
        _score += amount;
        OnScoreChanged?.Invoke(_score);
    }

    /// <summary>
    /// Сбрасывает счёт в ноль. Вызывать при старте новой игры.
    /// </summary>
    public void ResetScore()
    {
        _score = 0;

        // Сообщаем всем подписчикам (ScoreUI и другим), что счёт изменился,
        // чтобы надпись на экране сразу обновилась.
        OnScoreChanged?.Invoke(_score);
    }
}
