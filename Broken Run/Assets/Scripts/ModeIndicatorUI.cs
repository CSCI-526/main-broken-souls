using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ModeIndicatorUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Image radial; // optional, can be null

    [Header("Icons")]
    public Sprite reversedIcon;    // ↔ icon
    public Sprite antiGravityIcon; // ↑↓ icon

    Coroutine running;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    public void ShowForecast(ModeType mode, float seconds)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(ForecastRoutine(mode, seconds));
    }

    IEnumerator ForecastRoutine(ModeType mode, float seconds)
    {
        // Content
        if (icon != null)
            icon.sprite = (mode == ModeType.ReversedControls) ? reversedIcon : antiGravityIcon;

        if (label != null)
            label.text = (mode == ModeType.ReversedControls) ? "Reversed controls in…" : "Anti-gravity in…";

        // Visuals
        if (radial != null) radial.fillAmount = 1f;
        if (countdownText != null) countdownText.text = Mathf.CeilToInt(seconds).ToString();

        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, 0.15f));

        float t = seconds;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime; // unscaled so it ignores pause
            if (countdownText != null) countdownText.text = Mathf.Max(0, Mathf.CeilToInt(t)).ToString();
            if (radial != null) radial.fillAmount = Mathf.Clamp01(t / seconds);
            yield return null;
        }

        // Fade out (the Mode Warning Banner + actual timers will take over when mode starts)
        yield return StartCoroutine(Fade(1f, 0f, 0.15f));
        running = null;
    }

    public void HideImmediate()
    {
        if (running != null) StopCoroutine(running);
        running = null;
        if (group != null) group.alpha = 0f;
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        if (group == null) yield break;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        group.alpha = to;
    }
}
