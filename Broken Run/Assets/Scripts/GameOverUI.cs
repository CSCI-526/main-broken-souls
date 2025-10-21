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

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        // 1) fade out the “Back to normal in …” timers / banners
        if (modeUI != null) modeUI.HideAllWithFade(0.25f);

        // 2) now show the game-over UI
        gameOverPanel.SetActive(true);

        int finalScore = ScoreManager.Instance.GetFinalScore();
        finalScoreText.text = $"Final Score: {finalScore}";

        List<int> topScores = ScoreManager.Instance.GetTopScores();

        string leaderboard = "Leaderboard:\n";
        for (int i = 0; i < topScores.Count; i++)
            leaderboard += $"{i + 1}. {topScores[i]}\n";
        leaderboardText.text = leaderboard;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
