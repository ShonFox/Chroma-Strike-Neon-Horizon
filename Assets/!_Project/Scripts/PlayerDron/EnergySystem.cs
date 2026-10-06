using UnityEngine;
using System;

/// <summary>
/// EnergySystem — управление запасом энергии объекта.
/// Энергия тратится только при нажатой W (буст).
/// Восстанавливается автоматически после небольшой задержки.
/// </summary>
public class EnergySystem : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  СОБЫТИЕ ДЛЯ UI
    // ─────────────────────────────────────────────

    // Вызывается каждый раз, когда энергия меняется.
    // EnergyUI подписывается на него и обновляет полоску.
    // float — текущее значение энергии (0.._maxEnergy).
    public event Action<float, float> OnEnergyChanged; // (current, max)
    public event Action OnEnergyDepleted;

    // ─────────────────────────────────────────────
    //  НАСТРОЙКИ В ИНСПЕКТОРЕ
    // ─────────────────────────────────────────────

    // Максимальный запас энергии.
    [SerializeField, Range(80f, 200f)] private float _maxEnergy = 100f;

    // Сколько энергии тратится в секунду при бусте.
    [SerializeField, Range(20f, 50f)] private float _drainRate = 30f;

    // Сколько энергии восстанавливается в секунду.
    [SerializeField, Range(10f, 20f)] private float _regenRate = 30f;

    // Задержка перед началом восстановления (в секундах).
    // После отпускания W энергия не сразу начнёт расти.
    [SerializeField, Range(0f, 8f)] private float _regenDelay = 1f;

    // ─────────────────────────────────────────────
    //  ВНУТРЕННИЕ ПЕРЕМЕННЫЕ
    // ─────────────────────────────────────────────

    private float _currentEnergy;   // Текущий запас энергии.
    private bool _isDraining;       // Тратим ли энергию прямо сейчас.
    private float _regenTimer;      // Таймер до начала восстановления.

    private void Start()
    {
        // В начале игры энергия полностью заряжена.
        _currentEnergy = _maxEnergy;

        // Сразу оповещаем UI, чтобы полоска показала полный запас.
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }

    private void Update()
    {

        // ── РАСХОД ЭНЕРГИИ (когда зажата W) ──
        if (_isDraining)
        {
            // Уменьшаем энергию на drainRate * время кадра.
            _currentEnergy -= _drainRate * Time.deltaTime;

            // Если энергия кончилась — не уходим в минус.
            if (_currentEnergy <= 0f)
            {
                _currentEnergy = 0f;
                _isDraining = false;

                // Сообщаем внешнему миру, что энергия кончилась!
                OnEnergyDepleted?.Invoke();
            }
        }
        // ── ВОССТАНОВЛЕНИЕ ЭНЕРГИИ ──
        else if (_currentEnergy < _maxEnergy)
        {
            // Ждём задержку перед восстановлением.
            _regenTimer += Time.deltaTime;

            if (_regenTimer >= _regenDelay)
            {
                // Восстанавливаем энергию.
                _currentEnergy += _regenRate * Time.deltaTime;

                // Не превышаем максимум.
                if (_currentEnergy > _maxEnergy)
                    _currentEnergy = _maxEnergy;
            }
        }

        // Оповещаем UI о текущем значении.
        OnEnergyChanged?.Invoke(_currentEnergy, _maxEnergy);
    }

    /// <summary>
    /// Начать тратить энергию. Вызывается из BallController.OnBoost()
    /// когда игрок нажимает W.
    /// </summary>
    public void StartDrain()
    {
        // Если энергии нет — не даём тратить.
        if (_currentEnergy <= 0f) return;

        _isDraining = true;

        // Сбрасываем таймер восстановления.
        _regenTimer = 0f;
    }

    /// <summary>
    /// Проверяет, можно ли сейчас использовать буст (достаточно ли энергии).
    /// </summary>
    public bool CanBoost()
    {
        return _currentEnergy > 0;
    }

    /// <summary>
    /// Прекратить тратить энергию. Вызывается из BallController.OnBoost()
    /// когда игрок отпускает W.
    /// </summary>
    public void StopDrain()
    {
        _isDraining = false;

        // Запускаем таймер задержки восстановления.
        _regenTimer = 0f;
    }

    /// <summary>
    /// Текущий процент энергии (0..1). Удобно для UI.
    /// </summary>
    public float GetEnergyNormalized()
    {
        return _currentEnergy / _maxEnergy;
    }

    /// <summary>
    /// Осталась ли энергия. Полезно для проверки в BallController.
    /// </summary>
    public bool HasEnergy()
    {
        return _currentEnergy > 0f;
    }
}
