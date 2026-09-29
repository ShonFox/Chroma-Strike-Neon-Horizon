using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target & Position")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private float _height = 20f; // Высота камеры над ареной

    [Header("Arena Bounds (100x100)")]
    [SerializeField] private float _arenaSizeX = 100f;
    [SerializeField] private float _arenaSizeZ = 100f;
    [SerializeField] private float _cameraOffsetFromEdge = 5f; // Отступ от края, чтобы не видеть пустоту

    [Header("Rotation")]
    [SerializeField, Range(1f, 360f)] private float _turnSpeed = 100f;
    [SerializeField] private bool _lockRotation = false; // Если true — камера не вращается, всегда смотрит строго вниз

    [Header("Collision & Smoothing")]
    [SerializeField, Range(0.1f, 2f)] private float _collisionOffset = 0.5f;
    [SerializeField] private LayerMask _obstacleLayerMask;
    [SerializeField] private float _smoothSpeed = 10f; // Плавность следования за игроком

    private Vector2 _orbitAngles;
    private const float _startVerticalAngle = 90f; // 90 градусов — это строго сверху
    private const float _startHorizontalAngle = 0f;

    private void Awake()
    {
        if (_playerTransform == null)
        {
            Debug.LogError("[CameraOrbit] Не назначен Player Transform в инспекторе!");
            enabled = false;
            return;
        }

        _orbitAngles = new Vector2(_startVerticalAngle, _startHorizontalAngle);

        // Сразу ставим камеру в центр арены при старте
        transform.position = new Vector3(0, _height, 0);
    }

    private void LateUpdate()
    {
        // 1. Вычисляем целевую позицию камеры
        Vector3 targetPosition = CalculateTargetPosition();

        // 2. Применяем плавность (Lerp) для следования за игроком
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.unscaledDeltaTime * _smoothSpeed);

        // 3. Вращение
        if (!_lockRotation)
        {
            Quaternion rotation = Quaternion.Euler(_orbitAngles.x, _orbitAngles.y, 0f);
            transform.rotation = rotation;
        }
        else
        {
            // Если вращение заблокировано, принудительно ставим вид строго сверху
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    private Vector3 CalculateTargetPosition()
    {
        // Базовое направление (вниз по оси Y для вида сверху)
        Vector3 direction = Vector3.down;

        // Идеальная позиция: позиция игрока + смещение вниз на высоту
        // Но мы хотим, чтобы камера следовала за игроком, оставаясь в рамках арены
        Vector3 idealPosition = _playerTransform.position - direction * _height;

        // 4. ОГРАНИЧЕНИЕ ПО ГРАНИЦАМ АРЕНЫ (100x100)
        float halfSizeX = (_arenaSizeX * 0.5f) - _cameraOffsetFromEdge;
        float halfSizeZ = (_arenaSizeZ * 0.5f) - _cameraOffsetFromEdge;

        float clampedX = Mathf.Clamp(idealPosition.x, -halfSizeX, halfSizeX);
        float clampedZ = Mathf.Clamp(idealPosition.z, -halfSizeZ, halfSizeZ);

        Vector3 clampedPosition = new Vector3(clampedX, idealPosition.y, clampedZ);

        // 5. ПРОВЕРКА НА ПРЕПЯТСТВИЯ (Linecast)
        // В top-down шутерах камера часто висит высоко, но коллизии всё равно нужны,
        // если камера может опускаться или есть высокие объекты.
        if (Physics.Linecast(_playerTransform.position, clampedPosition, out RaycastHit hit, _obstacleLayerMask))
        {
            float distanceToHit = Vector3.Distance(_playerTransform.position, hit.point);
            float adjustedDistance = distanceToHit - _collisionOffset;

            // Для вида сверху коллизия чаще всего означает, что камера "провалилась" в объект.
            // Мы просто немного поднимаем её или сдвигаем, если это критично.
            // В данном случае просто корректируем Y, если попали в объект ниже камеры
            if (hit.point.y < clampedPosition.y)
            {
                clampedPosition.y = hit.point.y + _collisionOffset;
            }
        }

        return clampedPosition;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (_lockRotation) return; // Если вращение заблокировано, игнорируем ввод

        Vector2 input = context.ReadValue<Vector2>();

        float deltaX = input.x;
        float deltaY = -input.y; // Инверсия Y для мыши

        _orbitAngles.x += deltaY * _turnSpeed * Time.unscaledDeltaTime;
        _orbitAngles.y += deltaX * _turnSpeed * Time.unscaledDeltaTime;

        // Для вида сверху вертикальный угол должен быть близок к 90. 
        // Если вы хотите дать игроку возможность немного наклонять камеру (как в изометрии), раскомментируйте строку ниже:
        // _orbitAngles.x = Mathf.Clamp(_orbitAngles.x, 70f, 90f); 

        // Если нужен строго вид сверху без наклона:
        _orbitAngles.x = 90f;

        _orbitAngles.y = Mathf.Repeat(_orbitAngles.y, 360f);
    }
}
