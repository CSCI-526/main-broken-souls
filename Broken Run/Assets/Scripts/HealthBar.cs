using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;   // Assign your Slider in Inspector
    public Text healthText;       // Optional: legacy UI.Text, can be null

    // NEW: reference to the Fill image so we can change its color
    [Header("Visuals")]
    [SerializeField] private Image fillImage;

    [Tooltip("Color when health > midThreshold (e.g., 60%)")]
    public Color healthyColor = new Color(0f, 0.8f, 0.2f); // green
    [Tooltip("Color when lowThreshold < health <= midThreshold")]
    public Color midColor = new Color(1f, 0.57f, 0f);      // orange
    [Tooltip("Color when health <= lowThreshold")]
    public Color lowColor = new Color(0.83f, 0f, 0f);      // red

    [Range(0.05f, 0.95f)] public float midThreshold = 0.6f;
    [Range(0.01f, 0.5f)] public float lowThreshold = 0.3f;

    private float maxHealth = 100f;

    void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>(); // Auto-grab if not set
    }

    void Start()
    {
        ResetHealth();
        UpdateFillColor(); // ensure correct color at start
    }

    public void SetHealth(float value)
    {
        if (healthSlider == null) return;

        // Clamp and assign
        float clamped = Mathf.Clamp(value, 0f, maxHealth);
        healthSlider.value = clamped;

        // Optional numeric text
        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(clamped)}";

        // Update bar color based on % remaining
        UpdateFillColor();
    }

    public void ResetHealth()
    {
        SetHealth(maxHealth);
    }

    // --- NEW: color logic for the Fill image ---
    private void UpdateFillColor()
    {
        if (fillImage == null || healthSlider == null) return;

        float pct = (maxHealth <= 0f) ? 0f : (healthSlider.value / maxHealth);

        Color target =
            (pct > midThreshold) ? healthyColor :
            (pct > lowThreshold) ? midColor :
                                   lowColor;

        // Instant set (simple). If you prefer a soft blend, use CrossFadeColor:
        // fillImage.CrossFadeColor(target, 0.15f, true, true);
        fillImage.color = target;
    }
}
