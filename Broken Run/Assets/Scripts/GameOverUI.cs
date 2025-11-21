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

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        // 2) stop music
        if (musicManager != null) musicManager.StopMusic();

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
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        if (musicManager != null) musicManager.PlayMusic();

        if (ScoreManager.Instance != null && ScoreManager.Instance.scoreText != null)
            ScoreManager.Instance.scoreText.gameObject.SetActive(true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
