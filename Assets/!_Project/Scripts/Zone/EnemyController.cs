using UnityEngine;

/// <summary>
/// EnemyController — мозг врага.
/// Если игрок в зоне (PlayerZoneDetector) — доворачивает нос на игрока,
/// и ObjectMovement сам увозит врага в его сторону.
/// Стреляет только когда нос наведён на цель достаточно точно.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [SerializeField] private PlayerZoneDetector _zoneDetector;
    [SerializeField] private EnemyShooter _shooter;

    // Скорость доворота на игрока (градусов в секунду).
    [SerializeField] private float _turnSpeed = 120f;

    // Порог наведённости: 0.9 — допустим разброс примерно в 25 градусов.
    [SerializeField, Range(0.5f, 1f)] private float _aimThreshold = 0.9f;

    private void Awake()
    {
        // Страховка: если забыли назначить зону в инспекторе — ищем на этом же объекте.
        if (_zoneDetector == null)
        {
            _zoneDetector = GetComponentInChildren<PlayerZoneDetector>();
        }

        if (_shooter == null)
        {
            _shooter = GetComponentInChildren<EnemyShooter>();
        }
    }

    private void Update()
    {
        // Защита от пустых ссылок: без зоны и шутера враг молчит, а не сыпет ошибками.
        if (_zoneDetector == null || _shooter == null)
        {
            Debug.LogWarning($"{name}: у EnemyController не заполнены ссылки — проверь инспектор префаба!");
            enabled = false;
            return;
        }

        if (_zoneDetector.IsPlayerInZone)
        {
            PursuePlayer();
        }
        else
        {
            _shooter.Shoot(false);
        }
    }

    private void PursuePlayer()
    {
        Vector3 targetPosition = _zoneDetector.GetTargetPosition();
        Vector3 directionToTarget = GetDirectionToTarget(targetPosition);

        // Погоня = доворот носа на цель. ObjectMovement сам толкает вперёд,
        // куда смотрит нос — отдельное «ехать» не нужно.
        RotateTowards(directionToTarget);

        // Стреляем только когда смотрим достаточно точно на игрока.
        bool isAimed = Vector3.Dot(transform.forward, directionToTarget) >= _aimThreshold;
        _shooter.Shoot(isAimed);
    }

    private Vector3 GetDirectionToTarget(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0f;
        return directionToTarget.normalized;
    }

    private void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }
}
