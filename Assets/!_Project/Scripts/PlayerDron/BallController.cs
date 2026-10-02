using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// BallController — точка входа ввода игрока.
/// Вешается на тот же объект, что и PlayerInput (с Behavior = Invoke Unity Events).
/// Этот скрипт читает нажатия клавиш и передаёт команды в ObjectMovement и EnergySystem.
/// </summary>
public class BallController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  НАСТРОЙКИ В ИНСПЕКТОРЕ
    // ─────────────────────────────────────────────

    // Компонент движения объекта. Перетащите в инспекторе объект,
    // на котором висит ObjectMovement (обычно это тот же объект).
    [SerializeField] private ObjectMovement _objectMovement;

    // Компонент энергии. Перетащите в инспекторе объект с EnergySystem.
    [SerializeField] private EnergySystem _energySystem;

    // Скорость поворота в градусах в секунду.
    // Например, 180 — объект поворачивается на 180° за 1 секунду.
    [SerializeField] private float _turnSpeed = 180f;

    // ─────────────────────────────────────────────
    //  ВНУТРЕННИЕ ПЕРЕМЕННЫЕ
    // ─────────────────────────────────────────────

    // Текущее направление поворота: -1 (влево), 0 (нет поворота), +1 (вправо).
    private float _turnInput;

    // Флаг: зажата ли кнопка ускорения (W).
    private bool _isBoostHeld;

    private void Start()
    {
        // Подписываемся на событие окончания энергии
        if (_energySystem != null)
        {
            _energySystem.OnEnergyDepleted += HandleEnergyDepleted;
        }
    }

    // Кулдаун автоповорота — сколько кадров ждать, прежде чем применять поворот в FixedUpdate.
    private void FixedUpdate()
    {
        // Поворачиваем объект каждый физический кадр, если есть ввод.
        // Mathf.Clamp ограничивает значение в диапазоне [-1; 1].
        if (_turnInput != 0f)
        {
            // Поворот вокруг оси Y (вертикальной).
            // _turnInput: -1 = поворот влево, +1 = поворот вправо.
            // Time.fixedDeltaTime — время одного физического кадра.
            float angle = _turnInput * _turnSpeed * Time.fixedDeltaTime;

            // Создаём вращение и применяем к объекту.
            // Поворот происходит вокруг локальной оси Y — это «вверх» в top-down.
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            transform.rotation = rotation * transform.rotation;
        }
    }

    // ═════════════════════════════════════════════════════
    //  МЕТОДЫ ДЛЯ PLAYER INPUT (Invoke Unity Events)
    // ═════════════════════════════════════════════════════
    //  Эти методы привязываются в инспекторе компонента PlayerInput
    //  через Events → Invoke Unity Events.
    //  Для каждого Action нужно выбрать объект с BallController
    //  и указать соответствующий метод.
    // ═════════════════════════════════════════════════════

    /// <summary>
    /// Вызывается Action "Turn" (кнопки A/D или стик лево/право).
    /// В инспекторе PlayerInput → Events → Turn → выбираем этот метод.
    /// </summary>
    public void OnTurn(InputAction.CallbackContext context)
    {
        // context.phase: Started, Performed, Canceled
        // context.ReadValue<float>() возвращает -1 (влево), +1 (вправо) или 0.

        if (context.performed)
        {
            // Кнопка зажата — читаем значение поворота.
            _turnInput = context.ReadValue<float>();
        }
        else if (context.canceled)
        {
            // Кнопка отпущена — перестаём поворачивать.
            _turnInput = 0f;
        }
    }

    /// <summary>
    /// Вызывается Action "Boost" (кнопка W).
    /// </summary>
    public void OnBoost(InputAction.CallbackContext context)
    {
        // 1. Если системы энергии нет вообще — буст невозможен.
        if (_energySystem == null)
        {
            _objectMovement?.SetBoosting(false);
            return;
        }

        if (context.started)
        {
            // Игрок только что нажал W.
            _isBoostHeld = true;

            // ГЛАВНОЕ ИСПРАВЛЕНИЕ:
            // Проверяем, есть ли хоть капля энергии.
            // Твой EnergySystem.CanBoost() просто смотрит на _currentEnergy > 0.
            if (_energySystem.CanBoost())
            {
                // Энергии хватает -> включаем буст в физике и запускаем таймер расхода.
                _objectMovement.SetBoosting(true);
                _energySystem.StartDrain();
            }
            else
            {
                // Энергии нет -> буст НЕ включаем, даже если игрок жмет W.
                _objectMovement.SetBoosting(false);

                // Опционально: тут можно добавить звук "пусто" или мигание UI.
            }
        }
        else if (context.canceled)
        {
            // Игрок отпустил W.
            _isBoostHeld = false;

            // Отключаем буст в физике.
            _objectMovement?.SetBoosting(false);

            // Останавливаем расход. Восстановление начнется само внутри EnergySystem.
            _energySystem.StopDrain();
        }
    }


    /// <summary>
    /// Этот метод НЕ нужен для top-down автодвижения, но оставлен
    /// на случай, если вы захотите добавить ручное движение.
    /// Сейчас движение происходит автоматически в ObjectMovement.
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        // Заглушка — автодвижение работает без ввода.
        // Если захотите ручное управление направлением —
        // uncomment код ниже:
        //
        // if (context.performed)
        //     _moveDirection = context.ReadValue<Vector2>();
        // else if (context.canceled)
        //     _moveDirection = Vector2.zero;
    }

    /// <summary>
    /// Вызывается автоматически, когда в EnergySystem энергия падает до 0.
    /// </summary>
    private void HandleEnergyDepleted()
    {
        // Если энергия кончилась, принудительно выключаем буст в движении,
        // даже если игрок все еще держит кнопку W.
        _objectMovement?.SetBoosting(false);

        // Опционально: можно сбросить флаг _isBoostHeld, если хочешь,
        // чтобы при следующем нажатии W нужно было нажать заново,
        // но обычно лучше оставить его true, пока кнопка нажата,
        // а буст просто не сработает из-за проверки CanBoost().
        // _isBoostHeld = false; 
    }

    // И не забудь отписаться в OnDestroy, чтобы не было ошибок при удалении объекта:
    private void OnDestroy()
    {
        if (_energySystem != null)
        {
            _energySystem.OnEnergyDepleted -= HandleEnergyDepleted;
        }
    }
}
