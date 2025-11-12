using UnityEngine;
using System.Collections;

public class TutorialPlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;              

    [Header("Tutorial Lock")]
    public bool lockFlipsOnStart = true;
    public bool killRandomRoutineAtStart = true;

    [Header("Forecast Defaults")]
    [Tooltip("if forecast")]
    public bool playForecastOnTrigger = true;

    [Tooltip("forecast second")]
    public float triggerForecastSeconds = 3f;

    private ModeUIController modeUI;             
    private bool tutorialLock = false;           
    private bool effectRunning = false;         


    void Awake()
    {
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (player == null)
        {
            Debug.LogError("[TutorialPlayerController] PlayerController not found");
            enabled = false;
            return;
        }
        modeUI = player.modeUI;
        if (modeUI == null)
        {
            Debug.LogWarning("[TutorialPlayerController] player.modeUI is null");
        }
    }

    void Start()
    {
        tutorialLock = lockFlipsOnStart;
        if (killRandomRoutineAtStart)
            StartCoroutine(DelayKillRandomRoutine());
        ForceClearFlips();
    }

    private IEnumerator DelayKillRandomRoutine()
    {
        yield return null;
        player.StopAllCoroutines(); 
        ForceClearFlips();
        if (modeUI != null) modeUI.HideAll();
    }

    void LateUpdate()
    {
        // lock the effect during tutorial
        if (tutorialLock && !effectRunning)
        {
            ForceClearFlips();
        }
    }

    private void ForceClearFlips()
    {
        player.SendMessage("SetControlFlip", false, SendMessageOptions.DontRequireReceiver);
        player.SendMessage("SetGravityFlip", false, SendMessageOptions.DontRequireReceiver);
        if (modeUI != null) modeUI.HideAll();
    }

    // ===== For Outer Use =====
    public void TriggerEffect(ZoneEffectType effect, float duration)
    {
        TriggerEffectWithForecast(effect, triggerForecastSeconds, duration);
    }

    // ===== For Outer Use =====
    public void TriggerEffectWithForecast(ZoneEffectType effect, float forecastSeconds, float duration)
    {
        if (effectRunning)
        {
            // clear the last effect
            StopAllCoroutines();
            effectRunning = false;
            tutorialLock = true;
            ForceClearFlips();
            if (modeUI != null) modeUI.HideAll();
        }

        StartCoroutine(EffectRoutine(effect, forecastSeconds, duration));
    }

    private IEnumerator EffectRoutine(ZoneEffectType effect, float forecastSeconds, float duration)
    {
        effectRunning = true;
        tutorialLock = false;
        ModeType mode = (effect == ZoneEffectType.ReversedControls)
            ? ModeType.ReversedControls
            : ModeType.AntiGravity;

        if (player.image != null)
        {
            Debug.Log("set color");
            if (mode == ModeType.ReversedControls)
                player.image.color = player.flippedColor;
            else
                player.image.color = player.gravityFlippedColor;
        }else{
            Debug.LogWarning("image is null");
        }

        // 1) Forecast
        if (playForecastOnTrigger && forecastSeconds > 0f)
        {
            if (modeUI != null) modeUI.ShowForecast(mode, forecastSeconds);
            yield return new WaitForSeconds(forecastSeconds);
        }

        // 2) active efftect + UI
        if (mode == ModeType.ReversedControls)
            player.SendMessage("SetControlFlip", true, SendMessageOptions.DontRequireReceiver);
        else
            player.SendMessage("SetGravityFlip", true, SendMessageOptions.DontRequireReceiver);

        if (modeUI != null)
        {
            modeUI.PlayWarningBanner(mode);
            modeUI.StartModeTimer(mode, duration);
        }

        yield return new WaitForSeconds(duration);

        // 3) clear and lock again
        ForceClearFlips();
        tutorialLock = true;
        effectRunning = false;
    }
}
