using UnityEngine;
using TMPro;
using System.Collections;

public class ModeUIController : MonoBehaviour
{
    [Header("Runtime Timers")]
    [SerializeField] private TextMeshProUGUI flipTimerText;        // FlipTimerText (TMP)
    [SerializeField] private TextMeshProUGUI antiGravityTimerText; // AntiGravityTimerText (TMP)

    [Header("Warning Banner")]
    [SerializeField] private GameObject flipWarningBanner;         // Panel root
    [SerializeField] private TextMeshProUGUI bannerLabel;          // Text inside
    [SerializeField] private CanvasGroup bannerCanvasGroup;        // On the same panel
    [SerializeField] private float bannerSlidePixels = 120f;       // slide distance
    [SerializeField] private float bannerAnimTime = 0.35f;         // slide-in time
    [SerializeField] private float bannerHold = 0.65f;             // stay visible
    [SerializeField] private float bannerFadeTime = 0.35f;         // fade-out time

    [Header("Forecast (3s) — Simple Mode")]
    [SerializeField] private GameObject modeIndicatorRoot;         // optional: simple panel
    [SerializeField] private TextMeshProUGUI indicatorLabel;       // "Incoming: ..."
    [SerializeField] private TextMeshProUGUI indicatorCountdown;   // "3", "2", "1"

    [Header("Forecast (3s) — Advanced")]
    [SerializeField] private ModeIndicatorUI modeIndicator;        // optional: advanced script

    [Header("Root Fade (for Game Over)")]
    [SerializeField] private CanvasGroup rootGroup;                // CanvasGroup on the ModeUI root

    Coroutine timerRoutine;
    Coroutine bannerRoutine;
    Coroutine forecastRoutine;
    Coroutine fadeRoutine;

    void Awake()
    {
        // Ensure everything is hidden at start
        if (flipTimerText) flipTimerText.gameObject.SetActive(false);
        if (antiGravityTimerText) antiGravityTimerText.gameObject.SetActive(false);
        if (modeIndicatorRoot) modeIndicatorRoot.SetActive(false);
        if (flipWarningBanner) flipWarningBanner.SetActive(false);

        if (rootGroup == null) rootGroup = GetComponent<CanvasGroup>();
        
        // Enhance visibility of all text elements
        EnhanceTextVisibility();
    }
    
    /// <summary>
    /// Enhances visibility of all mode warning texts (bold, white, larger)
    /// </summary>
    private void EnhanceTextVisibility()
    {
        // Enhance timer texts
        EnhanceTextElement(flipTimerText);
        EnhanceTextElement(antiGravityTimerText);
        
        // Enhance banner label
        EnhanceTextElement(bannerLabel);
        
        // Enhance forecast indicator texts
        EnhanceTextElement(indicatorLabel);
        EnhanceTextElement(indicatorCountdown);
    }
    
    /// <summary>
    /// Helper method to enhance a single text element
    /// </summary>
    private void EnhanceTextElement(TextMeshProUGUI textElement)
    {
        if (textElement == null) return;
        
        // Set text color to bright white
        textElement.color = Color.white;
        
        // Make text bold
        textElement.fontStyle = FontStyles.Bold;
        
        // Set font size based on text type
        // Forecast texts (indicatorLabel, indicatorCountdown) get smaller size
        bool isForecastText = (textElement == indicatorLabel || textElement == indicatorCountdown);
        
        if (isForecastText)
        {
            // Reduced size for forecast texts
            if (textElement.fontSize < 31)
            {
                textElement.fontSize = 31;
            }
            else if (textElement.fontSize > 31)
            {
                textElement.fontSize = 31;
            }
        }
        else
        {
            // Other texts (timers, banners) keep larger size
            if (textElement.fontSize < 36)
            {
                textElement.fontSize = 36;
            }
        }
        
        // Force update
        textElement.SetAllDirty();
    }

    // ===================== PUBLIC API =====================

    /// <summary>Show a 3-second forecast for the next mode.</summary>
    public void ShowForecast(ModeType mode, float seconds)
    {
        // If an advanced ModeIndicatorUI is provided, use that.
        if (modeIndicator != null)
        {
            modeIndicator.ShowForecast(mode, seconds);
            return;
        }

        // Otherwise use the simple label + numeric countdown panel.
        if (modeIndicatorRoot == null) return;
        if (forecastRoutine != null) StopCoroutine(forecastRoutine);
        forecastRoutine = StartCoroutine(ForecastCo(mode, seconds));
    }

    /// <summary>Start an on-screen timer while the mode is active.</summary>
    public void StartModeTimer(ModeType mode, float seconds)
    {
        if (timerRoutine != null) StopCoroutine(timerRoutine);
        timerRoutine = StartCoroutine(TimerCo(mode, seconds));
    }

    /// <summary>Slide-in red banner when a mode activates.</summary>
    public void PlayWarningBanner(ModeType activeMode)
    {
        if (flipWarningBanner == null || bannerCanvasGroup == null || bannerLabel == null) return;

        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerLabel.text = (activeMode == ModeType.ReversedControls)
            ? "⚠ Controls Reversed!"
            : "⚠ Anti-Gravity Active!";
        EnhanceTextElement(bannerLabel); // Ensure visibility
        bannerRoutine = StartCoroutine(BannerCo());
    }

    /// <summary>Hide all forecast/timers/banners immediately (no fade).</summary>
    public void HideAll()
    {
        if (timerRoutine != null) { StopCoroutine(timerRoutine); timerRoutine = null; }
        if (forecastRoutine != null) { StopCoroutine(forecastRoutine); forecastRoutine = null; }
        if (bannerRoutine != null) { StopCoroutine(bannerRoutine); bannerRoutine = null; }

        if (flipTimerText) flipTimerText.gameObject.SetActive(false);
        if (antiGravityTimerText) antiGravityTimerText.gameObject.SetActive(false);
        if (modeIndicatorRoot) modeIndicatorRoot.SetActive(false);
        if (flipWarningBanner) flipWarningBanner.SetActive(false);

        if (modeIndicator != null) modeIndicator.HideImmediate();
    }

    /// <summary>
    /// Fade the whole ModeUI out, then hide & reset everything.
    /// Call this on Game Over.
    /// </summary>
    public void HideAllWithFade(float duration = 0.25f)
    {
        // Stop any running animations/timers first
        if (timerRoutine != null) { StopCoroutine(timerRoutine); timerRoutine = null; }
        if (forecastRoutine != null) { StopCoroutine(forecastRoutine); forecastRoutine = null; }
        if (bannerRoutine != null) { StopCoroutine(bannerRoutine); bannerRoutine = null; }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutAndHide(duration));
    }

    // ===================== COROUTINES =====================

    // Simple 3s forecast (used only when ModeIndicatorUI is not assigned)
    IEnumerator ForecastCo(ModeType mode, float seconds)
    {
        modeIndicatorRoot.SetActive(true);

        if (indicatorLabel)
        {
            indicatorLabel.text = (mode == ModeType.ReversedControls)
                ? "Incoming: Reversed Controls"
                : "Incoming: Anti-Gravity";
            EnhanceTextElement(indicatorLabel); // Ensure visibility
        }
        
        if (indicatorCountdown)
        {
            EnhanceTextElement(indicatorCountdown); // Ensure visibility
        }

        float t = seconds;
        while (t > 0f)
        {
            if (indicatorCountdown) indicatorCountdown.text = Mathf.Ceil(t).ToString();
            t -= Time.unscaledDeltaTime; // unscaled so it works if game is paused
            yield return null;
        }

        modeIndicatorRoot.SetActive(false);
        forecastRoutine = null;
    }

    // Active mode timer (text countdown in the corner)
    IEnumerator TimerCo(ModeType mode, float seconds)
    {
        TextMeshProUGUI target = (mode == ModeType.ReversedControls) ? flipTimerText : antiGravityTimerText;
        if (target == null) yield break;

        const string prefix = "Back to normal in ";
        float t = seconds;
        const float showLastSeconds = 5f; // Only show timer for LAST 5 seconds

        // Hide timer initially
        if (flipTimerText) flipTimerText.gameObject.SetActive(false);
        if (antiGravityTimerText) antiGravityTimerText.gameObject.SetActive(false);

        // Wait silently until last 5 seconds
        while (t > showLastSeconds)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        // Now show timer for last 5 seconds
        target.gameObject.SetActive(true);
        EnhanceTextElement(target); // Ensure visibility when shown
        
        while (t > 0f)
        {
            target.text = prefix + Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime; // scaled time (follows gameplay)
            yield return null;
        }

        // Hide when done
        target.gameObject.SetActive(false);
        timerRoutine = null;
    }

    // Slide/fade banner animation
    IEnumerator BannerCo()
    {
        flipWarningBanner.SetActive(true);

        RectTransform rt = flipWarningBanner.transform as RectTransform;
        Vector2 basePos = rt.anchoredPosition;
        Vector2 from = basePos + Vector2.up * bannerSlidePixels;
        Vector2 to = basePos;

        bannerCanvasGroup.alpha = 0f;
        rt.anchoredPosition = from;

        // Slide-in + fade-in
        float t = 0f;
        while (t < bannerAnimTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / bannerAnimTime);
            rt.anchoredPosition = Vector2.Lerp(from, to, a);
            bannerCanvasGroup.alpha = a;
            yield return null;
        }
        bannerCanvasGroup.alpha = 1f;
        rt.anchoredPosition = to;

        // Hold
        float hold = 0f;
        while (hold < bannerHold)
        {
            hold += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade-out
        t = 0f;
        while (t < bannerFadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = 1f - Mathf.Clamp01(t / bannerFadeTime);
            bannerCanvasGroup.alpha = a;
            yield return null;
        }

        flipWarningBanner.SetActive(false);
        bannerRoutine = null;
    }

    // Fade out root, then fully hide/reset UI
    IEnumerator FadeOutAndHide(float dur)
    {
        if (rootGroup != null)
        {
            float start = rootGroup.alpha;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;             // fade works even if game is paused
                rootGroup.alpha = Mathf.Lerp(start, 0f, t / dur);
                yield return null;
            }
            rootGroup.alpha = 0f;
        }

        HideAll();                 // fully disable everything
        fadeRoutine = null;

        // Optional: restore alpha so next time it shows immediately
        if (rootGroup != null) rootGroup.alpha = 1f;
    }
}
