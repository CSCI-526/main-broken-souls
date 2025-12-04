using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI leaderboardText;

    [Header("Other UI")]
    public ModeUIController modeUI;

    [Header("Music")]
    public MusicManager musicManager; // assign in Inspector

    [Header("Game Over Sound")]
    public AudioSource sfxSource;          // a separate AudioSource for SFX
    public AudioClip gameOverClip;  
    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        // 2) stop music
        if (musicManager != null) musicManager.StopMusic();

        if (sfxSource != null && gameOverClip != null)
            sfxSource.PlayOneShot(gameOverClip);

        if (modeUI != null) modeUI.HideAllWithFade(0.25f);

        gameOverPanel.SetActive(true);

        int finalScore = ScoreManager.Instance.GetFinalScore();
        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {finalScore}";

        List<int> topScores = ScoreManager.Instance.GetTopScores();
        string leaderboard = "Leaderboard:\n";
        for (int i = 0; i < topScores.Count; i++)
            leaderboard += $"{i + 1}. {topScores[i]}\n";

        if (leaderboardText != null)
            leaderboardText.text = leaderboard;

        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
            ScoreManager.Instance.scoreText.gameObject.SetActive(false);

        // Send analytics data
        SendAnalytics(finalScore);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (musicManager != null) musicManager.PlayMusic();

        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
            ScoreManager.Instance.scoreText.gameObject.SetActive(true);

        // Reset cause of death tracker for new game
        CauseOfDeathTracker.Reset();

        // Start new analytics session
        if (EnhancedAnalytics.Instance != null)
        {
            EnhancedAnalytics.Instance.StartNewGameSession();
        }

        // Start new session for old analytics systems
        GoogleFormAnalytics formAnalytics = FindObjectOfType<GoogleFormAnalytics>();
        if (formAnalytics != null)
        {
            formAnalytics.StartNewSession();
        }
        else if (SurvivalAnalytics.Instance != null)
        {
            SurvivalAnalytics.Instance.StartNewSession();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SendAnalytics(int finalScore)
    {
        // Get game speed
        float gameSpeed = 5f;
        EndlessGround ground = FindObjectOfType<EndlessGround>();
        if (ground != null)
        {
            gameSpeed = ground.scrollSpeed;
        }

        // Get survival time
        float survivalTime = Time.timeSinceLevelLoad;

        // Get cause of death
        string causeOfDeath = CauseOfDeathTracker.LastCauseOfDeath;

        // Get gravity and controls flip states
        bool gravityFlipped = false;
        bool controlsReversed = false;
        
        // Try to find PlayerController to check flip states
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            gravityFlipped = playerController.IsGravityFlipped();
            controlsReversed = playerController.IsControlsFlipped();
        }
        else
        {
            // Fallback: try NewPlayerController
            NewPlayerController newPlayerController = FindObjectOfType<NewPlayerController>();
            if (newPlayerController != null)
            {
                gravityFlipped = newPlayerController.IsGravityFlipped();
                controlsReversed = newPlayerController.IsControlsFlipped();
            }
        }

        // Send to old analytics systems (for backwards compatibility)
        GoogleFormAnalytics formAnalytics = FindObjectOfType<GoogleFormAnalytics>();
        if (formAnalytics != null)
        {
            formAnalytics.OnPlayerDeath(finalScore, gameSpeed);
        }
        else if (SurvivalAnalytics.Instance != null)
        {
            SurvivalAnalytics.Instance.OnPlayerDeath(finalScore, gameSpeed);
        }

        // Send to Enhanced Analytics
        if (EnhancedAnalytics.Instance != null)
        {
            Debug.Log($"📊 Sending Enhanced Analytics: Score={finalScore}, Time={survivalTime:F2}s, Speed={gameSpeed:F2}, Cause={causeOfDeath}, GravityFlipped={gravityFlipped}, ControlsReversed={controlsReversed}");
            EnhancedAnalytics.Instance.OnPlayerDeath(
                causeOfDeath,
                survivalTime,
                finalScore,
                gameSpeed,
                gravityFlipped,
                controlsReversed
            );
        }
        else
        {
            Debug.LogWarning("⚠️ EnhancedAnalytics not found! Did you add it to the scene?");
        }
    }
}
