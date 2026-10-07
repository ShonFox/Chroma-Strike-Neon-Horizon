using UnityEngine;

/// <summary>
/// GameTimer — считает время, проведённое в игре.
/// Живёт между сценами (DontDestroyOnLoad) и обнуляется при старте новой игры.
/// В игровой сцене ничего не отображает — только копит время.
/// </summary>
public class GameTimer : MonoBehaviour
{
    // Единственный экземпляр — для доступа из любых скриптов.
    public static GameTimer Instance { get; private set; }

    // Время в игре в секундах (float — чтобы не терять точность).
    public float ElapsedTime { get; private set; }

    // Счёт идёт только во время игры: не тикает при паузе.
    private bool _isRunning;

    private void Awake()
    {
        // Защита от дублей: второй экземпляр уничтожаем.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Объект существует только в игровой сцене — стартуем сразу.
        // При рестарте Start не повторится (объект DontDestroyOnLoad),
        // поэтому в LoadGameLevel после сброса таймер запускаем вручную.
        StartTimer();
    }

    private void Update()
    {
        if (_isRunning)
        {
            ElapsedTime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Запускает отсчёт. Вызывай при старте игровой сцены.
    /// </summary>
    public void StartTimer()
    {
        _isRunning = true;
    }

    /// <summary>
    /// Останавливает отсчёт. Вызывай при смерти дрона.
    /// </summary>
    public void StopTimer()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Полный сброс: время в ноль, отсчёт остановлен.
    /// Вызывай при рестарте игры.
    /// </summary>
    public void ResetTimer()
    {
        ElapsedTime = 0f;
        _isRunning = false;
    }
}
