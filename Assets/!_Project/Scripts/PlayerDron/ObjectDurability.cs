using UnityEngine;
using UnityEngine.Events;

public class ObjectDurability : MonoBehaviour
{
    // Максимально возможное значение прочности.
    [SerializeField] private float _maxState = 100f;

    // Событие, вызываемое при любом изменении прочности.
    // Передаёт два параметра: максимальную и текущую прочность (для обновления UI).
    [SerializeField] private UnityEvent<float, float> _onStateChanged;

    // Событие, вызываемое при полном разрушении объекта (когда прочность достигает нуля).
    // Настраивается в инспекторе Unity.
    [SerializeField] private UnityEvent _onDestroyed;

    // Текущее значение прочности (внутреннее поле).
    private float _currentState = 100f;

    /// <summary>
    /// Публичное свойство для внешнего доступа к максимальной прочности.
    /// </summary>
    public float MaxState => _maxState;

    /// <summary>
    /// Публичное свойство для внешнего доступа к текущей прочности.
    /// </summary>
    public float CurrentState => _currentState;

    /// <summary>
    /// Возвращает true, если объект ещё не разрушен (прочность больше нуля).
    /// </summary>
    public bool IsAlive => _currentState > 0;

    /// <summary>
    /// Вызывается один раз при создании объекта или активации сцены.
    /// Инициализирует прочность до максимального значения.
    /// </summary>
    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// Сбрасывает текущую прочность до максимального значения.
    /// </summary>
    public void Initialize()
    {
        SetCurrentState(_maxState);
    }

    /// <summary>
    /// Восстанавливает прочность на указанное количество единиц.
    /// Не превышает максимальное значение.
    /// Не имеет эффекта, если объект уже уничтожен.
    /// </summary>
    /// <param name="amount">Количество единиц прочности для восстановления (должно быть положительным)</param>
    public void Restore(float amount)
    {
        if (IsAlive)
        {
            float restoreAmount = Mathf.Abs(amount);
            ChangeState(restoreAmount);
        }
    }

    /// <summary>
    /// Мгновенно уничтожает объект, устанавливая прочность в 0.
    /// Вызывает событие разрушения, даже если прочность уже была 0.
    /// </summary>
    public void KillInstantly()
    {
        // Если персонаж жив
        if (IsAlive)
        {
            // Принудительно устанавливаем прочность в 0,
            // минуя обычную логику урона.
            SetCurrentState(0);
        }
    }

    /// <summary>
    /// Наносит урон объекту.
    /// Урон уменьшает текущую прочность.
    /// Не имеет эффекта, если объект уже разрушен.
    /// </summary>
    /// <param name="damageAmount">Величина урона (должна быть положительной)</param>
    public void TakeDamage(float damageAmount)
    {
        // Если персонаж жив
        if (IsAlive)
        {
            // Преобразуем урон в отрицательное значение,
            // если было передано положительное значение. 
            float damage = Mathf.Abs(damageAmount) * -1;
            ChangeState(damage);
        }
    }

    /// <summary>
    /// Изменяет текущую прочность на указанную величину.
    /// Выполняет проверку на реальное изменение, чтобы избежать лишних вызовов.
    /// </summary>
    /// <param name="amount">Изменение прочности (может быть отрицательным или положительным)</param>
    private void ChangeState(float amount)
    {
        // Рассчитываем новое значение прочности объекта.
        float newState = _currentState + amount;
        // Если новое значение отличается от текущего
        if (_currentState != newState)
        {
            // Меняем значение прочности на новое.
            SetCurrentState(newState);
        }
    }

    /// <summary>
    /// Устанавливает новое значение прочности с ограничением в диапазоне \[0, _maxState\].
    /// Обновляет UI через событие _onStateChanged и проверяет условие разрушения.
    /// </summary>
    /// <param name="state">Новое значение прочности</param>
    private void SetCurrentState(float state)
    {
        // Ограничиваем значение прочности в допустимых пределах,
        // от 0 до максимального.
        _currentState = Mathf.Clamp(state, 0, _maxState);

        // Уведомляем подписчиков об изменении (например, UI)
        _onStateChanged.Invoke(_maxState, _currentState);

        // Если прочности у объекта не осталось
        if (IsAlive == false)
        {
            // Уведомляем подписчиков о разрушении объекта
            _onDestroyed.Invoke();
        }
    }
}
