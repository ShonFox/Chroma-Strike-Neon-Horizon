using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EnergyUI — выводит полоску энергии на экран.
/// Вешается на пустой объект. В инспекторе нужно указать Slider,
/// который будет отображать энергию.
/// </summary>
public class EnergyUI : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  НАСТРОЙКИ В ИНСПЕКТОРЕ
    // ─────────────────────────────────────────────

    // Ссылка на Slider, который отображает энергию.
    // Создайте UI → Slider и перетащите его сюда.
    [SerializeField] private Slider _energySlider;

    // Ссылка на EnergySystem. Перетащите объект с EnergySystem.
    [SerializeField] private EnergySystem _energySystem;

    private void Start()
    {
        // Подписываемся на событие изменения энергии.
        // Каждый раз, когда энергия меняется, вызывается UpdateSlider.
        _energySystem.OnEnergyChanged += UpdateSlider;

        // Сразу устанавливаем максимальное значение слайдера.
        // EnergySystem пришлёт (current, max) при первом вызове.
    }

    /// <summary>
    /// Обновляет слайдер. Вызывается из EnergySystem через событие.
    /// </summary>
    private void UpdateSlider(float current, float max)
    {
        // Устанавливаем диапазон слайдера.
        _energySlider.maxValue = max;
        _energySlider.value = current;
    }

    private void OnDestroy()
    {
        // Отписываемся, чтобы не было утечки памяти.
        if (_energySystem != null)
            _energySystem.OnEnergyChanged -= UpdateSlider;
    }
}
