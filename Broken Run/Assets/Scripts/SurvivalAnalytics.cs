using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class SessionData
{
    public string sessionID;
    public float survivalTime;
    public int finalScore;
    public float gameSpeed;
    public string timestamp;
    public int sessionNumber;
}

[System.Serializable]
public class AnalyticsDataCollection
{
    public List<SessionData> sessions = new List<SessionData>();
}

public class SurvivalAnalytics : MonoBehaviour
{
    public static SurvivalAnalytics Instance { get; private set; }

    [Header("Web Analytics")]
    [SerializeField] private string webAppUrl = "https://script.google.com/macros/s/AKfycbzUn-eRnyfdKoBG5Q1HY0STVPLPBQorJumFDXqErTekoK9BHzhJRaeb3CI0fJjzIOo8Dw/exec";
    [SerializeField] private bool sendToWeb = true;
    
    [Header("Local Data Storage")]
    [SerializeField] private bool saveLocally = true;
    [SerializeField] private int maxSessionsStored = 100;

    private float startTime;
    private string sessionID;
    private int sessionNumber;
    private AnalyticsDataCollection dataCollection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLocalData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartNewSession();
    }

    public void StartNewSession()
    {
        startTime = Time.time;
        sessionID = System.Guid.NewGuid().ToString();
        sessionNumber = GetTotalSessions() + 1;
        Debug.Log($"📊 Started Analytics Session #{sessionNumber}");
    }

    public void OnPlayerDeath(int finalScore, float gameSpeed)
    {
        float survivalTime = Time.time - startTime;
        
        SessionData session = new SessionData
        {
            sessionID = sessionID,
            survivalTime = survivalTime,
            finalScore = finalScore,
            gameSpeed = gameSpeed,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            sessionNumber = sessionNumber
        };

        // Save locally
        if (saveLocally)
        {
            SaveSessionData(session);
        }

        // Send to web
        if (sendToWeb)
        {
            StartCoroutine(SendDataToWeb(session));
        }

        // Log statistics
        LogStatistics(session);
    }

    private void SaveSessionData(SessionData session)
    {
        dataCollection.sessions.Add(session);
        
        // Keep only last N sessions to prevent data bloat
        if (dataCollection.sessions.Count > maxSessionsStored)
        {
            dataCollection.sessions.RemoveAt(0);
        }

        string json = JsonUtility.ToJson(dataCollection);
        PlayerPrefs.SetString("AnalyticsData", json);
        PlayerPrefs.SetInt("TotalSessions", sessionNumber);
        PlayerPrefs.Save();
        
        Debug.Log($"💾 Session #{sessionNumber} saved locally");
    }

    private void LoadLocalData()
    {
        string json = PlayerPrefs.GetString("AnalyticsData", "");
        
        if (string.IsNullOrEmpty(json))
        {
            dataCollection = new AnalyticsDataCollection();
        }
        else
        {
            dataCollection = JsonUtility.FromJson<AnalyticsDataCollection>(json);
        }

        Debug.Log($"📂 Loaded {dataCollection.sessions.Count} previous sessions");
    }

    private void LogStatistics(SessionData currentSession)
    {
        float avgSurvivalTime = GetAverageSurvivalTime();
        float minTime = GetMinSurvivalTime();
        float maxTime = GetMaxSurvivalTime();
        
        Debug.Log("=== 📊 SESSION ANALYTICS ===");
        Debug.Log($"Session #{currentSession.sessionNumber}");
        Debug.Log($"Survival Time: {currentSession.survivalTime:F2}s");
        Debug.Log($"Final Score: {currentSession.finalScore}");
        Debug.Log($"Game Speed: {currentSession.gameSpeed:F2}");
        Debug.Log($"Average Survival Time (All Sessions): {avgSurvivalTime:F2}s");
        Debug.Log($"Min/Max: {minTime:F2}s / {maxTime:F2}s");
        Debug.Log($"Total Sessions: {GetTotalSessions()}");
        Debug.Log("============================");
    }

    IEnumerator SendDataToWeb(SessionData session)
    {
        string json = JsonUtility.ToJson(session);

        UnityWebRequest www = UnityWebRequest.PostWwwForm(webAppUrl, "");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result == UnityWebRequest.Result.Success)
#else
        if (!www.isNetworkError && !www.isHttpError)
#endif
            Debug.Log("✅ Analytics sent to web successfully!");
        else
            Debug.LogWarning("⚠️ Could not send to web: " + www.error);
        
        www.Dispose();
    }

    // === PUBLIC API FOR ANALYTICS ===

    public float GetAverageSurvivalTime()
    {
        if (dataCollection.sessions.Count == 0) return 0f;
        
        float total = 0f;
        foreach (var session in dataCollection.sessions)
        {
            total += session.survivalTime;
        }
        return total / dataCollection.sessions.Count;
    }

    public float GetMinSurvivalTime()
    {
        if (dataCollection.sessions.Count == 0) return 0f;
        
        float min = float.MaxValue;
        foreach (var session in dataCollection.sessions)
        {
            if (session.survivalTime < min) min = session.survivalTime;
        }
        return min;
    }

    public float GetMaxSurvivalTime()
    {
        if (dataCollection.sessions.Count == 0) return 0f;
        
        float max = 0f;
        foreach (var session in dataCollection.sessions)
        {
            if (session.survivalTime > max) max = session.survivalTime;
        }
        return max;
    }

    public int GetTotalSessions()
    {
        return PlayerPrefs.GetInt("TotalSessions", 0);
    }

    public List<SessionData> GetAllSessionData()
    {
        return new List<SessionData>(dataCollection.sessions);
    }

    public string ExportDataAsCSV()
    {
        System.Text.StringBuilder csv = new System.Text.StringBuilder();
        csv.AppendLine("SessionNumber,SessionID,SurvivalTime,FinalScore,GameSpeed,Timestamp");
        
        foreach (var session in dataCollection.sessions)
        {
            csv.AppendLine($"{session.sessionNumber},{session.sessionID},{session.survivalTime:F2},{session.finalScore},{session.gameSpeed:F2},{session.timestamp}");
        }
        
        return csv.ToString();
    }

    public void ClearAllData()
    {
        dataCollection.sessions.Clear();
        PlayerPrefs.DeleteKey("AnalyticsData");
        PlayerPrefs.DeleteKey("TotalSessions");
        PlayerPrefs.Save();
        Debug.Log("🗑️ All analytics data cleared");
    }
}
