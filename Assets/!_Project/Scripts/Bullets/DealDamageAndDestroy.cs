using UnityEngine;

public class DealDamageAndDestroy : MonoBehaviour
{
    //Кол-во урона
    [SerializeField] private float _damage = 10f;

    //Теги объектов, которые пуля игнорирует (не наносит урон и не уничтожается об них).
    //В инспекторе впиши "Player" — тогда своя пуля пролетит сквозь тебя.
    [SerializeField] private string[] _ignoreTags = { "Player" };

    //Обычное столкновение (коллайдер без Is Trigger)
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    //Если коллайдер включён как триггер — сработает этот метод.
    //Оставлены оба: какой сработает, тот и обработает попадание.
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void HandleHit(GameObject target)
    {
        //Проверяем список игнорируемых тегов.
        foreach (string tag in _ignoreTags)
        {
            if (target.CompareTag(tag))
            {
                return; //Выходим, не нанося урон и НЕ уничтожая пулю.
            }
        }

        //Наносим урон, если у цели есть прочность.
        if (target.TryGetComponent<ObjectDurability>(out var durability))
        {
            durability.TakeDamage(_damage);
        }

        //Уничтожение пули после попадания.
        Destroy(gameObject);
    }
}
