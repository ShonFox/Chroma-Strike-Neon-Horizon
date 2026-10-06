using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AmmoPickupSpawner — раскидывает бонусы патронов по карте в случайных местах.
/// Вешается на любой пустой объект (например, "PickupSpawner" в корне сцены).
///
/// Ключевая идея экономии оперативки:
/// — На карте НИКОГДА не больше _maxPickups активных бонусов одновременно.
/// — Бонусы не копятся: подобрали один → через задержку появится один новый.
/// — Позиции подбираются случайно, но с проверками (не в стене, не на игроке).
/// </summary>
public class AmmoPickupSpawner : MonoBehaviour
{
    [Header("Связи")]
    // Префаб бонуса (объект со скриптом AmmoPickup).
    [SerializeField] private AmmoPickup _pickupPrefab;

    // Ссылка на игрока — чтобы не спавнить бонусы прямо на нём.
    [SerializeField] private Transform _player;

    [SerializeField] private LayerMask _obstacleMask;

    [Header("Количество (экономия памяти)")]
    // МАКСИМУМ бонусов на карте одновременно. Жёсткий предел!
    [SerializeField, Min(1)] private int _maxPickups = 20;

    // Сколько бонусов раскидать в начале игры.
    [SerializeField, Min(0)] private int _initialPickups = 10;

    // Задержка (сек) перед спавном нового бонуса после подбора старого.
    [SerializeField, Min(0f)] private float _respawnDelay = 10f;

    [Header("Зона спавна")]
    // Центр прямоугольной зоны, где появляются бонусы.
    [SerializeField] private Vector2 _areaCenter = Vector2.zero;

    // Размер зоны по X и Z (ширина и глубина).
    [SerializeField] private Vector2 _areaSize = new Vector2(30f, 30f);

    // Высота, на которой висит бонус.
    [SerializeField] private float _spawnHeight = 0.5f;

    [Header("Проверки позиции")]
    // Минимальная дистанция от игрока — чтобы бонус не появился под ним.
    [SerializeField] private float _minDistanceFromPlayer = 3f;

    // Радиус проверки «не застрял ли бонус в стене».
    [SerializeField] private float _obstacleCheckRadius = 1f;

    // Сколько раз пытаться найти свободное место (защита от зависания).
    [SerializeField, Min(1)] private int _maxSpawnAttempts = 20;

    // Список живых бонусов на карте.
    private readonly List<AmmoPickup> _activePickups = new List<AmmoPickup>();

    private void Start()
    {
        // Раскидываем стартовое количество.
        for (int i = 0; i < _initialPickups; i++)
        {
            Spawn();
        }
    }

    /// <summary>
    /// Создаёт один бонус в случайной свободной точке зоны.
    /// Проверяет лимит _maxPickups — больше этого числа на карте не будет НИКОГДА.
    /// </summary>
    private void Spawn()
    {
        // ЖЁСТКИЙ ЛИМИТ: это и защищает оперативку от переполнения.
        if (_activePickups.Count >= _maxPickups) return;
        if (_pickupPrefab == null)
        {
            Debug.LogWarning("AmmoPickupSpawner: не назначен префаб бонуса!");
            return;
        }

        Vector3 position = FindFreePosition();

        // Создаём бонус в найденной точке.
        AmmoPickup pickup = Instantiate(_pickupPrefab, position, Quaternion.identity);

        // Подписываемся на событие подбора, чтобы знать, когда он исчез.
        pickup.OnCollected += HandleCollected;

        // Запоминаем в списке живых.
        _activePickups.Add(pickup);
    }

    /// <summary>
    /// Вызывается, когда игрок подобрал бонус.
    /// Убираем его из списка и планируем спавн нового через задержку.
    /// </summary>
    private void HandleCollected(AmmoPickup pickup)
    {
        // Отписываемся — иначе утечка ссылок на уничтоженный объект.
        pickup.OnCollected -= HandleCollected;
        _activePickups.Remove(pickup);

        // Запускаем отложенный спавн.
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(_respawnDelay);

        // Спавним только если есть место по лимиту (на случай будущих изменений).
        Spawn();
    }

    /// <summary>
    /// Ищет случайную точку в зоне: не в стене и не слишком близко к игроку.
    /// Если за _maxSpawnAttempts попыток не нашли — берём центр зоны.
    /// </summary>
    private Vector3 FindFreePosition()
    {
        Vector3 lastCandidate = new Vector3(_areaCenter.x, _spawnHeight, _areaCenter.y);

        for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
        {
            float x = Random.Range(_areaCenter.x - _areaSize.x * 0.5f,
                                   _areaCenter.x + _areaSize.x * 0.5f);
            float z = Random.Range(_areaCenter.y - _areaSize.y * 0.5f,
                                   _areaCenter.y + _areaSize.y * 0.5f);

            Vector3 candidate = new Vector3(x, _spawnHeight, z);
            lastCandidate = candidate;

            // Не слишком близко к игроку.
            if (_player != null &&
                Vector3.Distance(candidate, _player.position) < _minDistanceFromPlayer)
            {
                continue;
            }

            // Проверяем, что точка не внутри препятствия.
            Collider[] hits = Physics.OverlapSphere(candidate, _obstacleCheckRadius,
                                                    _obstacleMask, QueryTriggerInteraction.Ignore);

            if (hits.Length > 0)
            {
                // Сразу видно, ЧТО мешает и на каком слое оно лежит.
                Debug.Log($"Точка занята: '{hits[0].name}', слой: " +
                          $"{LayerMask.LayerToName(hits[0].gameObject.layer)}");
                continue;
            }

            return candidate; // Идеальная точка: не в стене и не на игроке.
        }

        // Все попытки исчерпаны. Берём последнюю случайную точку, а не центр —
        // она хотя бы не совпадает с местом игрока гарантированно.
        // Если и тут мешает игрок — это редкий случай, бонус подберётся и рестартанёт сам.
        Debug.LogWarning("AmmoPickupSpawner: свободную точку найти не удалось, " +
                         "спавню в последней проверенной. Проверь маску препятствий!");
        return lastCandidate;
    }


    /// <summary>
    /// Рисует зону спавна в редакторе (видна только когда спавнер выделен).
    /// Удобно настраивать зону визуально.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(_areaCenter.x, _spawnHeight, _areaCenter.y);
        Vector3 size = new Vector3(_areaSize.x, 0.1f, _areaSize.y);
        Gizmos.DrawWireCube(center, size);
    }
}
