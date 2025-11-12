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
    // public GameObject shieldTipUI; // deleted
    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        // 1) fade out the "Back to normal in ..." timers / banners
        if (modeUI != null) modeUI.HideAllWithFade(0.25f);
        // new
        //if (shieldTipUI != null) shieldTipUI.SetActive(false);// deleted

        // 2) now show the game-over UI FIRST
        gameOverPanel.SetActive(true);

        int finalScore = ScoreManager.Instance.GetFinalScore();
        
        Debug.Log($"[GameOverUI] Showing final score: {finalScore}");
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
            finalScoreText.gameObject.SetActive(true); // Make sure it's visible
            Debug.Log($"[GameOverUI] Final score text updated and visible");
        }
        else
        {
            Debug.LogError("[GameOverUI] finalScoreText is NULL! Please assign it in Inspector.");
        }

        List<int> topScores = ScoreManager.Instance.GetTopScores();

        string leaderboard = "Leaderboard:\n";
        for (int i = 0; i < topScores.Count; i++)
            leaderboard += $"{i + 1}. {topScores[i]}\n";
        
        if (leaderboardText != null)
        {
            leaderboardText.text = leaderboard;
        }

        // 3) Hide the live score at top of screen AFTER setting game over text
        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
        {
            ScoreManager.Instance.scoreText.gameObject.SetActive(false);
        }

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

        // Calculate survival time (from GoogleFormAnalytics/SurvivalAnalytics startTime)
        float survivalTime = 0f;
        GoogleFormAnalytics formAnalytics = FindObjectOfType<GoogleFormAnalytics>();
        if (formAnalytics != null)
        {
            // GoogleFormAnalytics calculates it internally
            formAnalytics.OnPlayerDeath(finalScore, gameSpeed);
        }
        else if (SurvivalAnalytics.Instance != null)
        {
            SurvivalAnalytics.Instance.OnPlayerDeath(finalScore, gameSpeed);
        }
        else
        {
            Debug.LogWarning("⚠️ No old analytics system found in scene!");
        }

        // === NEW: Enhanced Analytics with 4 improved metrics ===
        if (EnhancedAnalytics.Instance != null)
        {
            // Get survival time from Time.time (same as old analytics)
            // Note: EnhancedAnalytics tracks its own startTime in StartNewSession
            // For now we'll calculate from ScoreManager if available
            survivalTime = Time.timeSinceLevelLoad; // Approximation
            
            // Get cause of death from tracker
            string causeOfDeath = CauseOfDeathTracker.LastCauseOfDeath;
            
            Debug.Log($"📊 Sending Enhanced Analytics: Score={finalScore}, Time={survivalTime:F2}s, Speed={gameSpeed:F2}, Cause={causeOfDeath}");
            
            EnhancedAnalytics.Instance.OnPlayerDeath(
                causeOfDeath,
                survivalTime,
                finalScore,
                gameSpeed
            );
        }
        else
        {
            Debug.LogWarning("⚠️ EnhancedAnalytics not found! Did you add it to the scene?");
        }
    }

    public void RestartGame()
    {
        // Reset time scale BEFORE loading scene
        Time.timeScale = 1f;

        // Show the score text again for next game
        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
        {
            ScoreManager.Instance.scoreText.gameObject.SetActive(true);
        }

        // Reset cause of death tracker for new game
        CauseOfDeathTracker.Reset();

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

        // Start new enhanced analytics session
        if (EnhancedAnalytics.Instance != null)
        {
            EnhancedAnalytics.Instance.StartNewGameSession();
        }

        // Reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

}
