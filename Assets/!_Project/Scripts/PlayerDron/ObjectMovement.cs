using UnityEngine;

/// <summary>
/// Физическое движение объекта в стиле top-down.
///
/// Объект ВСЕГДА движется вперёд (в сторону своего «носа» — transform.forward).
/// Игрок не останавливает движение, он только поворачивает (BallController → поворот)
/// и может кратковременно ускоряться (буст через SetBoosting).
///
/// Поворот выполняется в BallController, а этот скрипт просто едет вперёд.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ObjectMovement : MonoBehaviour
{
    // ─── Физические параметры ─────────────────────────────────────

    [Header("Обычное движение")]

    // Обычная скорость движения (юнитов в секунду).
    [SerializeField, Range(1f, 20f)] private float _maxSpeed = 6f;

    // Насколько быстро объект разгоняется до максимальной скорости.
    // Чем больше — тем быстрее достигает максимума.
    [SerializeField, Range(1f, 20f)] private float _acceleration = 4f;

    [Header("Буст (ускорение по W)")]

    // Множитель скорости при бусте. Например, 2.0 = в 2 раза быстрее обычной.
    [SerializeField, Range(1.2f, 5f)] private float _boostMultiplier = 2f;

    // Насколько быстро объект разгоняется при бусте (обычно быстрее обычного).
    [SerializeField, Range(1f, 30f)] private float _boostAcceleration = 12f;

    // ─── Внутренние переменные ──────────────────────────────────────

    // Ссылка на Rigidbody объекта — нужен для управления скоростью через физику.
    private Rigidbody _rigidbody;

    // Текущее состояние буста (true = ускоряемся, false = обычный режим).
    private bool _isBoosting;

    private void Awake()
    {
        // Получаем ссылку на Rigidbody, висящий на этом же объекте.
        _rigidbody = GetComponent<Rigidbody>();

        // Для top-down игры важно заморозить вращение по X и Z,
        // чтобы объект не кувыркался от столкновений.
        // Поворот управляем только по Y (через BallController).
        _rigidbody.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        // ─── ОПРЕДЕЛЯЕМ ЦЕЛЕВУЮ СКОРОСТЬ ───────────────────────────

        // Текущее направление «вперёд» объекта (по оси Z его трансформа).
        // Берём transform.forward, убираем вертикаль (Y = 0) и нормализуем.
        Vector3 forwardDirection = transform.forward;
        forwardDirection.y = 0f;
        forwardDirection.Normalize();

        // Текущая максимальная скорость: обычная или ускоренная.
        float targetSpeed = _isBoosting ? _maxSpeed * _boostMultiplier : _maxSpeed;

        // Текущее ускорение: обычное или бустовое.
        float currentAcceleration = _isBoosting ? _boostAcceleration : _acceleration;

        // Целевой вектор скорости (куда и как быстро ехать).
        Vector3 targetVelocity = forwardDirection * targetSpeed;

        // ─── ПЛАВНО МЕНЯЕМ СКОРОСТЬ ────────────────────────────────

        // Берём текущую скорость из Rigidbody.
        Vector3 currentVelocity = _rigidbody.linearVelocity;

        // Сохраняем вертикальную скорость, чтобы не влиять на гравитацию/прыжки.
        float verticalSpeed = currentVelocity.y;

        // Обнуляем Y для расчётов горизонтального движения.
        currentVelocity.y = 0f;

        // Приращение скорости за один физический кадр.
        float delta = currentAcceleration * Time.fixedDeltaTime;

        // Плавно движемся к целевой скорости.
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, delta);

        // Возвращаем вертикальную скорость обратно (гравитация и т.д.).
        currentVelocity.y = verticalSpeed;

        // Применяем итоговую скорость к Rigidbody.
        _rigidbody.linearVelocity = currentVelocity;
    }

    // ─── Публичные методы ──────────────────────────────────────────

    /// <summary>
    /// Включает или выключает режим ускорения (буста).
    /// Вызывается из BallController при нажатии/отпускании W.
    /// </summary>
    public void SetBoosting(bool isBoosting)
    {
        _isBoosting = isBoosting;
    }

    /// <summary>Текущая скорость объекта (по горизонтали), юнитов/сек.</summary>
    public float CurrentSpeed
    {
        get
        {
            Vector3 horizontal = _rigidbody.linearVelocity;
            horizontal.y = 0f;
            return horizontal.magnitude;
        }
    }
}
