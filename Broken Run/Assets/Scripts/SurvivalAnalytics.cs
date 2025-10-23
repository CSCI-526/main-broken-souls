using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SurvivalAnalytics : MonoBehaviour
{
    [SerializeField] private string googleFormURL = "https://docs.google.com/forms/d/e/1FAIpQLScaIdSiw94tDtioOrK-ytHdhnDLSJliUxjI65wAp-3LmilrtA/formResponse";
    [SerializeField] private string entryID = "entry.408832640";

    public SurvivalTimer timer; // 👈 Drag your SurvivalTimer object here in the Inspector

    private string sessionID;

    void Start()
    {
        sessionID = System.Guid.NewGuid().ToString();
        if (timer != null)
            timer.StartTimer(); // start when game begins
    }

    public void OnPlayerDeath()
    {
        if (timer != null)
            timer.StopTimer();

        float survivalTime = timer != null ? timer.GetElapsedTime() : 0f; // use same timer
        Debug.Log("Player survived for " + survivalTime + " seconds");
        StartCoroutine(SendDataToGoogleForm(survivalTime));
    }

    IEnumerator SendDataToGoogleForm(float survivalTime)
    {
        WWWForm form = new WWWForm();
        form.AddField(entryID, survivalTime.ToString("F2"));

        using (UnityWebRequest www = UnityWebRequest.Post(googleFormURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.Log("❌ Error sending analytics: " + www.error);
            else
                Debug.Log("✅ Analytics sent successfully!");
        }
    }
}
