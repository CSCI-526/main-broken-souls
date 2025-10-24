using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SurvivalAnalytics : MonoBehaviour
{
    private float startTime;
    private string sessionID;

    [SerializeField] private string webAppUrl = "https://script.google.com/macros/s/AKfycbzUn-eRnyfdKoBG5Q1HY0STVPLPBQorJumFDXqErTekoK9BHzhJRaeb3CI0fJjzIOo8Dw/exec";

    void Start()
    {
        startTime = Time.time;
        sessionID = System.Guid.NewGuid().ToString();
    }

    public void OnPlayerDeath()
    {
        float survivalTime = Time.time - startTime;
        Debug.Log($"Player survived for {survivalTime:F2} seconds");
        StartCoroutine(SendData(survivalTime));
    }

    IEnumerator SendData(float survivalTime)
    {
        var data = new
        {
            sessionID = sessionID,
            survivalTime = survivalTime.ToString("F2")
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest www = new UnityWebRequest(webAppUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log("✅ Analytics sent successfully!");
            else
                Debug.LogError("❌ Error sending analytics: " + www.error);
        }
    }
}
