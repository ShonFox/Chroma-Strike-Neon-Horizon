using UnityEngine;

/// <summary>
/// RamDamage — урон и отброс при столкновении телом.
/// Вешается только на тело врага (на пулях этот функционал не нужен —
/// у них свой DealDamageAndDestroy).
/// </summary>
public class RamDamage : MonoBehaviour
{
    // Урон при таране.
    [SerializeField] private float _damage = 10f;

    // Сила отброса.
    [SerializeField] private float _knockbackForce = 5f;

    // Кого не бьём телом (своих не толкаем и не калечим).
    [SerializeField] private string[] _ignoreTags = { "Enemy" };

    private void OnCollisionEnter(Collision collision)
    {
        foreach (string tag in _ignoreTags)
        {
            if (collision.gameObject.CompareTag(tag)) return;
        }

        if (collision.gameObject.TryGetComponent<ObjectDurability>(out var durability))
        {
            durability.TakeDamage(_damage);
        }

        if (collision.rigidbody != null)
        {
            // Отброс строго по горизонтали — вертикальную составляющую гасим,
            // чтобы никого не подбрасывало вверх.
            Vector3 direction = collision.transform.position - transform.position;
            direction.y = 0f;
            direction = direction.normalized;

            collision.rigidbody.AddForce(direction * _knockbackForce, ForceMode.Impulse);

            // Гасим раскрутку: вращением управляют скрипты, не физика.
            collision.rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
