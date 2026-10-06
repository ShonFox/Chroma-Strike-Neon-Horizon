using System;
using UnityEngine;

/// <summary>
/// AmmoPickup — бонус на карте, который восстанавливает патроны.
/// Вешается на ПРЕФАБ бонуса (у префаба должен быть Collider с галкой Is Trigger).
///
/// Как работает:
/// — Медленно крутится, чтобы выглядеть заметно.
/// — Когда игрок касается триггера → добавляем патроны его счётчику,
///   сообщаем спавнеру «меня подобрали» и уничтожаем себя.
/// </summary>
public class AmmoPickup : MonoBehaviour
{
    [Header("Настройки бонуса")]
    // Сколько патронов даёт один бонус.
    [SerializeField, Min(1)] private int _ammoAmount = 10;

    // Скорость вращения бонуса (градусов в секунду) — для красоты.
    [SerializeField] private float _spinSpeed = 90f;

    // Событие для спавнера: он должен знать, что бонус подобран,
    // чтобы через N секунд заспавнить новый на месте.
    public event Action<AmmoPickup> OnCollected;

    // Защита от двойного подбора за один кадр (игрок и пуля, например).
    private bool _isCollected;

    private void Update()
    {
        // Красивое вращение вокруг вертикальной оси.
        transform.Rotate(Vector3.up, _spinSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Срабатывает, когда что-то с Rigidbody входит в триггер бонуса.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (_isCollected) return; // Уже подобран — второй раз не срабатываем.

        // Ищем счётчик патронов у того, кто вошёл в триггер.
        // GetComponentInParent нужен, если коллайдер висит на детёныше дрона.
        AmmoInventory inventory = other.GetComponentInParent<AmmoInventory>();

        // Если это не игрок (нет счётчика) — игнорируем. Пули и враги не подбирают.
        if (inventory == null) return;

        // Пополняем запас игроку.
        inventory.Add(_ammoAmount);

        // Помечаем подобранным и сообщаем спавнеру.
        _isCollected = true;
        OnCollected?.Invoke(this);

        // Бонус исчезает со сцены.
        Destroy(gameObject);
    }
}
