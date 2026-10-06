using System.Collections;
using UnityEngine;

/// <summary>
/// DronDeathHandler — реакция дрона на разрушение.
/// При смерти: взрыв частиц, скрытие модели, загрузка сцены поражения.
/// Вешается на корневой объект дрона.
/// </summary>
public class DronDeathHandler : MonoBehaviour
{
    [SerializeField] private SceneSwitcher _sceneSwitcher;

    // Префаб взрыва, который появляется в точке смерти.
    [SerializeField] private GameObject _explosionPrefab;

    [SerializeField] private float _delayBeforeGameOver = 3.0f;

    // Модель прячем — её заменяет эффект взрыва.
    [SerializeField] private bool _hideVisualsOnDeath = true;
    [SerializeField] private bool _disablePhysicsOnDeath = true;

    public void HandleDestroyed()
    {
        SpawnExplosion();

        if (_hideVisualsOnDeath)
            DisableAllRenderers();

        if (_disablePhysicsOnDeath)
            DisableAllPhysics();

        // Запускаем ожидание на SceneSwitcher: он не умирает вместе
        // с дроном, поэтому сопрограмма гарантированно дойдёт до конца.
        if (_sceneSwitcher != null)
        {
            _sceneSwitcher.StartCoroutine(WaitAndLoadFatalScene());
        }
        else
        {
            Debug.LogWarning("DronDeathHandler: не назначен SceneSwitcher!");
        }
    }

    private void SpawnExplosion()
    {
        if (_explosionPrefab == null)
        {
            Debug.LogWarning("DronDeathHandler: не назначен префаб взрыва!");
            return;
        }

        // Взрыв появляется в позиции дрона, но в мировых координатах,
        // а не ребёнком дрона — иначе умрёт вместе с ним.
        Object.Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
    }

    private void DisableAllRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }
    }

    private void DisableAllPhysics()
    {
        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody body in bodies)
        {
            body.linearVelocity = Vector3.zero; // Сброс ДО перевода в кинематик.
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    private IEnumerator WaitAndLoadFatalScene()
    {
        yield return new WaitForSeconds(_delayBeforeGameOver);
        _sceneSwitcher.LoadFatalScene();
    }
}
