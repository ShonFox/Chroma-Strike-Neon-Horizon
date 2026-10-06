using UnityEngine;

/// <summary>
/// ObjectMovement — физика движения объекта в top-down стиле.
/// Объект ВСЕГДА движется вперёд (в сторону transform.forward).
/// Игрок не останавливает движение — он только рулит (A/D) и ускоряется (W).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ObjectMovement : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  НАСТРОЙКИ В ИНСПЕКТОРЕ
    // ─────────────────────────────────────────────

    // Базовая скорость движения (единиц в секунду).
    // Объект едет с этой скоростью постоянно.
    [SerializeField, Range(2f, 8f)] private float _baseSpeed = 5f;

    // Множитель ускорения при бусте (W).
    // Например, 2.0 — скорость удваивается.
    [SerializeField, Range(1f, 5f)] private float _boostMultiplier = 2f;

    // Сила ускорения — как быстро объект разгоняется до целевой скорости.
    // Больше значение = резче старт. Меньше = плавнее.
    [SerializeField, Range(5f, 20f)] private float _acceleration = 10f;

    // Сила ускорения при бусте.
    [SerializeField, Range(10f, 30f)] private float _boostAcceleration = 20f;

    // ─────────────────────────────────────────────
    //  ВНУТРЕННИЕ ПЕРЕМЕННЫЕ
    // ─────────────────────────────────────────────

    // Разрешено ли движение вперёд. false — во время отброса от стены.
    private bool _movementEnabled = true;
    private Rigidbody _rb;
    private bool _isBoosting;

    private void Awake()
    {
        // Получаем Rigidbody, висящий на этом же объекте.
        // [RequireComponent] гарантирует, что он есть.
        _rb = GetComponent<Rigidbody>();

        // Замораживаем вращение по X и Z, чтобы объект не кувыркался.
        // Поворот управляется скриптом, не физикой.
        _rb.constraints = RigidbodyConstraints.FreezeRotationX |
                          RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (_rb.isKinematic) return;

        // Во время отброса движение не рулим и не толкаем вперёд.
        if (!_movementEnabled) return;

        // Текущая скорость объекта (в единицах/сек).
        Vector3 currentVelocity = _rb.linearVelocity;

        // Целевая скорость — объект всегда едет вперёд (transform.forward).
        // В top-down «вперёд» — это та сторона объекта, которая «смотрит» в направлении поворота.
        float targetSpeed = _isBoosting ? _baseSpeed * _boostMultiplier : _baseSpeed;
        Vector3 targetVelocity = transform.forward * targetSpeed;

        // Плавно разгоняемся к целевой скорости.
        // Если буст — ускоряемся быстрее (_boostAcceleration).
        float accel = _isBoosting ? _boostAcceleration : _acceleration;
        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            targetVelocity,
            accel * Time.fixedDeltaTime
        );

        // Применяем скорость к Rigidbody.
        _rb.linearVelocity = currentVelocity;
    }

    /// <summary>
    /// Включает/выключает автодвижение. Используется WallKnockback,
    /// чтобы на время отброса не перезаписывать скорость дрона.
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;
        // При выключении сразу гасим скорость, чтобы дрон не «вёз» старый импульс дальше.
        if (!enabled)
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            _rb.linearVelocity = velocity;
        }
    }

    /// <summary>
    /// Включить или выключить буст.
    /// Вызывается из BallController.OnBoost().
    /// </summary>
    public void SetBoosting(bool state)
    {
        _isBoosting = state;
    }
}
