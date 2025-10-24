using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Analytics using Google Forms (CORS-friendly for WebGL)
/// </summary>
public class GoogleFormAnalytics : MonoBehaviour
{
    [Header("Google Form Settings")]
    [Tooltip("Get this from your Google Form URL after '/viewform?'")]
    [SerializeField] private string formId = "YOUR_FORM_ID_HERE";
    
    [Header("Form Field Entry IDs")]
    [Tooltip("Find these in the form's HTML source (search for 'entry.')")]
    [SerializeField] private string sessionNumberEntry = "entry.123456789";
    [SerializeField] private string sessionIdEntry = "entry.987654321";
    [SerializeField] private string survivalTimeEntry = "entry.111111111";
    [SerializeField] private string finalScoreEntry = "entry.222222222";
    [SerializeField] private string gameSpeedEntry = "entry.333333333";
    [SerializeField] private string timestampEntry = "entry.444444444";
    
    [Header("Settings")]
    [SerializeField] private bool enableAnalytics = true;
    
    private string baseUrl => $"https://docs.google.com/forms/d/e/{formId}/formResponse";
    private int sessionNumber = 0;
    private string sessionId;
    private float startTime;

    void Start()
    {
        StartNewSession();
    }

    public void StartNewSession()
    {
        startTime = Time.time;
        sessionId = System.Guid.NewGuid().ToString();
        sessionNumber++;
        Debug.Log($"📊 Started Analytics Session #{sessionNumber}");
    }

    public void OnPlayerDeath(int finalScore, float gameSpeed)
    {
        if (!enableAnalytics) return;

        float survivalTime = Time.time - startTime;
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        StartCoroutine(SubmitToGoogleForm(sessionNumber, sessionId, survivalTime, finalScore, gameSpeed, timestamp));
    }

    private IEnumerator SubmitToGoogleForm(int sessionNum, string sessId, float survTime, int score, float speed, string time)
    {
        // Build form data
        WWWForm form = new WWWForm();
        form.AddField(sessionNumberEntry, sessionNum.ToString());
        form.AddField(sessionIdEntry, sessId);
        form.AddField(survivalTimeEntry, survTime.ToString("F2"));
        form.AddField(finalScoreEntry, score.ToString());
        form.AddField(gameSpeedEntry, speed.ToString("F2"));
        form.AddField(timestampEntry, time);

        // Submit to Google Form
        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl, form))
        {
            yield return www.SendWebRequest();

            // Google Forms returns a redirect, so any result is success
            if (www.result != UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log($"✅ Analytics submitted! Session #{sessionNum}, Survival: {survTime:F2}s, Score: {score}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not submit analytics: {www.error}");
            }
        }
    }
}

