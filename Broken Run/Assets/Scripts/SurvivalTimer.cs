using UnityEngine;
using TMPro; // Only if using TextMeshPro

public class SurvivalTimer : MonoBehaviour
{
    public TMP_Text timerText; // Assign your UI Text
    private float elapsedTime = 0f;
    private bool isRunning = false;

    void Start()
    {
       // ResetTimer(); // Ensure timer starts at 0
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            DisplayTime(elapsedTime);
        }
    }

    public float GetElapsedTime()
{
    return elapsedTime;
}
    void DisplayTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Call this to start/resume the timer
    public void StartTimer()
    {
        isRunning = true;
    }

    // Call this when player dies to pause the timer
    public void StopTimer()
    {
        isRunning = false;
    }

    // Reset the timer back to 0
    public void ResetTimer()
    {
        elapsedTime = 0f;
        DisplayTime(elapsedTime);
        isRunning = false;
    }
}
