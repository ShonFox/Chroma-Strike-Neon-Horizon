using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Вывод энергии на экран через UI Slider (полоска) и текст.
///
/// Как настроить в Unity:
///   1. Создайте Canvas (если ещё нет): GameObject → UI → Canvas.
///   2. Добавьте Slider: GameObject → UI → Slider.
///      Он автоматически создаст полоску с «Fill» внутри.
///   3. (Опц.) Добавьте Text рядом с полоской — для числового значения.
///   4. Повесьте EnergyUI на любой объект (можно на сам Slider или на Canvas).
///   5. В инспекторе перетащите:
///        EnergySlider  → ваш Slider
///        EnergyText    → ваш Text (если есть)
///        EnergySystem  → объект, на котором висит EnergySystem
///   6. Запустите сцену — полоска начнёт реагировать на энергию.
///
/// Альтернатива без Slider: можно использовать Image с типом Fill,
/// но Slider удобнее — он уже всё умеет.
/// </summary>
public class EnergyUI : MonoBehaviour
{
    // ─── Ссылки на UI-элементы ─────────────────────────────────────

    [Header("UI элементы")]

    // Полоска прогресса (Slider). Value меняется от 0 до 1.
    [SerializeField] private Slider _energySlider;

    // Текст для отображения энергии числом, например «75 / 100».
    // Может быть null — тогда текст не показывается.
    [SerializeField] private Text _energyText;

    // ─── Ссылка на систему энергии ─────────────────────────────────

    [Header("Источники данных")]

    // Объект, на котором висит EnergySystem.
    [SerializeField] private EnergySystem _energySystem;

    // ─── Цвета (опционально) ────────────────────────────────────────

    [Header("Цвет полоски (опц.)")]

    // Картинка-заливка полоски (Fill), чтобы менять цвет в зависимости от энергии.
    [SerializeField] private Image _fillImage;

    // Цвет при полном запасе.
    [SerializeField] private Color _fullColor = Color.green;

    // Цвет при низком запасе.
    [SerializeField] private Color _lowColor = Color.red;

    // Порог, ниже которого цвет меняется на «низкий» (0..1).
    [SerializeField, Range(0f, 1f)] private float _lowThreshold = 0.3f;

    private void Start()
    {
        // Проверки на то, что всё назначено.
        if (_energySystem == null)
        {
            Debug.LogWarning("[EnergyUI] EnergySystem не назначен! UI не будет работать.");
            return;
        }

        if (_energySlider != null)
        {
            // Настраиваем Slider: минимум 0, максимум 1 (нормализованное значение).
            _energySlider.minValue = 0f;
            _energySlider.maxValue = 1f;
            // Отключаем взаимодействие игрока с полоской (она только для показа).
            _energySlider.interactable = false;
        }

        // Подписываемся на событие изменения энергии в EnergySystem.
        // Каждый раз, когда энергия меняется, вызывается UpdateUI.
        _energySystem.OnEnergyChanged += UpdateUI;

        // Принудительно обновляем UI при старте.
        UpdateUI(_energySystem.CurrentEnergy, _energySystem.MaxEnergy);
    }

    private void OnDestroy()
    {
        // Важно отписаться, чтобы не было утечки памяти и ошибок.
        if (_energySystem != null)
            _energySystem.OnEnergyChanged -= UpdateUI;
    }

    /// <summary>
    /// Обновляет полоску и текст на экране.
    /// Вызывается автоматически при каждом изменении энергии (через событие).
    /// </summary>
    /// <param name="current">Текущее значение энергии.</param>
    /// <param name="max">Максимальное значение энергии.</param>
    private void UpdateUI(float current, float max)
    {
        // Нормализуем энергию в 0..1 для Slider.
        float normalized = max > 0f ? current / max : 0f;

        // Обновляем полоску.
        if (_energySlider != null)
        {
            _energySlider.value = normalized;
        }

        // Обновляем текст (если назначен).
        if (_energyText != null)
        {
            // Показываем целые числа для красоты: «75 / 100».
            _energyText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }

        // Меняем цвет заливки в зависимости от уровня энергии.
        if (_fillImage != null)
        {
            // Lerp между красным (низко) и зелёным (полно).
            // Mathf.InverseLerp: 0 при _lowThreshold → 0, 1 при 1.0 → 1.
            float colorT = Mathf.InverseLerp(_lowThreshold, 1f, normalized);
            _fillImage.color = Color.Lerp(_lowColor, _fullColor, colorT);
        }
    }
}
