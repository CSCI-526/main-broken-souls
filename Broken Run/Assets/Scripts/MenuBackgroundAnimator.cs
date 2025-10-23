using UnityEngine;
using UnityEngine.UI;

public class MenuBackgroundAnimator : MonoBehaviour
{
    [Header("Background Colors")]
    [Tooltip("Top color of the gradient")]
    public Color topColor = new Color(0.2f, 0.1f, 0.4f, 1f); // Dark purple
    [Tooltip("Bottom color of the gradient")]
    public Color bottomColor = new Color(0.1f, 0.05f, 0.2f, 1f); // Darker purple
    
    [Header("Animation Settings")]
    [Tooltip("Enable color animation")]
    public bool animateColors = true;
    [Tooltip("Speed of color transitions")]
    public float colorChangeSpeed = 0.5f;
    [Tooltip("Intensity of color variation")]
    public float colorVariation = 0.1f;
    
    [Header("Floating Particles (Optional)")]
    [Tooltip("Enable floating particles")]
    public bool enableParticles = true;
    [Tooltip("Particle prefab (simple white circle)")]
    public GameObject particlePrefab;
    [Tooltip("Number of particles to spawn")]
    public int particleCount = 20;
    
    private Image backgroundImage;
    private Color originalTopColor;
    private Color originalBottomColor;
    private float colorTimer = 0f;
    
    private void Start()
    {
        // Get or add Image component
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = gameObject.AddComponent<Image>();
        }
        
        // Store original colors
        originalTopColor = topColor;
        originalBottomColor = bottomColor;
        
        // Set initial gradient
        UpdateGradient();
        
        // Spawn particles if enabled
        if (enableParticles)
        {
            SpawnFloatingParticles();
        }
    }
    
    private void Update()
    {
        if (animateColors)
        {
            AnimateBackground();
        }
    }
    
    private void AnimateBackground()
    {
        colorTimer += Time.deltaTime * colorChangeSpeed;
        
        // Create smooth color variations using sine waves
        float r = Mathf.Sin(colorTimer) * colorVariation;
        float g = Mathf.Sin(colorTimer * 0.8f) * colorVariation;
        float b = Mathf.Cos(colorTimer * 1.2f) * colorVariation;
        
        topColor = originalTopColor + new Color(r, g, b * 2f, 0f);
        bottomColor = originalBottomColor + new Color(r * 0.5f, g * 0.5f, b, 0f);
        
        UpdateGradient();
    }
    
    private void UpdateGradient()
    {
        // Apply the interpolated color (middle point between top and bottom)
        backgroundImage.color = Color.Lerp(topColor, bottomColor, 0.5f);
    }
    
    private void SpawnFloatingParticles()
    {
        // Create a particle container
        GameObject particleContainer = new GameObject("ParticleContainer");
        particleContainer.transform.SetParent(transform);
        
        RectTransform containerRect = particleContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        
        // Spawn particles
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = CreateParticle(particleContainer.transform);
            
            // Add floating animation component
            var floater = particle.AddComponent<FloatingParticle>();
            floater.speed = Random.Range(10f, 30f);
            floater.amplitude = Random.Range(20f, 50f);
            floater.fadeSpeed = Random.Range(0.5f, 1.5f);
        }
    }
    
    private GameObject CreateParticle(Transform parent)
    {
        GameObject particle = new GameObject("Particle");
        particle.transform.SetParent(parent);
        
        // Add Image component
        Image img = particle.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, Random.Range(0.05f, 0.15f));
        
        // Create a simple circle sprite
        img.sprite = Resources.Load<Sprite>("UI/Skin/UISprite") ?? 
                     Resources.Load<Sprite>("Sprites/Circle") ?? 
                     null;
        
        // Random size
        RectTransform rect = particle.GetComponent<RectTransform>();
        float size = Random.Range(5f, 15f);
        rect.sizeDelta = new Vector2(size, size);
        
        // Random starting position
        rect.anchoredPosition = new Vector2(
            Random.Range(-Screen.width / 2f, Screen.width / 2f),
            Random.Range(-Screen.height / 2f, Screen.height / 2f)
        );
        
        return particle;
    }
}

// Helper class for floating particle animation
public class FloatingParticle : MonoBehaviour
{
    [HideInInspector] public float speed = 20f;
    [HideInInspector] public float amplitude = 30f;
    [HideInInspector] public float fadeSpeed = 1f;
    
    private RectTransform rectTransform;
    private Image image;
    private float startX;
    private float time;
    private float fadeTimer;
    private bool fadingIn = true;
    private float targetAlpha;
    
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        startX = rectTransform.anchoredPosition.x;
        time = Random.Range(0f, 100f);
        targetAlpha = Random.Range(0.05f, 0.15f);
    }
    
    private void Update()
    {
        time += Time.deltaTime;
        
        // Float upward
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y += speed * Time.deltaTime;
        
        // Oscillate horizontally
        pos.x = startX + Mathf.Sin(time * 2f) * amplitude;
        
        rectTransform.anchoredPosition = pos;
        
        // Fade in/out animation
        fadeTimer += Time.deltaTime * fadeSpeed;
        if (fadingIn)
        {
            Color c = image.color;
            c.a = Mathf.Lerp(0f, targetAlpha, fadeTimer);
            image.color = c;
            
            if (fadeTimer >= 1f)
            {
                fadingIn = false;
                fadeTimer = 0f;
            }
        }
        else
        {
            Color c = image.color;
            c.a = Mathf.Lerp(targetAlpha, 0f, fadeTimer);
            image.color = c;
            
            if (fadeTimer >= 1f)
            {
                fadingIn = true;
                fadeTimer = 0f;
            }
        }
        
        // Respawn at bottom if goes off top
        if (pos.y > Screen.height / 2f + 50f)
        {
            pos.y = -Screen.height / 2f - 50f;
            pos.x = Random.Range(-Screen.width / 2f, Screen.width / 2f);
            startX = pos.x;
            rectTransform.anchoredPosition = pos;
        }
    }
}

