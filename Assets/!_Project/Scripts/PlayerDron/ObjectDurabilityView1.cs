using UnityEngine;
using UnityEngine.UI;

public class ObjectDurabilityView1 : MonoBehaviour
{
    // Ссылка на компонент UI для отображения прочности.
    [SerializeField] private Slider _timerSlider;

    public void Display(float maxDurability, float currentDurability)
    {
        float progress = currentDurability / maxDurability;

        _timerSlider.value = progress;
    }
}
