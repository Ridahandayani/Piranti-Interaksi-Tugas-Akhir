using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Health targetHealth;

    void Start()
    {
        if (targetHealth != null)
        {
            slider.maxValue = targetHealth.maxHealth;
            slider.value = targetHealth.GetHealth();
        }
    }

    void Update()
    {
        if (targetHealth != null)
        {
            slider.value = targetHealth.GetHealth();
        }
    }

    // ini tambahan biar bisa dipanggil dari Health.cs
    public void SetHealth(int value)
    {
        slider.value = value;
    }
}
