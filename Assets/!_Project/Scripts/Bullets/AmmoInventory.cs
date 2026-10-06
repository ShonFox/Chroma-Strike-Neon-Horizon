using System;
using UnityEngine;

/// <summary>
/// AmmoInventory — счётчик патронов игрока.
/// Вешается на объект игрока.
///
/// Отдаёт патроны только если их хватает (TryConsume),
/// пополняется подбором бонусов (Add) и сообщает UI о каждом изменении.
/// </summary>
public class AmmoInventory : MonoBehaviour
{
    [Header("Настройки")]
    // Максимальный запас патронов — больше не накопится.
    [SerializeField, Min(1)] private int _maxAmmo = 30;

    // Сколько патронов у игрока в начале игры.
    [SerializeField, Min(0)] private int _startAmmo = 30;

    // Текущий запас. Читается другими скриптами, меняется только через методы.
    public int CurrentAmmo { get; private set; }

    // Максимум — для UI (чтобы нарисовать шкалу).
    public int MaxAmmo => _maxAmmo;

    // ── СОБЫТИЯ ДЛЯ UI ──
    // (текущее, максимальное) — вызывается при каждом изменении.
    public event Action<int, int> OnAmmoChanged;

    // Патроны закончились совсем — можно мигать полоской красным.
    public event Action OnAmmoEmpty;

    private void Start()
    {
        // Не даём стартовому значению превысить максимум.
        CurrentAmmo = Mathf.Min(_startAmmo, _maxAmmo);

        // Сразу оповещаем UI.
        OnAmmoChanged?.Invoke(CurrentAmmo, _maxAmmo);
    }

    /// <summary>
    /// Попытаться потратить патроны. Вернёт false, если их не хватает.
    /// Вызывается из PlayerShooter перед каждым выстрелом.
    /// </summary>
    public bool TryConsume(int amount)
    {
        if (CurrentAmmo < amount)
            return false; // Патронов мало — стрелять нельзя.

        CurrentAmmo -= amount;

        // Сообщаем UI новое значение.
        OnAmmoChanged?.Invoke(CurrentAmmo, _maxAmmo);

        // Совсем пусто — сигнал для UI/звука «нет патронов».
        if (CurrentAmmo == 0)
            OnAmmoEmpty?.Invoke();

        return true;
    }

    /// <summary>
    /// Добавить патроны (подбор бонуса). Лишнее сверх максимума отбрасывается.
    /// Вызывается из AmmoPickup при подборе.
    /// </summary>
    public void Add(int amount)
    {
        CurrentAmmo = Mathf.Min(CurrentAmmo + amount, _maxAmmo);

        OnAmmoChanged?.Invoke(CurrentAmmo, _maxAmmo);
    }
}
