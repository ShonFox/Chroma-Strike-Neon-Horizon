using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EnemyDeathHandler — реакция врага на уничтожение.
/// Метод HandleDestroyed назначается в инспекторе ObjectDurability
/// в событие OnDestroyed (как HandleDestroyed у дрона игрока).
/// 1) Создаёт эффект взрыва в точке смерти.
/// 2) Начисляет очки через ScoreManager.
/// 3) Сообщает EnemySpawner, что слот освободился (событие OnDefeated).
/// 4) Уничтожает объект врага.
/// </summary>
public class EnemyDeathHandler : MonoBehaviour
{
    // Сколько очков даётся за этого врага.
    [SerializeField] private int _scorePerKill = 100;

    // Префаб взрыва, который появляется в точке смерти.
    [SerializeField] private GameObject _explosionPrefab;

    // Событие для спавнера: «враг уничтожен, можно спавнить нового».
    public event Action<EnemyDeathHandler> OnDefeated;

    // Защита от двойного срабатывания события прочности.
    private bool _isDead;

    public void HandleDestroyed()
    {
        if (_isDead) return;
        _isDead = true;

        // Взрыв появляется в мировых координатах ДО уничтожения врага —
        // иначе вместе с объектом пропадёт и точка спавна эффекта.
        SpawnExplosion();

        // Сообщаем спавнеру ДО уничтожения, чтобы он успел отписаться.
        OnDefeated?.Invoke(this);

        // Начисляем очки. ScoreManager живёт в сцене отдельно от врага.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(_scorePerKill);
        }

        Destroy(gameObject);
    }

    private void SpawnExplosion()
    {
        if (_explosionPrefab == null)
        {
            Debug.LogWarning("EnemyDeathHandler: не назначен префаб взрыва!");
            return;
        }

        UnityEngine.Object.Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
    }
}
