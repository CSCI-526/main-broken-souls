using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SurvivalAnalytics : MonoBehaviour
{
    private float startTime;
    private string sessionID;

    [SerializeField] private string googleAppScriptURL = "https://script.google.com/macros/s/AKfycbxZ2AAW2GDeOBv4yG774AJAljz2qhlvNRqEgIVHu9nrh4BRinhoAnz1YuTrotsUldMW/exec ";

    void Start()
    {
        startTime = Time.time;
        sessionID = System.Guid.NewGuid().ToString();
    }

    public void OnPlayerDeath()
    {
        float survivalTime = Time.time - startTime;
        Debug.Log($"Player survived for {survivalTime:F2} seconds");
        StartCoroutine(SendDataToGoogleSheet(survivalTime));
    }

    IEnumerator SendDataToGoogleSheet(float survivalTime)
    {
        var data = new
        {
            sessionID = sessionID,
            survivalTime = survivalTime.ToString("F2")
        };

        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest www = new UnityWebRequest(googleAppScriptURL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Error sending analytics: {www.error}");
            else
                Debug.Log("✅ Analytics sent successfully!");
        }
    }
}
