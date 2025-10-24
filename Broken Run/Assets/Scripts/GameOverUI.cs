using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI leaderboardText;

    [Header("Other UI")]
    public ModeUIController modeUI;   // <-- assign in Inspector
    // new
    public GameObject shieldTipUI;
    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        // 1) fade out the �Back to normal in �� timers / banners
        if (modeUI != null) modeUI.HideAllWithFade(0.25f);
        // new
        if (shieldTipUI != null) shieldTipUI.SetActive(false);

        // 2) now show the game-over UI
        gameOverPanel.SetActive(true);

        int finalScore = ScoreManager.Instance.GetFinalScore();
        finalScoreText.text = $"Final Score: {finalScore}";

        List<int> topScores = ScoreManager.Instance.GetTopScores();

        string leaderboard = "Leaderboard:\n";
        for (int i = 0; i < topScores.Count; i++)
            leaderboard += $"{i + 1}. {topScores[i]}\n";
        leaderboardText.text = leaderboard;

        // 📊 Send Analytics Data
        SendAnalytics(finalScore);
    }

    private void SendAnalytics(int finalScore)
    {
        // Get current game speed from EndlessGround
        float gameSpeed = 5f; // default
#if UNITY_2022_2_OR_NEWER
        EndlessGround ground = FindFirstObjectByType<EndlessGround>();
#else
        EndlessGround ground = FindObjectOfType<EndlessGround>();
#endif
        if (ground != null)
        {
            gameSpeed = ground.scrollSpeed;
        }

        // Try Google Forms analytics (CORS-friendly)
        GoogleFormAnalytics formAnalytics = FindObjectOfType<GoogleFormAnalytics>();
        if (formAnalytics != null)
        {
            formAnalytics.OnPlayerDeath(finalScore, gameSpeed);
        }
        // Fallback to regular analytics
        else if (SurvivalAnalytics.Instance != null)
        {
            SurvivalAnalytics.Instance.OnPlayerDeath(finalScore, gameSpeed);
        }
        else
        {
            Debug.LogWarning("⚠️ No analytics system found in scene!");
        }
    }

    public void RestartGame()
    {
        // Reset time scale BEFORE loading scene
        Time.timeScale = 1f;

        // Start new analytics session for next game
        GoogleFormAnalytics formAnalytics = FindObjectOfType<GoogleFormAnalytics>();
        if (formAnalytics != null)
        {
            formAnalytics.StartNewSession();
        }
        else if (SurvivalAnalytics.Instance != null)
        {
            SurvivalAnalytics.Instance.StartNewSession();
        }

        // Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}
