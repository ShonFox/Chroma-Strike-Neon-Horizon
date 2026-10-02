using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Главный контроллер игрока для top-down игры.
/// Обрабатывает ввод с клавиатуры и передаёт команды в ObjectMovement и EnergySystem.
///
/// Управление:
///   A / D или стрелки влево/вправо — поворот объекта вокруг своей оси.
///   W или стрелка вверх            — кратковременное ускорение (буст), тратит энергию.
///
/// Важно: объект всегда движется вперёд автоматически (см. ObjectMovement).
/// Игрок только рулит направлением и может ускоряться.
/// </summary>
[RequireComponent(typeof(ObjectMovement))]
public class BallController : MonoBehaviour
{
    // ─── Ссылки на компоненты ──────────────────────────────────────

    [Header("Ссылки на компоненты")]

    // Компонент, который управляет физическим движением и поворотом объекта.
    // SerializeField = виден в инспекторе Unity, но закрыт для других скриптов.
    [SerializeField] private ObjectMovement _objectMovement;

    // Система энергии — нужна, чтобы проверять, хватит ли энергии для буста.
    [SerializeField] private EnergySystem _energySystem;

    // ─── Настройки ввода ────────────────────────────────────────────

    [Header("Настройки ввода")]

    // Скорость поворота в градусах в секунду. Чем больше — тем резче объект вращается.
    [SerializeField, Range(30f, 360f)] private float _turnSpeed = 120f;

    // ─── Внутренние переменные ──────────────────────────────────────

    // Текущий угол поворота объекта по оси Y (в градусах).
    // Храним отдельно, чтобы плавно крутить через Mathf.MoveTowardsAngle.
    private float _currentYRotation;

    // Направление поворота: -1 (влево), 0 (нет ввода), +1 (вправо).
    private float _turnInput;

    // Зажата ли кнопка ускорения (W).
    private bool _isBoosting;

    private void Start()
    {
        // Запоминаем начальный угол поворота объекта.
        // transform.eulerAngles.y — текущий поворот по оси Y в градусах.
        _currentYRotation = transform.eulerAngles.y;

        // Если ссылка не назначена в инспекторе — пытаемся найти автоматически.
        if (_objectMovement == null)
            _objectMovement = GetComponent<ObjectMovement>();

        // EnergySystem можно не назначать, если буст не нужен.
        // Но если он есть в сцене — найдём его на этом же объекте.
        if (_energySystem == null)
            _energySystem = GetComponent<EnergySystem>();
    }

    private void FixedUpdate()
    {
        // ─── ПОВОРОТ ───────────────────────────────────────────────

        // Если игрок нажимает A/D — _turnInput будет -1 / +1.
        // Умножаем на скорость поворота и на время кадра, чтобы поворот
        // не зависел от FPS (fixedDeltaTime — для физического цикла).
        if (_turnInput != 0f)
        {
            // Смещаем целевой угол: _turnInput > 0 — поворот вправо,
            // _turnInput < 0 — поворот влево.
            _currentYRotation += _turnInput * _turnSpeed * Time.fixedDeltaTime;

            // Нормализуем угол в диапазон 0..360 (не обязательно, но аккуратнее).
            _currentYRotation = Mathf.Repeat(_currentYRotation, 360f);

            // Применяем поворот к трансформу объекта.
            // Поворачиваем только по оси Y — это корректно для top-down вида.
            transform.rotation = Quaternion.Euler(0f, _currentYRotation, 0f);
        }

        // ─── ДВИЖЕНИЕ ──────────────────────────────────────────────

        // Передаём состояние буста в ObjectMovement.
        // Тот сам решит, ускоряться или ехать с обычной скоростью.
        _objectMovement.SetBoosting(_isBoosting);
    }

    // ═══════════════════════════════════════════════════════════════
    //  ОБРАБОТЧИКИ ВВОДА (Input System Events)
    //  Эти методы привязываются через Input Action в инспекторе
    //  или через компонент PlayerInput → Events.
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Обработчик поворота. Вызывается Input System при нажатии A/D или стрелок.
    /// В context.ReadValue<float>() приходит: -1 (влево), 0, +1 (вправо).
    /// </summary>
    public void OnTurn(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Кнопка нажата/удерживается — сохраняем направление поворота.
            _turnInput = context.ReadValue<float>();
        }
        else if (context.canceled)
        {
            // Кнопка отпущена — перестаём поворачивать.
            _turnInput = 0f;
        }
    }

    /// <summary>
    /// Обработчик ускорения. Вызывается Input System при нажатии W.
    /// В context.ReadValue<float>() приходит 1.0 при нажатии.
    /// </summary>
    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Кнопка W нажата — включаем буст, но только если есть энергия.
            // Проверку энергии делаем здесь, чтобы не стартовать буст впустую.
            if (_energySystem != null && _energySystem.HasEnergy)
            {
                _isBoosting = true;
                // Сообщаем системе энергии, что начали тратить.
                _energySystem.StartConsuming();
            }
            else if (_energySystem == null)
            {
                // Если системы энергии нет в сцене — разрешаем буст без ограничений.
                _isBoosting = true;
            }
        }
        else if (context.canceled)
        {
            // Кнопка W отпущена — выключаем буст и запускаем восстановление энергии.
            _isBoosting = false;
            if (_energySystem != null)
                _energySystem.StopConsuming();
        }
    }

    // ─── Геттеры для внешних скриптов (необязательно) ──────────────

    /// <summary>Возвращает true, если сейчас активен буст.</summary>
    public bool IsBoosting => _isBoosting;
}
