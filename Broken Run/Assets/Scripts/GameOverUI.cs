using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI leaderboardText; 

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);

        int finalScore = ScoreManager.Instance.GetFinalScore();
        finalScoreText.text = $"Final Score: {finalScore}";

        
        List<int> topScores = ScoreManager.Instance.GetTopScores();

        
        string leaderboard = "Leaderboard:\n";
        for (int i = 0; i < topScores.Count; i++)
        {
            leaderboard += $"{i + 1}. {topScores[i]}\n";
        }
        leaderboardText.text = leaderboard;
    }

    public void RestartGame()
    {
        // Reset time scale BEFORE loading scene
        Time.timeScale = 1f;
        
        // Use scene build index (more reliable than name)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
