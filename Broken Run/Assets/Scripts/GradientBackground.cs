using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class GradientBackground : MonoBehaviour
{
    [Header("Gradient Colors")]
    public Color topLeftColor = new Color(0.15f, 0.05f, 0.3f, 1f);      // Deep purple
    public Color topRightColor = new Color(0.3f, 0.1f, 0.4f, 1f);       // Purple
    public Color bottomLeftColor = new Color(0.05f, 0.05f, 0.15f, 1f);  // Dark blue
    public Color bottomRightColor = new Color(0.1f, 0.1f, 0.25f, 1f);   // Navy
    
    [Header("Animation")]
    public bool animateGradient = true;
    public float animationSpeed = 0.3f;
    public float colorShiftAmount = 0.15f;
    
    private RawImage rawImage;
    private Texture2D gradientTexture;
    private int textureWidth = 256;
    private int textureHeight = 256;
    private float animationTime = 0f;
    
    // Store original colors
    private Color origTopLeft, origTopRight, origBottomLeft, origBottomRight;
    
    private void Start()
    {
        rawImage = GetComponent<RawImage>();
        
        // Store original colors
        origTopLeft = topLeftColor;
        origTopRight = topRightColor;
        origBottomLeft = bottomLeftColor;
        origBottomRight = bottomRightColor;
        
        CreateGradientTexture();
        UpdateGradient();
    }
    
    private void Update()
    {
        if (animateGradient)
        {
            animationTime += Time.deltaTime * animationSpeed;
            AnimateColors();
            UpdateGradient();
        }
    }
    
    private void AnimateColors()
    {
        // Create smooth color shifts using sine waves at different frequencies
        float shift1 = Mathf.Sin(animationTime) * colorShiftAmount;
        float shift2 = Mathf.Sin(animationTime * 1.3f) * colorShiftAmount;
        float shift3 = Mathf.Cos(animationTime * 0.8f) * colorShiftAmount;
        float shift4 = Mathf.Cos(animationTime * 1.1f) * colorShiftAmount;
        
        topLeftColor = origTopLeft + new Color(shift1, shift2, shift3 * 0.5f, 0f);
        topRightColor = origTopRight + new Color(shift2, shift1, shift4 * 0.5f, 0f);
        bottomLeftColor = origBottomLeft + new Color(shift3, shift4, shift1 * 0.5f, 0f);
        bottomRightColor = origBottomRight + new Color(shift4, shift3, shift2 * 0.5f, 0f);
    }
    
    private void CreateGradientTexture()
    {
        gradientTexture = new Texture2D(textureWidth, textureHeight);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        gradientTexture.filterMode = FilterMode.Bilinear;
        rawImage.texture = gradientTexture;
    }
    
    private void UpdateGradient()
    {
        if (gradientTexture == null) return;
        
        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                float xPercent = (float)x / textureWidth;
                float yPercent = (float)y / textureHeight;
                
                // Interpolate between corners
                Color topColor = Color.Lerp(topLeftColor, topRightColor, xPercent);
                Color bottomColor = Color.Lerp(bottomLeftColor, bottomRightColor, xPercent);
                Color finalColor = Color.Lerp(bottomColor, topColor, yPercent);
                
                gradientTexture.SetPixel(x, y, finalColor);
            }
        }
        
        gradientTexture.Apply();
    }
    
    private void OnDestroy()
    {
        if (gradientTexture != null)
        {
            Destroy(gradientTexture);
        }
    }
}

