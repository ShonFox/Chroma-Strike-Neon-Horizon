using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner — спавнит врагов из порталов.
/// Правила:
/// — На карте НИКОГДА не больше _maxEnemies живых врагов.
/// — Убили врага → через задержку спавнится новый из случайного СВОБОДНОГО портала.
/// — Вешается на пустой объект в корне сцены.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Связи")]
    // Префаб врага (со всеми скриптами: ObjectMovement, EnemyController и т.д.).
    [SerializeField] private GameObject _enemyPrefab;

    // Четыре портала — пустые объекты на карте, из которых появляются враги.
    [SerializeField] private Transform[] _portals;

    [Header("Количество")]
    // ЖЁСТКИЙ предел врагов на карте.
    [SerializeField, Min(1)] private int _maxEnemies = 4;

    // Сколько врагов расставить в начале игры.
    [SerializeField, Min(0)] private int _initialEnemies = 2;

    // Задержка перед спавном нового врага после убийства.
    [SerializeField, Min(0f)] private float _respawnDelay = 5f;

    // Радиус «свободности» портала: не спавним в портал,
    // рядом с которым уже стоит другой враг.
    [SerializeField] private float _portalFreeRadius = 1f;

    // Высота, на которой появляется враг относительно позиции портала.
    // 0.5 — как у твоих бонусов патронов.
    [SerializeField] private float _spawnHeight = 0.5f;

    // Список живых врагов.
    private readonly List<GameObject> _activeEnemies = new List<GameObject>();

    private void Start()
    {
        for (int i = 0; i < _initialEnemies; i++)
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        // ЖЁСТКИЙ ЛИМИТ — как у бонусов патронов.
        if (_activeEnemies.Count >= _maxEnemies) return;

        if (_enemyPrefab == null || _portals == null || _portals.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: не назначен префаб врага или порталы!");
            return;
        }

        Transform portal = FindFreePortal();

        // Жёсткая высота над землёй независимо от позиции портала.
        Vector3 spawnPosition = new Vector3(portal.position.x, _spawnHeight, portal.position.z);
        GameObject enemy = Instantiate(_enemyPrefab, spawnPosition, portal.rotation);

        // Диагностика: убедимся, что спавнимся ровно на 0.5.
        Debug.Log($"Спавн врага: Y = {spawnPosition.y}");


        // Подписываемся на смерть врага, чтобы знать, когда освободится слот.
        if (enemy.TryGetComponent<EnemyDeathHandler>(out var deathHandler))
        {
            deathHandler.OnDefeated += HandleDefeated;
        }
        else
        {
            Debug.LogWarning("EnemySpawner: у префаба врага нет EnemyDeathHandler — " +
                             "спавнер не узнает о смерти врага!");
        }

        _activeEnemies.Add(enemy);
    }

    /// <summary>
    /// Ищет случайный портал, рядом с которым нет живых врагов.
    /// Если все заняты — возвращает случайный (не будем зависать).
    /// </summary>
    private Transform FindFreePortal()
    {
        List<Transform> freePortals = new List<Transform>();

        foreach (Transform portal in _portals)
        {
            if (portal == null) continue;

            bool occupied = false;
            foreach (GameObject enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    // Сравниваем только по горизонтали (Y обнуляем),
                    // чтобы высота спавна не мешала определению занятости.
                    Vector3 portalPoint = portal.position; portalPoint.y = 0f;
                    Vector3 enemyPoint = enemy.transform.position; enemyPoint.y = 0f;

                    if (Vector3.Distance(portalPoint, enemyPoint) < _portalFreeRadius)
                    {
                        occupied = true;
                        break;
                    }
                }
            }

            if (!occupied) freePortals.Add(portal);
        }

        // Все свободны или часть свободна — случайный из свободных.
        if (freePortals.Count > 0)
        {
            return freePortals[Random.Range(0, freePortals.Count)];
        }

        // Все заняты — берём случайный, чтобы не зависнуть.
        return _portals[Random.Range(0, _portals.Length)];
    }


    private void HandleDefeated(EnemyDeathHandler handler)
    {
        // Отписка обязательна — иначе ссылка на уничтоженный объект утекёт.
        handler.OnDefeated -= HandleDefeated;
        _activeEnemies.Remove(handler.gameObject);

        // Запускаем отложенный спавн нового врага.
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(_respawnDelay);

        // Спавним только если есть место по лимиту.
        Spawn();
    }
}
