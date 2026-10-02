using UnityEngine;

/// <summary>
/// CameraController — камера, которая следует за объектом сверху.
/// Вешается на Main Camera. Камера смотрит вниз (ось Y — вверх по миру).
/// Не выходит за границы карты благодаря Mathf.Clamp.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  НАСТРОЙКИ В ИНСПЕКТОРЕ
    // ─────────────────────────────────────────────

    // Объект, за которым следует камера (ваш игрок).
    [SerializeField] private Transform _target;

    // Высота камеры над объектом.
    [SerializeField] private float _height = 15f;

    // Плавность следования. Больше = резче, меньше = плавнее.
    // 0 — очень плавно, 10 — почти мгновенно.
    [SerializeField] private float _followSpeed = 5f;

    // Границы карты (в единицах Unity).
    // Камера не выйдет за эти пределы.
    [SerializeField] private float _mapMinX = -50f;
    [SerializeField] private float _mapMaxX = 50f;
    [SerializeField] private float _mapMinZ = -50f;
    [SerializeField] private float _mapMaxZ = 50f;

    // ─────────────────────────────────────────────
    //  ВНУТРЕННИЕ ПЕРЕМЕННЫЕ
    // ─────────────────────────────────────────────

    private Camera _cam;
    private float _halfWidth;  // Половина ширины обзора камеры (для orthographic).
    private float _halfHeight; // Половина высоты обзора камеры (для orthographic).

    private void Start()
    {
        _cam = GetComponent<Camera>();

        // Если камера ортографическая — считаем её размеры,
        // чтобы края камеры не показывали пустоту за картой.
        // Для perspective-камеры этот расчёт будет приблизительным.
        if (_cam.orthographic)
        {
            _halfHeight = _cam.orthographicSize;
            _halfWidth = _halfHeight * _cam.aspect;
        }
        else
        {
            // Для перспективной камеры берём примерный размер на высоте _height.
            // Это упрощение — для точных границ нужна другая формула.
            float distance = _height;
            float verticalFOV = _cam.fieldOfView * Mathf.Deg2Rad;
            _halfHeight = Mathf.Tan(verticalFOV * 0.5f) * distance;
            _halfWidth = _halfHeight * _cam.aspect;
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // Желаемая позиция камеры — прямо над игроком.
        Vector3 desiredPosition = new Vector3(
            _target.position.x,
            _height,
            _target.position.z
        );

        // Плавно движемся к цели через Lerp.
        // Time.deltaTime делает движение независимым от FPS.
        Vector3 smoothed = Vector3.Lerp(
            transform.position,
            desiredPosition,
            _followSpeed * Time.deltaTime
        );

        // Зажимаем позицию в границы карты.
        // Вычитаем половину размера камеры, чтобы край камеры
        // (а не центр) не выходил за границу.
        smoothed.x = Mathf.Clamp(smoothed.x,
            _mapMinX + _halfWidth,
            _mapMaxX - _halfWidth);
        smoothed.z = Mathf.Clamp(smoothed.z,
            _mapMinZ + _halfHeight,
            _mapMaxZ - _halfHeight);

        // Применяем позицию.
        transform.position = smoothed;

        // Камера всегда смотрит строго вниз.
        // Euler(90, 0, 0) = поворот на 90° по оси X.
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    /// <summary>
    /// Рисует границы карты в редакторе (красный прямоугольник).
    // Видно в Scene View при выделении камеры.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Рисуем прямоугольник границ карты.
        Vector3 bottomLeft  = new Vector3(_mapMinX, 0f, _mapMinZ);
        Vector3 bottomRight = new Vector3(_mapMaxX, 0f, _mapMinZ);
        Vector3 topLeft     = new Vector3(_mapMinX, 0f, _mapMaxZ);
        Vector3 topRight     = new Vector3(_mapMaxX, 0f, _mapMaxZ);

        Gizmos.DrawLine(bottomLeft,  bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight,    topLeft);
        Gizmos.DrawLine(topLeft,     bottomLeft);
    }
}
