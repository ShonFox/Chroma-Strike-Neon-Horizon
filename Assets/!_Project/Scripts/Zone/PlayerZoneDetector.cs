using UnityEngine;

/// <summary>
/// PlayerZoneDetector — зона преследования без триггера.
/// Просто измеряет расстояние до игрока каждый кадр.
/// Вешается на корень врага (или на ребёнка — радиус считается от позиции объекта).
/// Коллайдер-триггер больше НЕ нужен: удали сферу-триггер или оставь её для другого.
/// </summary>
public class PlayerZoneDetector : MonoBehaviour
{
    // Ссылка на игрока. В префабе не заполнить — найдём сами.
    [SerializeField] private BallController _player;

    // Радиус зоны преследования: враг реагирует, если игрок ближе этого расстояния.
    [SerializeField] private float _detectionRadius = 10f;

    private bool _isPlayerInZone;

    public bool IsPlayerInZone => _isPlayerInZone;

    private void Awake()
    {
        if (_player == null)
        {
            _player = FindObjectOfType<BallController>();
        }
    }

    private void Update()
    {
        if (_player == null)
        {
            // Игрок, возможно, погиб и уничтожен — ищем заново.
            _player = FindObjectOfType<BallController>();
            _isPlayerInZone = false;
            return;
        }

        // Расстояние между врагом и дроном по горизонтали.
        Vector3 offset = _player.transform.position - transform.position;
        offset.y = 0f;
        _isPlayerInZone = offset.magnitude <= _detectionRadius;
    }

    public Vector3 GetTargetPosition()
    {
        return _player != null ? _player.transform.position : transform.position;
    }
}
