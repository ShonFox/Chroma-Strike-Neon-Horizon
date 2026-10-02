using UnityEngine;

/// <summary>
/// Камера для top-down игры: следует за объектом сверху и не выходит за границы карты.
///
/// Как это работает:
///   1. Камера всегда смотрит сверху вниз на объект (fixed offset по высоте).
///   2. Позиция камеры вычисляется каждый кадр как позиция игрока + смещение.
///   3. Прежде чем применить позицию, она «зажимается» (Clamp) в пределах карты.
///      Границы карты задаются через Vector2 _mapBounds — это ПОЛОВИНА ширины/длины.
///      Например, _mapBounds = (50, 50) — карта 100x100 юнитов с центром в нуле.
///
/// Настройка в инспекторе:
///   - Target — объект игрока (тот, за которым следим).
///   - CameraHeight — высота камеры над игроком (чем больше, тем дальше обзор).
///   - MapBounds — половина ширины и длины карты (X и Z соответственно).
///   - FollowSpeed — насколько быстро камера догоняет игрока (0 = мгновенно).
/// </summary>
public class CameraController : MonoBehaviour
{
    // ─── Настройки камеры ──────────────────────────────────────────

    [Header("Цель и высота")]

    // Объект, за которым следит камера (обычно игрок).
    [SerializeField] private Transform _target;

    // Высота камеры над игроком. Чем больше — тем дальше «отлетает» камера.
    // Для top-down обычно 10–30 юнитов.
    [SerializeField, Range(5f, 50f)] private float _cameraHeight = 15f;

    [Header("Границы карты")]

    // Половина размера карты по X (ширина) и Z (длина).
    // Камера не будет выходить за эти пределы.
    // Например, (50, 50) → карта от -50 до +50 по обеим осям.
    [SerializeField] private Vector2 _mapBounds = new Vector2(50f, 50f);

    [Header("Сглаживание")]

    // Скорость следования камеры. 0 = мгновенно (жёстко), больше = плавнее.
    [SerializeField, Range(0f, 20f)] private float _followSpeed = 5f;

    // ─── Внутренние переменные ─────────────────────────────────────

    // Ссылка на компонент Transform камеры (для оптимизации — кэшируем).
    private Transform _cameraTransform;

    // Предполагаемая позиция камеры без учёта границ (вычисляется каждый кадр).
    private Vector3 _desiredPosition;

    // Границы, в которых может находиться камера с учётом её собственного размера.
    // Если orthographicSize не задан, граница = mapBounds.
    private float _minX, _maxX, _minZ, _maxZ;

    private void Start()
    {
        // Кэшируем трансформ камеры (this — это сама камера или объект с этим скриптом).
        _cameraTransform = transform;

        // Если target не назначен — выдаём предупреждение в консоль.
        if (_target == null)
        {
            Debug.LogWarning("[CameraController] Target не назначен! Камера будет стоять на месте.");
            return;
        }

        // Вычисляем границы камеры. Если камера orthographic, учитываем её размер,
        // чтобы края камеры не выходили за карту. Для perspective — упрощённо.
        Camera cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            // Для ортографической камеры: ортогональный размер = половина высоты viewport.
            float camHalfHeight = cam.orthographicSize;
            float camHalfWidth = camHalfHeight * cam.aspect; // aspect = ширина/высота экрана

            // Сдвигаем границы внутрь на половину размера камеры,
            // чтобы край экрана не показывал пустоту за картой.
            _minX = -_mapBounds.x + camHalfWidth;
            _maxX = _mapBounds.x - camHalfWidth;
            _minZ = -_mapBounds.y + camHalfHeight;
            _maxZ = _mapBounds.y - camHalfHeight;
        }
        else
        {
            // Для перспективной камеры — без коррекции на размер (упрощённый вариант).
            // Если нужно точнее — можно вычислить через угол обзора (fieldOfView) и высоту.
            _minX = -_mapBounds.x;
            _maxX = _mapBounds.x;
            _minZ = -_mapBounds.y;
            _maxZ = _mapBounds.y;
        }

        // Если границы «схлопнулись» (карта меньше камеры), центрируем.
        if (_minX > _maxX) { _minX = _maxX = 0f; }
        if (_minZ > _maxZ) { _minZ = _maxZ = 0f; }

        // Сразу ставим камеру на правильную позицию при старте.
        UpdateDesiredPosition();
        _cameraTransform.position = _desiredPosition;

        // Камера смотрит прямо вниз на игрока.
        _cameraTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // Вычисляем, где должна быть камера (без ограничений).
        UpdateDesiredPosition();

        // Плавно двигаем камеру к желаемой позиции (если followSpeed = 0 — мгновенно).
        if (_followSpeed > 0f)
        {
            // Vector3.Lerp с фактором, зависящим от времени — даёт плавное движение.
            float lerpFactor = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
            _cameraTransform.position = Vector3.Lerp(
                _cameraTransform.position, _desiredPosition, lerpFactor);
        }
        else
        {
            // Мгновенное следование.
            _cameraTransform.position = _desiredPosition;
        }
    }

    /// <summary>
    /// Вычисляет желаемую позицию камеры:
    /// берёт позицию игрока по X и Z, добавляет высоту, и зажимает в границы карты.
    /// </summary>
    private void UpdateDesiredPosition()
    {
        // Позиция игрока по X и Z (Y игнорируем — высота камеры фиксирована).
        float targetX = _target.position.x;
        float targetZ = _target.position.z;

        // Зажимаем (Clamp) координаты в границы карты.
        float clampedX = Mathf.Clamp(targetX, _minX, _maxX);
        float clampedZ = Mathf.Clamp(targetZ, _minZ, _maxZ);

        // Итоговая позиция: X и Z игрока (с ограничениями), Y = высота камеры.
        _desiredPosition = new Vector3(clampedX, _cameraHeight, clampedZ);
    }

    // ─── Отрисовка границ в редакторе (для удобства настройки) ─────

    private void OnDrawGizmosSelected()
    {
        // Рисуем прямоугольник карты в редакторе, чтобы видеть границы.
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(0f, _cameraHeight, 0f);
        Vector3 size = new Vector3(_mapBounds.x * 2f, 0.01f, _mapBounds.y * 2f);
        Gizmos.DrawWireCube(center, size);
    }
}
