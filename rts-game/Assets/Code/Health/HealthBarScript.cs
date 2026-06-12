using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    public void SetHealth(float health, float maxHealth)
    {
        if (slider == null) return;

        slider.maxValue = maxHealth;
        slider.value = health;
        
        slider.gameObject.SetActive(health < maxHealth);

        if (fill != null && gradient != null)
        {
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }
}