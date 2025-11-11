using UnityEngine;

/// <summary>
/// Simple animator for power-up icons to make them more noticeable.
/// Attach this to the icon sprite (child of power-up).
/// </summary>
public class PowerUpIconAnimator : MonoBehaviour
{
    [Header("Animation Type")]
    public bool pulse = true;
    public bool rotate = false;
    public bool bounce = false;
    
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f; // Scale between 1.0 and 1.2
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 50f; // Degrees per second
    
    [Header("Bounce Settings")]
    public float bounceSpeed = 3f;
    public float bounceHeight = 0.3f;
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float time;
    
    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        time = 0f;
    }
    
    void Update()
    {
        time += Time.deltaTime;
        
        // Pulse animation (scale in/out)
        if (pulse)
        {
            float scale = 1f + Mathf.Sin(time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * scale;
        }
        
        // Rotation animation
        if (rotate)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        // Bounce animation (up/down)
        if (bounce)
        {
            float yOffset = Mathf.Sin(time * bounceSpeed) * bounceHeight;
            transform.localPosition = originalPosition + new Vector3(0, yOffset, 0);
        }
    }
}

