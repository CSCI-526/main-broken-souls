using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AnalyticsDataExporter : MonoBehaviour
{
    [Header("Export Settings")]
    [SerializeField] private string exportFileName = "SurvivalAnalytics";
    [SerializeField] private KeyCode exportKey = KeyCode.E;
    [SerializeField] private KeyCode clearDataKey = KeyCode.C;
    [SerializeField] private KeyCode showStatsKey = KeyCode.S;

    void Update()
    {
        // Press E to export data
        if (Input.GetKeyDown(exportKey))
        {
            ExportToCSV();
        }

        // Press S to show statistics in console
        if (Input.GetKeyDown(showStatsKey))
        {
            ShowStatistics();
        }

        // Press C to clear all data (careful!)
        if (Input.GetKeyDown(clearDataKey) && Input.GetKey(KeyCode.LeftShift))
        {
            ClearAllData();
        }
    }

    public void ExportToCSV()
    {
        if (SurvivalAnalytics.Instance == null)
        {
            Debug.LogError("❌ SurvivalAnalytics not found!");
            return;
        }

        string csv = SurvivalAnalytics.Instance.ExportDataAsCSV();
        
        if (string.IsNullOrEmpty(csv))
        {
            Debug.LogWarning("⚠️ No data to export!");
            return;
        }

        // Save to persistent data path (works on all platforms)
        string filePath = Path.Combine(Application.persistentDataPath, $"{exportFileName}.csv");
        File.WriteAllText(filePath, csv);

        Debug.Log($"✅ Analytics exported to: {filePath}");
        Debug.Log($"📂 Full path: {filePath}");
        
        // Also log the data for easy copy-paste
        Debug.Log("=== CSV DATA ===\n" + csv);
    }

    public void ShowStatistics()
    {
        if (SurvivalAnalytics.Instance == null)
        {
            Debug.LogError("❌ SurvivalAnalytics not found!");
            return;
        }

        List<SessionData> sessions = SurvivalAnalytics.Instance.GetAllSessionData();
        
        if (sessions.Count == 0)
        {
            Debug.LogWarning("⚠️ No session data available!");
            return;
        }

        float avgSurvivalTime = SurvivalAnalytics.Instance.GetAverageSurvivalTime();
        float minTime = SurvivalAnalytics.Instance.GetMinSurvivalTime();
        float maxTime = SurvivalAnalytics.Instance.GetMaxSurvivalTime();
        int totalSessions = SurvivalAnalytics.Instance.GetTotalSessions();

        // Calculate additional statistics
        float totalTime = 0f;
        float totalScore = 0f;
        float avgSpeed = 0f;

        foreach (var session in sessions)
        {
            totalTime += session.survivalTime;
            totalScore += session.finalScore;
            avgSpeed += session.gameSpeed;
        }

        float avgScore = totalScore / sessions.Count;
        avgSpeed = avgSpeed / sessions.Count;

        Debug.Log("╔══════════════════════════════════════╗");
        Debug.Log("║     📊 SURVIVAL ANALYTICS STATS     ║");
        Debug.Log("╚══════════════════════════════════════╝");
        Debug.Log($"Total Sessions Played: {totalSessions}");
        Debug.Log($"Sessions in Memory: {sessions.Count}");
        Debug.Log("");
        Debug.Log("🕒 SURVIVAL TIME:");
        Debug.Log($"  Average: {avgSurvivalTime:F2}s");
        Debug.Log($"  Min: {minTime:F2}s");
        Debug.Log($"  Max: {maxTime:F2}s");
        Debug.Log($"  Total: {totalTime:F2}s");
        Debug.Log("");
        Debug.Log("🎯 SCORE:");
        Debug.Log($"  Average: {avgScore:F0}");
        Debug.Log("");
        Debug.Log("⚡ GAME SPEED:");
        Debug.Log($"  Average: {avgSpeed:F2}");
        Debug.Log("");
        Debug.Log("📈 TREND ANALYSIS:");
        
        // Check if survival time is improving
        if (sessions.Count >= 5)
        {
            float first3Avg = 0f;
            float last3Avg = 0f;
            
            for (int i = 0; i < 3; i++)
            {
                first3Avg += sessions[i].survivalTime;
                last3Avg += sessions[sessions.Count - 3 + i].survivalTime;
            }
            first3Avg /= 3f;
            last3Avg /= 3f;

            float improvement = ((last3Avg - first3Avg) / first3Avg) * 100f;
            
            if (improvement > 5f)
                Debug.Log($"  ✅ Players improving! {improvement:F1}% increase");
            else if (improvement < -5f)
                Debug.Log($"  ⚠️ Players declining! {improvement:F1}% decrease");
            else
                Debug.Log($"  ➡️ Steady performance ({improvement:F1}%)");
        }
        
        Debug.Log("════════════════════════════════════════");
        Debug.Log("Press 'E' to export CSV");
        Debug.Log("Press 'Shift+C' to clear all data");
    }

    public void ClearAllData()
    {
        if (SurvivalAnalytics.Instance == null)
        {
            Debug.LogError("❌ SurvivalAnalytics not found!");
            return;
        }

        Debug.LogWarning("🗑️ Clearing ALL analytics data...");
        SurvivalAnalytics.Instance.ClearAllData();
        Debug.Log("✅ All data cleared!");
    }

    // Public methods for UI buttons
    public void OnExportButtonClicked()
    {
        ExportToCSV();
    }

    public void OnShowStatsButtonClicked()
    {
        ShowStatistics();
    }
}

