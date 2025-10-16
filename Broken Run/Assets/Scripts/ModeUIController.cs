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

    Coroutine timerRoutine;
    Coroutine bannerRoutine;
    Coroutine forecastRoutine;

    void Awake()
    {
        // Ensure everything is hidden at start
        if (flipTimerText) flipTimerText.gameObject.SetActive(false);
        if (antiGravityTimerText) antiGravityTimerText.gameObject.SetActive(false);
        if (modeIndicatorRoot) modeIndicatorRoot.SetActive(false);
        if (flipWarningBanner) flipWarningBanner.SetActive(false);
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
        bannerRoutine = StartCoroutine(BannerCo());
    }

    /// <summary>Hide all forecast/timers/banners immediately.</summary>
    public void HideAll()
    {
        if (timerRoutine != null) StopCoroutine(timerRoutine);
        if (forecastRoutine != null) StopCoroutine(forecastRoutine);
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);

        if (flipTimerText) flipTimerText.gameObject.SetActive(false);
        if (antiGravityTimerText) antiGravityTimerText.gameObject.SetActive(false);
        if (modeIndicatorRoot) modeIndicatorRoot.SetActive(false);
        if (flipWarningBanner) flipWarningBanner.SetActive(false);

        if (modeIndicator != null) modeIndicator.HideImmediate();
    }

    // ===================== COROUTINES =====================

    // Simple 3s forecast (used only when ModeIndicatorUI is not assigned)
    IEnumerator ForecastCo(ModeType mode, float seconds)
    {
        modeIndicatorRoot.SetActive(true);

        if (indicatorLabel)
            indicatorLabel.text = (mode == ModeType.ReversedControls)
                ? "Incoming: Reversed Controls"
                : "Incoming: Anti-Gravity";

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

        // Enable only the relevant timer and hide the other
        if (flipTimerText) flipTimerText.gameObject.SetActive(mode == ModeType.ReversedControls);
        if (antiGravityTimerText) antiGravityTimerText.gameObject.SetActive(mode == ModeType.AntiGravity);

        const string prefix = "Back to normal in ";
        float t = seconds;

        while (t > 0f)
        {
            // e.g., "Back to normal in 3"
            target.text = prefix + Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime; // scaled time (follows gameplay)
            yield return null;
        }

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
}
