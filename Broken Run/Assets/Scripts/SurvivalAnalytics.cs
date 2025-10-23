using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SurvivalAnalytics : MonoBehaviour
{
    private float startTime;
    [SerializeField] private string googleFormURL = "https://docs.google.com/forms/d/e/1FAIpQLScaIdSiw94tDtioOrK-ytHdhnDLSJliUxjI65wAp-3LmilrtA/FormResponse";
    [SerializeField] private string entryID = "entry.408832640"; // Replace with your form’s entry ID

    private string sessionID;
    void Start()
    {
        // Start timer when the game begins
        startTime = Time.time;
        sessionID = System.Guid.NewGuid().ToString();
    }

    public void OnPlayerDeath()
    {
        // Calculate survival time
        float survivalTime = Time.time - startTime;
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
                Debug.Log("Error sending analytics: " + www.error);
            else
                Debug.Log("Analytics sent successfully!");
        }
    }
}
