using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Owns the live score, updates a TMP label, and exposes the value to other systems.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI scoreText;

    /// <summary> Simple per-scene singleton. </summary>
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [Tooltip("Points gained per second while the game is running.")]
    public float scoreRate = 10f;

    // Internal state
    private float score = 0f;
    private bool isGameOver = false;

    /// <summary> Integer score other systems should read. </summary>
    public int CurrentScore => Mathf.FloorToInt(score);

    /// <summary> Whether scoring is paused due to game over. </summary>
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        // Replace any previous instance on scene reloads.
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (isGameOver) return;

        // Passive score gain over time
        score += Time.deltaTime * scoreRate;
        UpdateScoreUI();
    }

    // ---------------- Public API ----------------

    /// <summary> Resets score & timers and resumes time. Call on scene start/restart. </summary>
    public void StartGame()
    {
        score = 0f;
        isGameOver = false;
        
        // Make sure score text is visible at game start
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            // Enhance text visibility with outline and shadow
            EnhanceScoreTextVisibility();
        }
        
        UpdateScoreUI();

        // Reset and start survival timer if present
#if UNITY_2023_1_OR_NEWER
        var timer = Object.FindFirstObjectByType<SurvivalTimer>();
#else
        var timer = Object.FindObjectOfType<SurvivalTimer>();
#endif
        if (timer != null)
        {
            timer.ResetTimer();
            timer.StartTimer();
        }

        Time.timeScale = 1f;
    }

    /// <summary> Adds a discrete amount to score (e.g., pickups, kills). </summary>
    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += Mathf.Max(0, amount);
        UpdateScoreUI();
    }

    /// <summary> Freezes passive scoring, stops timers, saves highscores. </summary>
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

#if UNITY_2023_1_OR_NEWER
        var timer = Object.FindFirstObjectByType<SurvivalTimer>();
#else
        var timer = Object.FindObjectOfType<SurvivalTimer>();
#endif
        if (timer != null) timer.StopTimer();

        SaveHighScores(CurrentScore);
    }

    /// <summary> Returns the current integer score. </summary>
    public int GetFinalScore() => CurrentScore;

    /// <summary> Returns top 5 scores (for a leaderboard UI). </summary>
    public List<int> GetTopScores()
    {
        var top = new List<int>(5);
        for (int i = 0; i < 5; i++)
            top.Add(PlayerPrefs.GetInt("HighScore" + i, 0));
        return top;
    }

    // ---------------- Helpers ----------------

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {CurrentScore}";
    }

    /// <summary>
    /// Enhances score text visibility by making it bold, white, and ensuring good contrast
    /// Note: For outline/shadow effects, configure them in the Inspector on the TextMeshPro component
    /// This method ensures the text is at least bold and white for visibility
    /// </summary>
    private void EnhanceScoreTextVisibility()
    {
        if (scoreText == null) return;

        // Set text color to bright white for maximum visibility
        //scoreText.color = Color.white;

        // Make text bold for better visibility
        //scoreText.fontStyle = FontStyles.Bold;

        // Increase font size slightly if it's too small (optional - adjust as needed)
        if (scoreText.fontSize < 40)
        {
            scoreText.fontSize = 40;
        }

        // Force update to apply changes
        // scoreText.SetAllDirty();
        
        // Note: For best results, in Unity Inspector:
        // 1. Select the Score Text GameObject
        // 2. In TextMeshPro component, go to "Extra Settings"
        // 3. Enable "Outline" and set:
        //    - Outline Width: 0.3
        //    - Outline Color: Black (0, 0, 0, 255)
        // 4. Or enable "Shadow" and set:
        //    - Shadow Offset: (2, -2)
        //    - Shadow Color: Black with some transparency
    }

    private static void SaveHighScores(int newScore)
    {
        // Read existing top 5
        var scores = new List<int>(6);
        for (int i = 0; i < 5; i++)
            scores.Add(PlayerPrefs.GetInt("HighScore" + i, 0));

        // Add and sort (desc), keep 5
        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a));
        for (int i = 0; i < 5; i++)
            PlayerPrefs.SetInt("HighScore" + i, scores[i]);
    }
}
