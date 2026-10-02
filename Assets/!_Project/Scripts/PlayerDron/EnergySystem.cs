using UnityEngine;

/// <summary>
/// Система энергии для буста.
///
/// Логика:
///   - Энергия тратится ТОЛЬКО когда нажата W (StartConsuming / StopConsuming).
///   - Когда W отпущена — энергия восстанавливается автоматически.
///   - Энергия не может упасть ниже 0 или подняться выше максимума.
///   - Когда энергия кончилась — буст отключается принудительно (HasEnergy → false).
///
/// В инспекторе настраиваются: максимум, скорость расхода, скорость восстановления.
/// Для UI есть событие OnEnergyChanged, на которое может подписаться EnergyUI.
/// </summary>
public class EnergySystem : MonoBehaviour
{
    // ─── Параметры энергии ─────────────────────────────────────────

    [Header("Параметры энергии")]

    // Максимальный запас энергии.
    [SerializeField, Range(10f, 500f)] private float _maxEnergy = 100f;

    // Сколько энергии тратится в секунду при активном бусте.
    [SerializeField, Range(1f, 100f)] private float _consumeRate = 30f;

    // Сколько энергии восстанавливается в секунду, когда буст не активен.
    [SerializeField, Range(1f, 100f)] private float _regenRate = 15f;

    // Задержка перед началом восстановления (в секундах) после окончания буста.
    // Например, 1.0 = энергия начнёт восстанавливаться через 1 сек после отпускания W.
    [SerializeField, Range(0f, 5f)] private float _regenDelay = 0.5f;

    // ─── Событие для UI ────────────────────────────────────────────

    // Вызывается каждый раз, когда значение энергии изменилось.
    // Параметр: текущее значение энергии (float от 0 до _maxEnergy).
    // EnergyUI подписывается на это событие, чтобы обновлять полоску на экране.
    public event System.Action<float, float> OnEnergyChanged; // (current, max)

    // ─── Внутренние переменные ─────────────────────────────────────

    // Текущий запас энергии.
    private float _currentEnergy;

    // Тратится ли энергия прямо сейчас.
    private bool _isConsuming;

    // Таймер задержки восстановления.
    private float _regenTimer;

    private void Start()
    {
        // Начинаем с полным запасом энергии.
        _currentEnergy = _maxEnergy;

        // Сразу уведомляем UI, чтобы полоска показала 100% при старте.
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }

    private void Update()
    {
        // ─── РАСХОД ЭНЕРГИИ ────────────────────────────────────────

        if (_isConsuming)
        {
            // Буст активен — уменьшаем энергию.
            _currentEnergy -= _consumeRate * Time.deltaTime;

            // Не даём уйти ниже нуля.
            if (_currentEnergy <= 0f)
            {
                _currentEnergy = 0f;
                // Энергия кончилась — буст нужно выключить.
                // Уведомляем контроллер через флаг HasEnergy.
                _isConsuming = false;
            }

            // Сбрасываем таймер задержки восстановления.
            _regenTimer = _regenDelay;
        }
        // ─── ВОССТАНОВЛЕНИЕ ЭНЕРГИИ ────────────────────────────────
        else
        {
            // Буст не активен — ждём задержку, потом восстанавливаем.
            if (_regenTimer > 0f)
            {
                // Ещё ждём — уменьшаем таймер.
                _regenTimer -= Time.deltaTime;
            }
            else
            {
                // Задержка прошла — восстанавливаем энергию.
                _currentEnergy += _regenRate * Time.deltaTime;

                // Не даём превысить максимум.
                if (_currentEnergy > _maxEnergy)
                    _currentEnergy = _maxEnergy;
            }
        }

        // Уведомляем подписчиков (EnergyUI) об изменении.
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }

    // ─── Публичные методы ──────────────────────────────────────────

    /// <summary>Начать расход энергии (вызывается из BallController при нажатии W).</summary>
    public void StartConsuming()
    {
        // Включаем расход только если энергия ещё есть.
        if (_currentEnergy > 0f)
            _isConsuming = true;
    }

    /// <summary>Остановить расход энергии (вызывается из BallController при отпускании W).</summary>
    public void StopConsuming()
    {
        _isConsuming = false;
    }

    /// <summary>Есть ли ещё энергия для буста? (BallController проверяет это перед стартом).</summary>
    public bool HasEnergy => _currentEnergy > 0f;

    /// <summary>Текущее значение энергии (0..MaxEnergy). Используется для UI.</summary>
    public float CurrentEnergy => _currentEnergy;

    /// <summary>Максимальное значение энергии.</summary>
    public float MaxEnergy => _maxEnergy;

    /// <summary>Доля энергии в виде 0..1 (удобно для Slider).</summary>
    public float EnergyNormalized => _currentEnergy / _maxEnergy;
}
