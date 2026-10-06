using UnityEngine;
using TMPro;

/// <summary>
/// AmmoUI — выводит количество патронов на экран в формате «12 / 30».
/// Вешается на текстовый объект Canvas (TextMeshPro - Text).
///
/// Настройка: перетащи объект игрока (с AmmoInventory) в поле _inventory.
/// Скрипт сам подпишется на событие и будет обновлять текст.
/// </summary>
public class AmmoUI : MonoBehaviour
{
    // Ссылка на счётчик патронов игрока.
    [SerializeField] private AmmoInventory _inventory;

    // Текстовый элемент UI.
    [SerializeField] private TextMeshProUGUI _ammoText;

    // Дополнительно: полоска-шкала (Slider), если хочешь визуальную полосу.
    [SerializeField] private UnityEngine.UI.Slider _ammoSlider;

    private void Start()
    {
        if (_inventory == null)
        {
            Debug.LogWarning("AmmoUI: не назначен AmmoInventory!");
            return;
        }

        // Подписываемся на событие изменения патронов.
        _inventory.OnAmmoChanged += UpdateDisplay;

        // Сразу показываем стартовое значение.
        UpdateDisplay(_inventory.CurrentAmmo, _inventory.MaxAmmo);
    }

    private void OnDestroy()
    {
        // Отписка обязательна — иначе после смерти игрока будут ошибки.
        if (_inventory != null)
        {
            _inventory.OnAmmoChanged -= UpdateDisplay;
        }
    }

    /// <summary>
    /// Обновляет текст и полоску UI.
    /// </summary>
    private void UpdateDisplay(int current, int max)
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"{current} / {max}";
        }

        if (_ammoSlider != null)
        {
            // Слайдер работает с float 0..1, переводим в проценты.
            _ammoSlider.maxValue = 1f;
            _ammoSlider.value = max > 0 ? (float)current / max : 0f;
        }
    }
}
