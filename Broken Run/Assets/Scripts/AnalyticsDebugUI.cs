using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Optional UI to display real-time analytics in-game
/// </summary>
public class AnalyticsDebugUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    private bool isVisible = false;

    void Start()
    {
        if (debugPanel != null)
            debugPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDebugPanel();
        }

        if (isVisible && statsText != null)
        {
            UpdateStatsDisplay();
        }
    }

    public void ToggleDebugPanel()
    {
        isVisible = !isVisible;
        if (debugPanel != null)
            debugPanel.SetActive(isVisible);
    }

    private void UpdateStatsDisplay()
    {
        if (SurvivalAnalytics.Instance == null)
        {
            statsText.text = "Analytics Not Available";
            return;
        }

        float avgTime = SurvivalAnalytics.Instance.GetAverageSurvivalTime();
        float minTime = SurvivalAnalytics.Instance.GetMinSurvivalTime();
        float maxTime = SurvivalAnalytics.Instance.GetMaxSurvivalTime();
        int totalSessions = SurvivalAnalytics.Instance.GetTotalSessions();

        statsText.text = $"<b>📊 Analytics Debug</b>\n\n" +
                        $"Total Sessions: {totalSessions}\n" +
                        $"Avg Survival: {avgTime:F2}s\n" +
                        $"Min: {minTime:F2}s\n" +
                        $"Max: {maxTime:F2}s\n\n" +
                        $"<size=12>Press F1 to toggle\n" +
                        $"Press E to export CSV\n" +
                        $"Press S for detailed stats</size>";
    }
}

